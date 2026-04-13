using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.AssetLifecycle.Commands;

public sealed record TransitionAssetStageCommand(int MaterielId, string Stage, string? Notes) : IRequest<AssetLifecycleRecord>;
