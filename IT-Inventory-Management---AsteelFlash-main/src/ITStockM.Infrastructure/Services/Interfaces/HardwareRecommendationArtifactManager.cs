using Microsoft.ML;
using Microsoft.ML.Data;

namespace ITStockM.Services;

/// <summary>
/// Dedicated artifact manager for hardware-recommendation model.
/// </summary>
public sealed class HardwareRecommendationArtifactManager
{
    private readonly AiModelArtifactManager _inner;

    public HardwareRecommendationArtifactManager(AiModelArtifactManager inner)
    {
        _inner = inner;
    }

    public bool TryLoadActiveModel(string? expectedConfigHash, out ITransformer? model, out AiModelArtifactManager.ModelManifest? manifest)
        => _inner.TryLoadActiveModel(expectedConfigHash, out model, out manifest);

    public bool TryLoadPreviousModel(string? expectedConfigHash, out ITransformer? model, out AiModelArtifactManager.ModelManifest? manifest)
        => _inner.TryLoadPreviousModel(expectedConfigHash, out model, out manifest);

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
        _inner.SaveVersionedModel(
            model,
            schema,
            trainingSampleCount,
            validationMetric,
            lastRetrainStatus,
            driftScore,
            datasetSnapshotId,
            trainingCommit,
            configHash,
            baselineUsageLevel: baselineUsageLevel,
            baselineNeedsHighPerformance: baselineNeedsHighPerformance,
            baselineNeedsGraphics: baselineNeedsGraphics);
    }
}
