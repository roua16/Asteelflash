using AutoMapper;
using ITStockM.Application.Features.AssetLifecycle.Commands;
using ITStockM.Application.Features.AssetLifecycle.DTOs;
using ITStockM.Application.Features.AssetLifecycle.Queries;
using ITStockM.Application.Features.Common.DTOs;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.AssetLifecycle.Handlers;

public sealed class GetLifecycleHistoryHandler : IRequestHandler<GetLifecycleHistoryQuery, IEnumerable<AssetLifecycleRecordDto>>
{
    private readonly IAssetLifecycleService _service;
    private readonly IMapper _mapper;

    public GetLifecycleHistoryHandler(IAssetLifecycleService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AssetLifecycleRecordDto>> Handle(GetLifecycleHistoryQuery request, CancellationToken cancellationToken)
    {
        var records = await _service.GetLifecycleHistoryAsync(request.MaterielId, cancellationToken);
        return _mapper.Map<IEnumerable<AssetLifecycleRecordDto>>(records);
    }
}

public sealed class TransitionAssetStageHandler : IRequestHandler<TransitionAssetStageCommand, AssetLifecycleRecordDto>
{
    private readonly IAssetLifecycleService _service;
    private readonly IMapper _mapper;

    public TransitionAssetStageHandler(IAssetLifecycleService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<AssetLifecycleRecordDto> Handle(TransitionAssetStageCommand request, CancellationToken cancellationToken)
    {
        var record = await _service.TransitionStageAsync(request.MaterielId, request.Stage, request.Notes, cancellationToken);
        return _mapper.Map<AssetLifecycleRecordDto>(record);
    }
}

public sealed class GetAssetsByStageHandler : IRequestHandler<GetAssetsByStageQuery, IEnumerable<MaterielDto>>
{
    private readonly IAssetLifecycleService _service;
    private readonly IMapper _mapper;

    public GetAssetsByStageHandler(IAssetLifecycleService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MaterielDto>> Handle(GetAssetsByStageQuery request, CancellationToken cancellationToken)
    {
        var assets = await _service.GetAssetsByStageAsync(request.Stage, cancellationToken);
        return _mapper.Map<IEnumerable<MaterielDto>>(assets);
    }
}

public sealed class GetAssetsNearingEndOfLifeHandler : IRequestHandler<GetAssetsNearingEndOfLifeQuery, IEnumerable<MaterielDto>>
{
    private readonly IAssetLifecycleService _service;
    private readonly IMapper _mapper;

    public GetAssetsNearingEndOfLifeHandler(IAssetLifecycleService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MaterielDto>> Handle(GetAssetsNearingEndOfLifeQuery request, CancellationToken cancellationToken)
    {
        var assets = await _service.GetAssetsNearingEndOfLifeAsync(request.WithinMonths, cancellationToken);
        return _mapper.Map<IEnumerable<MaterielDto>>(assets);
    }
}
