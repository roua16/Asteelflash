using ITStockM.Models.ITStockManagment;
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.DeleveryOrder
{
    public partial class DeliveryOrderHistoryDetails
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public Models.ITStockManagment.DeliveryOrder DeliveryOrder { get; set; }

        protected IEnumerable<DeliveryOrderMateriel> DeliveryOrderMateriels;

        protected RadzenDataGrid<DeliveryOrderMateriel> grid0;

        protected override async Task OnInitializedAsync()
        {
            DeliveryOrderMateriels = DeliveryOrder.DeliveryOrderMateriels;
           
        }

        protected async void Edit(DeliveryOrderMateriel data) { 
            await grid0.EditRow(data); 
        }
        protected async void ChangeValue(KeyboardEventArgs e, DeliveryOrderMateriel deliveryOrderMateriel)
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


                    var response = await DialogService.OpenAsync<ConfirmMaterialQuantityChange>("", new Dictionary<string, object> { { "DeliveryOrderMaterial", deliveryOrderMateriel }, { "ToDelete" , false } }, options);

                    if (response != null)
                    {

                        deliveryOrderMateriel.Materiel.QuantityPDRStock = deliveryOrderMateriel.Materiel.QuantityPDRStock - (oldQte - deliveryOrderMateriel.Qte);
                        deliveryOrderMateriel.DeliveryOrder.Descriptoin = deliveryOrderMateriel.DeliveryOrder.Descriptoin + "\n" + DateTime.Now + ": Returned " + (oldQte - deliveryOrderMateriel.Qte) + " " + deliveryOrderMateriel.Materiel.MaterielName;


                        if (response)
                        {
                            deliveryOrderMateriel.DeliveryOrder.HasDelayedM = true;

                        }
                        await ITStockManagmentService.UpdateDeliveryOrderMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.DeliveryOrderNumber, deliveryOrderMateriel);

                    }
                    else
                    {
                        deliveryOrderMateriel.Qte = oldQte;
                    }


                }

                await grid0.UpdateRow(deliveryOrderMateriel);
            }
        }

        protected async void DeleteDeliveryOrderMaterial(DeliveryOrderMateriel deliveryOrderMateriel)
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
            var response = await DialogService.OpenAsync<ConfirmMaterialQuantityChange>("", new Dictionary<string, object> { { "DeliveryOrderMaterial", deliveryOrderMateriel },{ "ToDelete", true  } }, options);
            if (response != null)
            {
                if (deliveryOrderMateriel.Qte > deliveryOrderMateriel.Materiel.QuantityPDRStock)
                {
                    NotificationService.Notify(NotificationSeverity.Warning, "You can't make this modification");
                    return;
                }
                deliveryOrderMateriel.Materiel.QuantityPDRStock = deliveryOrderMateriel.Materiel.QuantityPDRStock - deliveryOrderMateriel.Qte;

                await ITStockManagmentService.UpdateMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.Materiel);
                deliveryOrderMateriel.DeliveryOrder.Descriptoin = deliveryOrderMateriel.DeliveryOrder.Descriptoin + "\n" + DateTime.Now + ": removed "+deliveryOrderMateriel.Qte + " "+ deliveryOrderMateriel.Materiel.MaterielName;
                if (response)
                {
                    deliveryOrderMateriel.DeliveryOrder.HasDelayedM = true;
                    

                }
                await ITStockManagmentService.UpdateDeliveryOrder(deliveryOrderMateriel.DeliveryOrder.DeleveryOrderNumber, deliveryOrderMateriel.DeliveryOrder);

                await ITStockManagmentService.DeleteDeliveryOrderMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.DeliveryOrderNumber);
                await grid0.Reload();


            }
        }

        }




    }

