using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;
using ITStockM.Services.DeliveryOrders;
using ITStockM.WebApi.Services;


namespace ITStockM.Components.Layout
{
    public partial class SideLayout : IDisposable
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public AppThemeService AppThemeService { get; set; } = default!;

        [Inject]
        public ILogger<SideLayout> Logger { get; set; } = default!;

        bool sidebarExpanded = true;

        protected string user = "Guest";

        protected int orderNumbersNF = 0;

        protected bool coreExpanded = true;

        protected bool inventoryExpanded = true;

        protected bool lifecycleExpanded = true;

        protected bool purchasingExpanded = true;

        protected bool archivesExpanded = false;

        protected bool adminExpanded = false;

        protected bool IsDarkTheme => AppThemeService.IsDark;

        protected override async Task OnInitializedAsync()
        {
            AppThemeService.ThemeChanged += OnThemeChanged;

            // NOTE: Do NOT call ProtectedLocalStorage here — it uses JS interop which is
            // unavailable during prerendering. All storage reads are deferred to OnAfterRenderAsync.
            var deliveryOrders = await DeliveryOrderService.GetDeliveryOrdersList();
            orderNumbersNF = deliveryOrders.Where(dlo => dlo.OrderNumber == null).Count();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
            {
                return;
            }

            user = await LoadUserNameAsync();

            var persistedSidebarState = await LoadSidebarStateAsync();
            if (persistedSidebarState.HasValue)
            {
                sidebarExpanded = persistedSidebarState.Value;
            }

            await AppThemeService.InitializeAsync();
            StateHasChanged();
        }

        async Task SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
            await PersistSidebarStateAsync(sidebarExpanded);
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

        private void OnThemeChanged()
        {
            _ = InvokeAsync(StateHasChanged);
        }

        private void ForceRefresh(MouseEventArgs _)
        {
            NavigationManager.NavigateTo("delivery-order-history", forceLoad: true);
        }

        private async Task<string> LoadUserNameAsync()
        {
            try
            {
                return (await LocalStorage.GetAsync<UserSession>("UserSession")).Value?.FullName ?? "Guest";
            }
            catch (JSDisconnectedException)
            {
                Logger.LogDebug("JS disconnected while loading user display name from local storage.");
                return "Guest";
            }
            catch (InvalidOperationException ex) when (IsPrerenderingInteropException(ex))
            {
                Logger.LogDebug(ex, "User display name unavailable during prerender.");
                return "Guest";
            }
        }

        private async Task<bool?> LoadSidebarStateAsync()
        {
            try
            {
                var sidebarState = await LocalStorage.GetAsync<bool>("sidebarExpanded");
                return sidebarState.Success ? sidebarState.Value : null;
            }
            catch (JSDisconnectedException)
            {
                Logger.LogDebug("JS disconnected while loading sidebar state from local storage.");
                return null;
            }
            catch (InvalidOperationException ex) when (IsPrerenderingInteropException(ex))
            {
                Logger.LogDebug(ex, "Sidebar state unavailable during prerender.");
                return null;
            }
        }

        private async Task PersistSidebarStateAsync(bool expanded)
        {
            try
            {
                await LocalStorage.SetAsync("sidebarExpanded", expanded);
            }
            catch (JSDisconnectedException)
            {
                Logger.LogDebug("JS disconnected while persisting sidebar state.");
            }
            catch (InvalidOperationException ex) when (IsPrerenderingInteropException(ex))
            {
                Logger.LogDebug(ex, "Sidebar state persistence skipped during prerender.");
            }
        }

        private static bool IsPrerenderingInteropException(InvalidOperationException exception)
        {
            return exception.Message.Contains("prerender", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("JavaScript interop calls cannot be issued", StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            AppThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}
