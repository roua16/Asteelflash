namespace ITStockM.Application.Features.Predictions.DTOs;

public sealed record AssetPredictionDto(
    int Id,
    int MaterielId,
    DateTime CalculatedAt,
    decimal HealthScore,
    string HealthStatus,
    DateTime? PredictedFailureDate,
    DateTime? RecommendedReplacementDate,
    string? RecommendationReason,
    decimal? EstimatedReplacementCost,
    string? MaterielName,
    string? MaterielType,
    string? MaterielSerialNumber);
