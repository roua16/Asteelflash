using ITStockM.Application.Features.Dashboard.DTOs;
using ITStockM.Services.Materiels;
using ITStockM.Services.Assignments;
using Microsoft.AspNetCore.Mvc;

namespace ITStockM.WebApi.Controllers;

/// <summary>
/// Dashboard API endpoints for KPI statistics and inventory overview.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class DashboardController : BaseApiController
{
    private readonly IMaterielService _materielService;
    private readonly IAssignmentService _assignmentService;

    public DashboardController(
        MediatR.IMediator mediator,
        IMaterielService materielService,
        IAssignmentService assignmentService)
        : base(mediator)
    {
        _materielService = materielService ?? throw new ArgumentNullException(nameof(materielService));
        _assignmentService = assignmentService ?? throw new ArgumentNullException(nameof(assignmentService));
    }

    /// <summary>
    /// Get dashboard KPI statistics (total materials, assignments, pending requests).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard statistics</returns>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var materialsQuery = await _materielService.GetMateriels();
            var assignmentsQuery = await _assignmentService.GetAssignments();

            var totalMaterials = materialsQuery.Count();
            var totalAssignments = assignmentsQuery.Count();
            
            // Count pending assignments (those without restore date / still active)
            var pendingRequests = assignmentsQuery.Where(a => a.RestoreDate == null).Count();

            var stats = new DashboardStatsDto(
                TotalMaterials: totalMaterials,
                TotalAssignments: totalAssignments,
                PendingRequests: pendingRequests);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error retrieving dashboard statistics", error = ex.Message });
        }
    }

    /// <summary>
    /// Get top 3 materials by total quantity (IT + PDR stock).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Top 3 materials</returns>
    [HttpGet("top-materials")]
    [ProducesResponseType(typeof(IEnumerable<DashboardMaterialItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTopMaterials(CancellationToken cancellationToken = default)
    {
        try
        {
            var materialsQuery = await _materielService.GetMateriels();
            var topMaterials = materialsQuery
                .Where(m => !m.IsDeleted)
                .OrderByDescending(m => m.QuantityITStock + m.QuantityPDRStock)
                .Take(3)
                .Select(m => new DashboardMaterialItemDto
                {
                    Id = m.Id,
                    MaterielName = m.MaterielName,
                    Type = m.Type,
                    QuantityITStock = m.QuantityITStock,
                    QuantityPDRStock = m.QuantityPDRStock
                })
                .ToList();

            return Ok(topMaterials);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving top materials", error = ex.Message });
        }
    }

    /// <summary>
    /// Get complete inventory with all materials and their stock levels.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All materials with status</returns>
    [HttpGet("inventory")]
    [ProducesResponseType(typeof(IEnumerable<DashboardMaterialItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInventory(CancellationToken cancellationToken = default)
    {
        try
        {
            var materialsQuery = await _materielService.GetMateriels();
            var inventory = materialsQuery
                .Where(m => !m.IsDeleted)
                .Select(m => new DashboardMaterialItemDto
                {
                    Id = m.Id,
                    MaterielName = m.MaterielName,
                    Type = m.Type,
                    QuantityITStock = m.QuantityITStock,
                    QuantityPDRStock = m.QuantityPDRStock
                })
                .ToList();

            return Ok(inventory);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving inventory", error = ex.Message });
        }
    }

    /// <summary>
    /// Get monthly assignment trends for the last 12 months.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Monthly assignment data</returns>
    [HttpGet("assignments-monthly")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyAssignmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMonthlyAssignments(CancellationToken cancellationToken = default)
    {
        try
        {
            var assignmentsQuery = await _assignmentService.GetAssignments();

            // Group assignments by month created
            var now = DateTime.UtcNow;
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var monthlyData = last12Months
                .Select(month =>
                {
                    var count = assignmentsQuery
                        .Where(a => !a.IsDeleted &&
                                   a.CreatedAt.Year == month.Year &&
                                   a.CreatedAt.Month == month.Month)
                        .Count();

                    return new MonthlyAssignmentDto
                    {
                        Month = month.ToString("MMM"),
                        Value = count
                    };
                })
                .ToList();

            return Ok(monthlyData);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "Error retrieving monthly assignments", error = ex.Message });
        }
    }
}
