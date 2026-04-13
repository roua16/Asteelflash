using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.Maintenance.Commands;

public sealed record UpdateMaintenanceTicketCommand(int Id, MaintenanceTicket Ticket) : IRequest<MaintenanceTicket>;
