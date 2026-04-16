using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITStockM.Domain.Entities;
using ITStockM.Services.Requests;
using ITStockM.Services.Offers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// Infrastructure management API endpoints.
/// Handles infrastructure requests, offers, and related operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class InfrastructureController : ControllerBase
{
    private readonly IRequestService _requestService;
    private readonly IOfferService _offerService;

    public InfrastructureController(
        IRequestService requestService,
        IOfferService offerService)
    {
        _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        _offerService = offerService ?? throw new ArgumentNullException(nameof(offerService));
    }

    /// <summary>
    /// Get all infrastructure requests.
    /// </summary>
    /// <returns>List of all requests</returns>
    [HttpGet("requests")]
    [ProducesResponseType(typeof(IEnumerable<Request>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequests()
    {
        try
        {
            var requestsQuery = await _requestService.GetRequests();
            var requests = requestsQuery
                .Where(r => !r.IsDeleted)
                .ToList();

            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving requests", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new infrastructure request.
    /// </summary>
    /// <param name="request">Request details</param>
    /// <returns>Created request</returns>
    [HttpPost("request")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateRequest([FromBody] Request request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request data is required" });

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest(new { message = "Request title is required" });

            if (string.IsNullOrWhiteSpace(request.Description))
                return BadRequest(new { message = "Request description is required" });

            var createdRequest = await _requestService.CreateRequest(request);
            return CreatedAtAction(nameof(GetRequestById), new { id = createdRequest.Id }, createdRequest);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error creating request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific request by ID.
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <returns>Request details</returns>
    [HttpGet("request/{id}")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRequestById(int id)
    {
        try
        {
            var request = await _requestService.GetRequestById(id);
            if (request == null || request.IsDeleted)
                return NotFound(new { message = $"Request with ID {id} not found" });

            return Ok(request);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving request", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing infrastructure request.
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <param name="request">Updated request details</param>
    /// <returns>Updated request</returns>
    [HttpPut("request/{id}")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] Request request)
    {
        try
        {
            if (request == null)
                return BadRequest(new { message = "Request data is required" });

            var existingRequest = await _requestService.GetRequestById(id);
            if (existingRequest == null || existingRequest.IsDeleted)
                return NotFound(new { message = $"Request with ID {id} not found" });

            request.Id = id;
            var updatedRequest = await _requestService.UpdateRequest(id, request);
            return Ok(updatedRequest);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error updating request", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete an infrastructure request.
    /// </summary>
    /// <param name="id">Request ID</param>
    /// <returns>No content</returns>
    [HttpDelete("request/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteRequest(int id)
    {
        try
        {
            var request = await _requestService.GetRequestById(id);
            if (request == null || request.IsDeleted)
                return NotFound(new { message = $"Request with ID {id} not found" });

            await _requestService.DeleteRequest(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error deleting request", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all available offers for infrastructure requests.
    /// </summary>
    /// <returns>List of all offers grouped by request</returns>
    [HttpGet("offers")]
    [ProducesResponseType(typeof(IEnumerable<Offer>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOffers()
    {
        try
        {
            var offersQuery = await _offerService.GetOffers();
            var offers = offersQuery
                .Where(o => !o.IsDeleted)
                .ToList();

            return Ok(offers);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving offers", error = ex.Message });
        }
    }
}
