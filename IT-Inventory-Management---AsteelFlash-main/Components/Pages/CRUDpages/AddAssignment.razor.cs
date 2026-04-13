
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
            assignment = new Domain.Entities.Assignment();

            employeesForAssignedTo = await EmployeeService.GetEmployeesList();

            employeesForAssignedBy = await EmployeeService.GetEmployeesList();

            projectsForProjectId = await ProjectService.GetProjectsList();
        }
        protected bool errorVisible;
        protected Domain.Entities.Assignment assignment = new();

        protected IEnumerable<Domain.Entities.Employee> employeesForAssignedTo = new List<Domain.Entities.Employee>();

        protected IEnumerable<Domain.Entities.Employee> employeesForAssignedBy = new List<Domain.Entities.Employee>();

        protected IEnumerable<Domain.Entities.Project> projectsForProjectId = new List<Domain.Entities.Project>();

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