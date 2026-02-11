
using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class AddDeliveryOrder
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            deliveryOrder = new Models.ITStockManagment.DeliveryOrder();

            suppliersForSupplierName = await ITStockManagmentService.GetSuppliersList();

            employeesForEmployeeId = await ITStockManagmentService.GetEmployeesList();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.DeliveryOrder deliveryOrder;

        protected IEnumerable<Models.ITStockManagment.Supplier> suppliersForSupplierName;

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForEmployeeId;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.CreateDeliveryOrder(deliveryOrder);
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