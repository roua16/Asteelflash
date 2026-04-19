using MediatR;

namespace ITStockM.Application.Features.Maintenance.Queries;

public sealed record GetOpenTicketCountQuery : IRequest<int>;
