namespace ITStockM.Application.Common.Models;

/// <summary>
/// Immutable result produced by an Active Directory authentication attempt.
/// Follows the Value-Object pattern: equality is structural, not referential.
/// </summary>
public sealed record ActiveDirectoryAuthResult
{
    /// <summary>True when the LDAP bind and user-lookup both succeeded.</summary>
    public bool IsAuthenticated { get; init; }

    /// <summary>
    /// displayName attribute returned by AD (may be null when AD is disabled
    /// or when the attribute is absent from the directory entry).
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>The sAMAccountName used for authentication.</summary>
    public string? SamAccountName { get; init; }

    // ── Convenience factory members ──────────────────────────────────────────

    /// <summary>Returned when AD is disabled in configuration.</summary>
    public static readonly ActiveDirectoryAuthResult Disabled =
        new() { IsAuthenticated = false };

    /// <summary>Returned when LDAP authentication fails for any reason.</summary>
    public static readonly ActiveDirectoryAuthResult Failed =
        new() { IsAuthenticated = false };

    /// <summary>Creates a successful result with user attributes from AD.</summary>
    public static ActiveDirectoryAuthResult Success(string samAccountName, string? displayName) =>
        new() { IsAuthenticated = true, SamAccountName = samAccountName, DisplayName = displayName };
}
