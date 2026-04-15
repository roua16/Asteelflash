using MediatR;
using ITStockM.Application.Features.Maintenance.DTOs;

namespace ITStockM.Application.Features.Maintenance.Commands;

public sealed record CloseMaintenanceTicketCommand(int Id, string Resolution, decimal? Cost) : IRequest<MaintenanceTicketDto>;
