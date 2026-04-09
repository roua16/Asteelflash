
using ITStockM.Services.Assignments;
using ITStockM.Services.Employees;
using ITStockM.Services.Projects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddAssignment
    {
        

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IAssignmentService AssignmentService { get; set; } = default!;

        [Inject]
        public IEmployeeService EmployeeService { get; set; } = default!;

        [Inject]
        public IProjectService ProjectService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            assignment = new Models.ITStockManagment.Assignment();

            employeesForAssignedTo = await EmployeeService.GetEmployeesList();

            employeesForAssignedBy = await EmployeeService.GetEmployeesList();

            projectsForProjectId = await ProjectService.GetProjectsList();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Assignment assignment = new();

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForAssignedTo = new List<Models.ITStockManagment.Employee>();

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForAssignedBy = new List<Models.ITStockManagment.Employee>();

        protected IEnumerable<Models.ITStockManagment.Project> projectsForProjectId = new List<Models.ITStockManagment.Project>();

        protected async Task FormSubmit()
        {
            try
            {
                await AssignmentService.CreateAssignment(assignment);
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