
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditSupplier
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public string SupplierName { get; set; }

        protected override async Task OnInitializedAsync()
        {
            supplier = await ITStockManagmentService.GetSupplierBySupplierName(SupplierName);
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Supplier supplier;

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