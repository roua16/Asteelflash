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

            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__ITStockManagmentConnection")
                ?? "Data Source=ITStockManagement.db";

            if (IsSqlServerConnection(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
            else
            {
                optionsBuilder.UseSqlite(connectionString);
            }

            return new ITStockManagmentContext(optionsBuilder.Options);
        }

        private static bool IsSqlServerConnection(string connectionString)
        {
            return connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("MultipleActiveResultSets=", StringComparison.OrdinalIgnoreCase) ||
                   connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase);
        }
    }
}
