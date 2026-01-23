
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class EditOffer
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            offer = await ITStockManagmentService.GetOfferById(Id);

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
                await ITStockManagmentService.UpdateOffer(Id, offer);
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