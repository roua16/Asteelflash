using ITStockM.Application.Features.Recommendations.DTOs;
using ITStockM.Application.Common.Models;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Domain.Enums;
using ITStockM.Services.Interfaces;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ITStockM.Services.Recommendations;

public sealed class HardwareRecommendationService : IHardwareRecommendationService
{
    private static readonly string[] HighPerformanceTokens = ["workstation", "i7", "i9", "ryzen 7", "ryzen 9", "thinkpad", "precision"];
    private static readonly string[] GraphicsTokens = ["gpu", "rtx", "quadro", "radeon", "workstation", "graphic"];
    private static readonly string[] OfficeTokens = ["pc", "desktop", "standard", "office", "bureautique"];

    private readonly ITStockManagmentContext _context;
    private readonly AiModelRetrainingOptions _options;
    private readonly MLContext _mlContext = new(42);
    private readonly SemaphoreSlim _trainLock = new(1, 1);

    private ITransformer? _model;
    private DateTime _lastTrainingUtc = DateTime.MinValue;
    private static readonly string ModelDirectory = Path.Combine(AppContext.BaseDirectory, "ml-models");
    private static readonly string ScopedModelDirectory = Path.Combine(ModelDirectory, "hardware-recommendation");
    private static readonly string ManifestPath = Path.Combine(ScopedModelDirectory, "manifest.json");

    private readonly object _driftLock = new();
    private double _baselineUsageLevel = 3;
    private double _baselineNeedsHighPerformance = 0.5;
    private double _baselineNeedsGraphics = 0.5;
    private double _observedUsageLevel = 3;
    private double _observedNeedsHighPerformance = 0.5;
    private double _observedNeedsGraphics = 0.5;
    private double _driftScore;
    private int _trainingSampleCount;
    private double _validationMetric;
    private string _lastRetrainStatus = "not_trained";
    private string? _activeVersion;
    private string? _previousVersion;
    private string? _activeModelPath;

    public HardwareRecommendationService(
        ITStockManagmentContext context,
        IOptions<AiModelRetrainingOptions>? options = null)
    {
        _context = context;
        _options = options?.Value ?? new AiModelRetrainingOptions();
    }

    private TimeSpan RetrainInterval => TimeSpan.FromMinutes(Math.Max(15, _options.IntervalMinutes));

    public async Task<IReadOnlyList<HardwareRecommendationDto>> RecommendAsync(
        HardwareRecommendationRequestDto request,
        CancellationToken ct = default)
    {
        TrackRequestForDrift(request);
        await EnsureModelAsync(ct);

        var topN = Math.Clamp(request.TopN, 1, 10);

        var materiels = await _context.Materiels
            .Where(m => m.QuantityITStock > 0 && m.LifecycleStatus != LifecycleStage.Retired)
            .Include(m => m.MaintenanceTickets)
            .ToListAsync(ct);

        if (_model is null || materiels.Count == 0)
        {
            return Array.Empty<HardwareRecommendationDto>();
        }

        var modelInputs = materiels.Select(m => BuildInferenceInput(m, request)).ToList();
        var inputView = _mlContext.Data.LoadFromEnumerable(modelInputs);
        var transformed = _model.Transform(inputView);
        var scored = _mlContext.Data.CreateEnumerable<RecommendationScoredRow>(transformed, reuseRowObject: false).ToList();

        var byMateriel = materiels.ToDictionary(m => m.Id);

        var results = scored
            .Where(s => byMateriel.ContainsKey(s.MaterielId))
            .Select(s =>
            {
                var materiel = byMateriel[s.MaterielId];
                var baseScore = Math.Clamp((decimal)s.Probability * 100m, 0m, 100m);
                var score = ApplyRequestBias(baseScore, materiel, request);
                var reason = BuildReason(materiel, request, score);

                return new HardwareRecommendationDto(
                    materiel.Id,
                    materiel.MaterielName,
                    materiel.Type,
                    score,
                    materiel.QuantityITStock,
                    reason);
            })
            .OrderByDescending(x => x.MatchScore)
            .ThenByDescending(x => x.AvailableQuantity)
            .Take(topN)
            .ToList();

        return results;
    }

    public async Task RetrainAsync(CancellationToken ct = default)
    {
        _lastTrainingUtc = DateTime.MinValue;
        await EnsureModelAsync(ct);
    }

    public Task<AiModelStatusDto> GetModelStatusAsync(CancellationToken ct = default)
    {
        var status = new AiModelStatusDto(
            ModelName: "hardware-recommendation",
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

            var trainingSet = await BuildTrainingSetAsync(ct);

            if (trainingSet.Count < Math.Max(4, _options.MinimumHardwareTrainingSamples))
            {
                _model = null;
                _lastTrainingUtc = DateTime.UtcNow;
                _lastRetrainStatus = "insufficient_training_data";
                return;
            }

            var trainView = _mlContext.Data.LoadFromEnumerable(trainingSet);
            var split = _mlContext.Data.TrainTestSplit(trainView, testFraction: 0.2, seed: 42);
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding(new[]
                {
                    new InputOutputColumnPair("RoleEncoded", nameof(RecommendationModelInput.Role)),
                    new InputOutputColumnPair("ServiceEncoded", nameof(RecommendationModelInput.Service)),
                    new InputOutputColumnPair("PreferredTypeEncoded", nameof(RecommendationModelInput.PreferredType)),
                    new InputOutputColumnPair("MaterielTypeEncoded", nameof(RecommendationModelInput.MaterielType))
                })
                .Append(_mlContext.Transforms.Text.FeaturizeText("MaterielNameFeats", nameof(RecommendationModelInput.MaterielName)))
                .Append(_mlContext.Transforms.Concatenate(
                    "Features",
                    "RoleEncoded",
                    "ServiceEncoded",
                    "PreferredTypeEncoded",
                    "MaterielTypeEncoded",
                    "MaterielNameFeats",
                    nameof(RecommendationModelInput.UsageLevel),
                    nameof(RecommendationModelInput.NeedsHighPerformance),
                    nameof(RecommendationModelInput.NeedsGraphics),
                    nameof(RecommendationModelInput.StockQuantity),
                    nameof(RecommendationModelInput.HealthScore),
                    nameof(RecommendationModelInput.OpenTicketCount)))
                .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                    labelColumnName: nameof(RecommendationModelInput.Label),
                    featureColumnName: "Features"));

            var candidateModel = pipeline.Fit(split.TrainSet);
            var candidateMetric = EvaluateBinaryModelSafely(candidateModel, split.TestSet, trainView);

            var currentMetric = _validationMetric;
            var tolerance = Math.Clamp(_options.ActivationMetricTolerance, 0, 0.5);
            var shouldActivate = _model is null || candidateMetric >= currentMetric - tolerance;

            if (shouldActivate)
            {
                _model = candidateModel;
                _validationMetric = candidateMetric;
                _trainingSampleCount = trainingSet.Count;
                _lastRetrainStatus = "activated";

                UpdateBaseline(trainingSet);
                SaveVersionedModelToDisk(candidateModel, split.TrainSet.Schema, trainingSet.Count, candidateMetric);
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

            _baselineUsageLevel = manifest.BaselineUsageLevel;
            _baselineNeedsHighPerformance = manifest.BaselineNeedsHighPerformance;
            _baselineNeedsGraphics = manifest.BaselineNeedsGraphics;
        }
        catch
        {
            _model = null;
            _lastTrainingUtc = DateTime.MinValue;
        }
    }

    private void SaveVersionedModelToDisk(ITransformer model, DataViewSchema schema, int sampleCount, double validationMetric)
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
        manifest.TrainingSampleCount = sampleCount;
        manifest.ValidationMetric = validationMetric;
        manifest.BaselineUsageLevel = _baselineUsageLevel;
        manifest.BaselineNeedsHighPerformance = _baselineNeedsHighPerformance;
        manifest.BaselineNeedsGraphics = _baselineNeedsGraphics;
        manifest.LastRetrainStatus = _lastRetrainStatus;

        File.WriteAllText(ManifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

        _activeVersion = manifest.ActiveVersion;
        _previousVersion = manifest.PreviousVersion;
        _activeModelPath = manifest.ActiveModelPath;

        CleanupOldModelArtifacts(manifest);
    }

    private void UpdateBaseline(IReadOnlyCollection<RecommendationModelInput> trainingSet)
    {
        if (trainingSet.Count == 0)
        {
            return;
        }

        _baselineUsageLevel = trainingSet.Average(t => t.UsageLevel);
        _baselineNeedsHighPerformance = trainingSet.Average(t => t.NeedsHighPerformance);
        _baselineNeedsGraphics = trainingSet.Average(t => t.NeedsGraphics);
    }

    private void TrackRequestForDrift(HardwareRecommendationRequestDto request)
    {
        lock (_driftLock)
        {
            const double alpha = 0.1;

            _observedUsageLevel = (1 - alpha) * _observedUsageLevel + alpha * Math.Clamp(request.UsageLevel, 1, 5);
            _observedNeedsHighPerformance = (1 - alpha) * _observedNeedsHighPerformance + alpha * (request.NeedsHighPerformance ? 1 : 0);
            _observedNeedsGraphics = (1 - alpha) * _observedNeedsGraphics + alpha * (request.NeedsGraphics ? 1 : 0);

            var usageDelta = Math.Abs(_observedUsageLevel - _baselineUsageLevel) / 4.0;
            var hpDelta = Math.Abs(_observedNeedsHighPerformance - _baselineNeedsHighPerformance);
            var graphicsDelta = Math.Abs(_observedNeedsGraphics - _baselineNeedsGraphics);

            _driftScore = Math.Round((usageDelta * 0.5 + hpDelta * 0.25 + graphicsDelta * 0.25) * 100, 2);
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

    private double EvaluateBinaryModelSafely(ITransformer model, IDataView testSet, IDataView fullSet)
    {
        try
        {
            var testMetrics = _mlContext.BinaryClassification.Evaluate(
                model.Transform(testSet),
                labelColumnName: nameof(RecommendationModelInput.Label));

            if (!double.IsNaN(testMetrics.AreaUnderRocCurve) && testMetrics.AreaUnderRocCurve > 0)
            {
                return testMetrics.AreaUnderRocCurve;
            }

            return testMetrics.Accuracy;
        }
        catch (ArgumentOutOfRangeException)
        {
            var fullMetrics = _mlContext.BinaryClassification.Evaluate(
                model.Transform(fullSet),
                labelColumnName: nameof(RecommendationModelInput.Label));

            if (!double.IsNaN(fullMetrics.AreaUnderRocCurve) && fullMetrics.AreaUnderRocCurve > 0)
            {
                return fullMetrics.AreaUnderRocCurve;
            }

            return fullMetrics.Accuracy;
        }
    }

    private async Task<List<RecommendationModelInput>> BuildTrainingSetAsync(CancellationToken ct)
    {
        var rand = new Random(42);
        var training = new List<RecommendationModelInput>();

        var materiels = await _context.Materiels
            .Where(m => m.LifecycleStatus != LifecycleStage.Retired)
            .Include(m => m.MaintenanceTickets)
            .ToListAsync(ct);

        if (materiels.Count == 0)
        {
            return training;
        }

        var assignments = await _context.Assignments
            .Include(a => a.AssignedEmployee)
            .Include(a => a.AssignmentMateriels)
            .ToListAsync(ct);

        var usageByEmployee = assignments
            .Where(a => a.AssignedTo.HasValue)
            .GroupBy(a => a.AssignedTo!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var assignment in assignments)
        {
            var employee = assignment.AssignedEmployee;
            var role = employee?.Role ?? "Employee";
            var service = employee?.Service ?? "General";
            var usageLevel = assignment.AssignedTo.HasValue && usageByEmployee.TryGetValue(assignment.AssignedTo.Value, out var count)
                ? Math.Clamp(count / 4f, 1f, 5f)
                : 3f;

            foreach (var am in assignment.AssignmentMateriels)
            {
                var positive = materiels.FirstOrDefault(m => m.Id == am.MaterielId);
                if (positive is null)
                {
                    continue;
                }

                training.Add(BuildTrainingRow(positive, role, service, usageLevel, label: true));

                var negatives = materiels
                    .Where(m => m.Id != positive.Id)
                    .OrderBy(_ => rand.Next())
                    .Take(2)
                    .ToList();

                foreach (var negative in negatives)
                {
                    training.Add(BuildTrainingRow(negative, role, service, usageLevel, label: false));
                }
            }
        }

        // Bootstrap rows keep the model trainable even with sparse historical assignments.
        foreach (var materiel in materiels)
        {
            var text = $"{materiel.MaterielName} {materiel.Type}".ToLowerInvariant();
            var highPerf = ContainsAny(text, HighPerformanceTokens);
            var graphics = ContainsAny(text, GraphicsTokens);
            var office = ContainsAny(text, OfficeTokens);

            training.Add(BuildTrainingRow(materiel, "Developer", "IT", 5f, label: highPerf));
            training.Add(BuildTrainingRow(materiel, "Designer", "Marketing", 4f, label: graphics));
            training.Add(BuildTrainingRow(materiel, "HR", "RH", 2f, label: office || !highPerf));
        }

        return training;
    }

    private static RecommendationModelInput BuildTrainingRow(
        Materiel materiel,
        string role,
        string service,
        float usageLevel,
        bool label)
    {
        var openTickets = materiel.MaintenanceTickets?.Count(t => t.Status != Domain.Enums.MaintenanceTicketStatus.Closed) ?? 0;

        return new RecommendationModelInput
        {
            Label = label,
            Role = role,
            Service = service,
            UsageLevel = usageLevel,
            NeedsHighPerformance = RoleSuggestsHighPerformance(role, service) ? 1f : 0f,
            NeedsGraphics = RoleSuggestsGraphics(role, service) ? 1f : 0f,
            PreferredType = materiel.Type,
            MaterielId = materiel.Id,
            MaterielType = materiel.Type,
            MaterielName = materiel.MaterielName,
            StockQuantity = materiel.QuantityITStock,
            HealthScore = (float)(materiel.CurrentHealthScore ?? 70m),
            OpenTicketCount = openTickets
        };
    }

    private static RecommendationModelInput BuildInferenceInput(Materiel materiel, HardwareRecommendationRequestDto request)
    {
        var openTickets = materiel.MaintenanceTickets?.Count(t => t.Status != Domain.Enums.MaintenanceTicketStatus.Closed) ?? 0;

        return new RecommendationModelInput
        {
            Label = false,
            Role = request.Role,
            Service = request.Service,
            UsageLevel = Math.Clamp(request.UsageLevel, 1, 5),
            NeedsHighPerformance = request.NeedsHighPerformance ? 1f : 0f,
            NeedsGraphics = request.NeedsGraphics ? 1f : 0f,
            PreferredType = request.PreferredType ?? string.Empty,
            MaterielId = materiel.Id,
            MaterielType = materiel.Type,
            MaterielName = materiel.MaterielName,
            StockQuantity = materiel.QuantityITStock,
            HealthScore = (float)(materiel.CurrentHealthScore ?? 70m),
            OpenTicketCount = openTickets
        };
    }

    private static string BuildReason(Materiel materiel, HardwareRecommendationRequestDto request, decimal score)
    {
        var parts = new List<string>
        {
            $"ml-score={score:0.##}",
            $"stock={materiel.QuantityITStock}",
            $"health={(materiel.CurrentHealthScore ?? 70m):0.##}"
        };

        if (request.NeedsHighPerformance)
        {
            parts.Add("high-performance profile");
        }

        if (request.NeedsGraphics)
        {
            parts.Add("graphics workload");
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredType))
        {
            parts.Add($"preferred type={request.PreferredType}");
        }

        return string.Join(" | ", parts);
    }

    private static decimal ApplyRequestBias(decimal baseScore, Materiel materiel, HardwareRecommendationRequestDto request)
    {
        var score = baseScore;
        var text = $"{materiel.MaterielName} {materiel.Type}".ToLowerInvariant();

        if (request.NeedsHighPerformance && ContainsAny(text, HighPerformanceTokens))
        {
            score += 15m;
        }
        else if (request.NeedsHighPerformance)
        {
            score -= 20m;
        }

        if (request.NeedsGraphics && ContainsAny(text, GraphicsTokens))
        {
            score += 12m;
        }
        else if (request.NeedsGraphics)
        {
            score -= 15m;
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredType) && text.Contains(request.PreferredType.ToLowerInvariant()))
        {
            score += 10m;
        }

        return Math.Round(Math.Clamp(score, 0m, 100m), 2);
    }

    private static bool RoleSuggestsHighPerformance(string role, string service)
    {
        var roleL = role.ToLowerInvariant();
        var serviceL = service.ToLowerInvariant();
        return roleL.Contains("develop") || roleL.Contains("engineer") || roleL.Contains("it") || serviceL.Contains("it");
    }

    private static bool RoleSuggestsGraphics(string role, string service)
    {
        var roleL = role.ToLowerInvariant();
        var serviceL = service.ToLowerInvariant();
        return roleL.Contains("design") || serviceL.Contains("design") || serviceL.Contains("marketing");
    }

    private static bool ContainsAny(string text, IEnumerable<string> keywords)
        => keywords.Any(text.Contains);

    private sealed class RecommendationModelInput
    {
        public bool Label { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public float UsageLevel { get; set; }
        public float NeedsHighPerformance { get; set; }
        public float NeedsGraphics { get; set; }
        public string PreferredType { get; set; } = string.Empty;
        public int MaterielId { get; set; }
        public string MaterielType { get; set; } = string.Empty;
        public string MaterielName { get; set; } = string.Empty;
        public float StockQuantity { get; set; }
        public float HealthScore { get; set; }
        public float OpenTicketCount { get; set; }
    }

    private sealed class RecommendationScoredRow
    {
        public int MaterielId { get; set; }
        public float Probability { get; set; }
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
        public double BaselineUsageLevel { get; set; } = 3;
        public double BaselineNeedsHighPerformance { get; set; } = 0.5;
        public double BaselineNeedsGraphics { get; set; } = 0.5;
        public string LastRetrainStatus { get; set; } = "not_trained";
    }
}
