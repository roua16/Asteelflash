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

public sealed record AiReadinessModelGateDto(
    string ModelName,
    string LastRetrainStatus,
    double ValidationMetric,
    double MinimumRequiredMetric,
    bool MeetsMinimumMetric,
    bool RetrainBlockedByQualityGate,
    bool RetrainBlockedByAcceptanceGate,
    bool IsOperational);

public sealed record AiReadinessSnapshotDto(
    DateTime GeneratedAtUtc,
    bool ReadyForProduction,
    IReadOnlyList<AiReadinessModelGateDto> Models,
    string Summary);
