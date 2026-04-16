namespace ITStockM.Application.Features.Dashboard.DTOs;

/// <summary>
/// Dashboard KPI statistics: total materials, assignments, and pending requests.
/// </summary>
public sealed record DashboardStatsDto(
    int TotalMaterials,
    int TotalAssignments,
    int PendingRequests);
