namespace ITStockM.Application.Features.Dashboard.DTOs;

/// <summary>
/// Dashboard material item: name, type, stock quantities, and status.
/// </summary>
public sealed class DashboardMaterialItemDto
{
    public int Id { get; set; }
    public string MaterielName { get; set; }
    public string Type { get; set; }
    public int QuantityITStock { get; set; }
    public int QuantityPDRStock { get; set; }
    
    public int Total => QuantityITStock + QuantityPDRStock;
    
    public string StatusBadge
    {
        get => Total > 20 ? "OK" : Total > 5 ? "LOW" : "CRITICAL";
    }
}
