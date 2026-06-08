using ITStockM.Application.Features.Maintenance.DTOs;
using MediatR;

namespace ITStockM.Application.Features.Maintenance.Queries;

public sealed record PredictTicketPriorityQuery(TicketPriorityRequestDto Request) : IRequest<TicketPriorityPredictionDto>;
