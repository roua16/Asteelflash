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

        protected string user ;

        protected int orderNumbersNF = 0;

        string theme ;
        protected override async Task OnInitializedAsync()
        {
            user = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.FullName;
             orderNumbersNF = (await ITStockManagmentService.GetDeliveryOrders()).Where(dlo => dlo.OrderNumber == null).Count();

              theme = (await LocalStorage.GetAsync<string>("theme")).Value;
            if (theme != null)
            {
                ThemeService.SetTheme(theme);
                
            }
            else
            {
               await LocalStorage.SetAsync("theme", "humanistic");
                ThemeService.SetTheme("humanistic");
            }
                
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