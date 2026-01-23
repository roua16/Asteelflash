using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.Supplier
{
    public partial class EditSupplier
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public string SupplierName { get; set; }

        protected bool errorVisible;
        protected Models.ITStockManagment.Supplier supplier;

        protected override async Task OnInitializedAsync()
        {
            supplier = await ITStockManagmentService.GetSupplierBySupplierName(SupplierName);
        }
        
        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.UpdateSupplier(SupplierName, supplier);
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