using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.Employees;
using ITStockM.Services.Requests;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddRequest
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IEmployeeService EmployeeService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            request = new Domain.Entities.Request();

            employeesForEmployeeId = await EmployeeService.GetEmployeesList();
        }
        protected bool errorVisible;
        protected Domain.Entities.Request request = new();

        protected IEnumerable<Domain.Entities.Employee> employeesForEmployeeId = new List<Domain.Entities.Employee>();

        protected async Task FormSubmit()
        {
            try
            {
                await RequestService.CreateRequest(request);
                DialogService.Close(request);
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