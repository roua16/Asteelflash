using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ITStockM.WebApi.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private AuthenticationState _currentAuthenticationState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_currentAuthenticationState);
    }

    public Task MarkUserAsAuthenticated(ClaimsPrincipal user)
    {
        _currentAuthenticationState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthenticationState));
        return Task.CompletedTask;
    }

    public Task MarkUserAsLoggedOut()
    {
        _currentAuthenticationState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthenticationState));
        return Task.CompletedTask;
    }

    public async Task UpdateAuthenticationState(UserSession? userSession = null)
    {
        if (userSession is null)
        {
            await MarkUserAsLoggedOut();
            return;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userSession.Id.ToString()),
            new(ClaimTypes.Email, userSession.Email),
            new(ClaimTypes.Name, userSession.FullName),
            new(ClaimTypes.Role, userSession.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, "Custom");
        var user = new ClaimsPrincipal(claimsIdentity);
        await MarkUserAsAuthenticated(user);
    }
}
