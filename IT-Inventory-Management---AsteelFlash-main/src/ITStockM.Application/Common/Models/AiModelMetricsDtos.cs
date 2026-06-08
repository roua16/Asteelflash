namespace ITStockM.Application.Common.Models;

public sealed record AiModelStatusDto(
    string ModelName,
    string? ActiveVersion,
    string? PreviousVersion,
    DateTime? LastTrainedUtc,
    int RetrainIntervalMinutes,
    int TrainingSampleCount,
    double ValidationMetric,
    double DriftScore,
    string DriftLevel,
    string LastRetrainStatus,
    string? ActiveModelPath);

public sealed record AiModelMetricsSnapshotDto(
    DateTime GeneratedAtUtc,
    IReadOnlyList<AiModelStatusDto> Models,
    double MaxDriftScore,
    string FleetDriftLevel);
