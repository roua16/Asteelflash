using ITStockM.Models.ViewModels;

namespace ITStockM.Application.Common.Interfaces;

/// <summary>
/// Application-layer contract for user authentication.
///
/// SOLID compliance:
///   I – The interface exposes only what the Application layer needs.
///       Low-level AD details (previously <c>ADAuthenticateUser</c>) belong
///       to <see cref="IActiveDirectoryService"/> in the Infrastructure layer.
///   D – Callers depend on this abstraction, never on <c>AuthService</c>.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates the supplied credentials and returns the matching user,
    /// or <c>null</c> when authentication fails.
    ///
    /// Authentication strategy (AD first, fallback seed credentials) is an
    /// implementation concern hidden behind this interface.
    /// </summary>
    Task<AppUser?> Authenticate(string email, string password);

    /// <summary>
    /// Retrieves a user by e-mail address without verifying credentials.
    /// Returns <c>null</c> when no matching account exists.
    /// </summary>
    Task<AppUser?> GetUserByEmail(string email);
}
