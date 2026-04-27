using ITStockM.Models.ViewModels;

namespace ITStockM.Services
{
    public interface IAuthService
    {
        Task<AppUser?> Authenticate(string email, string password);

        Task<AppUser?> GetUserByEmail(string email);

        /// <summary>
        /// Authenticates a user against the AsteelFlash Active Directory (LDAP).
        /// Returns true if credentials are valid in AD. Returns false on any failure.
        /// </summary>
        bool ADAuthenticateUser(string username, string password);
    }

}
