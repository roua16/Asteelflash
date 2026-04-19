using AutoMapper;
using ITStockM.Application.Features.Predictions.Commands;
using ITStockM.Application.Features.Predictions.DTOs;
using ITStockM.Application.Features.Predictions.Queries;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.Predictions.Handlers;

public sealed class GetLatestPredictionsHandler : IRequestHandler<GetLatestPredictionsQuery, IEnumerable<AssetPredictionDto>>
{
    private readonly IPredictionService _service;
    private readonly IMapper _mapper;

    public GetLatestPredictionsHandler(IPredictionService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AssetPredictionDto>> Handle(GetLatestPredictionsQuery request, CancellationToken cancellationToken)
    {
        var predictions = await _service.GetLatestPredictionsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<AssetPredictionDto>>(predictions);
    }
}

public sealed class GetHighRiskAssetsHandler : IRequestHandler<GetHighRiskAssetsQuery, IEnumerable<AssetPredictionDto>>
{
    private readonly IPredictionService _service;
    private readonly IMapper _mapper;

    public GetHighRiskAssetsHandler(IPredictionService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AssetPredictionDto>> Handle(GetHighRiskAssetsQuery request, CancellationToken cancellationToken)
    {
        var predictions = await _service.GetHighRiskAssetsAsync(request.MaxScore, cancellationToken);
        return _mapper.Map<IEnumerable<AssetPredictionDto>>(predictions);
    }
}

public sealed class RecalculatePredictionHandler : IRequestHandler<RecalculatePredictionCommand, AssetPredictionDto>
{
    private readonly IPredictionService _service;
    private readonly IMapper _mapper;

    public RecalculatePredictionHandler(IPredictionService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<AssetPredictionDto> Handle(RecalculatePredictionCommand request, CancellationToken cancellationToken)
    {
        var prediction = await _service.CalculateAndSavePredictionAsync(request.MaterielId, cancellationToken);
        return _mapper.Map<AssetPredictionDto>(prediction);
    }
}

public sealed class RecalculateAllPredictionsHandler : IRequestHandler<RecalculateAllPredictionsCommand, IEnumerable<AssetPredictionDto>>
{
    private readonly IPredictionService _service;
    private readonly IMapper _mapper;

    public RecalculateAllPredictionsHandler(IPredictionService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AssetPredictionDto>> Handle(RecalculateAllPredictionsCommand request, CancellationToken cancellationToken)
    {
        var predictions = await _service.RecalculateAllPredictionsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<AssetPredictionDto>>(predictions);
    }
}
