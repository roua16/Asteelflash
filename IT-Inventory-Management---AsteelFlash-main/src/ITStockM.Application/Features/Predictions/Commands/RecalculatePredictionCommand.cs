using ITStockM.Domain.Entities;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Commands;

public sealed record RecalculatePredictionCommand(int MaterielId) : IRequest<AssetPrediction>;
