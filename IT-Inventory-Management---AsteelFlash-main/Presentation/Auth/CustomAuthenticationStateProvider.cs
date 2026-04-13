using System.Security.Claims;
using ITStockM.Models.Constants;
using ITStockM.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IAuthService _authService;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;


    public CustomAuthenticationStateProvider(ProtectedLocalStorage localStorage, IAuthService authService, ILogger<CustomAuthenticationStateProvider> logger)
    {
        _localStorage = localStorage;
        _authService = authService;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionStorageResult = await _localStorage.GetAsync<UserSession>("UserSession");
            var userSession = userSessionStorageResult.Success ? userSessionStorageResult.Value : null;

            if (userSession == null)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));


            var user = await _authService.GetUserByEmail(userSession.Email);
            if (user == null)
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            var roleCandidate = !string.IsNullOrWhiteSpace(user.Role) ? user.Role : user.Post;
            var role = UserRoles.NormalizeRole(roleCandidate);

            // Some pages read the stored session role directly; keep it normalized.
            if (!string.Equals(userSession.Role, role, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(userSession.FullName, user.FullName, StringComparison.Ordinal) ||
                userSession.Id != user.Id)
            {
                userSession.Role = role;
                userSession.FullName = user.FullName;
                userSession.Id = user.Id;
                await _localStorage.SetAsync("UserSession", userSession);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", user.FullName)
            };

            _logger?.LogInformation("AuthenticationState built for {Email} with role {Role}", user.Email, role);

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
            await _localStorage.SetAsync("UserSession", userSession);

            var normalizedRole = UserRoles.NormalizeRole(userSession.Role);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userSession.Email),
                new Claim(ClaimTypes.Role, normalizedRole),
                new Claim("FullName", userSession.FullName)

            };
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));
        }
        else
        {
            await _localStorage.DeleteAsync("UserSession");
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        }

        _logger?.LogInformation("UpdateAuthenticationState: {Email} role={Role}", userSession?.Email, userSession?.Role);
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