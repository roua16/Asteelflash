using ITStockM.Services.Offers;
using ITStockM.Services.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.Purchase
{
    public partial class RequestsDetails
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; } = default!;

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        protected NotificationService NotificationService { get; set; } = default!;
        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IOfferService OfferService { get; set; } = default!;

        [Parameter]
        public int Id { get; set; }
        protected bool errorVisible;
        protected Domain.Entities.Request request = new();
        protected IEnumerable<Domain.Entities.Offer> offers = new List<Domain.Entities.Offer>();
        protected bool hasChanges = false;
        protected bool canEdit = true;

        protected override async Task OnInitializedAsync()
        {

            request = await RequestService.GetRequestById(Id) ?? new Domain.Entities.Request();

            offers = await OfferService.GetOffers(new Query
            {
                Filter = "i =>  i.RequestId == @0",
                FilterParameters = new object[] {  Id },
                Expand = "Request,Supplier"
            });


        }
       

        void OnRowRender(RowRenderEventArgs<Domain.Entities.Offer> args)
        {
            if (args.Data.Selected == true)
            {
                args.Attributes["style"] = "background-color: var(--rz-success-lighter) !important;";

            }

        }

        async Task RequestDone()
        {

            bool? result = await DialogService.Confirm(
            "Are you sure this request is Done ?",   
            "Confirmation",                               
            new ConfirmOptions { OkButtonText = "Yes", CancelButtonText = "No" } 
        );
            if (result == true)
            {
                request.Status = "Done";
                request.ApprovedAt = DateTime.Now;
                await RequestService.UpdateRequest(Id, request);
                DialogService.Close(true);
            }

        }
        protected async Task DownloadFile(Domain.Entities.Request request)
        {
            if (request.File != null && request.File.Length > 0)
            {
                
                await JSRuntime.InvokeVoidAsync("downloadFile", request.FileName, request.File);
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

        
        private BadgeStyle GetStatusStyle(string status)
        {
            return status.ToLower() switch
            {
                "urgent" => BadgeStyle.Warning,
                "normal" => BadgeStyle.Success,
                "critical" => BadgeStyle.Danger,
                _ => BadgeStyle.Secondary
            };
        }



    }
}