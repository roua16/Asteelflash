using ITStockM.Services.Suppliers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;

namespace ITStockM.Components.Pages.Supplier
{
    public partial class AddSupplier
    {
        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        protected override Task OnInitializedAsync()
        {
            supplier = new Domain.Entities.Supplier();
            return Task.CompletedTask;
        }
        protected bool errorVisible;
        protected Domain.Entities.Supplier supplier = new();

        protected async Task FormSubmit()
        {
            try
            {
                await SupplierService.CreateSupplier(supplier);
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