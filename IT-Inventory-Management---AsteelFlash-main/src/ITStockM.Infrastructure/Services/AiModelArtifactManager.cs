using Microsoft.ML;
using Microsoft.ML.Data;
using System.Text.Json;

namespace ITStockM.Services;


/// <summary>
/// Shared helper for ML.NET model persistence.
/// Manages:
/// - versioned zip artifacts
/// - manifest with active/previous pointers
/// - retention policy
/// - basic config hash guard (optional; caller decides whether to load)
/// </summary>
public sealed class AiModelArtifactManager
{
    private readonly MLContext _mlContext;

    private readonly string _scopedModelDirectory;
    private readonly string _manifestPath;
    private readonly int _maxVersionsToKeep;

    public AiModelArtifactManager(
        MLContext mlContext,
        string modelRootDirectory,
        string scopedModelDirectoryName,
        int maxVersionsToKeep)
    {
        _mlContext = mlContext ?? throw new ArgumentNullException(nameof(mlContext));
        if (string.IsNullOrWhiteSpace(modelRootDirectory)) throw new ArgumentException("Model root directory is required.", nameof(modelRootDirectory));
        if (string.IsNullOrWhiteSpace(scopedModelDirectoryName)) throw new ArgumentException("Scoped model directory name is required.", nameof(scopedModelDirectoryName));
        if (maxVersionsToKeep < 2) throw new ArgumentOutOfRangeException(nameof(maxVersionsToKeep), "Keep at least 2 versions (active + previous)." );

        _scopedModelDirectory = Path.Combine(modelRootDirectory, scopedModelDirectoryName);
        _manifestPath = Path.Combine(_scopedModelDirectory, "manifest.json");
        _maxVersionsToKeep = maxVersionsToKeep;
    }

    public ModelManifest ReadManifestOrNull()
    {
        if (!File.Exists(_manifestPath)) return null;
        try
        {
            return JsonSerializer.Deserialize<ModelManifest>(File.ReadAllText(_manifestPath));
        }
        catch
        {
            return null;
        }
    }

    public bool TryLoadActiveModel(string? expectedConfigHash, out ITransformer? model, out ModelManifest? manifest)
    {
        model = null;
        manifest = ReadManifestOrNull();

        if (manifest?.ActiveModelPath is null)
            return false;

        if (!File.Exists(manifest.ActiveModelPath))
            return false;

        if (!string.IsNullOrWhiteSpace(expectedConfigHash) && !string.Equals(manifest.ConfigHash, expectedConfigHash, StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            model = _mlContext.Model.Load(manifest.ActiveModelPath, out _);
            return model != null;
        }
        catch
        {
            model = null;
            return false;
        }
    }

    public bool TryLoadPreviousModel(string? expectedConfigHash, out ITransformer? model, out ModelManifest? manifest)
    {
        model = null;
        manifest = ReadManifestOrNull();

        if (manifest?.PreviousModelPath is null)
            return false;

        if (!File.Exists(manifest.PreviousModelPath))
            return false;

        if (!string.IsNullOrWhiteSpace(expectedConfigHash) && !string.Equals(manifest.ConfigHash, expectedConfigHash, StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            model = _mlContext.Model.Load(manifest.PreviousModelPath, out _);
            return model != null;
        }
        catch
        {
            model = null;
            return false;
        }
    }

    public void SaveVersionedModel(
        ITransformer model,
        DataViewSchema schema,
        int trainingSampleCount,
        double validationMetric,
        string lastRetrainStatus,
        double driftScore,
        string datasetSnapshotId,
        string trainingCommit,
        string configHash,
        double? baselineUsageLevel = null,
        double? baselineNeedsHighPerformance = null,
        double? baselineNeedsGraphics = null,
        double? baselineUrgency = null,
        double? baselineImpactedUsers = null,
        double? baselineEquipmentCriticality = null)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        if (schema is null) throw new ArgumentNullException(nameof(schema));
        if (string.IsNullOrWhiteSpace(lastRetrainStatus)) throw new ArgumentException("Last retrain status is required.", nameof(lastRetrainStatus));

        Directory.CreateDirectory(_scopedModelDirectory);

        var version = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var filePath = Path.Combine(_scopedModelDirectory, $"model_{version}.zip");

        using (var stream = File.Create(filePath))
        {
            _mlContext.Model.Save(model, schema, stream);
        }

        var manifest = ReadManifestOrNull() ?? new ModelManifest();

        manifest.PreviousVersion = manifest.ActiveVersion;
        manifest.PreviousModelPath = manifest.ActiveModelPath;

        manifest.ActiveVersion = version;
        manifest.ActiveModelPath = filePath;
        manifest.LastTrainedUtc = DateTime.UtcNow;

        manifest.TrainingSampleCount = trainingSampleCount;
        manifest.ValidationMetric = validationMetric;
        manifest.DriftScore = driftScore;
        manifest.LastRetrainStatus = lastRetrainStatus;
        manifest.DatasetSnapshotId = datasetSnapshotId;
        manifest.TrainingCommit = trainingCommit;
        manifest.ConfigHash = configHash;

        // Baselines - not all services use all fields.
        manifest.BaselineUsageLevel = baselineUsageLevel ?? manifest.BaselineUsageLevel;
        manifest.BaselineNeedsHighPerformance = baselineNeedsHighPerformance ?? manifest.BaselineNeedsHighPerformance;
        manifest.BaselineNeedsGraphics = baselineNeedsGraphics ?? manifest.BaselineNeedsGraphics;

        manifest.BaselineUrgency = baselineUrgency ?? manifest.BaselineUrgency;
        manifest.BaselineImpactedUsers = baselineImpactedUsers ?? manifest.BaselineImpactedUsers;
        manifest.BaselineEquipmentCriticality = baselineEquipmentCriticality ?? manifest.BaselineEquipmentCriticality;

        var serializedManifest = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });

        Directory.CreateDirectory(_scopedModelDirectory);
        var manifestTempPath = _manifestPath + $".tmp.{Guid.NewGuid():N}";

        File.WriteAllText(manifestTempPath, serializedManifest);

        try
        {
            if (File.Exists(_manifestPath))
                File.Replace(manifestTempPath, _manifestPath, destinationBackupFileName: null);
            else
                File.Move(manifestTempPath, _manifestPath);
        }
        catch
        {
            // Best-effort atomicity: fall back to overwrite
            try { File.Delete(_manifestPath); } catch { }
            File.Move(manifestTempPath, _manifestPath);
        }

        CleanupOldModelArtifacts(manifest);
    }

    private void CleanupOldModelArtifacts(ModelManifest manifest)
    {
        var maxVersions = Math.Max(2, _maxVersionsToKeep);

        if (!Directory.Exists(_scopedModelDirectory)) return;

        var modelFiles = Directory
            .GetFiles(_scopedModelDirectory, "model_*.zip", SearchOption.TopDirectoryOnly)
            .OrderByDescending(Path.GetFileName)
            .ToList();

        if (modelFiles.Count <= maxVersions)
            return;

        var keep = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            manifest.ActiveModelPath ?? string.Empty,
            manifest.PreviousModelPath ?? string.Empty
        };

        foreach (var file in modelFiles.Take(maxVersions))
            keep.Add(file);

        foreach (var file in modelFiles)
        {
            if (keep.Contains(file)) continue;

            try { File.Delete(file); }
            catch { /* best-effort cleanup */ }
        }
    }

    public sealed class ModelManifest
    {
        public string? ActiveVersion { get; set; }
        public string? PreviousVersion { get; set; }

        public string? ActiveModelPath { get; set; }
        public string? PreviousModelPath { get; set; }

        public DateTime LastTrainedUtc { get; set; }
        public int TrainingSampleCount { get; set; }
        public double ValidationMetric { get; set; }
        public double DriftScore { get; set; }

        public string LastRetrainStatus { get; set; } = "not_trained";
        public string DatasetSnapshotId { get; set; } = string.Empty;
        public string TrainingCommit { get; set; } = "unknown";
        public string ConfigHash { get; set; } = string.Empty;

        // Hardware baselines
        public double BaselineUsageLevel { get; set; } = 3;
        public double BaselineNeedsHighPerformance { get; set; } = 0.5;
        public double BaselineNeedsGraphics { get; set; } = 0.5;

        // Ticket baselines
        public double BaselineUrgency { get; set; } = 3;
        public double BaselineImpactedUsers { get; set; } = 10;
        public double BaselineEquipmentCriticality { get; set; } = 3;
    }
}

