using ITStockM.Services.Offers;
using ITStockM.Services.Requests;
using ITStockM.Services.Suppliers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;


namespace ITStockM.Components.Pages.Purchase
{
    public partial class AddOffers
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

        protected string requestTitle = string.Empty;

        protected bool errorVisible;

        protected Domain.Entities.Offer offer = new();

        protected IEnumerable<Domain.Entities.Request> requestsForRequestId = new List<Domain.Entities.Request>();
        protected List<Domain.Entities.Supplier> suppliersForSupplierName = new();

        protected override async Task OnInitializedAsync()
        {
            offer = new Domain.Entities.Offer();

            var req = await RequestService.GetRequestById(Id);

            requestTitle = req?.Title ?? string.Empty;

            suppliersForSupplierName = await SupplierService.GetSuppliersList();

        }
        
        

        protected async Task FormSubmit()
        {
            try
            {
                offer.RequestId = Id;
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
        protected async Task AddSupplier()
        {
            var options = new DialogOptions
            {
                Style = "min-width: 600px;", 
                CssClass = "dialog-animation",
                CloseDialogOnOverlayClick = true,
                Resizable = true,
                Draggable = true,
                CloseDialogOnEsc = true
            };

            var result = await DialogService.OpenAsync<Supplier.AddSupplier>("", null, options);
            if (result != null)
            {
                suppliersForSupplierName = await SupplierService.GetSuppliersList();
            }
        }
    }
}