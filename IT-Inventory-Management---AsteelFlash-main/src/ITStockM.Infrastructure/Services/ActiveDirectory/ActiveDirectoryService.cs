using System.DirectoryServices.Protocols;
using System.Net;
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
            var endpoint = ParseLdapEndpoint(_options.LdapPath);
            var searchBase = ResolveSearchBase(_options, _options.LdapPath);

            // 1) Authenticate user credentials (supports both AD and generic LDAP).
            var bindUser = TryAuthenticate(endpoint, searchBase, username, password, out var authenticatedUserDn)
                ? (authenticatedUserDn ?? username)
                : null;

            if (bindUser is null)
            {
                _logger.LogDebug("LDAP authentication failed for username {Username}.", username);
                return Task.FromResult(ActiveDirectoryAuthResult.Failed);
            }

            // 2) Resolve display name + groups for RBAC mapping.
            var profile = ResolveUserProfile(endpoint, searchBase, username, password, bindUser, authenticatedUserDn);
            var resolvedRole = ResolveRoleFromGroups(profile.GroupNames);

            _logger.LogInformation(
                "LDAP authentication succeeded. User: {DisplayName} ({Username}). ResolvedRole={ResolvedRole}.",
                profile.DisplayName ?? username,
                username,
                resolvedRole ?? "<none>");

            return Task.FromResult(
                ActiveDirectoryAuthResult.Success(
                    username,
                    profile.DisplayName,
                    profile.GroupNames,
                    resolvedRole));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "LDAP authentication failed for username {Username}: {ErrorMessage}",
                username,
                ex.Message);

            return Task.FromResult(ActiveDirectoryAuthResult.Failed);
        }
    }

    private bool TryAuthenticate(
        LdapEndpoint endpoint,
        string searchBase,
        string username,
        string password,
        out string? authenticatedUserDn)
    {
        authenticatedUserDn = null;

        // Try explicit user DN pattern first for generic LDAP providers.
        if (!string.IsNullOrWhiteSpace(_options.UserDnPattern))
        {
            var candidateDn = _options.UserDnPattern
                .Replace("{0}", username, StringComparison.Ordinal)
                .Replace("{1}", searchBase, StringComparison.Ordinal);

            if (TryBind(endpoint, candidateDn, password))
            {
                authenticatedUserDn = candidateDn;
                return true;
            }
        }

        // Try AD-style principal names.
        foreach (var candidate in BuildUsernameCandidates(username))
        {
            if (TryBind(endpoint, candidate, password))
                return true;
        }

        // Service-account search fallback: find user DN, then bind as that DN.
        if (!string.IsNullOrWhiteSpace(_options.BindDn) && !string.IsNullOrWhiteSpace(_options.BindPassword))
        {
            var userDn = FindUserDn(endpoint, searchBase, _options.BindDn, _options.BindPassword, username);
            if (!string.IsNullOrWhiteSpace(userDn) && TryBind(endpoint, userDn, password))
            {
                authenticatedUserDn = userDn;
                return true;
            }
        }

        return false;
    }

    private UserProfile ResolveUserProfile(
        LdapEndpoint endpoint,
        string searchBase,
        string username,
        string password,
        string bindUser,
        string? authenticatedUserDn)
    {
        // Prefer service-account search when configured; fallback to authenticated user.
        var searchBindDn = !string.IsNullOrWhiteSpace(_options.BindDn) ? _options.BindDn : bindUser;
        var searchBindPassword = !string.IsNullOrWhiteSpace(_options.BindPassword) ? _options.BindPassword : password;

        using var connection = CreateConnection(endpoint, searchBindDn, searchBindPassword, _options.TimeoutSeconds);

        var filterValue = EscapeLdapFilter(username);
        var filter =
            $"(|(sAMAccountName={filterValue})(uid={filterValue})(mail={filterValue})(userPrincipalName={filterValue}))";

        var request = new SearchRequest(
            searchBase,
            filter,
            SearchScope.Subtree,
            ["displayName", "cn", "mail", "memberOf"]);

        var response = (SearchResponse)connection.SendRequest(request);
        var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();

        var displayName = ReadFirstAttribute(entry, "displayName")
                          ?? ReadFirstAttribute(entry, "cn")
                          ?? ReadFirstAttribute(entry, "mail");

        var userDn = authenticatedUserDn ?? entry?.DistinguishedName;
        var groups = new List<string>();

        // AD usually returns memberOf directly.
        if (entry?.Attributes["memberOf"] is DirectoryAttribute memberOf)
        {
            foreach (var value in memberOf.GetValues(typeof(string)).OfType<string>())
            {
                if (!string.IsNullOrWhiteSpace(value))
                    groups.Add(value);
            }
        }

        // OpenLDAP often stores only "member" on group entries.
        if (!string.IsNullOrWhiteSpace(userDn))
        {
            var groupRequest = new SearchRequest(
                searchBase,
                $"(member={EscapeLdapFilter(userDn)})",
                SearchScope.Subtree,
                ["cn", "dn"]);

            var groupResponse = (SearchResponse)connection.SendRequest(groupRequest);
            foreach (var groupEntry in groupResponse.Entries.Cast<SearchResultEntry>())
            {
                groups.Add(groupEntry.DistinguishedName);

                var cn = ReadFirstAttribute(groupEntry, "cn");
                if (!string.IsNullOrWhiteSpace(cn))
                    groups.Add(cn);
            }
        }

        var normalizedGroups = ExpandCnAliases(groups)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UserProfile(displayName, normalizedGroups);
    }

    private string? FindUserDn(
        LdapEndpoint endpoint,
        string searchBase,
        string bindDn,
        string bindPassword,
        string username)
    {
        using var connection = CreateConnection(endpoint, bindDn, bindPassword, _options.TimeoutSeconds);

        var filterValue = EscapeLdapFilter(username);
        var filter =
            $"(|(sAMAccountName={filterValue})(uid={filterValue})(mail={filterValue})(userPrincipalName={filterValue}))";

        var request = new SearchRequest(searchBase, filter, SearchScope.Subtree, ["dn"]);
        var response = (SearchResponse)connection.SendRequest(request);
        var entry = response.Entries.Cast<SearchResultEntry>().FirstOrDefault();

        return entry?.DistinguishedName;
    }

    private bool TryBind(LdapEndpoint endpoint, string bindDnOrUser, string password)
    {
        try
        {
            using var connection = CreateConnection(endpoint, bindDnOrUser, password, _options.TimeoutSeconds);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static LdapConnection CreateConnection(LdapEndpoint endpoint, string bindDnOrUser, string password, int timeoutSeconds)
    {
        var identifier = new LdapDirectoryIdentifier(endpoint.Host, endpoint.Port, false, false);
        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Basic,
            Credential = new NetworkCredential(bindDnOrUser, password),
            Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds))
        };

        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.SecureSocketLayer = endpoint.UseSsl;
        connection.Bind();

        return connection;
    }

    private IEnumerable<string> BuildUsernameCandidates(string username)
    {
        yield return username;

        if (!username.Contains('@', StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(_options.Domain))
            yield return $"{username}@{_options.Domain}";
    }

    private static string ResolveSearchBase(ActiveDirectoryOptions options, string ldapPath)
    {
        if (!string.IsNullOrWhiteSpace(options.ResolvedSearchBase))
            return options.ResolvedSearchBase;

        if (TryParseLdapUri(ldapPath, out var uri) && !string.IsNullOrWhiteSpace(uri.AbsolutePath))
        {
            var path = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(path))
                return path;
        }

        return string.Empty;
    }

    private static LdapEndpoint ParseLdapEndpoint(string ldapPath)
    {
        if (TryParseLdapUri(ldapPath, out var uri))
        {
            var useSsl = string.Equals(uri.Scheme, "ldaps", StringComparison.OrdinalIgnoreCase);
            var port = uri.Port > 0 ? uri.Port : (useSsl ? 636 : 389);

            return new LdapEndpoint(uri.Host, port, useSsl);
        }

        // Host only fallback.
        return new LdapEndpoint(ldapPath.Trim(), 389, false);
    }

    private static bool TryParseLdapUri(string ldapPath, out Uri uri)
    {
        var normalized = ldapPath.Trim();
        if (normalized.StartsWith("LDAP://", StringComparison.OrdinalIgnoreCase))
            normalized = "ldap://" + normalized.Substring("LDAP://".Length);

        return Uri.TryCreate(normalized, UriKind.Absolute, out uri!);
    }

    private static string? ReadFirstAttribute(SearchResultEntry? entry, string attributeName)
    {
        if (entry?.Attributes[attributeName] is not DirectoryAttribute attribute)
            return null;

        return attribute.GetValues(typeof(string)).OfType<string>().FirstOrDefault();
    }

    private static IEnumerable<string> ExpandCnAliases(IEnumerable<string> groups)
    {
        foreach (var group in groups)
        {
            if (string.IsNullOrWhiteSpace(group))
                continue;

            yield return group;

            if (group.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                var cnPart = group.Substring(3);
                var commaIndex = cnPart.IndexOf(',', StringComparison.Ordinal);
                yield return commaIndex > -1 ? cnPart.Substring(0, commaIndex) : cnPart;
            }
        }
    }

    private string? ResolveRoleFromGroups(IReadOnlyList<string> groupNames)
    {
        if (groupNames is null || groupNames.Count == 0)
            return null;

        if (_options.RoleMappings is null || _options.RoleMappings.Count == 0)
            return null;

        foreach (var mapping in _options.RoleMappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.Group) || string.IsNullOrWhiteSpace(mapping.Role))
                continue;

            var expected = mapping.Group.Trim();

            foreach (var groupName in groupNames)
            {
                if (string.IsNullOrWhiteSpace(groupName))
                    continue;

                // substring + case-insensitive match: works for CN and full DN strings
                if (groupName.Contains(expected, StringComparison.OrdinalIgnoreCase))
                    return mapping.Role.Trim();
            }
        }

        return null;
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

    private sealed record LdapEndpoint(string Host, int Port, bool UseSsl);

    private sealed record UserProfile(string? DisplayName, string[] GroupNames);
}
