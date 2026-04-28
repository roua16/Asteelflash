using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace ITStockM.WebApi.Services;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedLocalStorage _localStorage;

    private AuthenticationState _currentAuthenticationState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    // Ensures we only attempt one localStorage restore per circuit lifetime
    private bool _restorationAttempted = false;

    public CustomAuthenticationStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_currentAuthenticationState.User.Identity?.IsAuthenticated != true && !_restorationAttempted)
        {
            try
            {
                var result = await _localStorage.GetAsync<UserSession>("UserSession");
                _restorationAttempted = true;
                if (result.Success && result.Value is { } session)
                {
                    _currentAuthenticationState = BuildAuthState(session);
                }
            }
            catch (InvalidOperationException)
            {
                // JS interop unavailable (prerender phase) — leave anonymous, will retry on interactive render
            }
            catch (JSDisconnectedException)
            {
                // Circuit disconnected — leave anonymous
                _restorationAttempted = true;
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Data Protection key mismatch (e.g. container restarted) — can't decrypt, treat as logged out
                _restorationAttempted = true;
            }
            catch
            {
                _restorationAttempted = true;
            }
        }

        return _currentAuthenticationState;
    }

    public Task MarkUserAsAuthenticated(ClaimsPrincipal user)
    {
        _currentAuthenticationState = new AuthenticationState(user);
        NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthenticationState));
        return Task.CompletedTask;
    }

    public Task MarkUserAsLoggedOut()
    {
        _restorationAttempted = true;
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

        _currentAuthenticationState = BuildAuthState(userSession);
        _restorationAttempted = true;
        NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthenticationState));
    }

    private static AuthenticationState BuildAuthState(UserSession session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.Id.ToString()),
            new(ClaimTypes.Email, session.Email),
            new(ClaimTypes.Name, session.FullName),
            new(ClaimTypes.Role, session.Role)
        };
        var identity = new ClaimsIdentity(claims, "Custom");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
