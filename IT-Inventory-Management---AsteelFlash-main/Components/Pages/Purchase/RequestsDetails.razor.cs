using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.Purchase
{
    public partial class RequestsDetails
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }
        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }
        protected bool errorVisible;
        protected Models.ITStockManagment.Request request;
        protected IEnumerable<Models.ITStockManagment.Offer> offers;
        protected bool hasChanges = false;
        protected bool canEdit = true;

        protected override async Task OnInitializedAsync()
        {

            request = await ITStockManagmentService.GetRequestById(Id);

            offers = await ITStockManagmentService.GetOffers(new Query
            {
                Filter = "i =>  i.RequestId == @0",
                FilterParameters = new object[] {  Id },
                Expand = "Request,Supplier"
            });


        }
       

        void OnRowRender(RowRenderEventArgs<Models.ITStockManagment.Offer> args)
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
                await ITStockManagmentService.UpdateRequest(Id, request);
                DialogService.Close(true);
            }

        }
        protected async Task DownloadFile(Models.ITStockManagment.Request request)
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