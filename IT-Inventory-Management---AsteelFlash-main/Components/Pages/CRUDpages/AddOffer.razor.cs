using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using ITStockM.Services;
using ITStockM.Services.Offers;
using ITStockM.Services.Requests;
using ITStockM.Services.Suppliers;


namespace ITStockM.Components.Pages.CrudPages
{
    public partial class AddOffer
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IOfferService OfferService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            offer = new Domain.Entities.Offer();

            requestsForRequestId = await RequestService.GetRequestsList();

            suppliersForSupplierName = await SupplierService.GetSuppliersList();
        }
        protected bool errorVisible;
        protected Domain.Entities.Offer offer = new();

        protected IEnumerable<Domain.Entities.Request> requestsForRequestId = new List<Domain.Entities.Request>();

        protected IEnumerable<Domain.Entities.Supplier> suppliersForSupplierName = new List<Domain.Entities.Supplier>();

        protected async Task FormSubmit()
        {
            try
            {
                await OfferService.CreateOffer(offer);
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