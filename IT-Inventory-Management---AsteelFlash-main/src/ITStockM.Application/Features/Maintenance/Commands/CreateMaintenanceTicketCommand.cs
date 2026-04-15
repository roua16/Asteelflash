using MediatR;
using ITStockM.Application.Features.Maintenance.DTOs;

namespace ITStockM.Application.Features.Maintenance.Commands;

public sealed record CreateMaintenanceTicketCommand(CreateMaintenanceTicketDto Ticket) : IRequest<MaintenanceTicketDto>;
