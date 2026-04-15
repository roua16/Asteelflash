using ITStockM.Application.Features.AssetLifecycle.DTOs;
using MediatR;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed record GetLifecycleHistoryQuery(int MaterielId) : IRequest<IEnumerable<AssetLifecycleRecordDto>>;
