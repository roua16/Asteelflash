using System.DirectoryServices;
using System.Text;
using ITStockM.Application.Common.Interfaces;
using ITStockM.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ITStockM.Infrastructure.Services.ActiveDirectory;

/// <summary>
/// Concrete LDAP implementation of <see cref="IActiveDirectoryService"/>.
///
/// SOLID compliance:
///   S – Responsible solely for LDAP bind + user-lookup. Fallback credentials
///       are handled by <see cref="AuthService"/> (different responsibility).
///   O – New auth strategies extend the interface; this class is closed for
///       modification.
///   L – Substitutable wherever <see cref="IActiveDirectoryService"/> is needed.
///   I – Interface exposes only what callers need.
///   D – Depends on <see cref="IOptions{ActiveDirectoryOptions}"/> and
///       <see cref="ILogger{T}"/> abstractions, not concretions.
///
/// Security:
///   • LDAP-injection: every value inserted into a filter is passed through
///     <see cref="EscapeLdapFilter"/> (RFC 4515).
///   • Credentials are never logged.
///   • Exceptions are caught and demoted to debug-level log entries so that
///     callers can fall back gracefully without leaking stack-traces.
/// </summary>
public sealed class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly ActiveDirectoryOptions _options;
    private readonly ILogger<ActiveDirectoryService> _logger;

    public ActiveDirectoryService(
        IOptions<ActiveDirectoryOptions> options,
        ILogger<ActiveDirectoryService> logger)
    {
        _options = options.Value;
        _logger  = logger;
    }

    /// <inheritdoc />
    public Task<ActiveDirectoryAuthResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Active Directory authentication is disabled via configuration.");
            return Task.FromResult(ActiveDirectoryAuthResult.Disabled);
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Task.FromResult(ActiveDirectoryAuthResult.Failed);

        if (string.IsNullOrWhiteSpace(_options.LdapPath))
        {
            _logger.LogWarning("ActiveDirectory:LdapPath is not configured. Skipping AD authentication.");
            return Task.FromResult(ActiveDirectoryAuthResult.Disabled);
        }

        try
        {
            // DirectoryEntry bind authenticates the user implicitly when
            // NativeObject is accessed. Credentials are passed as separate
            // parameters – no injection vector here.
            using var entry = new DirectoryEntry(_options.LdapPath, username, password);

            // Force the LDAP bind; throws COMException / DirectoryServicesCOMException
            // when credentials are invalid or the server is unreachable.
            _ = entry.NativeObject;

            // Scope the search to the configured (or auto-derived) search base.
            using var searchRoot = string.IsNullOrWhiteSpace(_options.ResolvedSearchBase)
                ? entry
                : new DirectoryEntry($"{_options.LdapPath}/{_options.ResolvedSearchBase}", username, password);

            using var searcher = new DirectorySearcher(searchRoot)
            {
                // RFC 4515 – escape username before embedding in filter to prevent injection.
                Filter        = $"(&(objectClass=user)(sAMAccountName={EscapeLdapFilter(username)}))",
                ClientTimeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
            };

            searcher.PropertiesToLoad.Add("displayName");
            searcher.PropertiesToLoad.Add("sAMAccountName");

            var result = searcher.FindOne();

            if (result is null)
            {
                _logger.LogDebug("AD lookup returned no results for username {Username}.", username);
                return Task.FromResult(ActiveDirectoryAuthResult.Failed);
            }

            var displayName = result.Properties["displayName"].Count > 0
                ? result.Properties["displayName"][0]?.ToString()
                : null;

            _logger.LogInformation(
                "AD authentication succeeded. User: {DisplayName} ({SamAccountName}).",
                displayName, username);

            return Task.FromResult(ActiveDirectoryAuthResult.Success(username, displayName));
        }
        catch (Exception ex)
        {
            // Log at Debug so that expected failures (wrong password, server down
            // in dev) do not produce noise in production logs.
            _logger.LogDebug(
                ex,
                "AD authentication failed for username {Username}. " +
                "Caller will fall back to seed credentials if configured.",
                username);

            return Task.FromResult(ActiveDirectoryAuthResult.Failed);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Escapes a string so it can be safely embedded as an assertion value
    /// inside an LDAP search filter (RFC 4515, §3).
    ///
    /// Characters that must be escaped:
    ///   <c>\</c>  → <c>\5c</c>
    ///   <c>*</c>  → <c>\2a</c>
    ///   <c>(</c>  → <c>\28</c>
    ///   <c>)</c>  → <c>\29</c>
    ///   <c>NUL</c> → <c>\00</c>
    ///   <c>/</c>  → <c>\2f</c>
    /// </summary>
    private static string EscapeLdapFilter(string value)
    {
        var sb = new StringBuilder(value.Length);

        foreach (var ch in value)
        {
            sb.Append(ch switch
            {
                '\\'  => @"\5c",
                '*'   => @"\2a",
                '('   => @"\28",
                ')'   => @"\29",
                '\0'  => @"\00",
                '/'   => @"\2f",
                _     => ch
            });
        }

        return sb.ToString();
    }
}
