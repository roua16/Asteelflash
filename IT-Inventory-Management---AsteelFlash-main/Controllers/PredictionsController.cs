using ITStockM.Services.Interfaces;
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
        private readonly IPredictionService _service;
        private readonly ILogger<PredictionsController> _logger;

        public PredictionsController(IPredictionService service, ILogger<PredictionsController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        /// <summary>Latest prediction for every asset.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _service.GetLatestPredictionsAsync(ct));

        /// <summary>Assets whose health score is at or below <paramref name="maxScore"/> (default 50).</summary>
        [HttpGet("high-risk")]
        public async Task<IActionResult> HighRisk([FromQuery] decimal maxScore = 50m, CancellationToken ct = default)
            => Ok(await _service.GetHighRiskAssetsAsync(maxScore, ct));

        /// <summary>Force-recalculate the prediction for one asset.</summary>
        [HttpPost("{materielId:int}/recalculate")]
        public async Task<IActionResult> Recalculate(int materielId, CancellationToken ct)
        {
            try   { return Ok(await _service.CalculateAndSavePredictionAsync(materielId, ct)); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        /// <summary>Trigger a full recalculation for all assets (manual override of the daily job).</summary>
        [HttpPost("recalculate-all")]
        public async Task<IActionResult> RecalculateAll(CancellationToken ct)
        {
            var results = await _service.RecalculateAllPredictionsAsync(ct);
            return Ok(new { processed = results.Count() });
        }
    }
}
