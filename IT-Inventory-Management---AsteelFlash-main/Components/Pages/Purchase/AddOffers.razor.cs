using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;


namespace ITStockM.Components.Pages.Purchase
{
    public partial class AddOffers
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }

        protected string requestTitle;

        protected bool errorVisible;

        protected Models.ITStockManagment.Offer offer;

        protected IEnumerable<Models.ITStockManagment.Request> requestsForRequestId;
        protected List<Models.ITStockManagment.Supplier> suppliersForSupplierName;

        protected override async Task OnInitializedAsync()
        {
            offer = new Models.ITStockManagment.Offer();

            var req = await ITStockManagmentService.GetRequestById(Id);

            requestTitle = req.Title;

            suppliersForSupplierName = (await ITStockManagmentService.GetSuppliers()).ToList();

        }
        
        

        protected async Task FormSubmit()
        {
            try
            {
                offer.RequestId = Id;
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
                suppliersForSupplierName = (await ITStockManagmentService.GetSuppliers()).ToList();
            }
        }
    }
}