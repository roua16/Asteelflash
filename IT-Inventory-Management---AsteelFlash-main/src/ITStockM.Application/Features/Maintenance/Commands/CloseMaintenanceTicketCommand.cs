using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.Maintenance.Commands;

public sealed record CloseMaintenanceTicketCommand(int Id, string Resolution, decimal? Cost) : IRequest<MaintenanceTicket>;
