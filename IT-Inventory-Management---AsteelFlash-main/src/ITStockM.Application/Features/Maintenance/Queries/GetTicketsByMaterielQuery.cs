using MediatR;
using ITStockM.Domain.Entities;

namespace ITStockM.Application.Features.Maintenance.Queries;

public sealed record GetTicketsByMaterielQuery(int MaterielId) : IRequest<IEnumerable<MaintenanceTicket>>;
