using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.Maintenance.Queries;

public sealed record GetMaintenanceTicketByIdQuery(int Id) : IRequest<MaintenanceTicket?>;
