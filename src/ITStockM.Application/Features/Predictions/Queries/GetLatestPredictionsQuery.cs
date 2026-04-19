using ITStockM.Application.Features.Predictions.DTOs;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Queries;

public sealed record GetLatestPredictionsQuery : IRequest<IEnumerable<AssetPredictionDto>>;
