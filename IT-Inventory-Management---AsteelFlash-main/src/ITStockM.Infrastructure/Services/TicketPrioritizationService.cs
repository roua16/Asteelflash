using System.Text.RegularExpressions;
using System.Text.Json;
using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Application.Common.Models;
using ITStockM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using ITStockM.Services.Interfaces;

namespace ITStockM.Services.Maintenance;

public sealed class TicketPrioritizationService : ITicketPrioritizationService
{
    private static readonly Dictionary<string, decimal> KeywordWeights = new(StringComparer.OrdinalIgnoreCase)
    {
        ["down"] = 30m,
        ["inaccessible"] = 30m,
        ["server"] = 20m,
        ["production"] = 20m,
        ["security"] = 20m,
        ["breach"] = 25m,
        ["urgent"] = 18m,
        ["critical"] = 22m,
        ["cannot"] = 12m,
        ["failed"] = 12m,
        ["crash"] = 15m,
        ["slow"] = 6m,
        ["printer"] = 4m,
        ["mouse"] = 2m,
        ["keyboard"] = 2m
    };

    private readonly ITStockManagmentContext? _context;
    private readonly AiModelRetrainingOptions _options;
    private readonly MLContext _mlContext = new(17);
    private readonly SemaphoreSlim _trainLock = new(1, 1);

    private ITransformer? _model;
    private DateTime _lastTrainingUtc = DateTime.MinValue;
    private static readonly string ModelDirectory = Path.Combine(AppContext.BaseDirectory, "ml-models");
    private static readonly string ScopedModelDirectory = Path.Combine(ModelDirectory, "ticket-prioritization");
    private static readonly string ManifestPath = Path.Combine(ScopedModelDirectory, "manifest.json");

    private readonly object _driftLock = new();
    private double _baselineUrgency = 3;
    private double _baselineImpactedUsers = 10;
    private double _baselineEquipmentCriticality = 3;
    private double _observedUrgency = 3;
    private double _observedImpactedUsers = 10;
    private double _observedEquipmentCriticality = 3;
    private double _driftScore;
    private int _trainingSampleCount;
    private double _validationMetric;
    private string _lastRetrainStatus = "not_trained";
    private string? _activeVersion;
    private string? _previousVersion;
    private string? _activeModelPath;

    public TicketPrioritizationService(
        ITStockManagmentContext? context = null,
        IOptions<AiModelRetrainingOptions>? options = null)
    {
        _context = context;
        _options = options?.Value ?? new AiModelRetrainingOptions();
    }

    private TimeSpan RetrainInterval => TimeSpan.FromMinutes(Math.Max(15, _options.IntervalMinutes));

    public async Task<TicketPriorityPredictionDto> PredictPriorityAsync(
        TicketPriorityRequestDto request,
        CancellationToken ct = default)
    {
        TrackRequestForDrift(request);
        await EnsureModelAsync(ct);

        var text = $"{request.Title} {request.Description} {request.Category} {request.EquipmentType}";
        var tokens = Tokenize(text);

        var matchedKeywords = KeywordWeights.Keys
            .Where(k => tokens.Contains(k, StringComparer.OrdinalIgnoreCase))
            .ToList();

        var input = new TicketPriorityModelInput
        {
            Text = $"{request.Title} {request.Description}",
            Category = request.Category ?? string.Empty,
            EquipmentType = request.EquipmentType ?? string.Empty,
            UrgencyLevel = Math.Clamp(request.UrgencyLevel, 1, 5),
            ImpactedUsers = Math.Clamp(request.ImpactedUsers, 0, 10_000),
            EquipmentCriticality = Math.Clamp(request.EquipmentCriticality, 1, 5)
        };

        string priority = "Moyenne";
        decimal score = 50m;

        if (_model is not null)
        {
            var engine = _mlContext.Model.CreatePredictionEngine<TicketPriorityModelInput, TicketPriorityModelOutput>(_model);
            var prediction = engine.Predict(input);
            priority = string.IsNullOrWhiteSpace(prediction.PredictedLabel)
                ? DetermineFallbackPriority(matchedKeywords, request)
                : prediction.PredictedLabel;
        }
        else
        {
            priority = DetermineFallbackPriority(matchedKeywords, request);
        }

        var keywordScore = matchedKeywords.Sum(k => KeywordWeights[k]);
        score = ComputeSeverityScore(priority, request, keywordScore);
        var explanation = $"priority={priority}, severity={score:0.##}, keywords={keywordScore:0.##}, matched={matchedKeywords.Count}";

        var response = new TicketPriorityPredictionDto(
            priority,
            score,
            matchedKeywords,
            explanation);

        return response;
    }

    public async Task RetrainAsync(CancellationToken ct = default)
    {
        _lastTrainingUtc = DateTime.MinValue;
        await EnsureModelAsync(ct);
    }

    public Task<AiModelStatusDto> GetModelStatusAsync(CancellationToken ct = default)
    {
        var status = new AiModelStatusDto(
            ModelName: "ticket-prioritization",
            ActiveVersion: _activeVersion,
            PreviousVersion: _previousVersion,
            LastTrainedUtc: _lastTrainingUtc == DateTime.MinValue ? null : _lastTrainingUtc,
            RetrainIntervalMinutes: (int)RetrainInterval.TotalMinutes,
            TrainingSampleCount: _trainingSampleCount,
            ValidationMetric: _validationMetric,
            DriftScore: _driftScore,
            DriftLevel: DriftLevel(_driftScore),
            LastRetrainStatus: _lastRetrainStatus,
            ActiveModelPath: _activeModelPath);

        return Task.FromResult(status);
    }

    private async Task EnsureModelAsync(CancellationToken ct)
    {
        if (_model is null)
        {
            TryLoadModelFromDisk();
        }

        if (_model is not null && DateTime.UtcNow - _lastTrainingUtc < RetrainInterval)
        {
            return;
        }

        await _trainLock.WaitAsync(ct);
        try
        {
            if (_model is not null && DateTime.UtcNow - _lastTrainingUtc < RetrainInterval)
            {
                return;
            }

            var trainingRows = await BuildTrainingSetAsync(ct);
            if (trainingRows.Count < Math.Max(12, _options.MinimumTicketTrainingSamples))
            {
                _model = null;
                _lastTrainingUtc = DateTime.UtcNow;
                _lastRetrainStatus = "insufficient_training_data";
                return;
            }

            var distinctLabels = trainingRows
                .Select(r => r.Label)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            if (distinctLabels < _options.MinimumTicketDistinctLabels)
            {
                _lastTrainingUtc = DateTime.UtcNow;
                _lastRetrainStatus = "blocked_data_quality_low_label_diversity";
                return;
            }

            var missingTextRate = trainingRows.Average(r => string.IsNullOrWhiteSpace(r.Text) ? 1.0 : 0.0);
            if (missingTextRate > _options.MaximumTicketMissingTextRate)
            {
                _lastTrainingUtc = DateTime.UtcNow;
                _lastRetrainStatus = "blocked_data_quality_missing_text";
                return;
            }

            var trainView = _mlContext.Data.LoadFromEnumerable(trainingRows);
            var split = _mlContext.Data.TrainTestSplit(trainView, testFraction: 0.2, seed: 17);

            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(
                    outputColumnName: "LabelKey",
                    inputColumnName: nameof(TicketPriorityModelInput.Label))
                .Append(_mlContext.Transforms.Text.FeaturizeText("TextFeats", nameof(TicketPriorityModelInput.Text)))
                .Append(_mlContext.Transforms.Text.FeaturizeText("CategoryFeats", nameof(TicketPriorityModelInput.Category)))
                .Append(_mlContext.Transforms.Text.FeaturizeText("EquipmentTypeFeats", nameof(TicketPriorityModelInput.EquipmentType)))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features",
                    "TextFeats",
                    "CategoryFeats",
                    "EquipmentTypeFeats",
                    nameof(TicketPriorityModelInput.UrgencyLevel),
                    nameof(TicketPriorityModelInput.ImpactedUsers),
                    nameof(TicketPriorityModelInput.EquipmentCriticality)))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                    labelColumnName: "LabelKey",
                    featureColumnName: "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue(
                    outputColumnName: nameof(TicketPriorityModelOutput.PredictedLabel),
                    inputColumnName: nameof(TicketPriorityModelOutput.PredictedLabel)));

            var candidateModel = pipeline.Fit(split.TrainSet);
            var candidateEval = _mlContext.MulticlassClassification.Evaluate(
                candidateModel.Transform(split.TestSet),
                labelColumnName: "LabelKey",
                scoreColumnName: nameof(TicketPriorityModelOutput.Score));

            var candidateMetric = candidateEval.MicroAccuracy;

            if (candidateMetric < _options.MinimumTicketValidationMetric)
            {
                _lastTrainingUtc = DateTime.UtcNow;
                _lastRetrainStatus = "blocked_acceptance_min_metric";
                return;
            }

            var currentMetric = _validationMetric;
            var tolerance = Math.Clamp(_options.ActivationMetricTolerance, 0, 0.5);
            var shouldActivate = _model is null || candidateMetric >= currentMetric - tolerance;

            if (shouldActivate)
            {
                _model = candidateModel;
                _validationMetric = candidateMetric;
                _trainingSampleCount = trainingRows.Count;
                _lastRetrainStatus = "activated";

                UpdateBaseline(trainingRows);
                SaveVersionedModelToDisk(candidateModel, split.TrainSet.Schema, trainingRows, candidateMetric);
            }
            else
            {
                _lastRetrainStatus = "rolled_over_to_previous";
            }

            _lastTrainingUtc = DateTime.UtcNow;
        }
        finally
        {
            _trainLock.Release();
        }
    }

    private void TryLoadModelFromDisk()
    {
        if (!File.Exists(ManifestPath))
        {
            return;
        }

        try
        {
            var manifest = JsonSerializer.Deserialize<ModelManifest>(File.ReadAllText(ManifestPath));
            if (manifest?.ActiveModelPath is null || !File.Exists(manifest.ActiveModelPath))
            {
                if (manifest?.PreviousModelPath is not null && File.Exists(manifest.PreviousModelPath))
                {
                    _model = _mlContext.Model.Load(manifest.PreviousModelPath, out _);
                    _activeVersion = manifest.PreviousVersion;
                    _previousVersion = null;
                    _activeModelPath = manifest.PreviousModelPath;
                    _lastTrainingUtc = manifest.LastTrainedUtc;
                    _trainingSampleCount = manifest.TrainingSampleCount;
                    _validationMetric = manifest.ValidationMetric;
                    _lastRetrainStatus = "rolled_over_to_previous";
                    return;
                }

                return;
            }

            _model = _mlContext.Model.Load(manifest.ActiveModelPath, out _);
            _activeVersion = manifest.ActiveVersion;
            _previousVersion = manifest.PreviousVersion;
            _activeModelPath = manifest.ActiveModelPath;
            _lastTrainingUtc = manifest.LastTrainedUtc;
            _trainingSampleCount = manifest.TrainingSampleCount;
            _validationMetric = manifest.ValidationMetric;
            _lastRetrainStatus = manifest.LastRetrainStatus;

            _baselineUrgency = manifest.BaselineUrgency;
            _baselineImpactedUsers = manifest.BaselineImpactedUsers;
            _baselineEquipmentCriticality = manifest.BaselineEquipmentCriticality;
        }
        catch
        {
            _model = null;
            _lastTrainingUtc = DateTime.MinValue;
        }
    }

    private void SaveVersionedModelToDisk(ITransformer model, DataViewSchema schema, IReadOnlyCollection<TicketPriorityModelInput> trainingRows, double validationMetric)
    {
        Directory.CreateDirectory(ScopedModelDirectory);

        var version = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var filePath = Path.Combine(ScopedModelDirectory, $"model_{version}.zip");
        using (var stream = File.Create(filePath))
        {
            _mlContext.Model.Save(model, schema, stream);
        }

        var manifest = File.Exists(ManifestPath)
            ? JsonSerializer.Deserialize<ModelManifest>(File.ReadAllText(ManifestPath)) ?? new ModelManifest()
            : new ModelManifest();

        manifest.PreviousVersion = manifest.ActiveVersion;
        manifest.PreviousModelPath = manifest.ActiveModelPath;
        manifest.ActiveVersion = version;
        manifest.ActiveModelPath = filePath;
        manifest.LastTrainedUtc = DateTime.UtcNow;
        manifest.TrainingSampleCount = trainingRows.Count;
        manifest.ValidationMetric = validationMetric;
        manifest.BaselineUrgency = _baselineUrgency;
        manifest.BaselineImpactedUsers = _baselineImpactedUsers;
        manifest.BaselineEquipmentCriticality = _baselineEquipmentCriticality;
        manifest.LastRetrainStatus = _lastRetrainStatus;
        manifest.DatasetSnapshotId = ComputeDatasetSnapshotId(trainingRows);
        manifest.TrainingCommit = ResolveTrainingCommit();
        manifest.ConfigHash = ComputeConfigHash();

        File.WriteAllText(ManifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        _activeVersion = manifest.ActiveVersion;
        _previousVersion = manifest.PreviousVersion;
        _activeModelPath = manifest.ActiveModelPath;

        CleanupOldModelArtifacts(manifest);
    }

    private void UpdateBaseline(IReadOnlyCollection<TicketPriorityModelInput> trainingRows)
    {
        if (trainingRows.Count == 0)
        {
            return;
        }

        _baselineUrgency = trainingRows.Average(r => r.UrgencyLevel);
        _baselineImpactedUsers = trainingRows.Average(r => r.ImpactedUsers);
        _baselineEquipmentCriticality = trainingRows.Average(r => r.EquipmentCriticality);
    }

    private void TrackRequestForDrift(TicketPriorityRequestDto request)
    {
        lock (_driftLock)
        {
            const double alpha = 0.1;
            _observedUrgency = (1 - alpha) * _observedUrgency + alpha * Math.Clamp(request.UrgencyLevel, 1, 5);
            _observedImpactedUsers = (1 - alpha) * _observedImpactedUsers + alpha * Math.Clamp(request.ImpactedUsers, 0, 10_000);
            _observedEquipmentCriticality = (1 - alpha) * _observedEquipmentCriticality + alpha * Math.Clamp(request.EquipmentCriticality, 1, 5);

            var urgencyDelta = Math.Abs(_observedUrgency - _baselineUrgency) / 4.0;
            var impactedDelta = Math.Min(1.0, Math.Abs(_observedImpactedUsers - _baselineImpactedUsers) / Math.Max(_baselineImpactedUsers, 1));
            var criticalityDelta = Math.Abs(_observedEquipmentCriticality - _baselineEquipmentCriticality) / 4.0;

            _driftScore = Math.Round((urgencyDelta * 0.35 + impactedDelta * 0.4 + criticalityDelta * 0.25) * 100, 2);
        }
    }

    private string DriftLevel(double score)
    {
        if (score >= _options.DriftHighThreshold) return "high";
        if (score >= _options.DriftMediumThreshold) return "medium";
        return "low";
    }

    private void CleanupOldModelArtifacts(ModelManifest manifest)
    {
        var maxVersions = Math.Max(2, _options.MaxModelVersionsToKeep);

        var modelFiles = Directory
            .GetFiles(ScopedModelDirectory, "model_*.zip", SearchOption.TopDirectoryOnly)
            .OrderByDescending(Path.GetFileName)
            .ToList();

        if (modelFiles.Count <= maxVersions)
        {
            return;
        }

        var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            manifest.ActiveModelPath ?? string.Empty,
            manifest.PreviousModelPath ?? string.Empty
        };

        foreach (var file in modelFiles.Take(maxVersions))
        {
            keep.Add(file);
        }

        foreach (var file in modelFiles)
        {
            if (keep.Contains(file))
            {
                continue;
            }

            try
            {
                File.Delete(file);
            }
            catch
            {
                // Retention cleanup is best effort and must not block serving predictions.
            }
        }
    }

    private static string ComputeDatasetSnapshotId(IEnumerable<TicketPriorityModelInput> trainingRows)
    {
        var materialized = trainingRows
            .Select(r => $"{r.Label}|{r.Category}|{r.EquipmentType}|{r.UrgencyLevel:0.###}|{r.ImpactedUsers:0.###}|{r.EquipmentCriticality:0.###}|{r.Text}")
            .OrderBy(x => x, StringComparer.Ordinal)
            .Take(4000);

        var payload = string.Join("\n", materialized);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private string ComputeConfigHash()
    {
        var payload = string.Join("|",
            _options.IntervalMinutes,
            _options.MinimumTicketTrainingSamples,
            _options.ActivationMetricTolerance,
            _options.MinimumTicketValidationMetric,
            _options.MinimumTicketDistinctLabels,
            _options.MaximumTicketMissingTextRate,
            _options.MaxModelVersionsToKeep,
            _options.DriftMediumThreshold,
            _options.DriftHighThreshold);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ResolveTrainingCommit()
    {
        var commit = Environment.GetEnvironmentVariable("GIT_COMMIT_SHA")
            ?? Environment.GetEnvironmentVariable("SOURCE_VERSION")
            ?? Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION");

        return string.IsNullOrWhiteSpace(commit) ? "unknown" : commit.Trim();
    }

    private async Task<List<TicketPriorityModelInput>> BuildTrainingSetAsync(CancellationToken ct)
    {
        var rows = BuildSeedData();

        if (_context is null)
        {
            return rows;
        }

        var tickets = await _context.MaintenanceTickets
            .OrderByDescending(t => t.ReportedAt)
            .Take(300)
            .ToListAsync(ct);

        foreach (var ticket in tickets)
        {
            rows.Add(new TicketPriorityModelInput
            {
                Text = ticket.ProblemDescription,
                Category = ticket.Status,
                EquipmentType = string.Empty,
                UrgencyLevel = InferUrgencyFromText(ticket.ProblemDescription),
                ImpactedUsers = InferImpactedUsers(ticket.ProblemDescription),
                EquipmentCriticality = InferEquipmentCriticality(ticket.ProblemDescription),
                Label = InferLabelFromTicket(ticket)
            });
        }

        return rows;
    }

    private static List<TicketPriorityModelInput> BuildSeedData()
    {
        return new List<TicketPriorityModelInput>
        {
            new() { Text = "production server down inaccessible all users blocked", Category = "Infrastructure", EquipmentType = "Server", UrgencyLevel = 5, ImpactedUsers = 300, EquipmentCriticality = 5, Label = "Critique" },
            new() { Text = "database crash cannot connect erp", Category = "Infrastructure", EquipmentType = "Server", UrgencyLevel = 5, ImpactedUsers = 220, EquipmentCriticality = 5, Label = "Critique" },
            new() { Text = "security breach suspected urgent", Category = "Security", EquipmentType = "Firewall", UrgencyLevel = 5, ImpactedUsers = 120, EquipmentCriticality = 5, Label = "Critique" },
            new() { Text = "pc does not start for manager", Category = "Hardware", EquipmentType = "Desktop", UrgencyLevel = 4, ImpactedUsers = 3, EquipmentCriticality = 4, Label = "Haute" },
            new() { Text = "network unstable in accounting department", Category = "Network", EquipmentType = "Switch", UrgencyLevel = 4, ImpactedUsers = 25, EquipmentCriticality = 4, Label = "Haute" },
            new() { Text = "laptop battery failing and random crash", Category = "Hardware", EquipmentType = "Laptop", UrgencyLevel = 4, ImpactedUsers = 2, EquipmentCriticality = 3, Label = "Haute" },
            new() { Text = "application is slow for a few users", Category = "Application", EquipmentType = "Workstation", UrgencyLevel = 3, ImpactedUsers = 8, EquipmentCriticality = 3, Label = "Moyenne" },
            new() { Text = "screen flickering occasionally", Category = "Hardware", EquipmentType = "Monitor", UrgencyLevel = 3, ImpactedUsers = 1, EquipmentCriticality = 2, Label = "Moyenne" },
            new() { Text = "intermittent vpn issue", Category = "Network", EquipmentType = "Router", UrgencyLevel = 3, ImpactedUsers = 6, EquipmentCriticality = 3, Label = "Moyenne" },
            new() { Text = "printer slow in office", Category = "Peripheral", EquipmentType = "Printer", UrgencyLevel = 1, ImpactedUsers = 1, EquipmentCriticality = 1, Label = "Faible" },
            new() { Text = "mouse not comfortable", Category = "Peripheral", EquipmentType = "Mouse", UrgencyLevel = 1, ImpactedUsers = 1, EquipmentCriticality = 1, Label = "Faible" },
            new() { Text = "keyboard key stuck", Category = "Peripheral", EquipmentType = "Keyboard", UrgencyLevel = 1, ImpactedUsers = 1, EquipmentCriticality = 1, Label = "Faible" },
            new() { Text = "server room cooling failure", Category = "Infrastructure", EquipmentType = "Cooling", UrgencyLevel = 5, ImpactedUsers = 80, EquipmentCriticality = 5, Label = "Critique" },
            new() { Text = "critical workstation gpu failed for designer", Category = "Hardware", EquipmentType = "Workstation", UrgencyLevel = 4, ImpactedUsers = 2, EquipmentCriticality = 4, Label = "Haute" },
            new() { Text = "scanner needs maintenance", Category = "Peripheral", EquipmentType = "Scanner", UrgencyLevel = 2, ImpactedUsers = 2, EquipmentCriticality = 2, Label = "Faible" }
        };
    }

    private static string InferLabelFromTicket(Domain.Entities.MaintenanceTicket ticket)
    {
        var text = ticket.ProblemDescription.ToLowerInvariant();
        if (text.Contains("server") && (text.Contains("down") || text.Contains("inaccessible") || text.Contains("cannot")))
        {
            return "Critique";
        }

        if (text.Contains("critical") || text.Contains("urgent") || text.Contains("crash") || text.Contains("failed"))
        {
            return "Haute";
        }

        if (text.Contains("slow") || text.Contains("unstable") || text.Contains("intermittent"))
        {
            return "Moyenne";
        }

        return "Faible";
    }

    private static float InferUrgencyFromText(string text)
    {
        var t = text.ToLowerInvariant();
        if (t.Contains("urgent") || t.Contains("critical") || t.Contains("down")) return 5;
        if (t.Contains("cannot") || t.Contains("failed") || t.Contains("crash")) return 4;
        if (t.Contains("slow") || t.Contains("error")) return 3;
        return 2;
    }

    private static float InferImpactedUsers(string text)
    {
        var t = text.ToLowerInvariant();
        if (t.Contains("all users") || t.Contains("production")) return 150;
        if (t.Contains("department") || t.Contains("team")) return 20;
        return 2;
    }

    private static float InferEquipmentCriticality(string text)
    {
        var t = text.ToLowerInvariant();
        if (t.Contains("server") || t.Contains("database") || t.Contains("firewall")) return 5;
        if (t.Contains("network") || t.Contains("switch") || t.Contains("router")) return 4;
        if (t.Contains("laptop") || t.Contains("desktop") || t.Contains("pc")) return 3;
        return 2;
    }

    private static decimal ComputeSeverityScore(string priority, TicketPriorityRequestDto request, decimal keywordScore)
    {
        var baseline = priority switch
        {
            "Critique" => 90m,
            "Haute" => 70m,
            "Moyenne" => 50m,
            _ => 20m
        };

        var urgencyAdj = (Math.Clamp(request.UrgencyLevel, 1, 5) - 3) * 6m;
        var impactAdj = (decimal)Math.Log10(Math.Max(request.ImpactedUsers, 1)) * 4m;
        var criticalityAdj = (Math.Clamp(request.EquipmentCriticality, 1, 5) - 3) * 3m;
        var keywordAdj = Math.Clamp((keywordScore - 20m) / 5m, -6m, 12m);

        return Math.Round(Math.Clamp(baseline + urgencyAdj + impactAdj + criticalityAdj + keywordAdj, 0m, 100m), 2);
    }

    private static IReadOnlyList<string> Tokenize(string text)
    {
        return Regex.Matches(text.ToLowerInvariant(), "[a-z0-9]+")
            .Select(m => m.Value)
            .ToList();
    }

    private static string DetermineFallbackPriority(IReadOnlyList<string> matchedKeywords, TicketPriorityRequestDto request)
    {
        var keywordScore = matchedKeywords.Sum(k => KeywordWeights[k]);
        var urgencyScore = Math.Clamp(request.UrgencyLevel, 1, 5) * 10m;
        var impactScore = (decimal)Math.Log10(Math.Max(request.ImpactedUsers, 1)) * 5m;
        var criticalityScore = (Math.Clamp(request.EquipmentCriticality, 1, 5) - 3) * 6m;

        var total = keywordScore + urgencyScore + impactScore + criticalityScore;

        return total switch
        {
            >= 95m => "Critique",
            >= 70m => "Haute",
            >= 45m => "Moyenne",
            _ => "Faible",
        };
    }

    private sealed class TicketPriorityModelInput
    {
        public string Text { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public float UrgencyLevel { get; set; }
        public float ImpactedUsers { get; set; }
        public float EquipmentCriticality { get; set; }
        public string Label { get; set; } = "Moyenne";
    }

    private sealed class TicketPriorityModelOutput
    {
        public string PredictedLabel { get; set; } = string.Empty;
        public float[] Score { get; set; } = Array.Empty<float>();
    }

    private sealed class ModelManifest
    {
        public string? ActiveVersion { get; set; }
        public string? PreviousVersion { get; set; }
        public string? ActiveModelPath { get; set; }
        public string? PreviousModelPath { get; set; }
        public DateTime LastTrainedUtc { get; set; }
        public int TrainingSampleCount { get; set; }
        public double ValidationMetric { get; set; }
        public double BaselineUrgency { get; set; } = 3;
        public double BaselineImpactedUsers { get; set; } = 10;
        public double BaselineEquipmentCriticality { get; set; } = 3;
        public string LastRetrainStatus { get; set; } = "not_trained";
        public string DatasetSnapshotId { get; set; } = string.Empty;
        public string TrainingCommit { get; set; } = "unknown";
        public string ConfigHash { get; set; } = string.Empty;
    }
}
