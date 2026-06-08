using ITStockM.Application.Features.Maintenance.Commands;
using ITStockM.Application.Features.Maintenance.DTOs;
using ITStockM.Application.Features.Maintenance.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// Maintenance ticket management endpoints.
/// Handles creation, updating, and tracking of maintenance tickets.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class MaintenanceController : BaseApiController
{
    public MaintenanceController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get all maintenance tickets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all maintenance tickets</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllMaintenanceTickets(CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetAllMaintenanceTicketsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific maintenance ticket by ID.
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Maintenance ticket details</returns>
    [HttpGet("{ticketId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMaintenanceTicketById(
        int ticketId,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetMaintenanceTicketByIdQuery(ticketId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get tickets for a specific equipment.
    /// </summary>
    /// <param name="materielId">Equipment ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tickets for the equipment</returns>
    [HttpGet("equipment/{materielId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTicketsByMateriel(
        int materielId,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetTicketsByMaterielQuery(materielId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get count of open maintenance tickets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of open tickets</returns>
    [HttpGet("count/open")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOpenTicketCount(CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetOpenTicketCountQuery(), cancellationToken);
        return Ok(new { openTickets = result });
    }

    /// <summary>
    /// Create a new maintenance ticket.
    /// </summary>
    /// <param name="ticket">Maintenance ticket data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created ticket details</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateMaintenanceTicket(
        [FromBody] CreateMaintenanceTicketDto ticket,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateMaintenanceTicketCommand(ticket);
        var result = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMaintenanceTicketById), new { ticketId = result.Id }, result);
    }

    /// <summary>
    /// Update an existing maintenance ticket.
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="ticket">Updated ticket data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated ticket details</returns>
    [HttpPut("{ticketId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateMaintenanceTicket(
        int ticketId,
        [FromBody] UpdateMaintenanceTicketDto ticket,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateMaintenanceTicketCommand(ticketId, ticket);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Close a maintenance ticket.
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="request">Close request with resolution and optional cost</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Closed ticket details</returns>
    [HttpPost("{ticketId}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CloseMaintenanceTicket(
        int ticketId,
        [FromBody] CloseTicketDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new CloseMaintenanceTicketCommand(ticketId, request.Resolution, request.Cost);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Predict maintenance ticket priority with AI scoring.
    /// </summary>
    /// <param name="request">Ticket details for scoring</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Predicted priority and explainability signals</returns>
    [HttpPost("prioritize")]
    [ProducesResponseType(typeof(TicketPriorityPredictionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PredictTicketPriority(
        [FromBody] TicketPriorityRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new PredictTicketPriorityQuery(request), cancellationToken);
        return Ok(result);
    }
}
