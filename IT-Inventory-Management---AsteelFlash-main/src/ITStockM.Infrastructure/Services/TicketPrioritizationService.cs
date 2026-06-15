using System.Text.RegularExpressions;
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

    private readonly ITStockManagmentContext? _context;
    private readonly AiModelRetrainingOptions _options;
    private readonly MLContext _mlContext = new(17);
    private readonly SemaphoreSlim _trainLock = new(1, 1);

    private readonly TicketPrioritizationArtifactManager _artifactManager;

    private ITransformer? _model;
    private DateTime _lastTrainingUtc = DateTime.MinValue;

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
        ITStockManagmentContext? context,
        TicketPrioritizationArtifactManager artifactManager,
        IOptions<AiModelRetrainingOptions>? options = null)
    {
        _context = context;
        _artifactManager = artifactManager;
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

        var matchedKeywords = TicketPriorityRulePolicy.KeywordWeightsPublic.Keys
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
                ? TicketPriorityRulePolicy.DetermineFallbackPriority(matchedKeywords, request)
                : prediction.PredictedLabel;
        }
        else
        {
            priority = TicketPriorityRulePolicy.DetermineFallbackPriority(matchedKeywords, request);
        }

        var keywordScore = TicketPriorityRulePolicy.ComputeKeywordScore(matchedKeywords);
        score = ComputeSeverityScore(priority, request, keywordScore);

        var explanation = $"priority={priority}, severity={score:0.##}, keywords={keywordScore:0.##}, matched={matchedKeywords.Count}";

        return new TicketPriorityPredictionDto(
            priority,
            score,
            matchedKeywords,
            explanation);
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
                // Atomic activation: persist candidate first, then swap in memory.
                var candidateToActivate = candidateModel;

                _validationMetric = candidateMetric;
                _trainingSampleCount = trainingRows.Count;
                _lastRetrainStatus = "activated";

                UpdateBaseline(trainingRows);
                SaveVersionedModelToDisk(candidateToActivate, split.TrainSet.Schema, trainingRows, candidateMetric);

                _model = candidateToActivate;
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
        var expectedConfigHash = ComputeConfigHash();

        if (_artifactManager.TryLoadActiveModel(expectedConfigHash, out var model, out var manifest) && model is not null)
        {
            _model = model;
            _activeVersion = manifest?.ActiveVersion;
            _previousVersion = manifest?.PreviousVersion;
            _activeModelPath = manifest?.ActiveModelPath;

            _lastTrainingUtc = manifest?.LastTrainedUtc ?? DateTime.MinValue;
            _trainingSampleCount = manifest?.TrainingSampleCount ?? 0;
            _validationMetric = manifest?.ValidationMetric ?? 0;
            _lastRetrainStatus = manifest?.LastRetrainStatus ?? "not_trained";

            _baselineUrgency = manifest?.BaselineUrgency ?? _baselineUrgency;
            _baselineImpactedUsers = manifest?.BaselineImpactedUsers ?? _baselineImpactedUsers;
            _baselineEquipmentCriticality = manifest?.BaselineEquipmentCriticality ?? _baselineEquipmentCriticality;
            return;
        }

        if (_artifactManager.TryLoadPreviousModel(expectedConfigHash, out model, out manifest) && model is not null)
        {
            _model = model;
            _activeVersion = manifest?.PreviousVersion;
            _previousVersion = null;
            _activeModelPath = manifest?.PreviousModelPath;

            _lastTrainingUtc = manifest?.LastTrainedUtc ?? DateTime.MinValue;
            _trainingSampleCount = manifest?.TrainingSampleCount ?? 0;
            _validationMetric = manifest?.ValidationMetric ?? 0;
            _lastRetrainStatus = "rolled_over_to_previous";

            _baselineUrgency = manifest?.BaselineUrgency ?? _baselineUrgency;
            _baselineImpactedUsers = manifest?.BaselineImpactedUsers ?? _baselineImpactedUsers;
            _baselineEquipmentCriticality = manifest?.BaselineEquipmentCriticality ?? _baselineEquipmentCriticality;
        }
    }

    private void SaveVersionedModelToDisk(
        ITransformer model,
        DataViewSchema schema,
        IReadOnlyCollection<TicketPriorityModelInput> trainingRows,
        double validationMetric)
    {
        var datasetSnapshotId = ComputeDatasetSnapshotId(trainingRows);
        var trainingCommit = ResolveTrainingCommit();
        var configHash = ComputeConfigHash();

        _artifactManager.SaveVersionedModel(
            model,
            schema,
            trainingSampleCount: trainingRows.Count,
            validationMetric: validationMetric,
            lastRetrainStatus: _lastRetrainStatus,
            driftScore: _driftScore,
            datasetSnapshotId: datasetSnapshotId,
            trainingCommit: trainingCommit,
            configHash: configHash,
            baselineUrgency: _baselineUrgency,
            baselineImpactedUsers: _baselineImpactedUsers,
            baselineEquipmentCriticality: _baselineEquipmentCriticality);

        // Refresh pointers from persisted manifest.
        if (_artifactManager.TryLoadActiveModel(configHash, out _, out var manifest) && manifest is not null)
        {
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
                Label = TicketPriorityRulePolicy.InferLabelFromText(ticket.ProblemDescription)
            });
        }

        return rows;
    }

    private static List<TicketPriorityModelInput> BuildSeedData()
    {
        // Goal: richer, balanced seed data (better precision early, before enough historical tickets exist).
        // Labels are inferred from the exact same rule policy used at runtime.
        var rand = new Random(1337);

        var templates = new List<(string LabelHint, string Category, string EquipmentType, int BaseUrgency, int BaseImpactedUsers, int BaseCriticality, string[] Variants)>
        {
            ("Critique", "Infrastructure", "Server", 5, 250, 5, ["production server down", "erp inaccessible", "database crash", "server outage"]),
            ("Critique", "Security", "Firewall", 5, 120, 5, ["security breach suspected", "breach detected", "credential compromise", "security incident"]),
            ("Critique", "Infrastructure", "Cooling", 5, 70, 5, ["cooling failure", "data center cooling down", "overheating risk", "thermal shutdown"]),
            ("Haute", "Hardware", "Desktop", 4, 4, 4, ["pc does not start", "desktop won't boot", "system crash", "startup failure"]),
            ("Haute", "Network", "Switch", 4, 25, 4, ["network unstable", "packet loss", "switch connectivity issues", "intermittent disconnect"]),
            ("Haute", "Hardware", "Laptop", 4, 2, 3, ["laptop crash", "random freeze", "battery failing", "device restarts"]),
            ("Moyenne", "Application", "Workstation", 3, 8, 3, ["application slow", "lag and errors", "performance degradation", "timeouts for users"]),
            ("Moyenne", "Hardware", "Monitor", 3, 1, 2, ["screen flickering", "display artifacts", "monitor instability", "black screen"]),
            ("Moyenne", "Network", "Router", 3, 6, 3, ["intermittent vpn issue", "vpn unstable", "router flapping", "cannot connect sometimes"]),
            ("Faible", "Peripheral", "Printer", 1, 1, 1, ["printer slow", "printing delays", "paper feed issue", "spool lag"]),
            ("Faible", "Peripheral", "Mouse", 1, 1, 1, ["mouse not comfortable", "tracking issues", "cursor jumps", "input device glitch"]),
            ("Faible", "Peripheral", "Keyboard", 1, 1, 1, ["keyboard key stuck", "keys not responding", "keyboard malfunction", "typing errors"]),
            ("Faible", "Peripheral", "Scanner", 2, 2, 2, ["scanner needs maintenance", "scans fail", "calibration required", "low quality scans"]),
        };

        var criticalityKeywords = new Dictionary<string, string[]>
        {
            ["Critique"] = ["down", "inaccessible", "blocked", "cannot", "crash", "failed", "breach", "urgent", "production"],
            ["Haute"] = ["critical", "urgent", "crash", "failed", "cannot", "unstable", "intermittent"],
            ["Moyenne"] = ["slow", "error", "intermittent", "unstable", "timeouts"],
            ["Faible"] = ["printer", "mouse", "keyboard", "scan", "maintenance", "occasionally", "failing"],
        };

        var extraKeywordsByCategory = new Dictionary<string, string[]>
        {
            ["Infrastructure"] = ["all users blocked", "erp", "database", "server room"],
            ["Security"] = ["breach", "security", "credential", "compromised"],
            ["Network"] = ["department", "accounting", "vpn", "switch", "router"],
            ["Hardware"] = ["manager", "designer", "laptop", "desktop", "workstation"],
            ["Application"] = ["users", "timeouts", "lag", "errors"],
            ["Peripheral"] = ["office", "desk", "printing", "typing", "scanning"],
        };

        var seeds = new List<TicketPriorityModelInput>(capacity: 500);

        // Generate multiple variants per template to cover keyword combinations.
        foreach (var tpl in templates)
        {
            foreach (var baseVariant in tpl.Variants)
            {
                var baseTextParts = new List<string> { baseVariant };

                // Add 1-3 keywords to make text less uniform.
                var keywordPool = criticalityKeywords[tpl.LabelHint].Concat(extraKeywordsByCategory[tpl.Category]).ToArray();
                var chosen = keywordPool.OrderBy(_ => rand.Next()).Distinct().Take(3).ToArray();

                // Add explicit operational words to ensure rule coverage.
                var operational = tpl.LabelHint switch
                {
                    "Critique" => new[] { "production", "down", "cannot" },
                    "Haute" => new[] { "urgent", "critical" },
                    "Moyenne" => new[] { "slow", "intermittent" },
                    _ => new[] { "occasionally", "maintenance" }
                };

                var allTokens = chosen.Concat(operational).Distinct().ToArray();

                baseTextParts.Add(string.Join(" ", allTokens));

                var text = string.Join(" ", baseTextParts).Trim();

                var label = TicketPriorityRulePolicy.InferLabelFromText(text);

                // Correlate numeric features with the inferred severity to help early model quality.
                int urgency = tpl.BaseUrgency + rand.Next(-1, 2);
                int impacted = tpl.BaseImpactedUsers + rand.Next(-Math.Max(1, tpl.BaseImpactedUsers / 10), Math.Max(2, tpl.BaseImpactedUsers / 8));
                int crit = tpl.BaseCriticality + rand.Next(-1, 2);

                urgency = (int)Math.Clamp(urgency, 1, 5);
                impacted = (int)Math.Clamp(impacted, 0, 10_000);
                crit = (int)Math.Clamp(crit, 1, 5);

                // Ensure label agrees with rule policy, and skip if inference produced unexpected label (safety).
                if (!string.Equals(label, tpl.LabelHint, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                seeds.Add(new TicketPriorityModelInput
                {
                    Text = text,
                    Category = tpl.Category,
                    EquipmentType = tpl.EquipmentType,
                    UrgencyLevel = urgency,
                    ImpactedUsers = impacted,
                    EquipmentCriticality = crit,
                    Label = label
                });

                // Add small controlled variations by appending synonyms.
                var synonymAppend = tpl.LabelHint switch
                {
                    "Critique" => new[] { "inaccessible", "blocked", "all users" },
                    "Haute" => new[] { "failed", "crash", "unstable" },
                    "Moyenne" => new[] { "timeouts", "slow", "intermittent" },
                    _ => new[] { "maintenance", "occasionally", "low impact" }
                };

                foreach (var syn in synonymAppend)
                {
                    if (seeds.Count >= 600) break;

                    var text2 = $"{baseVariant} {syn} {string.Join(" ", chosen.Take(2))}".Trim();
                    var label2 = TicketPriorityRulePolicy.InferLabelFromText(text2);

                    if (!string.Equals(label2, tpl.LabelHint, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    seeds.Add(new TicketPriorityModelInput
                    {
                        Text = text2,
                        Category = tpl.Category,
                        EquipmentType = tpl.EquipmentType,
                        UrgencyLevel = Math.Clamp(urgency + rand.Next(-1, 2), 1, 5),
                        ImpactedUsers = Math.Clamp(impacted + rand.Next(-5, 6), 0, 10_000),
                        EquipmentCriticality = Math.Clamp(crit + rand.Next(-1, 2), 1, 5),
                        Label = label2
                    });
                }

                if (seeds.Count >= 600) break;
            }

            if (seeds.Count >= 600) break;
        }

        // Final cap to keep training fast.
        // Also ensure at least a minimal balanced size exists even if skipping reduced count.
        if (seeds.Count < 250)
        {
            // Fallback to the original tiny set (safety).
            seeds.AddRange(new[]
            {
                new TicketPriorityModelInput { Text = "production server down inaccessible all users blocked", Category = "Infrastructure", EquipmentType = "Server", UrgencyLevel = 5, ImpactedUsers = 300, EquipmentCriticality = 5, Label = "Critique" },
                new TicketPriorityModelInput { Text = "database crash cannot connect erp", Category = "Infrastructure", EquipmentType = "Server", UrgencyLevel = 5, ImpactedUsers = 220, EquipmentCriticality = 5, Label = "Critique" },
                new TicketPriorityModelInput { Text = "security breach suspected urgent", Category = "Security", EquipmentType = "Firewall", UrgencyLevel = 5, ImpactedUsers = 120, EquipmentCriticality = 5, Label = "Critique" },
                new TicketPriorityModelInput { Text = "pc does not start for manager", Category = "Hardware", EquipmentType = "Desktop", UrgencyLevel = 4, ImpactedUsers = 3, EquipmentCriticality = 4, Label = "Haute" },
                new TicketPriorityModelInput { Text = "printer slow in office", Category = "Peripheral", EquipmentType = "Printer", UrgencyLevel = 1, ImpactedUsers = 1, EquipmentCriticality = 1, Label = "Faible" },
                new TicketPriorityModelInput { Text = "application is slow for a few users", Category = "Application", EquipmentType = "Workstation", UrgencyLevel = 3, ImpactedUsers = 8, EquipmentCriticality = 3, Label = "Moyenne" },
            });
        }

        // Ensure deterministic order for snapshot hash stability.
        return seeds
            .OrderBy(x => x.Label, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.EquipmentType, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Text, StringComparer.OrdinalIgnoreCase)
            .Take(600)
            .ToList();
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
        => Regex.Matches(text.ToLowerInvariant(), "[a-z0-9]+")
            .Select(m => m.Value)
            .ToList();

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
}
