using ITStockM.Application.Features.AssetLifecycle.Commands;
using ITStockM.Application.Features.AssetLifecycle.Queries;
using ITStockM.Domain.Entities;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.AssetLifecycle.Handlers;

public sealed class GetLifecycleHistoryHandler : IRequestHandler<GetLifecycleHistoryQuery, IEnumerable<AssetLifecycleRecord>>
{
    private readonly IAssetLifecycleService _service;

    public GetLifecycleHistoryHandler(IAssetLifecycleService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<AssetLifecycleRecord>> Handle(GetLifecycleHistoryQuery request, CancellationToken cancellationToken)
        => await _service.GetLifecycleHistoryAsync(request.MaterielId, cancellationToken);
}

public sealed class TransitionAssetStageHandler : IRequestHandler<TransitionAssetStageCommand, AssetLifecycleRecord>
{
    private readonly IAssetLifecycleService _service;

    public TransitionAssetStageHandler(IAssetLifecycleService service)
    {
        _service = service;
    }

    public async Task<AssetLifecycleRecord> Handle(TransitionAssetStageCommand request, CancellationToken cancellationToken)
        => await _service.TransitionStageAsync(request.MaterielId, request.Stage, request.Notes, cancellationToken);
}

public sealed class GetAssetsByStageHandler : IRequestHandler<GetAssetsByStageQuery, IEnumerable<Materiel>>
{
    private readonly IAssetLifecycleService _service;

    public GetAssetsByStageHandler(IAssetLifecycleService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<Materiel>> Handle(GetAssetsByStageQuery request, CancellationToken cancellationToken)
        => await _service.GetAssetsByStageAsync(request.Stage, cancellationToken);
}

public sealed class GetAssetsNearingEndOfLifeHandler : IRequestHandler<GetAssetsNearingEndOfLifeQuery, IEnumerable<Materiel>>
{
    private readonly IAssetLifecycleService _service;

    public GetAssetsNearingEndOfLifeHandler(IAssetLifecycleService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<Materiel>> Handle(GetAssetsNearingEndOfLifeQuery request, CancellationToken cancellationToken)
        => await _service.GetAssetsNearingEndOfLifeAsync(request.WithinMonths, cancellationToken);
}
