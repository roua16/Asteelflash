using ITStockM.Domain.Entities;
using ITStockM.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;

namespace ITStockM.Components.Pages.Predictions
{
    public partial class PredictionsPage
    {
        [Inject] protected IPredictionService PredictionService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        protected List<AssetPrediction> predictions = [];
        protected RadzenDataGrid<AssetPrediction> grid;

        protected int goodCount, fairCount, poorCount;
        protected decimal avgScore;
        protected string healthFilter;
        protected string search = "";
        protected bool highRiskOnly;
        protected bool calculating;

        protected List<string> healthOptions = ["Good", "Fair", "Poor"];

        protected override async Task OnInitializedAsync() => await LoadPredictions();

        private async Task LoadPredictions()
        {
            var rawAll = highRiskOnly
                ? await PredictionService.GetHighRiskAssetsAsync()
                : await PredictionService.GetLatestPredictionsAsync();
            var all = rawAll.ToList();

            goodCount = all.Count(p => p.HealthStatus == "Good");
            fairCount = all.Count(p => p.HealthStatus == "Fair");
            poorCount = all.Count(p => p.HealthStatus == "Poor");
            avgScore  = all.Any() ? all.Average(p => p.HealthScore) : 0m;

            predictions = all
                .Where(p => string.IsNullOrWhiteSpace(healthFilter) || p.HealthStatus == healthFilter)
                .Where(p => string.IsNullOrWhiteSpace(search)
                            || (p.Materiel?.MaterielName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                            || (p.Materiel?.SerialNumber?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        protected async Task OnHealthFilter(string status)
        {
            healthFilter = status;
            await LoadPredictions();
        }

        protected async Task OnSearchChanged(string value)
        {
            search = value;
            await LoadPredictions();
        }

        protected async Task RecalculateAll()
        {
            calculating = true;
            await PredictionService.RecalculateAllPredictionsAsync(CancellationToken.None);
            calculating = false;
            await LoadPredictions();
        }

        protected async Task RecalculateSingle(int materielId)
        {
            await PredictionService.CalculateAndSavePredictionAsync(materielId);
            await LoadPredictions();
        }

        protected static string HealthCss(string status) => status switch
        {
            "Good" => "badge-good",
            "Fair" => "badge-fair",
            "Poor" => "badge-poor",
            _       => ""
        };

        protected static string ScoreColor(decimal score) => score switch
        {
            >= 75 => "#10b981",
            >= 45 => "#f59e0b",
            _     => "#ef4444"
        };
    }
}
