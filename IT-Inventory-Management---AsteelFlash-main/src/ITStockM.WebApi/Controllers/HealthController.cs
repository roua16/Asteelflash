using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ITStockM.Application.Common.Models;
using ITStockM.Services.Interfaces;
using ITStockM.Services;
using Microsoft.Extensions.Options;

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
    private readonly AiModelRetrainingOptions _retrainingOptions;

    public HealthController(
        IHardwareRecommendationService hardwareRecommendationService,
        ITicketPrioritizationService ticketPrioritizationService,
        IOptions<AiModelRetrainingOptions> retrainingOptions)
    {
        _hardwareRecommendationService = hardwareRecommendationService;
        _ticketPrioritizationService = ticketPrioritizationService;
        _retrainingOptions = retrainingOptions.Value;
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
            var score when score >= _retrainingOptions.DriftHighThreshold => "high",
            var score when score >= _retrainingOptions.DriftMediumThreshold => "medium",
            _ => "low"
        };

        var snapshot = new AiModelMetricsSnapshotDto(
            GeneratedAtUtc: DateTime.UtcNow,
            Models: models,
            MaxDriftScore: maxDrift,
            FleetDriftLevel: fleetLevel);

        return Ok(snapshot);
    }

    /// <summary>
    /// Get AI readiness gate status derived from validation metrics and retraining outcomes.
    /// </summary>
    [HttpGet("ai-readiness")]
    [ProducesResponseType(typeof(AiReadinessSnapshotDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAiReadiness(CancellationToken cancellationToken = default)
    {
        var hardware = await _hardwareRecommendationService.GetModelStatusAsync(cancellationToken);
        var tickets = await _ticketPrioritizationService.GetModelStatusAsync(cancellationToken);

        var readinessModels = new List<AiReadinessModelGateDto>
        {
            BuildReadiness(
                hardware,
                _retrainingOptions.MinimumHardwareValidationMetric),
            BuildReadiness(
                tickets,
                _retrainingOptions.MinimumTicketValidationMetric)
        };

        var ready = readinessModels.All(m => m.IsOperational);
        var summary = ready
            ? "all models operational and above minimum acceptance thresholds"
            : "one or more models are below acceptance threshold or retraining is blocked";

        var snapshot = new AiReadinessSnapshotDto(
            GeneratedAtUtc: DateTime.UtcNow,
            ReadyForProduction: ready,
            Models: readinessModels,
            Summary: summary);

        return Ok(snapshot);
    }

    private static AiReadinessModelGateDto BuildReadiness(AiModelStatusDto model, double minimumRequiredMetric)
    {
        var status = model.LastRetrainStatus ?? string.Empty;
        var blockedByQuality = status.StartsWith("blocked_data_quality", StringComparison.OrdinalIgnoreCase);
        var blockedByAcceptance = status.StartsWith("blocked_acceptance", StringComparison.OrdinalIgnoreCase);
        var meetsMetric = model.ValidationMetric >= minimumRequiredMetric;
        var operational = meetsMetric && !blockedByQuality && !blockedByAcceptance;

        return new AiReadinessModelGateDto(
            ModelName: model.ModelName,
            LastRetrainStatus: status,
            ValidationMetric: model.ValidationMetric,
            MinimumRequiredMetric: minimumRequiredMetric,
            MeetsMinimumMetric: meetsMetric,
            RetrainBlockedByQualityGate: blockedByQuality,
            RetrainBlockedByAcceptanceGate: blockedByAcceptance,
            IsOperational: operational);
    }
}
