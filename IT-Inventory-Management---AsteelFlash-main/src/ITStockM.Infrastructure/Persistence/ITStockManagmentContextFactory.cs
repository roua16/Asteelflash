using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ITStockM.Data;

namespace ITStockM.Infrastructure.Persistence
{
    /// <summary>
    /// Design-time factory for creating DbContext instances during EF Core migrations.
    /// This allows migrations to be created without a running startup project.
    /// </summary>
    public class ITStockManagmentContextFactory : IDesignTimeDbContextFactory<ITStockManagmentContext>
    {
        public ITStockManagmentContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ITStockManagmentContext>();

            // Use SQL Server with a development connection string
            // This can be overridden via environment variables or configuration files
            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                ?? "Server=.;Database=ITStockManagment;Trusted_Connection=true;Encrypt=false";

            optionsBuilder.UseSqlServer(connectionString, options =>
                options.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));

            return new ITStockManagmentContext(optionsBuilder.Options);
        }
    }
}
