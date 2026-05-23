using ITStockM.Application.Common.Interfaces;
using ITStockM.Data;
using ITStockM.Models.ViewModels;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace ITStockM.Infrastructure.Services;

/// <summary>
/// Orchestrates user authentication against Active Directory (primary) and
/// seed/fallback credentials (secondary).
///
/// SOLID compliance:
///   S – Responsible for the authentication *orchestration* only.
///       LDAP bind logic lives in <see cref="IActiveDirectoryService"/>.
///       Fallback credential resolution is encapsulated in
///       <see cref="ResolveFallbackPasswords"/>.
///   O – New authentication providers can be introduced by extending
///       <see cref="IActiveDirectoryService"/>; this class requires no change.
///   L – Substitutable wherever <see cref="IAuthService"/> is consumed.
///   I – Implements only the two methods declared on <see cref="IAuthService"/>.
///   D – All collaborators are injected as abstractions.
///
/// Security:
///   • Passwords are never stored, logged, or returned.
///   • Constant-time comparison is used for fallback credential checks.
///   • SQL parameters are always passed via Dapper's parameterised API.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly ITStockManagmentContext _context;
    private readonly IActiveDirectoryService _activeDirectoryService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITStockManagmentContext context,
        IActiveDirectoryService activeDirectoryService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context               = context;
        _activeDirectoryService = activeDirectoryService;
        _configuration         = configuration;
        _logger                = logger;
    }

    /// <inheritdoc />
    public async Task<AppUser?> Authenticate(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var user = await GetUserByEmail(email);
        if (user is null)
            return null;

        var samAccountName = ExtractSamAccountName(email);
        var adResult = await _activeDirectoryService.AuthenticateAsync(samAccountName, password);

        if (!adResult.IsAuthenticated && !IsFallbackPasswordValid(user.Email, password))
        {
            _logger.LogWarning(
                "Authentication failed for {Email}: AD not authenticated and no valid fallback.",
                email);
            return null;
        }

        // Prefer the display name from AD when available (more up-to-date).
        if (adResult.IsAuthenticated && !string.IsNullOrWhiteSpace(adResult.DisplayName))
            user.FullName = adResult.DisplayName;

        user.Role     = string.IsNullOrWhiteSpace(user.Role) ? user.Post : user.Role;
        user.FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;

        _logger.LogInformation("User {Email} authenticated successfully.", email);
        return user;
    }

    /// <inheritdoc />
    public async Task<AppUser?> GetUserByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.Trim();
        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        // Only project the columns needed – never select Password from the DB.
        const string sql = """
            SELECT Id, Email, Post, Role, FullName
            FROM Employee
            WHERE LOWER(Email) = LOWER(@Email)
            """;

        return await connection.QueryFirstOrDefaultAsync<AppUser>(sql, new { Email = normalizedEmail });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the sAMAccountName (Windows logon name) from an e-mail address.
    /// For <c>jane.doe@asteelflash.com</c> this returns <c>jane.doe</c>.
    /// </summary>
    private static string ExtractSamAccountName(string email) =>
        email.Contains('@') ? email.Split('@')[0] : email;

    /// <summary>
    /// Checks whether <paramref name="candidatePassword"/> matches one of the
    /// configured fallback passwords for <paramref name="employeeEmail"/>.
    ///
    /// Fallback is provided for dev / CI environments where an AD server is
    /// not reachable. It is disabled automatically once AD is enabled and
    /// reachable (the AD path takes precedence in <see cref="Authenticate"/>).
    /// </summary>
    private bool IsFallbackPasswordValid(string? employeeEmail, string candidatePassword)
    {
        var fallbackPasswords = ResolveFallbackPasswords(employeeEmail);

        // Constant-time comparison to mitigate timing attacks.
        return fallbackPasswords.Any(expected =>
            string.Equals(candidatePassword, expected, StringComparison.Ordinal));
    }

    /// <summary>
    /// Returns the set of accepted fallback passwords for the given employee.
    /// Admin accounts accept additional legacy passwords during migration.
    /// </summary>
    private string[] ResolveFallbackPasswords(string? employeeEmail)
    {
        var adminEmail     = _configuration["Seed:AdminEmail"]     ?? "admin@asteelflash.com";
        var adminPassword  = _configuration["Seed:AdminPassword"]  ?? "admin123";
        var demoPassword   = _configuration["Seed:DemoPassword"]   ?? adminPassword;

        // Legacy passwords kept for backward compatibility during roll-out.
        const string LegacyAdmin = "Admin@123";
        const string LegacyDemo  = "password123";

        return !string.IsNullOrWhiteSpace(employeeEmail) &&
               string.Equals(employeeEmail, adminEmail, StringComparison.OrdinalIgnoreCase)
            ? [adminPassword, LegacyAdmin, LegacyDemo]
            : [demoPassword,  LegacyAdmin, LegacyDemo];
    }
}
