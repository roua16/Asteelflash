using ITStockM.Services;
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
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }



        protected IEnumerable<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels;

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0;

       

        protected string search = "";

        protected List<string> Assignments = new List<string> { "To Employees", "To IT" };

        protected string SelectedAssignment = "To Employees";


        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = (await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.OnMission == false && assm.Qte == 0).OrderByDescending(assm => assm.Assignment.Date); 


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
                await ITStockManagmentService.ExportAssignmentMaterielsToCSV(query, "Archived - Assignments");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportAssignmentMaterielsToExcel(query, "Archived - Assignments");
            }
        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}".ToLower();

            await grid0.GoToPage(0);

            assignmentMateriels = assignmentMateriels.Where(assignmentMateriels => assignmentMateriels.Materiel.MaterielName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || assignmentMateriels.Assignment.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (assignmentMateriels.Materiel.SerialNumber != null && assignmentMateriels.Materiel.SerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.AssignedEmployee.FullName != null && assignmentMateriels.Assignment.AssignedEmployee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.Project!=null && assignmentMateriels.Assignment.Project.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase)));
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


            await DialogService.OpenAsync<ArchivedMaterialsAssignmnetsDetails>("", new Dictionary<string, object> { { "AssignmentMateriel", assignmentMateriel } }, options);
        }

        protected async void AssignemntFilter()
        {
            if (SelectedAssignment == "To IT")
            {
                assignmentMateriels = (await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.AssignedTo == 6);

            }
            else
            {
                assignmentMateriels = (await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.OnMission == false && assm.Qte == 0).OrderByDescending(assm => assm.Assignment.Date);
               

            }
        }


    }
}