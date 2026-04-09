
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Employees;
using ITStockM.Services.Suppliers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddDeliveryOrder
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        [Inject]
        public IEmployeeService EmployeeService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            deliveryOrder = new Models.ITStockManagment.DeliveryOrder();

            suppliersForSupplierName = await SupplierService.GetSuppliersList();

            employeesForEmployeeId = await EmployeeService.GetEmployeesList();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.DeliveryOrder deliveryOrder = new();

        protected IEnumerable<Models.ITStockManagment.Supplier> suppliersForSupplierName = new List<Models.ITStockManagment.Supplier>();

        protected IEnumerable<Models.ITStockManagment.Employee> employeesForEmployeeId = new List<Models.ITStockManagment.Employee>();

        protected async Task FormSubmit()
        {
            try
            {
                await DeliveryOrderService.CreateDeliveryOrder(deliveryOrder);
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