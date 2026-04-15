using ITStockM.Application.Features.Common.DTOs;
using MediatR;

namespace ITStockM.Application.Features.AssetLifecycle.Queries;

public sealed record GetAssetsNearingEndOfLifeQuery(int WithinMonths) : IRequest<IEnumerable<MaterielDto>>;
