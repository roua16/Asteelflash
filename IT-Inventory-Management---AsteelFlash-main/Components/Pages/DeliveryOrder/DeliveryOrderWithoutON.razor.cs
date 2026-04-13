using ITStockM.Domain.Entities;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Materiels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.DeliveryOrder
{
    public partial class DeliveryOrderWithoutON
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderMaterielService DeliveryOrderMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Parameter]
        public Domain.Entities.DeliveryOrder DeliveryOrder { get; set; } = new();

        protected IEnumerable<DeliveryOrderMateriel> DeliveryOrderMateriels = new List<DeliveryOrderMateriel>();

        protected RadzenDataGrid<DeliveryOrderMateriel> grid0 = default!;

        private bool showValidationError = false;

        private async Task ValidateAndUpdateDeliveryOrder()
        {
            showValidationError = true;

            if (!string.IsNullOrEmpty(DeliveryOrder.OrderNumber))
            {
                await updateDeliveryOrder();
            }
            else
            {

            }
        }
        protected async Task updateDeliveryOrder()
        {

            if (await DialogService.Confirm("Confirm OrderNumber ?") == true)
            {
                await DeliveryOrderService.UpdateDeliveryOrder(DeliveryOrder.DeleveryOrderNumber, DeliveryOrder);
                DialogService.Close(null);
            }

        }

        protected override async Task OnInitializedAsync()
        {
            DeliveryOrderMateriels = DeliveryOrder.DeliveryOrderMateriels;
        }


        protected async Task ChangeValue(KeyboardEventArgs e, Domain.Entities.DeliveryOrderMateriel deliveryOrderMateriel)
        {
            if (e.Key == "Enter")
            {
                int oldQte = deliveryOrderMateriel.Qte;

                await Task.Delay(100);

                if (deliveryOrderMateriel.Qte >= oldQte)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "You can't make this modification");

                }
                else if (deliveryOrderMateriel.Materiel.QuantityPDRStock <= deliveryOrderMateriel.Qte)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "Material(s) are out of stock");
                }
                else
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


                    var response = await DialogService.OpenAsync<ConfirmMaterialQuantityChange>("", new Dictionary<string, object> { { "DeliveryOrderMaterial", deliveryOrderMateriel }, { "ToDelete", false } }, options);

                    if (response != null)
                    {

                        deliveryOrderMateriel.Materiel.QuantityPDRStock = deliveryOrderMateriel.Materiel.QuantityPDRStock - (oldQte - deliveryOrderMateriel.Qte);
                        deliveryOrderMateriel.DeliveryOrder.Descriptoin = deliveryOrderMateriel.DeliveryOrder.Descriptoin + "\n" + DateTime.Now + ": Returned " + (oldQte - deliveryOrderMateriel.Qte) + " " + deliveryOrderMateriel.Materiel.MaterielName;


                        if (response)
                        {
                            deliveryOrderMateriel.DeliveryOrder.HasDelayedM = true;

                        }
                        await DeliveryOrderMaterielService.UpdateDeliveryOrderMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.DeliveryOrderNumber, deliveryOrderMateriel);

                    }
                    else
                    {
                        deliveryOrderMateriel.Qte = oldQte;
                    }


                }

                await grid0.UpdateRow(deliveryOrderMateriel);
            }
        }

        protected async Task DeleteDeliveryOrderMaterial(DeliveryOrderMateriel deliveryOrderMateriel)
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
            var response = await DialogService.OpenAsync<ConfirmMaterialQuantityChange>("", new Dictionary<string, object> { { "DeliveryOrderMaterial", deliveryOrderMateriel }, { "ToDelete", true } }, options);
            if (response != null)
            {
                if (deliveryOrderMateriel.Qte > deliveryOrderMateriel.Materiel.QuantityPDRStock)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "You can't make this modification");
                    return;
                }
                deliveryOrderMateriel.Materiel.QuantityPDRStock = deliveryOrderMateriel.Materiel.QuantityPDRStock - deliveryOrderMateriel.Qte;

                await MaterielService.UpdateMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.Materiel);
                deliveryOrderMateriel.DeliveryOrder.Descriptoin = deliveryOrderMateriel.DeliveryOrder.Descriptoin + "\n" + DateTime.Now + ": removed " + deliveryOrderMateriel.Qte + " " + deliveryOrderMateriel.Materiel.MaterielName;
                if (response)
                {
                    deliveryOrderMateriel.DeliveryOrder.HasDelayedM = true;


                }
                await DeliveryOrderService.UpdateDeliveryOrder(deliveryOrderMateriel.DeliveryOrder.DeleveryOrderNumber, deliveryOrderMateriel.DeliveryOrder);

                await DeliveryOrderMaterielService.DeleteDeliveryOrderMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.DeliveryOrderNumber);
                await grid0.Reload();


            }
        }


    }
}
