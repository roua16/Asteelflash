using ITStockM.Application.Features.Predictions.Commands;
using ITStockM.Application.Features.Predictions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.Controllers
{
    /// <summary>
    /// REST API for asset health predictions and replacement forecasts.
    /// </summary>
    [ApiController]
    [Route("api/assets/predictions")]
    public class PredictionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PredictionsController> _logger;

        public PredictionsController(IMediator mediator, ILogger<PredictionsController> logger)
        {
            _mediator = mediator;
            _logger  = logger;
        }

        /// <summary>Latest prediction for every asset.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _mediator.Send(new GetLatestPredictionsQuery(), ct));

        /// <summary>Assets whose health score is at or below <paramref name="maxScore"/> (default 50).</summary>
        [HttpGet("high-risk")]
        public async Task<IActionResult> HighRisk([FromQuery] decimal maxScore = 50m, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetHighRiskAssetsQuery(maxScore), ct));

        /// <summary>Force-recalculate the prediction for one asset.</summary>
        [HttpPost("{materielId:int}/recalculate")]
        public async Task<IActionResult> Recalculate(int materielId, CancellationToken ct)
        {
            try   { return Ok(await _mediator.Send(new RecalculatePredictionCommand(materielId), ct)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        /// <summary>Trigger a full recalculation for all assets (manual override of the daily job).</summary>
        [HttpPost("recalculate-all")]
        public async Task<IActionResult> RecalculateAll(CancellationToken ct)
        {
            var results = await _mediator.Send(new RecalculateAllPredictionsCommand(), ct);
            return Ok(new { processed = results.Count() });
        }
    }
}
