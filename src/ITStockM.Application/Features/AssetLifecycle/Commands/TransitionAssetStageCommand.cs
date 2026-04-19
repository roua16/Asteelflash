using ITStockM.Application.Features.AssetLifecycle.DTOs;
using MediatR;

namespace ITStockM.Application.Features.AssetLifecycle.Commands;

public sealed record TransitionAssetStageCommand(int MaterielId, string Stage, string? Notes) : IRequest<AssetLifecycleRecordDto>;
