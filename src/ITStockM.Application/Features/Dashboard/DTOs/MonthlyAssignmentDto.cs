namespace ITStockM.Application.Features.Dashboard.DTOs;

/// <summary>
/// Monthly assignment trend data for dashboard charts.
/// </summary>
public sealed class MonthlyAssignmentDto
{
    public string Month { get; set; }
    public int Value { get; set; }
}
