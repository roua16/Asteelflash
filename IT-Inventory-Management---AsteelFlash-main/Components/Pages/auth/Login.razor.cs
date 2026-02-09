using ITStockM.Services;
using ITStockM.Models.Constants;
using Microsoft.AspNetCore.Components;

namespace ITStockM.Components.Pages.auth

{
    public partial class Login  
    {
        [Inject]
        protected IAuthService authService { get; set; }

        [Inject]
        protected CustomAuthenticationStateProvider authStateProvider { get; set; }
        [Inject]
        protected NavigationManager navigationManager { get; set; }
        [Inject]
        protected CustomAuthenticationStateProvider AuthStateProvider { get; set; }

        private Models.ViewModels.LoginModel loginModel = new();
        private string errorMessage = string.Empty;
        private bool isChecking = true;
        private bool isAlreadyAuthenticated = false;

        protected override async Task OnInitializedAsync()
        {
            var authState = await authStateProvider.GetAuthenticationStateAsync();
            isAlreadyAuthenticated = authState.User.Identity?.IsAuthenticated ?? false;

            if (isAlreadyAuthenticated)
            {
                if (navigationManager.Uri.Contains("ReturnUrl"))
                {
                    navigationManager.NavigateTo("/access-denied", forceLoad: true);
                }
                else
                {
                    navigationManager.NavigateTo("/", forceLoad: true);
                }
            }
            else
            {
                isChecking = false;
            }
        }

        private async Task HandleLogin()
        {
            var user = await authService.Authenticate(loginModel.Email, loginModel.Password);

            if (user != null)
            {
                await AuthStateProvider.UpdateAuthenticationState(new UserSession
                {
                    Email = user.Email,
                    Role = UserRoles.NormalizeRole(!string.IsNullOrWhiteSpace(user.Role) ? user.Role : user.Post),
                    Id = user.Id,
                    FullName = user.FullName
                });

                navigationManager.NavigateTo("/", forceLoad: true);
            }
            else
            {
                errorMessage = "Invalid email or password.";
            }
        }

    }
}