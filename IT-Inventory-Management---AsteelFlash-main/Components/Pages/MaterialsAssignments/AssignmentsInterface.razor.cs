using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.MaterialsAssignments
{
    public partial class AssignmentsInterface
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }
        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected List<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels;

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0;

        protected int stockLeft;

        protected int activeAssignments;

        protected int missions;

        protected int totalAssignments;

        protected bool dangerMission = false;

        protected string search = "";

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = (await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).ToList();
            //Count missions
            missions = assignmentMateriels.Where(assm => assm.Assignment.OnMission == true && assm.Assignment.RestoreDate == null && assm.Qte != 0).Count();

            //Count total assignments
            totalAssignments = assignmentMateriels.Where(assm => assm.Qte != 0 && assm.Assignment.RestoreDate == null ).Count();

            //Check if there is a mission that will end in less than 7 days
            dangerMission = 0 != assignmentMateriels.Where(assm => assm.Assignment.RestoreDate == null && assm.Assignment.RestoreDateLimit < DateTime.Today.AddDays(7)).Count();

            //Specify the assignments only
            assignmentMateriels = assignmentMateriels.Where(assm => assm.Assignment.OnMission == false &&  assm.Assignment.AssignedTo != 6 && assm.Qte != 0).OrderByDescending(assm => assm.Assignment.Date).ToList();
            activeAssignments = assignmentMateriels.Count();

            var mats = await ITStockManagmentService.GetMateriels(new Query { });

            stockLeft = mats.Where(m => m.QuantityITStock != 0 || m.QuantityPDRStock != 0).Sum(m => m.QuantityPDRStock + m.QuantityITStock);

            



        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            var options = new DialogOptions
            {
                Style = "min-width: 800px;", 
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };


            var result = await DialogService.OpenAsync<AddMaterialsAssignments>("", null, options);
            if (result != null)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "Assignment added successfully",
                    Duration = 4000
                });
            }
            await OnInitializedAsync();
            StateHasChanged();



        }

        
        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            var query = new Query
            {
                Filter = "assm => assm.Assignment.OnMission == false && assm.Assignment.RestoreDate == null && assm.Assignment.AssignedTo != 6",
                Select = "Materiel.MaterielName, Qte, Assignment.Employee.FullName, Assignment.AssignedEmployee.FullName, Assignment.Project.ProjectName, Assignment.Date"
            };

            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportAssignmentMaterielsToCSV(query, "Assignments");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportAssignmentMaterielsToExcel(query, "Assignments");
            }
        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}".ToLower();

            await grid0.GoToPage(0);
            var assm = (await ITStockManagmentService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).Where(assm => assm.Assignment.OnMission == false && assm.Assignment.AssignedTo != 6 && assm.Qte != 0).ToList();
            assignmentMateriels = assm.Where(assignmentMateriels => assignmentMateriels.Assignment.AssignedTo != 6 && (assignmentMateriels.Materiel.Type.Contains(search, StringComparison.CurrentCultureIgnoreCase) || assignmentMateriels.Materiel.MaterielName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || assignmentMateriels.Assignment.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || (assignmentMateriels.Materiel.SerialNumber != null && assignmentMateriels.Materiel.SerialNumber.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.AssignedEmployee != null && assignmentMateriels.Assignment.AssignedEmployee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase)) || (assignmentMateriels.Assignment.Project!=null && assignmentMateriels.Assignment.Project.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase)))).ToList();
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


            bool? isReturned = await DialogService.OpenAsync<MaterialsAssignmentsDetails>("", new Dictionary<string, object> { { "AssignmentMateriel", assignmentMateriel } }, options);
            if (isReturned==true)
            {
                assignmentMateriels.Remove(assignmentMateriel);
                
               
                await grid0.Reload();
            }
           
            
        }

        protected async Task AssignmentDetails()
        {
            var options = new DialogOptions
            {
                Style = "min-width: 600px;", 
                CssClass = "dialog-animation",
                Width = "900px",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };


            await DialogService.OpenAsync<MissionInterface>("", null,options);
            await OnInitializedAsync();
            StateHasChanged();
        }






    }
}