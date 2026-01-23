
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class Offers
    {

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Models.ITStockManagment.Offer> offers;

        protected RadzenDataGrid<Models.ITStockManagment.Offer> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            offers = await ITStockManagmentService.GetOffers(new Query { Filter = $@"i => i.SupplierName.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Request,Supplier" });
        }
        protected override async Task OnInitializedAsync()
        {
            offers = await ITStockManagmentService.GetOffers(new Query { Filter = $@"i => i.SupplierName.Contains(@0)", FilterParameters = new object[] { search }, Expand = "Request,Supplier" });
        }

        protected async Task AddButtonClick(MouseEventArgs args)
        {
            await DialogService.OpenAsync<AddOffer>("Add Offer", null);
            await grid0.Reload();
        }

        protected async Task EditRow(DataGridRowMouseEventArgs<Models.ITStockManagment.Offer> args)
        {
            await DialogService.OpenAsync<EditOffer>("Edit Offer", new Dictionary<string, object> { {"Id", args.Data.Id} });
        }

        protected async Task GridDeleteButtonClick(MouseEventArgs args, Models.ITStockManagment.Offer offer)
        {
            try
            {
                if (await DialogService.Confirm("Are you sure you want to delete this record?") == true)
                {
                    var deleteResult = await ITStockManagmentService.DeleteOffer(offer.Id);

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
                    Detail = $"Unable to delete Offer"
                });
            }
        }

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportOffersToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Request,Supplier",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Offers");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportOffersToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "Request,Supplier",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Offers");
            }
        }
    }
}