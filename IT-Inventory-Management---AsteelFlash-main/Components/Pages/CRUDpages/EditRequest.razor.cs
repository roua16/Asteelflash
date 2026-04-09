
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.Employees;
using ITStockM.Services.Requests;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class EditRequest
    {


        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IEmployeeService EmployeeService { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            request = await RequestService.GetRequestById(Id) ?? new Models.ITStockManagment.Request();

            employeesForEmployeeId = await EmployeeService.GetEmployeesList();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Request request = new();

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForEmployeeId = new List<Models.ITStockManagment.Employee>();

        protected async Task FormSubmit()
        {
            try
            {
                await RequestService.UpdateRequest(Id, request);
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