using ITStockM.Application.Features.AssetLifecycle.Commands;
using ITStockM.Application.Features.AssetLifecycle.DTOs;
using ITStockM.Application.Features.AssetLifecycle.Queries;
using ITStockM.Application.Features.Common.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// Asset Lifecycle management endpoints.
/// Handles asset stage transitions and lifecycle tracking.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AssetLifecycleController : BaseApiController
{
    public AssetLifecycleController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Get assets grouped by lifecycle stage.
    /// </summary>
    /// <param name="stage">Lifecycle stage to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assets organized by stage</returns>
    [HttpGet("by-stage/{stage}")]
    [ProducesResponseType(typeof(IEnumerable<MaterielDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetsByStage(string stage, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetAssetsByStageQuery(stage), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get lifecycle history for an asset.
    /// </summary>
    /// <param name="materielId">Material/Asset ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lifecycle history records</returns>
    [HttpGet("{materielId}/history")]
    [ProducesResponseType(typeof(IEnumerable<AssetLifecycleRecordDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLifecycleHistory(int materielId, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetLifecycleHistoryQuery(materielId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get assets nearing end of life.
    /// </summary>
    /// <param name="withinMonths">Months until end of life threshold</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assets approaching end of life</returns>
    [HttpGet("nearing-end-of-life")]
    [ProducesResponseType(typeof(IEnumerable<MaterielDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAssetsNearingEndOfLife(
        [FromQuery] int withinMonths = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetAssetsNearingEndOfLifeQuery(withinMonths),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Transition an asset to a new lifecycle stage.
    /// </summary>
    /// <param name="materielId">Material ID</param>
    /// <param name="request">Transition request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated asset lifecycle status</returns>
    [HttpPost("{materielId}/transition")]
    [ProducesResponseType(typeof(AssetLifecycleRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TransitionAssetStage(
        int materielId,
        [FromBody] TransitionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var command = new TransitionAssetStageCommand(materielId, request.Stage, request.Notes);
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
