using ITStockM.Services.Export;
using ITStockM.Services.Suppliers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.Supplier
{
    public partial class Suppliers
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Domain.Entities.Supplier> suppliers = new List<Domain.Entities.Supplier>();

        protected RadzenDataGrid<Domain.Entities.Supplier> grid0 = default!;

        protected string search = "";

        private async Task LoadSuppliersAsync()
        {
            var allSuppliers = await SupplierService.GetSuppliersList();

            if (string.IsNullOrWhiteSpace(search))
            {
                suppliers = allSuppliers;
                return;
            }

            suppliers = allSuppliers.Where(i =>
                (i.SupplierName != null && i.SupplierName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (i.Adress != null && i.Adress.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (i.Email != null && i.Email.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (i.PhoneNumber != null && i.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase))
            ).ToList();
        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);
            await LoadSuppliersAsync();
        }
        protected override async Task OnInitializedAsync()
        {
            await LoadSuppliersAsync();
        }

        protected async Task AddButtonClick(MouseEventArgs args)
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

            await DialogService.OpenAsync<AddSupplier>("", null,options);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Domain.Entities.Supplier> args)
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

            await DialogService.OpenAsync<EditSupplier>("", new Dictionary<string, object> { { "SupplierName", args.Data.SupplierName } },options);
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Domain.Entities.Supplier supplier)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await SupplierService.DeleteSupplier(supplier.SupplierName);

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
                await ExportService.ExportToCSV("export/itstockmanagment/suppliers", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter) ? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Suppliers");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/suppliers", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter) ? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Suppliers");
            }
        }
  


    }

}