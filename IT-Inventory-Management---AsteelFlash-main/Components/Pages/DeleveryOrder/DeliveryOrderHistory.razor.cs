using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.DeleveryOrder
{
    public partial class DeliveryOrderHistory
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }
       



        protected IEnumerable<Models.ITStockManagment.DeliveryOrder> DeliveryOrders;

        protected RadzenDataGrid<Models.ITStockManagment.DeliveryOrder> grid0;

        

        protected string search = "";

        protected override async Task OnInitializedAsync()
        {

            DeliveryOrders = (await ITStockManagmentService.GetDeliveryOrders()).OrderByDescending(dlo => dlo.Date);


        }

        
        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            var query = new Query
            {
               
                Select = "DeliveryOrder.Employee.FullName, Materiel.MaterielName, Materiel.Type , DeliveryOrder.DeleveryOrderNumber , Qte, DeliveryOrder.Date, DeliveryOrder.DeliveryDate, DeliveryOrder.SupplierName, DeliveryOrder.Descriptoin"
            };

            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportDeliveryOrderMaterielsToCSV(query, "DeliveryOrdersHistory");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportDeliveryOrderMaterielsToExcel(query, "DeliveryOrderHistory");
            }
        }

        protected async Task Search(ChangeEventArgs args) 
        {
            search = $"{args.Value}".ToLower();

            await grid0.GoToPage(0);

            DeliveryOrders = DeliveryOrders.Where(delo => delo.DeliveryOrderMateriels.Any(dlom => dlom.Materiel.MaterielName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (dlom.Materiel.SerialNumber != null && dlom.Materiel.SerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || dlom.DeliveryOrder.DeleveryOrderNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase) || dlom.DeliveryOrder.SupplierName.Contains(search, StringComparison.CurrentCultureIgnoreCase))  );
        }




        protected async Task DeliveryOrderDetails(Models.ITStockManagment.DeliveryOrder deliveryOrder) 
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


            await DialogService.OpenAsync<DeliveryOrderHistoryDetails>("", new Dictionary<string, object> { { "DeliveryOrder", deliveryOrder } }, options);
        }
        
        protected async Task DeliveryOrderDetailsWithoutON(Models.ITStockManagment.DeliveryOrder deliveryOrder) 
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


            await DialogService.OpenAsync<DeliveryOrderWithoutON>("", new Dictionary<string, object> { { "DeliveryOrder", deliveryOrder } }, options);



            NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
        
        }
        protected async void AdddDelayedMaterial()
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
            await DialogService.OpenAsync<AdddDelayedMaterials>("", null, options);
            NavigationManager.NavigateTo("delivery-order-history", forceLoad: true);
        }





    }
}