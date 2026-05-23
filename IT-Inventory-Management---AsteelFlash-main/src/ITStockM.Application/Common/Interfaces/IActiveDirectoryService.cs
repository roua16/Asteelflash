using ITStockM.Application.Common.Models;

namespace ITStockM.Application.Common.Interfaces;

/// <summary>
/// Abstraction for Active Directory / LDAP authentication.
///
/// Placing the contract in the Application layer satisfies the
/// Dependency-Inversion Principle: higher-level policy (auth flow)
/// depends on an abstraction, not on a concrete LDAP library.
///
/// The Infrastructure layer owns the implementation; the Application
/// layer owns this interface.
/// </summary>
public interface IActiveDirectoryService
{
    /// <summary>
    /// Attempts to authenticate <paramref name="username"/> against the
    /// configured LDAP / Active Directory server.
    /// </summary>
    /// <param name="username">
    ///   The sAMAccountName (Windows logon name) – typically the part before
    ///   the '@' in an e-mail address.
    /// </param>
    /// <param name="password">The user's plain-text password (never stored).</param>
    /// <param name="cancellationToken">Propagates cancellation.</param>
    /// <returns>
    ///   An <see cref="ActiveDirectoryAuthResult"/> describing the outcome.
    ///   This method never throws for expected failure cases (wrong credentials,
    ///   server unreachable, feature disabled). Only truly unexpected exceptions
    ///   bubble up to the global exception handler.
    /// </returns>
    Task<ActiveDirectoryAuthResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}
