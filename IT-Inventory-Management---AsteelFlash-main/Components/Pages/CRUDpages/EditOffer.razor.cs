
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
    public partial class EditOffer
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IOfferService OfferService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public ISupplierService SupplierService { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        protected override async Task OnInitializedAsync()
        {
            offer = await OfferService.GetOfferById(Id) ?? throw new InvalidOperationException($"Offer with id {Id} was not found.");

            requestsForRequestId = await RequestService.GetRequestsList();

            suppliersForSupplierName = await SupplierService.GetSuppliersList();
        }
        protected bool errorVisible;
        protected Models.ITStockManagment.Offer offer = new();

        protected IEnumerable<Models.ITStockManagment.Request> requestsForRequestId = new List<Models.ITStockManagment.Request>();

        protected IEnumerable<Models.ITStockManagment.Supplier> suppliersForSupplierName = new List<Models.ITStockManagment.Supplier>();

        protected async Task FormSubmit()
        {
            try
            {
                await OfferService.UpdateOffer(Id, offer);
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