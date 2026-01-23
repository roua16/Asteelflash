
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditAssignment
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            assignment = await ITStockManagmentService.GetAssignmentById(Id);

            employeesForAssignedTo = await ITStockManagmentService.GetEmployees();

            employeesForAssignedBy = await ITStockManagmentService.GetEmployees();

            projectsForProjectId = await ITStockManagmentService.GetProjects();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Assignment assignment;

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForAssignedTo;

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForAssignedBy;

        protected IEnumerable<Models.ITStockManagment.Project> projectsForProjectId;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateAssignment(Id, assignment);
                DialogService.Close(assignment);
            }
            catch (Exception ex)
            {
                errorVisible = true;
            }
        }

        protected async Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
        }
    }
}