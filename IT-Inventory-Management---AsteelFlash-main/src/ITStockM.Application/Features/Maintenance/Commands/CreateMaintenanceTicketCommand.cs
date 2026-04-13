using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.Maintenance.Commands;

public sealed record CreateMaintenanceTicketCommand(MaintenanceTicket Ticket) : IRequest<MaintenanceTicket>;
