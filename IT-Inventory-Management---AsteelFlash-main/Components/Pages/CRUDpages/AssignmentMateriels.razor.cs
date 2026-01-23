
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class AssignmentMateriels
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels;

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            assignmentMateriels = await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment" });
        }
        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddAssignmentMateriel>("Add AssignmentMateriel", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<ITStockM.Models.ITStockManagment.AssignmentMateriel> args)
        {
            await DialogService.OpenAsync<EditAssignmentMateriel>("Edit AssignmentMateriel", new Dictionary<string, object> { {"MaterielId", args.Data.MaterielId}, {"AssignmentId", args.Data.AssignmentId} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, ITStockM.Models.ITStockManagment.AssignmentMateriel assignmentMateriel)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await ITStockManagmentService.DeleteAssignmentMateriel(assignmentMateriel.MaterielId, assignmentMateriel.AssignmentId);

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
                    Detail = $"Unable to delete AssignmentMateriel"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportAssignmentMaterielsToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Materiel,Assignment",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "AssignmentMateriels");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportAssignmentMaterielsToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Materiel,Assignment",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "AssignmentMateriels");
            }
        }
    }
}