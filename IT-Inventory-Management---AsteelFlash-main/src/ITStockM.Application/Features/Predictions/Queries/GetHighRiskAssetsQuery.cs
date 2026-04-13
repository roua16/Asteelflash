using ITStockM.Domain.Entities;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Queries;

public sealed record GetHighRiskAssetsQuery(decimal MaxScore) : IRequest<IEnumerable<AssetPrediction>>;
