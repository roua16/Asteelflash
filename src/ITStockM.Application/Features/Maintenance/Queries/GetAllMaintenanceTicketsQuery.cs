using MediatR;
using ITStockM.Application.Features.Maintenance.DTOs;

namespace ITStockM.Application.Features.Maintenance.Queries;

public sealed record GetAllMaintenanceTicketsQuery : IRequest<IEnumerable<MaintenanceTicketDto>>;
