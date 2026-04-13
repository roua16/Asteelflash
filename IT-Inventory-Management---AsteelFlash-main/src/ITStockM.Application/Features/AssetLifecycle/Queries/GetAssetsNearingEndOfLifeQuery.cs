using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed record GetAssetsNearingEndOfLifeQuery(int WithinMonths) : IRequest<IEnumerable<Materiel>>;
