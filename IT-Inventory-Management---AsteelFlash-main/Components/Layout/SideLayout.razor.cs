using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services;


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

                await AppThemeService.InitializeAsync();

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

        public void Dispose()
        {
            AppThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}
