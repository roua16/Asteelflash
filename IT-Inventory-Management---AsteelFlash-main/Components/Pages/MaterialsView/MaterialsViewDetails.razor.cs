using ITStockM.Components.Pages.CRUDpages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsView
{
    public partial class MaterialsViewDetails
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected ProtectedLocalStorage LocalStorage { get; set; }

        [Parameter]
        public List<Models.ITStockManagment.DeliveryOrderMateriel>  deliveryOrderMateriels { get; set; }
        
        [Parameter]
        public string CurrentCondition { get; set; }

        protected RadzenDataGrid<Models.ITStockManagment.DeliveryOrderMateriel> grid0;


        protected string Role;

        protected override async Task OnInitializedAsync()
        {
            Role = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value.Role;
        }

        protected async void  ChangeCondition()
        {
            var options = new DialogOptions
            {
                Style = "min-width: 600px;", 
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };
            var updated = await DialogService.OpenAsync<ConfirmConditionChange>("", new Dictionary<string, object> { { "MaxQte", deliveryOrderMateriels.FirstOrDefault().Materiel.QuantityITStock }, { "HasSN", deliveryOrderMateriels.FirstOrDefault().Materiel.SerialNumber != null },{ "materiels", deliveryOrderMateriels.Select(dlm => dlm.Materiel).Where(m => m.QuantityITStock != 0).ToList() },{ "CurrentCondition",CurrentCondition  } }, options);
            if (updated != null && updated == true)
            {
                DialogService.Close(true);
            }
        }

    }
}