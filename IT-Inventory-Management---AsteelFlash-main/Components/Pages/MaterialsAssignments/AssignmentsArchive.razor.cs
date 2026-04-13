using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Export;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class AssignmentsArchive
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IAssignmentMaterielService AssignmentMaterielService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;



        protected IEnumerable<Domain.Entities.AssignmentMateriel> assignmentMateriels = new List<Domain.Entities.AssignmentMateriel>();

        protected RadzenDataGrid<Domain.Entities.AssignmentMateriel> grid0 = default!;

       

        protected string search = "";

        protected List<string> Assignments = new List<string> { "To Employees", "To IT" };

        protected string SelectedAssignment = "To Employees";


        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = (await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.OnMission == false && assm.Qte == 0).OrderByDescending(assm => assm.Assignment.Date); 


        }

        
        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            var query = new Query
            {
                Filter = "am => am.Assignment.OnMission == false && am.Qte==0",
                Select = "Assignment.Id, Materiel.MaterielName, Assignment.Employee.FullName, Assignment.AssignedEmployee.FullName, Assignment.Project.ProjectName, Assignment.Date, Assignment.Descipriton"
            };

            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/assignmentmateriels", query, "Archived - Assignments");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/assignmentmateriels", query, "Archived - Assignments");
            }
        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}".ToLower();

            await grid0.GoToPage(0);

            assignmentMateriels = assignmentMateriels.Where(assignmentMateriels => assignmentMateriels.Materiel.MaterielName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || assignmentMateriels.Assignment.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (assignmentMateriels.Materiel.SerialNumber != null && assignmentMateriels.Materiel.SerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.AssignedEmployee.FullName != null && assignmentMateriels.Assignment.AssignedEmployee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.Project!=null && assignmentMateriels.Assignment.Project.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase)));
        }


        protected async Task AssignmentDetails(Domain.Entities.AssignmentMateriel assignmentMateriel)
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


            await DialogService.OpenAsync<ArchivedMaterialsAssignmnetsDetails>("", new Dictionary<string, object> { { "AssignmentMateriel", assignmentMateriel } }, options);
        }

        protected async Task AssignemntFilter()
        {
            if (SelectedAssignment == "To IT")
            {
                assignmentMateriels = (await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.AssignedTo == 6);

            }
            else
            {
                assignmentMateriels = (await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.OnMission == false && assm.Qte == 0).OrderByDescending(assm => assm.Assignment.Date);
               

            }
        }


    }
}