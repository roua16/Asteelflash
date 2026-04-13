
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services.DeliveryOrderMateriels;
using ITStockM.Services.Export;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class DeliveryOrderMateriels
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderMaterielService DeliveryOrderMaterielService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Domain.Entities.DeliveryOrderMateriel> deliveryOrderMateriels = new List<Domain.Entities.DeliveryOrderMateriel>();

        protected RadzenDataGrid<Domain.Entities.DeliveryOrderMateriel> grid0 = default!;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            deliveryOrderMateriels = await DeliveryOrderMaterielService.GetDeliveryOrderMateriels(new Query { Filter = $@"i => i.DeliveryOrderNumber.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Materiel,DeliveryOrder" });
        }
        protected override async Task OnInitializedAsync()
        {
            deliveryOrderMateriels = await DeliveryOrderMaterielService.GetDeliveryOrderMateriels(new Query { Filter = $@"i => i.DeliveryOrderNumber.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Materiel,DeliveryOrder" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddDeliveryOrderMateriel>("Add DeliveryOrderMateriel", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<ITStockM.Domain.Entities.DeliveryOrderMateriel> args)
        {
            await DialogService.OpenAsync<EditDeliveryOrderMateriel>("Edit DeliveryOrderMateriel", new Dictionary<string, object> { {"MaterielId", args.Data.MaterielId}, {"DeliveryOrderNumber", args.Data.DeliveryOrderNumber} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, ITStockM.Domain.Entities.DeliveryOrderMateriel deliveryOrderMateriel)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await DeliveryOrderMaterielService.DeleteDeliveryOrderMateriel(deliveryOrderMateriel.MaterielId, deliveryOrderMateriel.DeliveryOrderNumber);

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
                    Detail = $"Unable to delete DeliveryOrderMateriel"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/deliveryordermateriels", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Materiel,DeliveryOrder",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "DeliveryOrderMateriels");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/deliveryordermateriels", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Materiel,DeliveryOrder",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "DeliveryOrderMateriels");
            }
        }
    }
}