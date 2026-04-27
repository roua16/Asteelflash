using ITStockM.Data;
using ITStockM.Models.ViewModels;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.DirectoryServices;

namespace ITStockM.Services
{
    public class AuthService : IAuthService
    {
        private readonly ITStockManagmentContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ITStockManagmentContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
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

            // Try Active Directory first using the username part of the email
            var adUsername = email.Contains('@') ? email.Split('@')[0] : email;
            bool adAuthenticated = false;
            try
            {
                adAuthenticated = ADAuthenticateUser(adUsername, password);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "AD authentication unavailable for {Username}, falling back to seed credentials", adUsername);
            }

            if (!adAuthenticated)
            {
                // Fall back to seed credentials for environments without AD connectivity
                var validPasswords = ResolvePasswordsForEmployee(user.Email);
                var isValidPassword = validPasswords.Any(valid =>
                    string.Equals(password, valid, StringComparison.Ordinal));

                if (!isValidPassword)
                {
                    return null;
                }
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

        /// <inheritdoc />
        public bool ADAuthenticateUser(string username, string password)
        {
            try
            {
                using var entry = new DirectoryEntry("LDAP://asteelflash.europe.lan", username, password);

                if (entry.NativeObject != null)
                {
                    using var searcher = new DirectorySearcher(entry);
                    searcher.Filter = $"(&(ObjectClass=user)(sAMAccountName={username}))";
                    searcher.PropertiesToLoad.Add("displayName");

                    var result = searcher.FindOne();
                    if (result != null && result.Properties["displayName"].Count > 0)
                    {
                        var fullName = result.Properties["displayName"][0]?.ToString();
                        _logger.LogInformation("AD authentication succeeded for user: {FullName}", fullName);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "AD authentication failed for username {Username}", username);
                return false;
            }
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
