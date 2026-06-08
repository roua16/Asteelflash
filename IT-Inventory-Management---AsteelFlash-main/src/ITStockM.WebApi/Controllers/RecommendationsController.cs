using ITStockM.Application.Features.Recommendations.DTOs;
using ITStockM.Application.Features.Recommendations.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// AI-powered equipment recommendation endpoints.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class RecommendationsController : BaseApiController
{
    public RecommendationsController(IMediator mediator) : base(mediator) { }

    /// <summary>
    /// Recommends the most suitable hardware for a user profile.
    /// </summary>
    [HttpPost("hardware")]
    [ProducesResponseType(typeof(IReadOnlyList<HardwareRecommendationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecommendHardware(
        [FromBody] HardwareRecommendationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetHardwareRecommendationsQuery(request), cancellationToken);
        return Ok(result);
    }
}
