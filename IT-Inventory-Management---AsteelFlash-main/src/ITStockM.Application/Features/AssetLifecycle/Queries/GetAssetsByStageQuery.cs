using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed record GetAssetsByStageQuery(string Stage) : IRequest<IEnumerable<Materiel>>;
