
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;
using ITStockM.Services.Assignments;
using ITStockM.Services.Export;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class Assignments
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public IAssignmentService AssignmentService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Domain.Entities.Assignment> assignments = new List<Domain.Entities.Assignment>();

        protected RadzenDataGrid<Domain.Entities.Assignment> grid0 = default!;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            assignments = await AssignmentService.GetAssignments(new Query { Filter = $@"i => i.Descipriton.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AssignedEmployee,Employee,Project" });
        }
        protected override async Task OnInitializedAsync()
        {
            assignments = await AssignmentService.GetAssignments(new Query { Filter = $@"i => i.Descipriton.Contains(@0)", FilterParameters = new object[] { search }, Expand = "AssignedEmployee,Employee,Project" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAssignment>("Add Assignment", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Domain.Entities.Assignment> args)
        {
            await DialogService.OpenAsync<EditAssignment>("Edit Assignment", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Domain.Entities.Assignment assignment)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await AssignmentService.DeleteAssignment(assignment.Id);

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
                    Detail = $"Unable to delete Assignment"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/assignments", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Employee1,Employee,Project",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Assignments");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/assignments", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Employee1,Employee,Project",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Assignments");
            }
        }
    }
}