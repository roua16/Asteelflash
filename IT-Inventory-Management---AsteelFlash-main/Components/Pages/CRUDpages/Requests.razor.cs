
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;
using ITStockM.Services.Export;
using ITStockM.Services.Requests;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class Requests
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Models.ITStockManagment.Request> requests = new List<Models.ITStockManagment.Request>();

        protected RadzenDataGrid<Models.ITStockManagment.Request> grid0 = default!;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            requests = await RequestService.GetRequests(new Query { Filter = $@"i => i.Title.Contains(@0) || i.ProjectName.Contains(@0) || i.Description.Contains(@0) || i.MaterialType.Contains(@0) || i.Status.Contains(@0) || i.FileExtension.Contains(@0) || i.FileName.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Employee" });
        }
        protected override async Task OnInitializedAsync()
        {
            requests = await RequestService.GetRequests(new Query { Filter = $@"i => i.Title.Contains(@0) ||  i.ProjectName.Contains(@0) || i.Description.Contains(@0) || i.MaterialType.Contains(@0) || i.Status.Contains(@0) || i.FileExtension.Contains(@0) || i.FileName.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Employee" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddRequest>("Add Request", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Models.ITStockManagment.Request> args)
        {
            await DialogService.OpenAsync<EditRequest>("Edit Request", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Models.ITStockManagment.Request request)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await RequestService.DeleteRequest(request.Id);

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
                    Detail = $"Unable to delete Request"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/requests", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Employee",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Requests");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/requests", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Employee",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Requests");
            }
        }
    }
}