using ITStockM.Services.AssignmentMateriels;
using ITStockM.Services.Export;
using ITStockM.Services.Materiels;
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
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;

        [Inject]
        public IAssignmentMaterielService AssignmentMaterielService { get; set; } = default!;

        [Inject]
        public IMaterielService MaterielService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected List<Models.ITStockManagment.AssignmentMateriel> assignmentMateriels = new();

        protected RadzenDataGrid<Models.ITStockManagment.AssignmentMateriel> grid0 = default!;

        protected int stockLeft;

        protected int activeAssignments;

        protected int missions;

        protected int totalAssignments;

        protected bool dangerMission = false;

        protected string search = "";

        protected override async Task OnInitializedAsync()
        {
            assignmentMateriels = (await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " })).ToList();
            //Count missions
            missions = assignmentMateriels.Count(assm => assm.Assignment?.OnMission == true && assm.Assignment?.RestoreDate == null && assm.Qte != 0);

            //Count total assignments
            totalAssignments = assignmentMateriels.Count(assm => assm.Qte != 0 && assm.Assignment?.RestoreDate == null);

            //Check if there is a mission that will end in less than 7 days
            dangerMission = assignmentMateriels.Any(assm => assm.Assignment?.RestoreDate == null && assm.Assignment?.RestoreDateLimit < DateTime.Today.AddDays(7));

            //Specify the assignments only
            assignmentMateriels = assignmentMateriels
                .Where(assm => assm.Assignment?.OnMission == false && assm.Assignment?.AssignedTo != 6 && assm.Qte != 0)
                .OrderByDescending(assm => assm.Assignment?.Date)
                .ToList();
            activeAssignments = assignmentMateriels.Count();

            var mats = await MaterielService.GetMateriels(new Query { });

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
                await ExportService.ExportToCSV("export/itstockmanagment/assignmentmateriels", query, "Assignments");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/assignmentmateriels", query, "Assignments");
            }
        }

        protected async Task Search(ChangeEventArgs args)
        {
            search = args.Value?.ToString()?.Trim() ?? string.Empty;

            await grid0.GoToPage(0);
            var assm = (await AssignmentMaterielService.GetAssignmentMateriels(new Query { Expand = "Materiel,Assignment " }))
                .AsEnumerable()
                .Where(a => a.Assignment?.OnMission == false && a.Assignment?.AssignedTo != 6 && a.Qte != 0)
                .ToList();

            if (string.IsNullOrWhiteSpace(search))
            {
                assignmentMateriels = assm;
                return;
            }

            assignmentMateriels = assm.Where(a =>
                ContainsText(a.Materiel?.Type, search) ||
                ContainsText(a.Materiel?.MaterielName, search) ||
                ContainsText(a.Assignment?.Employee?.FullName, search) ||
                ContainsText(a.Materiel?.SerialNumber, search) ||
                ContainsText(a.Assignment?.AssignedEmployee?.FullName, search) ||
                ContainsText(a.Assignment?.Project?.ProjectName, search)).ToList();
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





        private static bool ContainsText(string? source, string query)
        {
            return !string.IsNullOrWhiteSpace(source) && source.Contains(query, StringComparison.CurrentCultureIgnoreCase);
        }


    }
}