using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.Infrastructure.Persistence;

/// <summary>
/// Creates and seeds the LanSweeper demo database on the same SQL Server instance
/// so the LanSweeper page works out-of-the-box in Docker without a real LanSweeper install.
/// </summary>
public static class LanSweeperDatabaseInitializer
{
    public static async Task InitializeAsync(IConfiguration configuration, ILogger logger)
    {
        var cs = configuration.GetConnectionString("LanSweeperConnection");
        if (string.IsNullOrWhiteSpace(cs) ||
            cs.Contains("YOUR_LANSWEEPER", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("LanSweeper connection string not configured – skipping LanSweeper DB init.");
            return;
        }

        try
        {
            // Connect to master to create the database if missing
            var builder = new SqlConnectionStringBuilder(cs);
            var dbName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            builder.ConnectTimeout = 15;

            using var master = new SqlConnection(builder.ToString());
            await master.ExecuteAsync($"""
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}')
                    CREATE DATABASE [{dbName}];
                """);

            logger.LogInformation("LanSweeper database '{Db}' ready.", dbName);

            // Connect to the target database to create schema + seed
            builder.InitialCatalog = dbName;
            using var conn = new SqlConnection(builder.ToString());

            await conn.ExecuteAsync("""
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = 'tblAssets'
                )
                BEGIN
                    CREATE TABLE [dbo].[tblAssets] (
                        [AssetID]       INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
                        [AssetName]     NVARCHAR(255)  NULL,
                        [IPAddress]     NVARCHAR(50)   NULL,
                        [Mac]           NVARCHAR(50)   NULL,
                        [Domain]        NVARCHAR(255)  NULL,
                        [Username]      NVARCHAR(255)  NULL,
                        [AssetTypename] NVARCHAR(100)  NULL,
                        [Lastseen]      DATETIME       NULL,
                        [Lasttried]     DATETIME       NULL
                    );
                END
                """);

            await conn.ExecuteAsync("""
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                    WHERE TABLE_NAME = 'tblAssetCustom'
                )
                BEGIN
                    CREATE TABLE [dbo].[tblAssetCustom] (
                        [AssetID]      INT           NOT NULL PRIMARY KEY,
                        [OSname]       NVARCHAR(255) NULL,
                        [Manufacturer] NVARCHAR(255) NULL,
                        [Model]        NVARCHAR(255) NULL,
                        [Serialnumber] NVARCHAR(255) NULL,
                        [Processor]    NVARCHAR(255) NULL,
                        [Memory]       INT           NULL,
                        [Location]     NVARCHAR(255) NULL
                    );
                END
                """);

            // Only seed if empty
            var count = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM [dbo].[tblAssets]");
            if (count > 0)
            {
                logger.LogInformation("LanSweeper tables already have data – skipping seed.");
                return;
            }

            await SeedDemoDevicesAsync(conn, logger);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LanSweeper DB initialization failed – continuing startup.");
        }
    }

    private static async Task SeedDemoDevicesAsync(SqlConnection conn, ILogger logger)
    {
        var now = DateTime.UtcNow;

        var assets = new[]
        {
            new { AssetName = "AFLASH-PC-001", IPAddress = "192.168.1.10", Mac = "A4:BB:6D:10:00:01", Domain = "ASTEELFLASH", Username = "jdupont",   AssetTypename = "Windows",  Lastseen = now.AddMinutes(-5),  Lasttried = now.AddMinutes(-5)  },
            new { AssetName = "AFLASH-PC-002", IPAddress = "192.168.1.11", Mac = "A4:BB:6D:10:00:02", Domain = "ASTEELFLASH", Username = "mmartin",   AssetTypename = "Windows",  Lastseen = now.AddMinutes(-18), Lasttried = now.AddMinutes(-18) },
            new { AssetName = "AFLASH-PC-003", IPAddress = "192.168.1.12", Mac = "A4:BB:6D:10:00:03", Domain = "ASTEELFLASH", Username = "lbernard",  AssetTypename = "Windows",  Lastseen = now.AddHours(-2),   Lasttried = now.AddHours(-2)   },
            new { AssetName = "AFLASH-LT-001", IPAddress = "192.168.1.20", Mac = "DC:F5:05:20:00:01", Domain = "ASTEELFLASH", Username = "aroua",     AssetTypename = "Laptop",   Lastseen = now.AddMinutes(-3),  Lasttried = now.AddMinutes(-3)  },
            new { AssetName = "AFLASH-LT-002", IPAddress = "192.168.1.21", Mac = "DC:F5:05:20:00:02", Domain = "ASTEELFLASH", Username = "kchakir",   AssetTypename = "Laptop",   Lastseen = now.AddHours(-1),   Lasttried = now.AddHours(-1)   },
            new { AssetName = "AFLASH-LT-003", IPAddress = "192.168.1.22", Mac = "DC:F5:05:20:00:03", Domain = "ASTEELFLASH", Username = "pbouchard", AssetTypename = "Laptop",   Lastseen = now.AddDays(-1),    Lasttried = now.AddDays(-1)    },
            new { AssetName = "AFLASH-SRV-01", IPAddress = "192.168.1.100", Mac = "00:1A:4B:00:00:01", Domain = "ASTEELFLASH", Username = "svcaccount", AssetTypename = "Server",  Lastseen = now.AddSeconds(-30), Lasttried = now.AddSeconds(-30) },
            new { AssetName = "AFLASH-SRV-02", IPAddress = "192.168.1.101", Mac = "00:1A:4B:00:00:02", Domain = "ASTEELFLASH", Username = "svcaccount", AssetTypename = "Server",  Lastseen = now.AddSeconds(-45), Lasttried = now.AddSeconds(-45) },
            new { AssetName = "AFLASH-SW-CORE", IPAddress = "192.168.1.1",   Mac = "C0:11:55:AA:01:01", Domain = "",            Username = "",           AssetTypename = "Switch",  Lastseen = now.AddMinutes(-1),  Lasttried = now.AddMinutes(-1)  },
            new { AssetName = "AFLASH-SW-FL1",  IPAddress = "192.168.1.2",   Mac = "C0:11:55:AA:01:02", Domain = "",            Username = "",           AssetTypename = "Switch",  Lastseen = now.AddMinutes(-2),  Lasttried = now.AddMinutes(-2)  },
            new { AssetName = "AFLASH-RT-GW",   IPAddress = "192.168.1.254", Mac = "F4:6D:04:FF:00:01", Domain = "",            Username = "",           AssetTypename = "Router",  Lastseen = now.AddMinutes(-1),  Lasttried = now.AddMinutes(-1)  },
            new { AssetName = "AFLASH-PR-IT",   IPAddress = "192.168.1.50",  Mac = "00:26:AB:50:00:01", Domain = "ASTEELFLASH", Username = "",           AssetTypename = "Printer", Lastseen = now.AddHours(-6),   Lasttried = now.AddHours(-6)   },
            new { AssetName = "AFLASH-PC-OLD1", IPAddress = "",              Mac = "B8:AC:6F:99:00:01", Domain = "ASTEELFLASH", Username = "exuser1",   AssetTypename = "Windows",  Lastseen = now.AddDays(-14),   Lasttried = now.AddDays(-14)   },
        };

        foreach (var a in assets)
        {
            var id = await conn.ExecuteScalarAsync<int>("""
                INSERT INTO [dbo].[tblAssets] (AssetName, IPAddress, Mac, Domain, Username, AssetTypename, Lastseen, Lasttried)
                VALUES (@AssetName, @IPAddress, @Mac, @Domain, @Username, @AssetTypename, @Lastseen, @Lasttried);
                SELECT SCOPE_IDENTITY();
                """, a);

            await conn.ExecuteAsync("""
                INSERT INTO [dbo].[tblAssetCustom] (AssetID, OSname, Manufacturer, Model, Serialnumber, Processor, Memory, Location)
                VALUES (@AssetID, @OSname, @Manufacturer, @Model, @Serialnumber, @Processor, @Memory, @Location)
                """, GetCustom(id, a.AssetTypename));
        }

        logger.LogInformation("LanSweeper demo data seeded ({Count} devices).", assets.Length);
    }

    private static object GetCustom(int id, string type) => type switch
    {
        "Laptop" => new { AssetID = id, OSname = "Windows 11 Pro", Manufacturer = "Dell", Model = "Latitude 5530", Serialnumber = $"DL{id:D6}", Processor = "Intel Core i7-1265U", Memory = 16384, Location = "AsteelFlash Office" },
        "Server" => new { AssetID = id, OSname = "Windows Server 2022", Manufacturer = "HP", Model = "ProLiant DL380 Gen10", Serialnumber = $"HP{id:D6}", Processor = "Intel Xeon Silver 4210R", Memory = 65536, Location = "Server Room" },
        "Switch" => new { AssetID = id, OSname = (string?)null, Manufacturer = "Cisco", Model = "Catalyst 2960-X", Serialnumber = $"CS{id:D6}", Processor = (string?)null, Memory = (int?)null, Location = "Network Rack" },
        "Router" => new { AssetID = id, OSname = (string?)null, Manufacturer = "Cisco", Model = "ASA 5506-X", Serialnumber = $"CS{id:D6}", Processor = (string?)null, Memory = (int?)null, Location = "Network Rack" },
        "Printer" => new { AssetID = id, OSname = (string?)null, Manufacturer = "HP", Model = "LaserJet Pro M404dn", Serialnumber = $"HP{id:D6}", Processor = (string?)null, Memory = (int?)null, Location = "IT Office" },
        _ => new { AssetID = id, OSname = "Windows 10 Pro", Manufacturer = "HP", Model = "EliteDesk 800 G6", Serialnumber = $"HP{id:D6}", Processor = "Intel Core i5-10500", Memory = 8192, Location = "AsteelFlash Office" },
    };
}
