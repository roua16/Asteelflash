using ITStockM.Models.ITStockManagment;
using ITStockM.Services;
using ITStockM.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.Maintenance
{
    public partial class MaintenancePage
    {
        [Inject] protected IMaintenanceService MaintenanceService { get; set; }
        [Inject] protected ITStockManagmentService ITStockManagmentService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected ProtectedLocalStorage LocalStorage { get; set; }

        protected List<MaintenanceTicket> tickets = [];
        protected RadzenDataGrid<MaintenanceTicket> grid;

        protected int openCount, inProgressCount, closedCount;
        protected string selectedStatus;
        protected string search = "";
        protected string userRole = "";

        protected List<string> statusOptions = ["Open", "InProgress", "Closed"];

        // New ticket dialog
        protected bool showNewDialog;
        protected int newMaterielId;
        protected string newDescription = "";
        protected List<Materiel> materielOptions = [];

        // Close ticket dialog
        protected bool showCloseDialog;
        protected MaintenanceTicket ticketToClose;
        protected string closeResolution = "";
        protected decimal closeCost;

        protected override async Task OnInitializedAsync()
        {
            materielOptions = (await ITStockManagmentService.GetMateriels()).ToList();
            await LoadTickets();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender) return;
            var session = (await LocalStorage.GetAsync<UserSession>("UserSession")).Value;
            userRole = session?.Role ?? "";
            StateHasChanged();
        }

        private async Task LoadTickets()
        {
            var all = (await MaintenanceService.GetAllTicketsAsync())
                .ToList();

            openCount      = all.Count(t => t.Status == "Open");
            inProgressCount= all.Count(t => t.Status == "InProgress");
            closedCount    = all.Count(t => t.Status == "Closed");

            tickets = all
                .Where(t => string.IsNullOrWhiteSpace(selectedStatus) || t.Status == selectedStatus)
                .Where(t => string.IsNullOrWhiteSpace(search)
                            || (t.Materiel?.MaterielName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                            || (t.ProblemDescription?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        protected async Task OnStatusFilter(string status)
        {
            selectedStatus = status;
            await LoadTickets();
        }

        protected async Task OnSearchChanged(string value)
        {
            search = value;
            await LoadTickets();
        }

        protected void OpenNewTicketDialog() => showNewDialog = true;
        protected void CloseDialogs()
        {
            showNewDialog   = false;
            showCloseDialog = false;
            ticketToClose   = null;
            newDescription  = "";
            closeResolution = "";
            closeCost       = 0;
        }

        protected void OpenCloseDialog(MaintenanceTicket ticket)
        {
            ticketToClose   = ticket;
            showCloseDialog = true;
        }

        protected async Task CreateTicket()
        {
            if (newMaterielId <= 0 || string.IsNullOrWhiteSpace(newDescription)) return;
            var ticket = new MaintenanceTicket
            {
                MaterielId          = newMaterielId,
                ProblemDescription  = newDescription,
                ReportedByEmployeeId = null
            };
            await MaintenanceService.CreateTicketAsync(ticket);
            CloseDialogs();
            await LoadTickets();
        }

        protected async Task CloseTicket()
        {
            if (ticketToClose is null) return;
            await MaintenanceService.CloseTicketAsync(ticketToClose.Id, closeResolution, closeCost);
            CloseDialogs();
            await LoadTickets();
        }

        protected static string StatusCss(string status) => status switch
        {
            "Open"       => "badge-open",
            "InProgress" => "badge-inprogress",
            "Closed"     => "badge-closed",
            _             => ""
        };
    }
}
