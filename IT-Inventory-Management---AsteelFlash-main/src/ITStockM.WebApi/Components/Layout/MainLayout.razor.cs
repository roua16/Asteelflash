using Microsoft.AspNetCore.Components;
using ITStockM.WebApi.Services;

namespace ITStockM.Components.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected AppThemeService AppThemeService { get; set; } = default!;

        protected AppThemeService ThemeProvider => AppThemeService;

        protected string ThemeClass => AppThemeService.BodyClass;

        protected string ThemeDataAttribute => AppThemeService.IsDark ? "dark" : "light";

        protected override void OnInitialized()
        {
            AppThemeService.ThemeChanged += OnThemeChanged;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            await AppThemeService.InitializeAsync();
        }

        private void OnThemeChanged()
        {
            _ = InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            AppThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}
