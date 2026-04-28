using ITStockM.Services;
using ITStockM.Models.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using ITStockM.WebApi.Services;
using System.Collections.Concurrent;

namespace ITStockM.Components.Pages.auth
{
    public partial class Login  
    {
        [Inject]
        protected IAuthService authService { get; set; } = default!;

        [Inject]
        protected NavigationManager navigationManager { get; set; } = default!;

        [Inject]
        protected CustomAuthenticationStateProvider AuthStateProvider { get; set; } = default!;

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        [Inject]
        protected ConcurrentDictionary<string, (UserSession Session, DateTime Expires)> AuthTokenStore { get; set; } = default!;

        private Models.ViewModels.LoginModel loginModel = new();
        private string errorMessage = string.Empty;
        private bool isChecking = true;
        private bool isAlreadyAuthenticated = false;
        private bool isSubmitting = false;
        private bool rememberMe = false;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            isAlreadyAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;

            if (isAlreadyAuthenticated)
            {
                navigationManager.NavigateTo("/dashboard");
            }
            else
            {
                isChecking = false;
            }
        }

        private async Task HandleLogin()
        {
            if (isSubmitting)
            {
                return;
            }

            isSubmitting = true;
            errorMessage = string.Empty;

            try
            {
                var user = await authService.Authenticate(loginModel.Email, loginModel.Password);

                if (user != null)
                {
                    var session = new UserSession
                    {
                        Email = user.Email,
                        Role = UserRoles.NormalizeRole(!string.IsNullOrWhiteSpace(user.Role) ? user.Role : user.Post),
                        Id = user.Id,
                        FullName = user.FullName,
                        LoginTime = DateTime.UtcNow
                    };

                    // Update Blazor in-memory auth state
                    await AuthStateProvider.UpdateAuthenticationState(session);

                    // Persist session across Blazor circuit restarts
                    try { await LocalStorage.SetAsync("UserSession", session); } catch { }

                    // Issue real HTTP auth cookie so [Authorize] pages work on direct navigation
                    try
                    {
                        var token = Guid.NewGuid().ToString("N");
                        AuthTokenStore.TryAdd(token, (session, DateTime.UtcNow.AddSeconds(30)));
                        await JS.InvokeVoidAsync("blazorAuth.signIn", token);
                    }
                    catch { }

                    navigationManager.NavigateTo("/dashboard");
                    return;
                }

                errorMessage = "Invalid email or password.";
            }
            finally
            {
                isSubmitting = false;
            }
        }

        private Task HandleInvalidLoginSubmit(EditContext _)
        {
            isSubmitting = false;
            errorMessage = string.Empty;
            return Task.CompletedTask;
        }

    }
}
