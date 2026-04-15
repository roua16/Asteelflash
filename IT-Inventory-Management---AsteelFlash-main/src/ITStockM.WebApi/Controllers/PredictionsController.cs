using ITStockM.Application.Features.Predictions.Commands;
using ITStockM.Application.Features.Predictions.DTOs;
using ITStockM.Application.Features.Predictions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// Asset prediction and forecasting endpoints.
/// Provides asset health predictions and risk assessments.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PredictionsController : BaseApiController
{
    public PredictionsController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get latest predictions for all assets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest asset predictions</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(IEnumerable<AssetPredictionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLatestPredictions(CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetLatestPredictionsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get high-risk assets that may need replacement.
    /// </summary>
    /// <param name="maxScore">Maximum health score threshold for high-risk classification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>High-risk assets list</returns>
    [HttpGet("high-risk")]
    [ProducesResponseType(typeof(IEnumerable<AssetPredictionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHighRiskAssets(
        [FromQuery] decimal maxScore = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetHighRiskAssetsQuery(maxScore),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Recalculate predictions for all assets.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recalculated predictions</returns>
    [HttpPost("recalculate")]
    [ProducesResponseType(typeof(IEnumerable<AssetPredictionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecalculateAllPredictions(
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new RecalculateAllPredictionsCommand(), cancellationToken);
        return Ok(result);
    }
}
