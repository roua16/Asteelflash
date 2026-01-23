using System.Security.Claims;
using ITStockM.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IAuthService _authService;


    public CustomAuthenticationStateProvider(ProtectedLocalStorage localStorage, IAuthService authService)
    {
        _localStorage = localStorage;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionStorageResult = await  _localStorage.GetAsync<UserSession>("UserSession");
            var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;

            if (userSession == null)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            
            var user = await _authService.GetUserByEmail(userSession.Email);
            if (user == null)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Post),
                new Claim("FullName", user.FullName)
            };

            var identity = new ClaimsIdentity(claims, "CustomAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public async Task UpdateAuthenticationState(UserSession userSession)
    {
        ClaimsPrincipal claimsPrincipal;

        if (userSession != null)
        {
            await  _localStorage.SetAsync("UserSession", userSession);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userSession.Email),
                new Claim(ClaimTypes.Role, userSession.Role),
                new Claim("FullName", userSession.FullName)

            };
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));
        }
        else
        {
            await  _localStorage.DeleteAsync("UserSession");
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}

public class UserSession
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }

    public string FullName { get; set; }

    public DateTime ExpiryTime { get; set; } = DateTime.UtcNow.AddDays(7);
}