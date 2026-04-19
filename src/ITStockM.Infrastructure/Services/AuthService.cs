using ITStockM.Data;
using ITStockM.Models.ViewModels;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace ITStockM.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITStockManagmentContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ITStockManagmentContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AppUser?> Authenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await GetUserByEmail(email);
            if (user == null)
            {
                return null;
            }

            // v2 schema doesn't store employee passwords.
            // Use seed credentials so login works in SQLite and SQL Server deployments.
            var validPasswords = ResolvePasswordsForEmployee(user.Email);
            var isValidPassword = validPasswords.Any(valid =>
                string.Equals(password, valid, StringComparison.Ordinal));

            if (!isValidPassword)
            {
                return null;
            }

            user.Role = string.IsNullOrWhiteSpace(user.Role) ? user.Post : user.Role;
            user.FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
            user.Password = string.Empty;
            return user;
        }

        public async Task<AppUser?> GetUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim();
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            const string sql = """
                               SELECT Id, Email, Post, Role, FullName
                               FROM Employee
                               WHERE LOWER(Email) = LOWER(@Email)
                               """;

            return await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Email = normalizedEmail });
        }

        private string[] ResolvePasswordsForEmployee(string? employeeEmail)
        {
            var adminEmail = _configuration["Seed:AdminEmail"] ?? "admin@asteelflash.com";
            var adminPassword = _configuration["Seed:AdminPassword"] ?? "admin123";
            var demoPassword = _configuration["Seed:DemoPassword"] ?? adminPassword;
            const string legacyPassword = "Admin@123";
            const string legacyDemoPassword = "password123";

            if (!string.IsNullOrWhiteSpace(employeeEmail) &&
                string.Equals(employeeEmail, adminEmail, StringComparison.OrdinalIgnoreCase))
            {
                return [adminPassword, legacyPassword, legacyDemoPassword];
            }

            return [demoPassword, legacyPassword, legacyDemoPassword];
        }
    }
}
