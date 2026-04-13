
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Materiels;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class EditDeliveryOrderMateriel
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderMaterielService DeliveryOrderMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Parameter]
        public int MaterielId { get; set; }

        [Parameter]
        public string DeliveryOrderNumber { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            deliveryOrderMateriel = await DeliveryOrderMaterielService.GetDeliveryOrderMaterielByMaterielIdAndDeliveryOrderNumber(MaterielId, DeliveryOrderNumber) ?? new Domain.Entities.DeliveryOrderMateriel();

            materielsForMaterielId = (await MaterielService.GetMateriels()).ToList();

            deliveryOrdersForDeliveryOrderNumber = await DeliveryOrderService.GetDeliveryOrdersList();
        }
        protected bool errorVisible;
        protected Domain.Entities.DeliveryOrderMateriel deliveryOrderMateriel = new();

        protected IEnumerable<Domain.Entities.Materiel> materielsForMaterielId = new List<Domain.Entities.Materiel>();

        protected IEnumerable<Domain.Entities.DeliveryOrder> deliveryOrdersForDeliveryOrderNumber = new List<Domain.Entities.DeliveryOrder>();

        protected async Task FormSubmit()
        {
            try
            {
                await DeliveryOrderMaterielService.UpdateDeliveryOrderMateriel(MaterielId, DeliveryOrderNumber, deliveryOrderMateriel);
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