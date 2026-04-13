
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Employees;
using ITStockM.Services.Suppliers;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class EditDeliveryOrder
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IDeliveryOrderService DeliveryOrderService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        [Inject]
        public IEmployeeService EmployeeService { get; set; } = default!;

        [Parameter]
        public string DeleveryOrderNumber { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            deliveryOrder = await DeliveryOrderService.GetDeliveryOrderByNumber(DeleveryOrderNumber) ?? new Domain.Entities.DeliveryOrder();

            suppliersForSupplierName = await SupplierService.GetSuppliersList();

            employeesForEmployeeId = await EmployeeService.GetEmployeesList();
        }
        protected bool errorVisible;
        protected Domain.Entities.DeliveryOrder deliveryOrder = new();

        protected IEnumerable<Domain.Entities.Supplier> suppliersForSupplierName = new List<Domain.Entities.Supplier>();

        protected IEnumerable<Domain.Entities.Employee> employeesForEmployeeId = new List<Domain.Entities.Employee>();

        protected async Task FormSubmit()
        {
            try
            {
                await DeliveryOrderService.UpdateDeliveryOrder(DeleveryOrderNumber, deliveryOrder);
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