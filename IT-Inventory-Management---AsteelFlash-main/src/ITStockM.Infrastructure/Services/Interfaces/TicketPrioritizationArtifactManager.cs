using Microsoft.ML;
using Microsoft.ML.Data;

namespace ITStockM.Services;

/// <summary>
/// Dedicated artifact manager for ticket-prioritization model.
/// </summary>
public sealed class TicketPrioritizationArtifactManager
{
    private readonly AiModelArtifactManager _inner;

    public TicketPrioritizationArtifactManager(AiModelArtifactManager inner)
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
            baselineUrgency: baselineUrgency,
            baselineImpactedUsers: baselineImpactedUsers,
            baselineEquipmentCriticality: baselineEquipmentCriticality,
            baselineUsageLevel: baselineUsageLevel,
            baselineNeedsHighPerformance: baselineNeedsHighPerformance,
            baselineNeedsGraphics: baselineNeedsGraphics);
    }
}
