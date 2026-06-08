namespace ITStockM.Application.Features.Maintenance.DTOs;

public sealed record TicketPriorityRequestDto(
    string Title,
    string Description,
    string? Category,
    int UrgencyLevel,
    int ImpactedUsers,
    int EquipmentCriticality,
    string? EquipmentType);

public sealed record TicketPriorityPredictionDto(
    string Priority,
    decimal PriorityScore,
    IReadOnlyList<string> MatchedKeywords,
    string Explanation);
