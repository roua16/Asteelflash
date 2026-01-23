
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;

namespace ITStockM.Components.Pages.CRUDpages
{
    public partial class Employees
    {

        [Inject]
        public ITStockManagmentService ITStockManagmentService { get; set; }

        protected IEnumerable<Models.ITStockManagment.Employee> employees;

        protected RadzenDataGrid<Models.ITStockManagment.Employee> grid0;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            employees = await ITStockManagmentService.GetEmployees(new Query { Filter = $@"i => i.FullName.Contains(@0) || i.Email.Contains(@0) || i.Password.Contains(@0) || i.Post.Contains(@0) || i.PhoneNumber.Contains(@0) || i.Service.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            employees = await ITStockManagmentService.GetEmployees(new Query { Filter = $@"i => i.FullName.Contains(@0) || i.Email.Contains(@0) || i.Password.Contains(@0) || i.Post.Contains(@0) || i.PhoneNumber.Contains(@0) || i.Service.Contains(@0)", FilterParameters = new object[] { search } });
        }  

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ITStockManagmentService.ExportEmployeesToCSV(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Employees");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ITStockManagmentService.ExportEmployeesToExcel(new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Employees");
            }
        }
    }
}