using Dapper;
using ITStockM.Application.Common.Models;
using ITStockM.Services.LanSweeper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.Infrastructure.Services;

/// <summary>
/// Queries the LanSweeper SQL Server database to retrieve network-discovered devices.
/// Uses Dapper for lightweight, read-only access.
/// </summary>
public class LanSweeperService : ILanSweeperService
{
    private readonly string? _connectionString;
    private readonly ILogger<LanSweeperService> _logger;

    public LanSweeperService(IConfiguration configuration, ILogger<LanSweeperService> logger)
    {
        _connectionString = configuration.GetConnectionString("LanSweeperConnection");
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_connectionString) &&
        !_connectionString.Contains("YOUR_LANSWEEPER_SERVER", StringComparison.OrdinalIgnoreCase);

    public async Task<List<LanSweeperDevice>> GetDevicesAsync(string? search = null)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("LanSweeper connection string is not configured.");
            return [];
        }

        try
        {
            using var conn = new SqlConnection(_connectionString);

            const string sql = """
                SELECT
                    a.AssetID        AS AssetId,
                    a.AssetName      AS AssetName,
                    a.IPAddress      AS IpAddress,
                    a.Mac            AS MacAddress,
                    a.Domain         AS Domain,
                    a.Username       AS Username,
                    a.AssetTypename  AS AssetType,
                    a.Lastseen       AS LastSeen,
                    a.Lasttried      AS LastTried,
                    ac.OSname        AS OsName,
                    ac.Manufacturer  AS Manufacturer,
                    ac.Model         AS Model,
                    ac.Serialnumber  AS SerialNumber,
                    ac.Processor     AS Processor,
                    ac.Memory        AS MemoryMb,
                    ac.Location      AS Location
                FROM dbo.tblAssets a
                LEFT JOIN dbo.tblAssetCustom ac ON a.AssetID = ac.AssetID
                WHERE (@search IS NULL
                    OR a.AssetName   LIKE '%' + @search + '%'
                    OR a.IPAddress   LIKE '%' + @search + '%'
                    OR a.Username    LIKE '%' + @search + '%'
                    OR a.Domain      LIKE '%' + @search + '%'
                    OR ac.Model      LIKE '%' + @search + '%'
                    OR ac.Manufacturer LIKE '%' + @search + '%')
                ORDER BY a.Lastseen DESC
                """;

            var results = await conn.QueryAsync<LanSweeperDevice>(sql, new { search = string.IsNullOrWhiteSpace(search) ? null : search });
            return results.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query LanSweeper database.");
            return [];
        }
    }

    public async Task<LanSweeperDevice?> GetDeviceByIdAsync(int assetId)
    {
        if (!IsConfigured) return null;

        try
        {
            using var conn = new SqlConnection(_connectionString);

            const string sql = """
                SELECT
                    a.AssetID        AS AssetId,
                    a.AssetName      AS AssetName,
                    a.IPAddress      AS IpAddress,
                    a.Mac            AS MacAddress,
                    a.Domain         AS Domain,
                    a.Username       AS Username,
                    a.AssetTypename  AS AssetType,
                    a.Lastseen       AS LastSeen,
                    a.Lasttried      AS LastTried,
                    ac.OSname        AS OsName,
                    ac.Manufacturer  AS Manufacturer,
                    ac.Model         AS Model,
                    ac.Serialnumber  AS SerialNumber,
                    ac.Processor     AS Processor,
                    ac.Memory        AS MemoryMb,
                    ac.Location      AS Location
                FROM dbo.tblAssets a
                LEFT JOIN dbo.tblAssetCustom ac ON a.AssetID = ac.AssetID
                WHERE a.AssetID = @assetId
                """;

            return await conn.QueryFirstOrDefaultAsync<LanSweeperDevice>(sql, new { assetId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query LanSweeper device {AssetId}.", assetId);
            return null;
        }
    }
}
