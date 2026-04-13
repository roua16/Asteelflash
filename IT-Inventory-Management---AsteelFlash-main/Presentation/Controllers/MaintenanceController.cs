using ITStockM.Application.Features.Maintenance.Commands;
using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Application.Features.Maintenance.Queries;
using ITStockM.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.Controllers
{
    /// <summary>
    /// REST API for maintenance-ticket management.
    /// </summary>
    [ApiController]
    [Route("api/assets/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
            IMediator mediator,
            ILogger<MaintenanceController> logger)
        {
            _mediator       = mediator;
            _logger         = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllMaintenanceTicketsQuery(), ct));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var ticket = await _mediator.Send(new GetMaintenanceTicketByIdQuery(id), ct);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        [HttpGet("materiel/{materielId:int}")]
        public async Task<IActionResult> GetByMateriel(int materielId, CancellationToken ct)
            => Ok(await _mediator.Send(new GetTicketsByMaterielQuery(materielId), ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaintenanceTicket ticket, CancellationToken ct)
        {
            try
            {
                var created = await _mediator.Send(new CreateMaintenanceTicketCommand(ticket), ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MaintenanceTicket ticket, CancellationToken ct)
        {
            try   { return Ok(await _mediator.Send(new UpdateMaintenanceTicketCommand(id, ticket), ct)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        /// <summary>Close a ticket with a resolution note and optional repair cost.</summary>
        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close(int id, [FromBody] CloseTicketDto request, CancellationToken ct)
        {
            try
            {
                var closed = await _mediator.Send(new CloseMaintenanceTicketCommand(id, request.Resolution, request.Cost), ct);
                return Ok(closed);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet("count/open")]
        public async Task<IActionResult> OpenCount(CancellationToken ct)
            => Ok(new { count = await _mediator.Send(new GetOpenTicketCountQuery(), ct) });
    }
}
