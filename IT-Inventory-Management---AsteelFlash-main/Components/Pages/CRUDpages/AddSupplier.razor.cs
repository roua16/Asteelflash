
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;
using ITStockM.Services.Suppliers;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddSupplier
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            supplier = new ITStockM.Models.ITStockManagment.Supplier();
        }
        protected bool errorVisible;
        protected ITStockM.Models.ITStockManagment.Supplier supplier = new();

        protected async Task FormSubmit()
        {
            try
            {
                await SupplierService.CreateSupplier(supplier);
                DialogService.Close(supplier);
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