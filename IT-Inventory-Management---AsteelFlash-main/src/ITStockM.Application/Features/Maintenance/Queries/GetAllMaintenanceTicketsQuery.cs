using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.Maintenance.Queries;

public sealed record GetAllMaintenanceTicketsQuery : IRequest<IEnumerable<MaintenanceTicket>>;
