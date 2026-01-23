
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditDeliveryOrderMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int MaterielId { get; set; }

        [Parameter]
        public string DeliveryOrderNumber { get; set; }

        protected override async Task OnInitializedAsync()
        {
            deliveryOrderMateriel = await ITStockManagmentService.GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(MaterielId, DeliveryOrderNumber);

            materielsForMaterielId = await ITStockManagmentService.GetMateriels();

            deliveryOrdersForDeliveryOrderNumber = await ITStockManagmentService.GetDeliveryOrders();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.DeliveryOrderMateriel deliveryOrderMateriel;

        protected IEnumerable<Models.ITStockManagment.Materiel> materielsForMaterielId;

        protected IEnumerable<Models.ITStockManagment.DeliveryOrder> deliveryOrdersForDeliveryOrderNumber;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateDeliveryOrderMateriel(MaterielId, DeliveryOrderNumber, deliveryOrderMateriel);
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