using ITStockM.Application.Features.Predictions.DTOs;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Commands;

public sealed record RecalculateAllPredictionsCommand : IRequest<IEnumerable<AssetPredictionDto>>;
