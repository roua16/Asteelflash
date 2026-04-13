using ITStockM.Services.Offers;
using ITStockM.Services.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.ArchivedRequests
{
    public partial class ArchivedRequestsDetails
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;
        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IOfferService OfferService { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }

        protected Domain.Entities.Request request = new();
        protected IEnumerable<Domain.Entities.Offer> offers = new List<Domain.Entities.Offer>();
        protected Domain.Entities.Offer? offer;
        protected override async Task OnInitializedAsync()
        {

            request = await RequestService.GetRequestById(Id) ?? new Domain.Entities.Request();

            offers = await OfferService.GetOffers(new Query
            {
                Filter = "i => i.SupplierName.Contains(@0) &&  i.RequestId == @1",
                FilterParameters = new object[] { "", Id },
                Expand = "Request,Supplier"
            });

            offer = (await OfferService.GetOffers(new Query
            {
                Filter = "i => i.SupplierName.Contains(@0) &&  i.RequestId == @1 && i.Selected==true",
                FilterParameters = new object[] { "", Id },
                Expand = "Request,Supplier"
            })).FirstOrDefault();





        }


       
        private BadgeStyle GetStatusStyle(string status)
        {
            return status.ToLower() switch
            {
                "urgent" => BadgeStyle.Warning,
                "done" => BadgeStyle.Success,
                "critical" => BadgeStyle.Danger,
                _ => BadgeStyle.Secondary
            };
        }

        protected async Task DownloadFile(Domain.Entities.Request request)
        {
            if (request.File != null && request.File.Length > 0)
            {
                // Generate the file name with the correct extension
                var fileName = request.FileName + request.FileExtension;
                await JSRuntime.InvokeVoidAsync("downloadFile", fileName, request.File);
            }
            else
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "No File",
                    Detail = "There is no file associated with this request."
                });
            }
        }



    }
}