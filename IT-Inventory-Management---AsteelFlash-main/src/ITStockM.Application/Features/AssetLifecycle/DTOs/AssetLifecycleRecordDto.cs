namespace ITStockM.Application.Features.AssetLifecycle.DTOs;

public sealed record AssetLifecycleRecordDto(
    int Id,
    int MaterielId,
    string Stage,
    DateTime StartDate,
    DateTime? EndDate,
    string? Notes);
