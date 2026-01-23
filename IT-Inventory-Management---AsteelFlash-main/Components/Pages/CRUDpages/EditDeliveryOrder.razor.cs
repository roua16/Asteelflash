
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditDeliveryOrder
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public string DeleveryOrderNumber { get; set; }

        protected override async Task OnInitializedAsync()
        {
            deliveryOrder = await ITStockManagmentService.GetDeliveryOrderByDeleveryOrderNumber(DeleveryOrderNumber);

            suppliersForSupplierName = await ITStockManagmentService.GetSuppliers();

            employeesForEmployeeId = await ITStockManagmentService.GetEmployees();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.DeliveryOrder deliveryOrder;

        protected IEnumerable<Models.ITStockManagment.Supplier> suppliersForSupplierName;

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForEmployeeId;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateDeliveryOrder(DeleveryOrderNumber, deliveryOrder);
                DialogService.Close(deliveryOrder);
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