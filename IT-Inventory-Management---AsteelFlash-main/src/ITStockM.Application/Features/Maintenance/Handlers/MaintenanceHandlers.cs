using ITStockM.Application.Features.Maintenance.Commands;
using ITStockM.Application.Features.Maintenance.Queries;
using ITStockM.Domain.Entities;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.Maintenance.Handlers;

public sealed class GetAllMaintenanceTicketsHandler : IRequestHandler<GetAllMaintenanceTicketsQuery, IEnumerable<MaintenanceTicket>>
{
    private readonly IMaintenanceService _service;

    public GetAllMaintenanceTicketsHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MaintenanceTicket>> Handle(GetAllMaintenanceTicketsQuery request, CancellationToken cancellationToken)
        => await _service.GetAllTicketsAsync(cancellationToken);
}

public sealed class GetMaintenanceTicketByIdHandler : IRequestHandler<GetMaintenanceTicketByIdQuery, MaintenanceTicket?>
{
    private readonly IMaintenanceService _service;

    public GetMaintenanceTicketByIdHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<MaintenanceTicket?> Handle(GetMaintenanceTicketByIdQuery request, CancellationToken cancellationToken)
        => await _service.GetTicketByIdAsync(request.Id, cancellationToken);
}

public sealed class GetTicketsByMaterielHandler : IRequestHandler<GetTicketsByMaterielQuery, IEnumerable<MaintenanceTicket>>
{
    private readonly IMaintenanceService _service;

    public GetTicketsByMaterielHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<IEnumerable<MaintenanceTicket>> Handle(GetTicketsByMaterielQuery request, CancellationToken cancellationToken)
        => await _service.GetTicketsByMaterielAsync(request.MaterielId, cancellationToken);
}

public sealed class CreateMaintenanceTicketHandler : IRequestHandler<CreateMaintenanceTicketCommand, MaintenanceTicket>
{
    private readonly IMaintenanceService _service;

    public CreateMaintenanceTicketHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<MaintenanceTicket> Handle(CreateMaintenanceTicketCommand request, CancellationToken cancellationToken)
        => await _service.CreateTicketAsync(request.Ticket, cancellationToken);
}

public sealed class UpdateMaintenanceTicketHandler : IRequestHandler<UpdateMaintenanceTicketCommand, MaintenanceTicket>
{
    private readonly IMaintenanceService _service;

    public UpdateMaintenanceTicketHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<MaintenanceTicket> Handle(UpdateMaintenanceTicketCommand request, CancellationToken cancellationToken)
        => await _service.UpdateTicketAsync(request.Id, request.Ticket, cancellationToken);
}

public sealed class CloseMaintenanceTicketHandler : IRequestHandler<CloseMaintenanceTicketCommand, MaintenanceTicket>
{
    private readonly IMaintenanceService _service;

    public CloseMaintenanceTicketHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<MaintenanceTicket> Handle(CloseMaintenanceTicketCommand request, CancellationToken cancellationToken)
        => await _service.CloseTicketAsync(request.Id, request.Resolution, request.Cost, cancellationToken);
}

public sealed class GetOpenTicketCountHandler : IRequestHandler<GetOpenTicketCountQuery, int>
{
    private readonly IMaintenanceService _service;

    public GetOpenTicketCountHandler(IMaintenanceService service)
    {
        _service = service;
    }

    public async Task<int> Handle(GetOpenTicketCountQuery request, CancellationToken cancellationToken)
        => await _service.GetOpenTicketCountAsync(cancellationToken);
}
