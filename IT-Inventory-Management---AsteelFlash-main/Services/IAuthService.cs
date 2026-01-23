using ITStockM.Models.ViewModels;

namespace ITStockM.Services
{
    public interface IAuthService
    {
        Task<AppUser> Authenticate(string email, string password);
        //Task<bool> Register(AppUser user, string password);

        Task<AppUser> GetUserByEmail(string email);
    }

}
