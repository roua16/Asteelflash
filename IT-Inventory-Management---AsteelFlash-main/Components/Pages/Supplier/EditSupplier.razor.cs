using ITStockM.Services.Suppliers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ITStockM.Components.Pages.Supplier
{
    public partial class EditSupplier
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        [Parameter]
        public string SupplierName { get; set; } = string.Empty;

        protected bool errorVisible;
        protected Models.ITStockManagment.Supplier supplier = new();

        protected override async Task OnInitializedAsync()
        {
            supplier = await SupplierService.GetSupplierBySupplierName(SupplierName) ?? new Models.ITStockManagment.Supplier();
        }
        
        protected async Task FormSubmit()
        {
            try
            {
                await SupplierService.UpdateSupplier(SupplierName, supplier);
                DialogService.Close(supplier);
            }
            catch (Exception)
            {
                errorVisible = true;
            }
        }

        protected Task CancelButtonClick(MouseEventArgs args)
        {
            DialogService.Close(null);
            return Task.CompletedTask;
        }
    }
}