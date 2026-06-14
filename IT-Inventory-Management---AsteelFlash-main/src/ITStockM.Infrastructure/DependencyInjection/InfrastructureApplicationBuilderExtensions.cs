using ITStockM.Data;
using ITStockM.Infrastructure.Identity;
using ITStockM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITStockM.Infrastructure.DependencyInjection;

public static class InfrastructureApplicationBuilderExtensions
{
    public static async Task InitializeInfrastructureAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ITStockManagmentContext>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

            await DatabaseInitializer.InitializeAsync(
                context,
                app.Environment,
                app.Configuration,
                logger,
                userManager,
                roleManager);

            // Seed the LanSweeper demo database (creates lansweeperdb on the same SQL Server)
            await LanSweeperDatabaseInitializer.InitializeAsync(app.Configuration, logger);
        }
        catch (Exception ex)
        {
            var logger = services.GetService<ILoggerFactory>()?.CreateLogger("Startup");
            logger?.LogError(ex, "Startup initialization failed");
            Console.WriteLine("Startup initialization failed: " + ex.Message);
        }
    }
}