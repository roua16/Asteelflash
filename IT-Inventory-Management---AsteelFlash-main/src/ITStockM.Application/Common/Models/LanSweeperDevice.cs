namespace ITStockM.Application.Common.Models;

/// <summary>
/// Represents a device discovered by LanSweeper network scanning.
/// Read-only projection from the LanSweeper SQL database.
/// </summary>
public class LanSweeperDevice
{
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? MacAddress { get; set; }
    public string? Domain { get; set; }
    public string? Username { get; set; }
    public string? AssetType { get; set; }
    public string? OsName { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? Processor { get; set; }
    public int? MemoryMb { get; set; }
    public string? Location { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime? LastTried { get; set; }

    /// <summary>
    /// Returns true if the device was seen within the last 24 hours.
    /// </summary>
    public bool IsOnline => LastSeen.HasValue && (DateTime.UtcNow - LastSeen.Value).TotalHours <= 24;

    public string MemoryDisplay => MemoryMb.HasValue
        ? MemoryMb.Value >= 1024
            ? $"{MemoryMb.Value / 1024} GB"
            : $"{MemoryMb.Value} MB"
        : "—";
}
