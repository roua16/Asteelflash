using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed record GetLifecycleHistoryQuery(int MaterielId) : IRequest<IEnumerable<AssetLifecycleRecord>>;
