using MediatR;
using ITStockM.Application.Features.Maintenance.DTOs;

namespace ITStockM.Application.Features.Maintenance.Commands;

public sealed record UpdateMaintenanceTicketCommand(int Id, UpdateMaintenanceTicketDto Ticket) : IRequest<MaintenanceTicketDto>;
