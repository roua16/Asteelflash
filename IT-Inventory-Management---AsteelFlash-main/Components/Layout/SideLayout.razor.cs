using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ITStockM.Services;


namespace ITStockM.Components.Layout
{
    public partial class SideLayout
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }
        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Inject]
        public ThemeService ThemeService { get; set; }

        bool sidebarExpanded = true;

        protected string user;

        protected int orderNumbersNF = 0;

        string theme;
        protected override async Task OnInitializedAsync()
        {
            // NOTE: Do NOT call ProtectedLocalStorage here — it uses JS interop which is
            // unavailable during prerendering. All storage reads are deferred to OnAfterRenderAsync.
            var deliveryOrders = await ITStockManagmentService.GetDeliveryOrdersList();
            orderNumbersNF = deliveryOrders.Where(dlo => dlo.OrderNumber == null).Count();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            // Circuit is now connected — JS interop (ProtectedLocalStorage) is safe to use.
            try
            {
                user = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value?.FullName ?? "Guest";
            }
            catch
            {
                await LocalStorage.DeleteAsync("UserSession");
                user = "Guest";
            }

            try
            {
                theme = (await LocalStorage.GetAsync<string>("theme")).Value;
                if (string.IsNullOrEmpty(theme))
                {
                    theme = "humanistic";
                    await LocalStorage.SetAsync("theme", theme);
                }
                ThemeService.SetTheme(theme);
            }
            catch
            {
                theme = "humanistic";
                await LocalStorage.SetAsync("theme", theme);
                ThemeService.SetTheme(theme);
            }

            StateHasChanged();
        }

        void SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
        }

        async void ChangeTheme()
        {
            if (ThemeService.Theme == "humanistic")
            {
                ThemeService.SetTheme("humanistic-dark");
                theme = "humanistic-dark";
                await LocalStorage.SetAsync("theme", "humanistic-dark");
            }
            else
            {
                ThemeService.SetTheme("humanistic");
                theme = "humanistic";
                await LocalStorage.SetAsync("theme", "humanistic");
            }
            StateHasChanged();
        }

        private void ForceRefresh()
        {
            NavigationManager.NavigateTo("delivery-order-history", forceLoad: true);
        }
    }
}