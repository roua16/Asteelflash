namespace ITStockM.Application.Common.Models;

/// <summary>
/// Query contract used by Application/Infrastructure services without UI library dependencies.
/// </summary>
public sealed class QueryOptions
{
    public string? Filter { get; set; }
    public object[]? FilterParameters { get; set; }
    public string? OrderBy { get; set; }
    public int? Skip { get; set; }
    public int? Top { get; set; }
    public string? Expand { get; set; }
}
