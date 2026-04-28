using ITStockM.Application.Common.Models;

namespace ITStockM.Services.LanSweeper;

/// <summary>
/// Provides read-only access to devices discovered by LanSweeper.
/// </summary>
public interface ILanSweeperService
{
    /// <summary>Returns all scanned devices, optionally filtered by a search term.</summary>
    Task<List<LanSweeperDevice>> GetDevicesAsync(string? search = null);

    /// <summary>Returns a single device by its LanSweeper AssetId.</summary>
    Task<LanSweeperDevice?> GetDeviceByIdAsync(int assetId);

    /// <summary>Returns true when a valid LanSweeper connection string is configured.</summary>
    bool IsConfigured { get; }
}
