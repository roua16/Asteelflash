using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using ITStockM.Models.ITStockManagment;
using ITStockM.Services;


namespace ITStockM.Components.Pages.ArchivedRequests
{
    public partial class ArchivedRequests
    {
        
        [Inject]
        protected DialogService DialogService { get; set; }

        

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Request> requests;

        protected RadzenDataGrid<Request> grid0;

        protected string search = "";



        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            requests = requests.Where(r => r.Title.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.ProjectName.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.MaterialType.Contains(search, StringComparison.CurrentCultureIgnoreCase)  || r.Status.Contains(search, StringComparison.CurrentCultureIgnoreCase) || r.Employee.FullName.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }
        protected override async Task OnInitializedAsync()
        {
            requests = await ITStockManagmentService.GetRequests(new Query
            {
                Filter = $@"i => i.Status == @0",
                FilterParameters = new object[] { "Done" },
                Expand = "Employee, Offers"
            });

            requests = requests.Where(request => request.Offers != null && request.Offers.Any(offer => offer.Selected != null && offer.Selected == true && offer.DeliveryDate < DateTime.Today)).OrderByDescending(r => r.Date);

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


            await DialogService.OpenAsync<ArchivedRequestsDetails>("", new Dictionary<string, object> { { "Id", reqId } }, options);

            
        }




        

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportArchivedRequestsToCSV(new Query
                {
                    Select = string.Join(",", new List<string> { "Employee.FullName", "Title", "Type", "ProjectName", "Description", "MaterialType", "Date", "Status" }.Select(c => c.Contains(".") ? c + " as " + c.Replace(".", "") : c)),
                }
                    );
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportArchivedRequestsToExcel(new Query
                {
                    Select = string.Join(",", new List<string> { "Employee.FullName", "Title", "Type", "ProjectName", "Description", "MaterialType", "Date", "Status" }.Select(c => c.Contains(".") ? c + " as " + c.Replace(".", "") : c)),
                }
                    );
            }
        }


    }

    

    }