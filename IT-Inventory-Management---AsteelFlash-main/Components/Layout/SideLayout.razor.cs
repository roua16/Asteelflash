using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ITStockM.Services.DeliveryOrders;


namespace ITStockM.Components.Layout
{
    public partial class SideLayout
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public ThemeService ThemeService { get; set; } = default!;

        [Inject]
        public IJSRuntime JSRuntime { get; set; } = default!;

        bool sidebarExpanded = true;

        protected string user = "Guest";

        protected int orderNumbersNF = 0;

        protected string theme = "humanistic-dark";

        protected bool coreExpanded = true;

        protected bool inventoryExpanded = true;

        protected bool lifecycleExpanded = true;

        protected bool purchasingExpanded = true;

        protected bool archivesExpanded = false;

        protected bool adminExpanded = false;

        protected bool IsDarkTheme => theme.Contains("dark", StringComparison.OrdinalIgnoreCase);

        protected string ThemeToggleIcon => IsDarkTheme ? "light_mode" : "dark_mode";

        protected string ThemeToggleLabel => IsDarkTheme ? "Switch to light theme" : "Switch to dark theme";

        protected override async Task OnInitializedAsync()
        {
            // NOTE: Do NOT call ProtectedLocalStorage here — it uses JS interop which is
            // unavailable during prerendering. All storage reads are deferred to OnAfterRenderAsync.
            var deliveryOrders = await DeliveryOrderService.GetDeliveryOrdersList();
            orderNumbersNF = deliveryOrders.Where(dlo => dlo.OrderNumber == null).Count();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;

            try
            {
                // Circuit is now connected — JS interop (ProtectedLocalStorage) is safe to use.
                try
                {
                    user = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value?.FullName ?? "Guest";
                }
                catch
                {
                    user = "Guest";
                }

                try
                {
                    var sidebarState = await LocalStorage.GetAsync<bool>("sidebarExpanded");
                    if (sidebarState.Success)
                    {
                        sidebarExpanded = sidebarState.Value;
                    }
                }
                catch
                {
                }

                try
                {
                    theme = (await LocalStorage.GetAsync<string>("theme")).Value;
                    if (string.IsNullOrEmpty(theme))
                    {
                        theme = "humanistic-dark";
                        await LocalStorage.SetAsync("theme", theme);
                    }
                    ThemeService.SetTheme(theme);
                    await ApplyThemeClassAsync();
                }
                catch
                {
                    theme = "humanistic-dark";
                    ThemeService.SetTheme(theme);
                    await ApplyThemeClassAsync();
                }

                StateHasChanged();
            }
            catch (JSDisconnectedException)
            {
                // Ignore expected teardown-time disconnections from JS interop.
            }
        }

        async Task SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
            try
            {
                await LocalStorage.SetAsync("sidebarExpanded", sidebarExpanded);
            }
            catch
            {
            }
        }

        async Task ChangeTheme()
        {
            theme = IsDarkTheme ? "humanistic" : "humanistic-dark";
            ThemeService.SetTheme(theme);
            try
            {
                await LocalStorage.SetAsync("theme", theme);
            }
            catch (JSDisconnectedException)
            {
                return;
            }
            await ApplyThemeClassAsync();
            StateHasChanged();
        }

        protected string MenuGroupStateClass(bool expanded) => expanded ? "menu-group-open" : "menu-group-closed";

        protected void ToggleMenuGroup(string group)
        {
            if (!sidebarExpanded)
            {
                return;
            }

            var shouldOpen = !IsMenuGroupOpen(group);

            if (!shouldOpen)
            {
                SetMenuGroupState(group, false);
                return;
            }

            coreExpanded = false;
            inventoryExpanded = false;
            lifecycleExpanded = false;
            purchasingExpanded = false;
            archivesExpanded = false;
            adminExpanded = false;

            SetMenuGroupState(group, true);
        }

        private bool IsMenuGroupOpen(string group)
        {
            return group switch
            {
                "core" => coreExpanded,
                "inventory" => inventoryExpanded,
                "lifecycle" => lifecycleExpanded,
                "purchasing" => purchasingExpanded,
                "archives" => archivesExpanded,
                "admin" => adminExpanded,
                _ => false
            };
        }

        private void SetMenuGroupState(string group, bool expanded)
        {
            switch (group)
            {
                case "core":
                    coreExpanded = expanded;
                    break;
                case "inventory":
                    inventoryExpanded = expanded;
                    break;
                case "lifecycle":
                    lifecycleExpanded = expanded;
                    break;
                case "purchasing":
                    purchasingExpanded = expanded;
                    break;
                case "archives":
                    archivesExpanded = expanded;
                    break;
                case "admin":
                    adminExpanded = expanded;
                    break;
            }
        }

        private async Task ApplyThemeClassAsync()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("applyThemeClass", theme);
            }
            catch
            {
                // Body class sync is cosmetic only; ignore transient JS interop failures.
            }
        }

        private void ForceRefresh(MouseEventArgs _)
        {
            NavigationManager.NavigateTo("delivery-order-history", forceLoad: true);
        }
    }
}