namespace ITStockM.Infrastructure.Services.ActiveDirectory;

/// <summary>
/// Strongly-typed configuration for the Active Directory / LDAP provider.
/// Bind this from the "ActiveDirectory" section of appsettings.json.
///
/// Single Responsibility: holds only AD connection/search parameters.
/// </summary>
public sealed class ActiveDirectoryOptions
{
    /// <summary>Configuration section key used during service registration.</summary>
    public const string Section = "ActiveDirectory";

    /// <summary>
    /// When false the service short-circuits immediately and returns
    /// <see cref="Application.Common.Models.ActiveDirectoryAuthResult.Disabled"/>.
    /// Set to true only in environments where the AD server is reachable.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Full LDAP path to the domain root.
    /// Example: <c>LDAP://asteelflash.europe.lan</c>
    /// </summary>
    public string LdapPath { get; set; } = string.Empty;

    /// <summary>
    /// The DNS domain name. Used to derive the LDAP search base when
    /// <see cref="SearchBase"/> is left blank.
    /// Example: <c>asteelflash.europe.lan</c>
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// LDAP distinguished name to scope the directory search.
    /// Leave empty to auto-derive from <see cref="Domain"/>.
    /// Example: <c>DC=asteelflash,DC=europe,DC=lan</c>
    /// </summary>
    public string SearchBase { get; set; } = string.Empty;

    /// <summary>
    /// Maximum seconds to wait for an LDAP response before treating
    /// the attempt as a failure. Defaults to 5 seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Derives the LDAP search base from <see cref="Domain"/> when
    /// <see cref="SearchBase"/> has not been set explicitly.
    /// </summary>
    public string ResolvedSearchBase =>
        !string.IsNullOrWhiteSpace(SearchBase)
            ? SearchBase
            : string.Join(',', Domain.Split('.').Select(part => $"DC={part}"));
}
