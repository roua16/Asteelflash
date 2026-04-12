using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Export;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.DeliveryOrder
{
    public partial class DeliveryOrderHistory
    {

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;
       



        protected IEnumerable<Models.ITStockManagment.DeliveryOrder> DeliveryOrders = new List<Models.ITStockManagment.DeliveryOrder>();
        private List<Models.ITStockManagment.DeliveryOrder> allDeliveryOrders = new();
        protected bool isLoading = true;
        protected string? loadError;

        protected RadzenDataGrid<Models.ITStockManagment.DeliveryOrder> grid0 = default!;

        

        protected string search = "";

        protected override async Task OnInitializedAsync()
        {
            isLoading = true;
            loadError = null;

            try
            {
                allDeliveryOrders = (await DeliveryOrderService.GetDeliveryOrdersList(new Query
                {
                    Expand = "DeliveryOrderMateriels,DeliveryOrderMateriels.Materiel,Employee,Supplier"
                }))
                .OrderByDescending(dlo => dlo.Date)
                .ToList();

                DeliveryOrders = allDeliveryOrders;
            }
            catch (Exception ex)
            {
                loadError = "Failed to load delivery order history.";
                Console.Error.WriteLine($"DeliveryOrderHistory load error: {ex}");
                DeliveryOrders = new List<Models.ITStockManagment.DeliveryOrder>();
            }
            finally
            {
                isLoading = false;
            }


        }

        
        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            var query = new Query
            {
               
                Select = "DeliveryOrder.Employee.FullName, Materiel.MaterielName, Materiel.Type , DeliveryOrder.DeleveryOrderNumber , Qte, DeliveryOrder.Date, DeliveryOrder.DeliveryDate, DeliveryOrder.SupplierName, DeliveryOrder.Descriptoin"
            };

            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/deliveryordermateriels", query, "DeliveryOrdersHistory");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/deliveryordermateriels", query, "DeliveryOrderHistory");
            }
        }

        protected async Task Search(ChangeEventArgs args) 
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            if (string.IsNullOrWhiteSpace(search))
            {
                DeliveryOrders = allDeliveryOrders;
                return;
            }

            DeliveryOrders = allDeliveryOrders.Where(delo =>
                delo.DeliveryOrderMateriels != null &&
                delo.DeliveryOrderMateriels.Any(dlom =>
                    (dlom.Materiel?.MaterielName?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (dlom.Materiel?.SerialNumber?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (dlom.DeliveryOrder?.DeleveryOrderNumber?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false) ||
                    (dlom.DeliveryOrder?.SupplierName?.Contains(search, StringComparison.CurrentCultureIgnoreCase) ?? false)));
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
        protected async Task AddDelayedMaterial()
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
            await DialogService.OpenAsync<AddDelayedMaterials>("", null, options);
            NavigationManager.NavigateTo("delivery-order-history", forceLoad: true);
        }





    }
}