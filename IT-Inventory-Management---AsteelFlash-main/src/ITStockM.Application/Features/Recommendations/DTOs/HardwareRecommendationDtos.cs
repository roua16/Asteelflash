namespace ITStockM.Application.Features.Recommendations.DTOs;

public sealed record HardwareRecommendationRequestDto(
    string Role,
    string Service,
    int UsageLevel,
    bool NeedsHighPerformance,
    bool NeedsGraphics,
    string? PreferredType,
    int TopN = 3);

public sealed record HardwareRecommendationDto(
    int MaterielId,
    string MaterielName,
    string MaterielType,
    decimal MatchScore,
    int AvailableQuantity,
    string Reason);
