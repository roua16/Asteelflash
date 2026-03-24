using ITStockM.Models.ITStockManagment;
using ITStockM.Models.Enums;
using ITStockM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.Controllers
{
    /// <summary>
    /// REST API for asset lifecycle management.
    /// </summary>
    [ApiController]
    [Route("api/assets/lifecycle")]
    public class AssetLifecycleController : ControllerBase
    {
        private readonly IAssetLifecycleService _service;
        private readonly ILogger<AssetLifecycleController> _logger;

        public AssetLifecycleController(IAssetLifecycleService service, ILogger<AssetLifecycleController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        /// <summary>Get the full stage-transition history for an asset.</summary>
        [HttpGet("{materielId:int}/history")]
        public async Task<IActionResult> GetHistory(int materielId, CancellationToken ct)
        {
            var records = await _service.GetLifecycleHistoryAsync(materielId, ct);
            return Ok(records);
        }

        /// <summary>Transition an asset to a new lifecycle stage.</summary>
        [HttpPost("{materielId:int}/transition")]
        public async Task<IActionResult> Transition(int materielId, [FromBody] TransitionRequest request, CancellationToken ct)
        {
            if (!LifecycleStage.All.Contains(request.Stage))
                return BadRequest($"Invalid stage. Must be one of: {string.Join(", ", LifecycleStage.All)}");

            try
            {
                var record = await _service.TransitionStageAsync(materielId, request.Stage, request.Notes, ct);
                return Ok(record);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>List all assets in a given stage.</summary>
        [HttpGet("by-stage/{stage}")]
        public async Task<IActionResult> ByStage(string stage, CancellationToken ct)
        {
            var assets = await _service.GetAssetsByStageAsync(stage, ct);
            return Ok(assets);
        }

        /// <summary>Assets whose warranty or end-of-life falls within the next N months.</summary>
        [HttpGet("nearing-eol")]
        public async Task<IActionResult> NearingEndOfLife([FromQuery] int withinMonths = 6, CancellationToken ct = default)
        {
            var assets = await _service.GetAssetsNearingEndOfLifeAsync(withinMonths, ct);
            return Ok(assets);
        }
    }

    public record TransitionRequest(string Stage, string? Notes);
}
