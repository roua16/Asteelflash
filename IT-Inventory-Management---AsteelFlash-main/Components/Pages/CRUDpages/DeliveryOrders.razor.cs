
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Export;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class DeliveryOrders
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Domain.Entities.DeliveryOrder> deliveryOrders = new List<Domain.Entities.DeliveryOrder>();

        protected RadzenDataGrid<Domain.Entities.DeliveryOrder> grid0 = default!;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            deliveryOrders = await DeliveryOrderService.GetDeliveryOrders(new Query { Filter = $@"i => (i.DeleveryOrderNumber ?? "").Contains(@0) || (i.OrderNumber ?? "").Contains(@0) || (i.Descriptoin ?? "").Contains(@0) || (i.SupplierName ?? "").Contains(@0)", FilterParameters = new object[] { search }, Expand = "Supplier,Employee" });
        }
        protected override async Task OnInitializedAsync()
        {
            deliveryOrders = await DeliveryOrderService.GetDeliveryOrders(new Query { Filter = $@"i => (i.DeleveryOrderNumber ?? "").Contains(@0) || (i.OrderNumber ?? "").Contains(@0) || (i.Descriptoin ?? "").Contains(@0) || (i.SupplierName ?? "").Contains(@0)", FilterParameters = new object[] { search }, Expand = "Supplier,Employee" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddDeliveryOrder>("Add DeliveryOrder", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<ITStockM.Domain.Entities.DeliveryOrder> args)
        {
            await DialogService.OpenAsync<EditDeliveryOrder>("Edit DeliveryOrder", new Dictionary<string, object> { {"DeleveryOrderNumber", args.Data.DeleveryOrderNumber} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, ITStockM.Domain.Entities.DeliveryOrder deliveryOrder)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await DeliveryOrderService.DeleteDeliveryOrder(deliveryOrder.DeleveryOrderNumber);

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
                await ExportService.ExportToCSV("export/itstockmanagment/deliveryorders", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Supplier,Employee",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "DeliveryOrders");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/deliveryorders", new Query
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