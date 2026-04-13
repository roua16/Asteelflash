using ITStockM.Domain.Entities;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Commands;

public sealed record RecalculateAllPredictionsCommand : IRequest<IEnumerable<AssetPrediction>>;
