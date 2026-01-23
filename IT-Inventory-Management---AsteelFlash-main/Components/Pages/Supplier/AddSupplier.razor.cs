using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.Supplier
{
    public partial class AddSupplier
    {
        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            supplier = new Models.ITStockManagment.Supplier();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Supplier supplier;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.CreateSupplier(supplier);
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