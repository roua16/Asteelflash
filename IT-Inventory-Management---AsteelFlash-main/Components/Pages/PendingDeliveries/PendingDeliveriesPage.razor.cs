using ITStockM.Domain.Entities;
using ITStockM.Services.Export;
using ITStockM.Services.Requests;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.PendingDeliveries
{
    public partial class PendingDeliveriesPage
    {

        [Inject]
        protected DialogService DialogService { get; set; } = default!;

        [Inject]
        public IRequestService RequestService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Request> requests = new List<Request>();

        protected RadzenDataGrid<Request> grid0 = default!;

        protected string search = "";


        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);
            
            requests = requests.Where(r => r.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.MaterialType.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||  r.Status.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase) );
        }
        protected override async Task OnInitializedAsync()
        {
            requests = await RequestService.GetRequests(new Query
            {
                Filter = $@"i => i.Status == @0",
                FilterParameters = new object[] { "Done" },
                Expand = "Employee, Offers"
            });
            requests = requests.Where(request => request.Offers != null && request.Offers.Any(offer => offer.Selected != null && offer.Selected == true && offer.DeliveryDate > DateTime.Today));

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


            var value = await DialogService.OpenAsync<PendingDeliveriesDetails>("", new Dictionary<string, object> { { "Id", reqId } }, options);

            if (value == true)
            {
                OnInitializedAsync();
            }
        }


        protected async Task ExportClick(RadzenSplitButtonItem args)
        {

            var query = new Query
            {
                Filter = "request => request.Status == \"Done\" && request.Offers != null && request.Offers.Any(offer => offer.Selected != null && offer.Selected == true && offer.DeliveryDate >  DateTime.Parse(\""+ DateTime.Today.ToString("yyyy-MM-dd") + "\")  ) )",
                Select = "Employee.FullName, Title, Type, ProjectName, Description, MaterialType, Date, Status",
                Expand = "Employee,Offers"
            };
            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/requests", query, "Pending Deliveries");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/requests", query, "Pending Deliveries");
            }
        }


        

    }

}