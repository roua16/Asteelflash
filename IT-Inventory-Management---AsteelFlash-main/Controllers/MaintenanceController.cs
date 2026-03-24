using ITStockM.Models.ITStockManagment;
using ITStockM.Services.Interfaces;
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
        private readonly IMaintenanceService _service;
        private readonly IOperationNotificationService _notifications;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
            IMaintenanceService service,
            IOperationNotificationService notifications,
            ILogger<MaintenanceController> logger)
        {
            _service        = service;
            _notifications  = notifications;
            _logger         = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _service.GetAllTicketsAsync(ct));

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var ticket = await _service.GetTicketByIdAsync(id, ct);
            return ticket is null ? NotFound() : Ok(ticket);
        }

        [HttpGet("materiel/{materielId:int}")]
        public async Task<IActionResult> GetByMateriel(int materielId, CancellationToken ct)
            => Ok(await _service.GetTicketsByMaterielAsync(materielId, ct));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaintenanceTicket ticket, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateTicketAsync(ticket, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] MaintenanceTicket ticket, CancellationToken ct)
        {
            try   { return Ok(await _service.UpdateTicketAsync(id, ticket, ct)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        /// <summary>Close a ticket with a resolution note and optional repair cost.</summary>
        [HttpPost("{id:int}/close")]
        public async Task<IActionResult> Close(int id, [FromBody] CloseTicketRequest request, CancellationToken ct)
        {
            try
            {
                var closed = await _service.CloseTicketAsync(id, request.Resolution, request.Cost, ct);
                return Ok(closed);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [HttpGet("count/open")]
        public async Task<IActionResult> OpenCount(CancellationToken ct)
            => Ok(new { count = await _service.GetOpenTicketCountAsync(ct) });
    }

    public record CloseTicketRequest(string Resolution, decimal? Cost);
}
