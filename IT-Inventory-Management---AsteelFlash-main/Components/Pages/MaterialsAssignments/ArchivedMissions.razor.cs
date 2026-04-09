using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Export;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class ArchivedMissions
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IAssignmentMaterielService AssignmentMaterielService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;



        protected IEnumerable<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels = new List<Models.ITStockManagment.AssignmentMateriel>();

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0 = default!;

        protected string search = "";

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment" }); 
            assignmentMateriels = assignmentMateriels.Where(assm => assm.Assignment.OnMission == true && (assm.Assignment.RestoreDate != null ||assm.Qte ==0)).OrderByDescending(assm => assm.Assignment.Date);


        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}".ToLower();

            await grid0.GoToPage(0);

            assignmentMateriels = assignmentMateriels.Where(assignmentMateriels => assignmentMateriels.Materiel.MaterielName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || assignmentMateriels.Assignment.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (assignmentMateriels.Materiel.SerialNumber != null && assignmentMateriels.Materiel.SerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.AssignedTo != null && assignmentMateriels.Assignment.AssignedEmployee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.Project != null && assignmentMateriels.Assignment.Project.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase)));
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            var query = new Query
            {
                Filter = "am => am.Assignment.OnMission == true && am.Assignment.RestoreDate != null",
                Select = "Assignment.Date, Materiel.MaterielName, Qte, Assignment.Employee.FullName, Assignment.AssignedEmployee.FullName, Assignment.Project.ProjectName, Assignment.RestoreDateLimit, Assignment.RestoreDate"
            };

            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/assignmentmateriels", query, "Missions");

            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/assignmentmateriels", query, "Missions");
            }
        }

        protected async Task AssignmentDetails(Models.ITStockManagment.AssignmentMateriel assignmentMateriel)
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


            await DialogService.OpenAsync<MaterialsAssignmentsDetails>("", new Dictionary<string, object> { { "AssignmentMateriel", assignmentMateriel } },options);
        }

        private BadgeStyle GetReturnStatusStyle(Models.ITStockManagment.AssignmentMateriel item)
        {


            return item.Assignment.RestoreDate <= item.Assignment.RestoreDateLimit
                ? BadgeStyle.Success
                : BadgeStyle.Danger;
        }

        private string GetReturnStatusText(Models.ITStockManagment.AssignmentMateriel item)
        {


            return item.Assignment.RestoreDate <= item.Assignment.RestoreDateLimit
                ? "On Time"
                : "Late";
        }
    }
}