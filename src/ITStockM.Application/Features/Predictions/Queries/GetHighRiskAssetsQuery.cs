using ITStockM.Application.Features.Predictions.DTOs;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Queries;

public sealed record GetHighRiskAssetsQuery(decimal MaxScore) : IRequest<IEnumerable<AssetPredictionDto>>;
