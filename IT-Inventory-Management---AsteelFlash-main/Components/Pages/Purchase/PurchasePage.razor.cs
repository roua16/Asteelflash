using ITStockM.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.Purchase
{
    public partial class PurchasePage
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Models.ITStockManagment.Request> requests;

        protected IEnumerable<Models.ITStockManagment.Offer> offers;

        protected RadzenDataGrid<Models.ITStockManagment.Request> grid0;

        protected string search = "";



        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);


            requests = requests.Where(r => r.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.MaterialType.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||  r.Status.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            
            
        }
        protected override async Task OnInitializedAsync()
        {
            requests = (await ITStockManagmentService.GetRequests(new Query
            {
                Filter = $@"i => i.Status != @0",
                FilterParameters = new object[] { "Done" },
                Expand = "Employee"
            })).OrderByDescending(r => r.Date);
            offers = await ITStockManagmentService.GetOffers();

        }

       

        protected async Task AddOfferClick(int reqId)
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

            await DialogService.OpenAsync<AddOffers>("", new Dictionary<string, object> { { "Id", reqId } }, options);
            await grid0.Reload();
        }




        protected async Task RequestDetails(int reqId)
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


            var value = await DialogService.OpenAsync<RequestsDetails>("", new Dictionary<string, object> { { "Id", reqId } }, options);

            if (value == true)
            {
                await OnInitializedAsync();
            }
        }




        protected async Task GridDeleteButtonClick(MouseEventArgs args, ITStockM.Models.ITStockManagment.Request request)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await ITStockManagmentService.DeleteRequest(request.Id);

                    if (deleteResult != null)
                    {
                        await grid0.Reload();
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = $"Error",
                    Detail = $"Unable to delete Request"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {

            var query = new Query
            {
                Filter = "r => r.Status != \"Done\"",
                Select = "Employee.FullName, Title, Type, ProjectName, Description, MaterialType, Date, Status"
            };
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportRequestsToExcel(query, "Purchase Requests");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportRequestsToExcel(query, "Purchase Requests");
            }
        }

        private BadgeStyle GetStatusStyle(string status)
        {
            return status?.ToLower() switch
            {
                "urgent" => BadgeStyle.Warning,
                "normal" => BadgeStyle.Success,
                "critical" => BadgeStyle.Danger,
                _ => BadgeStyle.Secondary
            };
        }

    }

}