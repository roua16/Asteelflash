using AutoMapper;
using ITStockM.Application.Features.Maintenance.Commands;
using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Application.Features.Maintenance.Queries;
using ITStockM.Domain.Entities;
using ITStockM.Services.Interfaces;
using MediatR;

namespace ITStockM.Application.Features.Maintenance.Handlers;

public sealed class GetAllMaintenanceTicketsHandler : IRequestHandler<GetAllMaintenanceTicketsQuery, IEnumerable<MaintenanceTicketDto>>
{
    private readonly IMaintenanceService _service;
    private readonly IMapper _mapper;

    public GetAllMaintenanceTicketsHandler(IMaintenanceService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MaintenanceTicketDto>> Handle(GetAllMaintenanceTicketsQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _service.GetAllTicketsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<MaintenanceTicketDto>>(tickets);
    }
}

public sealed class GetMaintenanceTicketByIdHandler : IRequestHandler<GetMaintenanceTicketByIdQuery, MaintenanceTicketDto?>
{
    private readonly IMaintenanceService _service;
    private readonly IMapper _mapper;

    public GetMaintenanceTicketByIdHandler(IMaintenanceService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<MaintenanceTicketDto?> Handle(GetMaintenanceTicketByIdQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _service.GetTicketByIdAsync(request.Id, cancellationToken);
        return ticket is null ? null : _mapper.Map<MaintenanceTicketDto>(ticket);
    }
}

public sealed class GetTicketsByMaterielHandler : IRequestHandler<GetTicketsByMaterielQuery, IEnumerable<MaintenanceTicketDto>>
{
    private readonly IMaintenanceService _service;
    private readonly IMapper _mapper;

    public GetTicketsByMaterielHandler(IMaintenanceService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IEnumerable<MaintenanceTicketDto>> Handle(GetTicketsByMaterielQuery request, CancellationToken cancellationToken)
    {
        var tickets = await _service.GetTicketsByMaterielAsync(request.MaterielId, cancellationToken);
        return _mapper.Map<IEnumerable<MaintenanceTicketDto>>(tickets);
    }
}

public sealed class CreateMaintenanceTicketHandler : IRequestHandler<CreateMaintenanceTicketCommand, MaintenanceTicketDto>
{
    private readonly IMaintenanceService _service;
    private readonly IMapper _mapper;

    public CreateMaintenanceTicketHandler(IMaintenanceService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<MaintenanceTicketDto> Handle(CreateMaintenanceTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = _mapper.Map<MaintenanceTicket>(request.Ticket);
        var createdTicket = await _service.CreateTicketAsync(ticket, cancellationToken);
        return _mapper.Map<MaintenanceTicketDto>(createdTicket);
    }
}

public sealed class UpdateMaintenanceTicketHandler : IRequestHandler<UpdateMaintenanceTicketCommand, MaintenanceTicketDto>
{
    private readonly IMaintenanceService _service;
    private readonly IMapper _mapper;

    public UpdateMaintenanceTicketHandler(IMaintenanceService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<MaintenanceTicketDto> Handle(UpdateMaintenanceTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = _mapper.Map<MaintenanceTicket>(request.Ticket);
        var updatedTicket = await _service.UpdateTicketAsync(request.Id, ticket, cancellationToken);
        return _mapper.Map<MaintenanceTicketDto>(updatedTicket);
    }
}

public sealed class CloseMaintenanceTicketHandler : IRequestHandler<CloseMaintenanceTicketCommand, MaintenanceTicketDto>
{
    private readonly IMaintenanceService _service;
    private readonly IMapper _mapper;

    public CloseMaintenanceTicketHandler(IMaintenanceService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<MaintenanceTicketDto> Handle(CloseMaintenanceTicketCommand request, CancellationToken cancellationToken)
    {
        var closedTicket = await _service.CloseTicketAsync(request.Id, request.Resolution, request.Cost, cancellationToken);
        return _mapper.Map<MaintenanceTicketDto>(closedTicket);
    }
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
