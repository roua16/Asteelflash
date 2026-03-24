using ITStockM.Models.ITStockManagment;
using ITStockM.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.AssetLifecycle
{
    public partial class LifecyclePage
    {
        [Inject] protected IAssetLifecycleService LifecycleService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        protected List<AssetLifecycleRecord> records = [];
        protected RadzenDataGrid<AssetLifecycleRecord> grid;

        protected string selectedStage;
        protected string search = "";

        protected List<string> stages = ["Purchased", "InStock", "Assigned", "UnderMaintenance", "Retired"];

        protected override async Task OnInitializedAsync()
        {
            await LoadRecords();
        }

        private async Task LoadRecords()
        {
            var all = await LifecycleService.GetCurrentStagesAsync();

            records = all
                .Where(r => string.IsNullOrWhiteSpace(selectedStage) || r.Stage == selectedStage)
                .Where(r => string.IsNullOrWhiteSpace(search)
                            || (r.Materiel?.MaterielName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                            || (r.Materiel?.SerialNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        protected async Task OnStageChanged(string stage)
        {
            selectedStage = stage;
            await LoadRecords();
        }

        protected async Task OnSearchChanged(string value)
        {
            search = value;
            await LoadRecords();
        }

        protected static string StageCss(string stage) => stage switch
        {
            "Purchased"        => "badge-purchased",
            "InStock"          => "badge-instock",
            "Assigned"         => "badge-assigned",
            "UnderMaintenance" => "badge-maintenance",
            "Retired"          => "badge-retired",
            _                  => ""
        };
    }
}
