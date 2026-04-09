
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Materiels;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddDeliveryOrderMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderMaterielService DeliveryOrderMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            deliveryOrderMateriel = new ITStockM.Models.ITStockManagment.DeliveryOrderMateriel();

            materielsForMaterielId = (await MaterielService.GetMateriels()).ToList();

            deliveryOrdersForDeliveryOrderNumber = await DeliveryOrderService.GetDeliveryOrdersList();
        }
        protected bool errorVisible;
        protected ITStockM.Models.ITStockManagment.DeliveryOrderMateriel deliveryOrderMateriel = new();

        protected IEnumerable<ITStockM.Models.ITStockManagment.Materiel> materielsForMaterielId = new List<ITStockM.Models.ITStockManagment.Materiel>();

        protected IEnumerable<ITStockM.Models.ITStockManagment.DeliveryOrder> deliveryOrdersForDeliveryOrderNumber = new List<ITStockM.Models.ITStockManagment.DeliveryOrder>();

        protected async Task FormSubmit()
        {
            try
            {
                await DeliveryOrderMaterielService.CreateDeliveryOrderMateriel(deliveryOrderMateriel);
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