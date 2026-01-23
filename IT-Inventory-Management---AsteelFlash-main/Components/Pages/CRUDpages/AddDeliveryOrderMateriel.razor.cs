
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class AddDeliveryOrderMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            deliveryOrderMateriel = new ITStockM.Models.ITStockManagment.DeliveryOrderMateriel();

            materielsForMaterielId = await ITStockManagmentService.GetMateriels();

            deliveryOrdersForDeliveryOrderNumber = await ITStockManagmentService.GetDeliveryOrders();
        }
        protected bool errorVisible;
        protected ITStockM.Models.ITStockManagment.DeliveryOrderMateriel deliveryOrderMateriel;

        protected IEnumerable<ITStockM.Models.ITStockManagment.Materiel> materielsForMaterielId;

        protected IEnumerable<ITStockM.Models.ITStockManagment.DeliveryOrder> deliveryOrdersForDeliveryOrderNumber;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.CreateDeliveryOrderMateriel(deliveryOrderMateriel);
                DialogService.Close(deliveryOrderMateriel);
            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
    }
}