namespace ITStockM.Application.Features.AssetLifecycle.DTOs;

public sealed record TransitionRequestDto(string Stage, string? Notes);
