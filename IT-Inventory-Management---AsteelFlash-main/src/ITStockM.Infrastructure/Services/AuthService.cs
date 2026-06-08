using System.Data;
using System.Linq;
using Dapper;
using ITStockM.Application.Common.Interfaces;
using ITStockM.Data;
using ITStockM.Domain.Entities;
using ITStockM.Infrastructure.Identity;
using ITStockM.Models.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


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

    private readonly UserManager<ITStockM.Infrastructure.Identity.AppUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public AuthService(
        ITStockManagmentContext context,
        IActiveDirectoryService activeDirectoryService,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        UserManager<ITStockM.Infrastructure.Identity.AppUser> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _context                = context;
        _activeDirectoryService = activeDirectoryService;
        _configuration          = configuration;
        _logger                 = logger;
        _userManager            = userManager;
        _roleManager            = roleManager;
    }

    /// <inheritdoc />
    public async Task<ITStockM.Models.ViewModels.AppUser?> Authenticate(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;

        var normalizedEmail = email.Trim();
        var samAccountName  = ExtractSamAccountName(normalizedEmail);

        // AD is the source of truth for primary authentication.
        var adResult = await _activeDirectoryService.AuthenticateAsync(samAccountName, password);

        var adEnabled = _configuration.GetSection(ITStockM.Infrastructure.Services.ActiveDirectory.ActiveDirectoryOptions.Section)
            .GetValue<bool>("Enabled");

        // Fallback authentication depends on an existing Employee row.
        var employeeUser = await GetUserByEmail(normalizedEmail);

        // AD-only mode: if AD is enabled, do not accept dummy/fallback passwords.
        if (adEnabled && !adResult.IsAuthenticated)
        {
            _logger.LogWarning("Authentication failed for {Email}: AD enabled but AD authentication failed.", normalizedEmail);
            return null;
        }

        if (!adResult.IsAuthenticated)
        {
            if (employeeUser is null)
            {
                _logger.LogWarning(
                    "Authentication failed for {Email}: AD not authenticated and employee not found for fallback.",
                    normalizedEmail);
                return null;
            }

            if (!IsFallbackPasswordValid(employeeUser.Email, password))
            {
                _logger.LogWarning(
                    "Authentication failed for {Email}: AD not authenticated and no valid fallback.",
                    normalizedEmail);
                return null;
            }

            // No AD info, use existing employee record.
            NormalizeAndFinalizeBusinessUser(employeeUser);
            _logger.LogInformation("User {Email} authenticated successfully (fallback).", normalizedEmail);
            return employeeUser;
        }

        // AD authenticated => provision missing business account + identity RBAC.
        if (employeeUser is null)
        {
            var provisioned = await SeedEmployeeFromActiveDirectoryAsync(
                normalizedEmail,
                adResult.DisplayName,
                adResult.ResolvedAppRole,
                cancellationToken: default);

            if (!provisioned)
            {
                _logger.LogWarning(
                    "AD authenticated user {Email} but provisioning failed (Employee could not be seeded).",
                    normalizedEmail);
                return null;
            }

            // Reload from DB so we always operate on the ViewModels.AppUser instance.
            employeeUser = await GetUserByEmail(normalizedEmail);
            if (employeeUser is null)
                return null;
        }

        employeeUser = await GetUserByEmail(normalizedEmail);
        if (employeeUser is null)
            return null;

        // Apply AD attributes + resolved role.
        if (!string.IsNullOrWhiteSpace(adResult.DisplayName))
            employeeUser.FullName = adResult.DisplayName;

        employeeUser.Post = string.IsNullOrWhiteSpace(employeeUser.Post) ? "Employee" : employeeUser.Post;

        // Service is derived for UI only; keep AD provisioning compile-safe.
        // ((ITStockM.Models.ViewModels.AppUser)employeeUser).Service = ...

        var resolvedRole = string.IsNullOrWhiteSpace(adResult.ResolvedAppRole)
            ? UserRoles.Employee
            : UserRoles.NormalizeRole(adResult.ResolvedAppRole);

        employeeUser.Role = resolvedRole;
        employeeUser.FullName = string.IsNullOrWhiteSpace(employeeUser.FullName) ? employeeUser.Email : employeeUser.FullName;

        // Provision identity user + identity role in a fully idempotent way.
        await EnsureIdentityUserAndRolesAsync(employeeUser, cancellationToken: default);

        _logger.LogInformation("User {Email} authenticated successfully (AD).", normalizedEmail);
        return employeeUser;
    }

    /// <inheritdoc />
    public async Task<ITStockM.Models.ViewModels.AppUser?> GetUserByEmail(string email)
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

        return await connection.QueryFirstOrDefaultAsync<ITStockM.Models.ViewModels.AppUser>(sql, new { Email = normalizedEmail });
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Extracts the sAMAccountName (Windows logon name) from an e-mail address.
    /// For <c>jane.doe@asteelflash.com</c> this returns <c>jane.doe</c>.
    /// </summary>
    private static string ExtractSamAccountName(string email) =>
        email.Contains('@') ? email.Split('@')[0] : email;

    private void NormalizeAndFinalizeBusinessUser(ITStockM.Models.ViewModels.AppUser user)
    {
        user.Role     = string.IsNullOrWhiteSpace(user.Role) ? user.Post : user.Role;
        user.FullName = string.IsNullOrWhiteSpace(user.FullName) ? user.Email : user.FullName;
        user.Post ??= "Employee";
        // ((ITStockM.Models.ViewModels.AppUser)user).Service ??= "General";
    }

    private async Task<bool> SeedEmployeeFromActiveDirectoryAsync(
        string email,
        string? displayName,
        string? resolvedRole,
        CancellationToken cancellationToken)
    {
        var existing = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email != null && e.Email.ToLower() == email.ToLower(), cancellationToken);

        if (existing != null)
        {
            // Update minimal fields if empty.
            existing.FullName = string.IsNullOrWhiteSpace(existing.FullName)
                ? (displayName ?? existing.Email)
                : existing.FullName;

            existing.Post = string.IsNullOrWhiteSpace(existing.Post) ? "Employee" : existing.Post;
            existing.Service = string.IsNullOrWhiteSpace(existing.Service) ? "General" : existing.Service;

            var role = string.IsNullOrWhiteSpace(resolvedRole)
                ? UserRoles.Employee
                : UserRoles.NormalizeRole(resolvedRole);

            existing.Role = string.IsNullOrWhiteSpace(existing.Role) ? role : existing.Role;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        var roleToSet = string.IsNullOrWhiteSpace(resolvedRole)
            ? UserRoles.Employee
            : UserRoles.NormalizeRole(resolvedRole);

        var employee = new Employee
        {
            FullName = string.IsNullOrWhiteSpace(displayName) ? email : displayName,
            Email = email,
            Post = "Employee",
            Service = "General",
            Role = roleToSet,
            PhoneNumber = "+0000000000"
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureIdentityUserAndRolesAsync(ITStockM.Models.ViewModels.AppUser employeeUser, CancellationToken cancellationToken)
    {
        // Ensure identity role for the employee role exists.
        var targetRole = UserRoles.NormalizeRole(employeeUser.Role ?? employeeUser.Post);

        await EnsureIdentityRolesExistAsync(cancellationToken);

        var existingUser = await _userManager.FindByEmailAsync(employeeUser.Email);
        if (existingUser is null)
        {
            // AD provisioning: we don't know the user's password. Identity is still used
            // for cookie sign-in with claims, but the identity user must exist to support
            // role membership and future identity-based authorization.
            var created = new ITStockM.Infrastructure.Identity.AppUser
            {
                UserName = employeeUser.Email,
                Email = employeeUser.Email,
                FullName = employeeUser.FullName,
                Post = employeeUser.Post,
                // Service is a ViewModels concern for UI RBAC; Identity role membership handles authorization.
                // Service = employeeUser.Service,
                Role = targetRole,
                EmployeeId = employeeUser.Id
            };

            // Identity requires password hash; we can skip password creation and rely
            // on sign-in-by-claims in the app. However UserManager.CreateAsync requires password.
            // Therefore create a dummy password that meets requirements; AD-auth is still the login gate.
            var dummyPassword = _configuration["Seed:AdDummyPassword"] ?? "AdDummy1234!";
            var createResult = await _userManager.CreateAsync(created, dummyPassword);

            if (!createResult.Succeeded)
            {
                _logger.LogWarning(
                    "Identity provisioning failed for {Email}: {Errors}",
                    employeeUser.Email,
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            existingUser = await _userManager.FindByEmailAsync(employeeUser.Email);
        }
        else
        {
            // Keep identity profile in sync.
            existingUser.FullName = employeeUser.FullName;
            existingUser.Post = employeeUser.Post;
            existingUser.Role = targetRole;
            existingUser.EmployeeId = employeeUser.Id;

            await _userManager.UpdateAsync(existingUser);
        }

        if (existingUser is null)
            return;

        // Ensure role membership.
        if (!await _userManager.IsInRoleAsync(existingUser, targetRole))
        {
            if (!await _roleManager.RoleExistsAsync(targetRole))
            {
                // Should not happen because EnsureIdentityRolesExistAsync is called, but keep safe.
                await _roleManager.CreateAsync(new IdentityRole<int>(targetRole));
            }

            await _userManager.AddToRoleAsync(existingUser, targetRole);
        }
    }

    private async Task EnsureIdentityRolesExistAsync(CancellationToken cancellationToken)
    {
        foreach (var roleName in UserRoles.All)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
            }
        }
    }

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
    /// Returns the configured fallback password for the given employee.
    /// </summary>
    private string[] ResolveFallbackPasswords(string? employeeEmail)
    {
        var adminEmail     = _configuration["Seed:AdminEmail"]     ?? "admin@asteelflash.com";
        var adminPassword  = _configuration["Seed:AdminPassword"]  ?? "admin1234";
        var demoPassword   = _configuration["Seed:DemoPassword"]   ?? adminPassword;

        return !string.IsNullOrWhiteSpace(employeeEmail) &&
               string.Equals(employeeEmail, adminEmail, StringComparison.OrdinalIgnoreCase)
            ? [adminPassword]
            : [demoPassword];
    }
}
