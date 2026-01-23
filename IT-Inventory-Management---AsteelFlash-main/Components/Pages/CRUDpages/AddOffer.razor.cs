using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;


namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class AddOffer
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            offer = new Models.ITStockManagment.Offer();

            requestsForRequestId = await ITStockManagmentService.GetRequests();

            suppliersForSupplierName = await ITStockManagmentService.GetSuppliers();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Offer offer;

        protected IEnumerable<Models.ITStockManagment.Request> requestsForRequestId;

        protected IEnumerable<Models.ITStockManagment.Supplier> suppliersForSupplierName;

        protected async Task FormSubmit()
        {
            try
            {
                await ITStockManagmentService.CreateOffer(offer);
                DialogService.Close(offer);
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