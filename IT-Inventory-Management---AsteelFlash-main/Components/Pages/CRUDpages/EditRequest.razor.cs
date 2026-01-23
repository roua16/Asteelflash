
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditRequest
    {


        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            request = await ITStockManagmentService.GetRequestById(Id);

            employeesForEmployeeId = await ITStockManagmentService.GetEmployees();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Request request;

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForEmployeeId;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateRequest(Id, request);
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