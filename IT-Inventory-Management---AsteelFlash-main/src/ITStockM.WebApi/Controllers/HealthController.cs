using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITStockM.Application.Common.Models;
using ITStockM.Services.Interfaces;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// Health and diagnostic endpoints.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly IHardwareRecommendationService _hardwareRecommendationService;
    private readonly ITicketPrioritizationService _ticketPrioritizationService;

    public HealthController(
        IHardwareRecommendationService hardwareRecommendationService,
        ITicketPrioritizationService ticketPrioritizationService)
    {
        _hardwareRecommendationService = hardwareRecommendationService;
        _ticketPrioritizationService = ticketPrioritizationService;
    }

    /// <summary>
    /// Simple health check endpoint.
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    /// <summary>
    /// Get API version information.
    /// </summary>
    /// <returns>Version details</returns>
    [HttpGet("version")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetVersion()
    {
        return Ok(new
        {
            apiVersion = "1.0.0",
            applicationName = "IT Stock Management API",
            description = "Enterprise-level IT asset inventory management system"
        });
    }

    /// <summary>
    /// Get detailed application info.
    /// </summary>
    /// <returns>Application information</returns>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetInfo()
    {
        return Ok(new
        {
            applicationName = "IT Stock Management System",
            version = "1.0.0",
            description = "Enterprise-level IT asset inventory management system",
            organization = "Asteelflash",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            timestamp = DateTime.UtcNow,
            uptime = "See logs for detailed uptime"
        });
    }

    /// <summary>
    /// Get AI model metrics including version metadata, freshness, and drift indicators.
    /// </summary>
    [HttpGet("ai-models")]
    [ProducesResponseType(typeof(AiModelMetricsSnapshotDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAiModelMetrics(CancellationToken cancellationToken = default)
    {
        var hardware = await _hardwareRecommendationService.GetModelStatusAsync(cancellationToken);
        var tickets = await _ticketPrioritizationService.GetModelStatusAsync(cancellationToken);

        var models = new List<AiModelStatusDto> { hardware, tickets };
        var maxDrift = models.Max(m => m.DriftScore);
        var fleetLevel = maxDrift switch
        {
            >= 60 => "high",
            >= 35 => "medium",
            _ => "low"
        };

        var snapshot = new AiModelMetricsSnapshotDto(
            GeneratedAtUtc: DateTime.UtcNow,
            Models: models,
            MaxDriftScore: maxDrift,
            FleetDriftLevel: fleetLevel);

        return Ok(snapshot);
    }
}
