using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;

namespace ITStockM.Components.Pages.PendingDeliveries
{
    public partial class PendingDeliveriesDetails
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }
        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        [Parameter]
        public int Id { get; set; }

        protected bool errorVisible;
        protected Models.ITStockManagment.Request request;
        protected Models.ITStockManagment.Offer offer;

        protected bool hasChanges = false;
        protected bool canEdit = true;


        protected override async Task OnInitializedAsync()
        {

            request = await ITStockManagmentService.GetRequestById(Id);

            offer = (await ITStockManagmentService.GetOffers(new Query
            {
                Filter = "i => i.SupplierName.Contains(@0) &&  i.RequestId == @1 && i.Selected==true",
                FilterParameters = new object[] { "", Id },
                Expand = "Request,Supplier"
            })).FirstOrDefault();






        }


        protected async Task DownloadFile(ITStockM.Models.ITStockManagment.Request request)
        {
            if (request.File != null && request.File.Length > 0)
            {
                
                var fileName = $"Request_{request.Id}_File{request.FileExtension}";
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

        private int currentPage = 1;


    }
}