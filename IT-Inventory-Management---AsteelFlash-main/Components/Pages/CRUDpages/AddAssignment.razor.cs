
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class AddAssignment
    {
        

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            assignment = new Models.ITStockManagment.Assignment();

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
                await ITStockManagmentService.CreateAssignment(assignment);
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