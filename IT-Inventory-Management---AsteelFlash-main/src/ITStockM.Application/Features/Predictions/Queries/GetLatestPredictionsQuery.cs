using ITStockM.Domain.Entities;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Queries;

public sealed record GetLatestPredictionsQuery : IRequest<IEnumerable<AssetPrediction>>;
