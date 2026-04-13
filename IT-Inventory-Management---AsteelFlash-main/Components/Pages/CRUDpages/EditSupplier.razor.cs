
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;
using ITStockM.Services.Suppliers;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class EditSupplier
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        [Parameter]
        public string SupplierName { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            supplier = await SupplierService.GetSupplierBySupplierName(SupplierName) ?? throw new InvalidOperationException($"Supplier '{SupplierName}' was not found.");
        }
        protected bool errorVisible;
        protected Domain.Entities.Supplier supplier = new();

        protected async Task FormSubmit()
        {
            try
            {
                await SupplierService.UpdateSupplier(SupplierName, supplier);
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