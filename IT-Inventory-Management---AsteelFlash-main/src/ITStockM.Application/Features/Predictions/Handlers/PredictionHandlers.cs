using ITStockM.Application.Features.Predictions.Commands;
using ITStockM.Application.Features.Predictions.Queries;
using ITStockM.Domain.Entities;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Handlers;

public sealed class GetLatestPredictionsHandler : IRequestHandler<GetLatestPredictionsQuery, IEnumerable<AssetPrediction>>
{
    private readonly IPredictionService _service;

    public GetLatestPredictionsHandler(IPredictionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<AssetPrediction>> Handle(GetLatestPredictionsQuery request, CancellationToken cancellationToken)
        => await _service.GetLatestPredictionsAsync(cancellationToken);
}

public sealed class GetHighRiskAssetsHandler : IRequestHandler<GetHighRiskAssetsQuery, IEnumerable<AssetPrediction>>
{
    private readonly IPredictionService _service;

    public GetHighRiskAssetsHandler(IPredictionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<AssetPrediction>> Handle(GetHighRiskAssetsQuery request, CancellationToken cancellationToken)
        => await _service.GetHighRiskAssetsAsync(request.MaxScore, cancellationToken);
}

public sealed class RecalculatePredictionHandler : IRequestHandler<RecalculatePredictionCommand, AssetPrediction>
{
    private readonly IPredictionService _service;

    public RecalculatePredictionHandler(IPredictionService service)
    {
        _service = service;
    }

    public async Task<AssetPrediction> Handle(RecalculatePredictionCommand request, CancellationToken cancellationToken)
        => await _service.CalculateAndSavePredictionAsync(request.MaterielId, cancellationToken);
}

public sealed class RecalculateAllPredictionsHandler : IRequestHandler<RecalculateAllPredictionsCommand, IEnumerable<AssetPrediction>>
{
    private readonly IPredictionService _service;

    public RecalculateAllPredictionsHandler(IPredictionService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<AssetPrediction>> Handle(RecalculateAllPredictionsCommand request, CancellationToken cancellationToken)
        => await _service.RecalculateAllPredictionsAsync(cancellationToken);
}
