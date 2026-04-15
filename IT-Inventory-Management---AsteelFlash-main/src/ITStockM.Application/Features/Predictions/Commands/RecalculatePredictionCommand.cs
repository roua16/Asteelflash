using ITStockM.Application.Features.Predictions.DTOs;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Commands;

public sealed record RecalculatePredictionCommand(int MaterielId) : IRequest<AssetPredictionDto>;
