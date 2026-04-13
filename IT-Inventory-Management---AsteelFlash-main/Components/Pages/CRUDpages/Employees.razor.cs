
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using ITStockM.Services;
using ITStockM.Services.Employees;
using ITStockM.Services.Export;

namespace ITStockM.Components.Pages.CrudPages
{
    public partial class Employees
    {

        [Inject]
        public IEmployeeService EmployeeService { get; set; } = default!;

        [Inject]
        public IExportService ExportService { get; set; } = default!;

        protected IEnumerable<Domain.Entities.Employee> employees = new List<Domain.Entities.Employee>();

        protected RadzenDataGrid<Domain.Entities.Employee> grid0 = default!;

        protected string search = "";

        protected async Task Search(ChangeEventArgs args)
        {
            search = $"{args.Value}";

            await grid0.GoToPage(0);

            employees = await EmployeeService.GetEmployees(new Query { Filter = $@"i => i.FullName.Contains(@0) || i.Email.Contains(@0) || i.Password.Contains(@0) || i.Post.Contains(@0) || i.PhoneNumber.Contains(@0) || i.Service.Contains(@0)", FilterParameters = new object[] { search } });
        }
        protected override async Task OnInitializedAsync()
        {
            employees = await EmployeeService.GetEmployees(new Query { Filter = $@"i => i.FullName.Contains(@0) || i.Email.Contains(@0) || i.Password.Contains(@0) || i.Post.Contains(@0) || i.PhoneNumber.Contains(@0) || i.Service.Contains(@0)", FilterParameters = new object[] { search } });
        }  

        protected async Task ExportClick(RadzenSplitButtonItem args)
        {
            if (args?.Value == "csv")
            {
                await ExportService.ExportToCSV("export/itstockmanagment/employees", new Query
                {
                    Filter = $@"{(string.IsNullOrEmpty(grid0.Query.Filter)? "true" : grid0.Query.Filter)}",
                    OrderBy = $"{grid0.Query.OrderBy}",
                    Expand = "",
                    Select = string.Join(",", grid0.ColumnsCollection.Where(c => c.GetVisible() && !string.IsNullOrEmpty(c.Property)).Select(c => c.Property.Contains(".") ? c.Property + " as " + c.Property.Replace(".", "") : c.Property))
                }, "Employees");
            }

            if (args == null || args.Value == "xlsx")
            {
                await ExportService.ExportToExcel("export/itstockmanagment/employees", new Query
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