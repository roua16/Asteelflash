
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class Suppliers
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Models.ITStockManagment.Supplier> suppliers;

        protected RadzenDataGrid<Models.ITStockManagment.Supplier> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            suppliers = await ITStockManagmentService.GetSuppliers(new Query { Filter = $@"i => i.SupplierName.Contains(@0) || i.Adress.Contains(@0) || i.Email.Contains(@0) || i.PhoneNumber.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            suppliers = await ITStockManagmentService.GetSuppliers(new Query { Filter = $@"i => i.SupplierName.Contains(@0) || i.Adress.Contains(@0) || i.Email.Contains(@0) || i.PhoneNumber.Contains(@0)", FilterParameters = new object[] { search } });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddSupplier>("Add Supplier", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Models.ITStockManagment.Supplier> args)
        {
            await DialogService.OpenAsync<EditSupplier>("Edit Supplier", new Dictionary<string, object> { {"SupplierName", args.Data.SupplierName} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Models.ITStockManagment.Supplier supplier)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await ITStockManagmentService.DeleteSupplier(supplier.SupplierName);

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
                    Detail = $"Unable to delete Supplier"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportSuppliersToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Suppliers");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportSuppliersToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Suppliers");
            }
        }
    }
}