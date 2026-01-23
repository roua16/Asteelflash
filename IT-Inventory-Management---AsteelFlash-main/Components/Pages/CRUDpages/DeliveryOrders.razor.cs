
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class DeliveryOrders
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Models.ITStockManagment.DeliveryOrder> deliveryOrders;

        protected RadzenDataGrid<Models.ITStockManagment.DeliveryOrder> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            deliveryOrders = await ITStockManagmentService.GetDeliveryOrders(new Query { Filter = $@"i => i.DeleveryOrderNumber.Contains(@0) || i.OrderNumber.Contains(@0) || i.Descriptoin.Contains(@0) || i.SupplierName.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Supplier,Employee" });
        }
        protected override async Task OnInitializedAsync()
        {
            deliveryOrders = await ITStockManagmentService.GetDeliveryOrders(new Query { Filter = $@"i => i.DeleveryOrderNumber.Contains(@0) || i.OrderNumber.Contains(@0) || i.Descriptoin.Contains(@0) || i.SupplierName.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Supplier,Employee" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddDeliveryOrder>("Add DeliveryOrder", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<ITStockM.Models.ITStockManagment.DeliveryOrder> args)
        {
            await DialogService.OpenAsync<EditDeliveryOrder>("Edit DeliveryOrder", new Dictionary<string, object> { {"DeleveryOrderNumber", args.Data.DeleveryOrderNumber} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, ITStockM.Models.ITStockManagment.DeliveryOrder deliveryOrder)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await ITStockManagmentService.DeleteDeliveryOrder(deliveryOrder.DeleveryOrderNumber);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete DeliveryOrder"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportDeliveryOrdersToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Supplier,Employee",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "DeliveryOrders");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportDeliveryOrdersToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Supplier,Employee",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "DeliveryOrders");
            }
        }
    }
}