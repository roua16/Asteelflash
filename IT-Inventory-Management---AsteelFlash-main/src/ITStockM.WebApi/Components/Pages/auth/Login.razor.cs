using ITStockM.Services;
using ITStockM.Models.Constants;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ITStockM.WebApi.Services;

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
                    await AuthStateProvider.UpdateAuthenticationState(new UserSession
                    {
                        Email = user.Email,
                        Role = UserRoles.NormalizeRole(!string.IsNullOrWhiteSpace(user.Role) ? user.Role : user.Post),
                        Id = user.Id,
                        FullName = user.FullName,
                        LoginTime = DateTime.UtcNow
                    });

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
