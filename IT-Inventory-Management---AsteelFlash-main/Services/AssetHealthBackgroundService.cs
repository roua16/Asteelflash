using ITStockM.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services
{
    /// <summary>
    /// Daily background job responsible for:
    ///   1. Recalculating health scores for all non-retired assets.
    ///   2. Sending warranty-expiry alerts.
    ///   3. Sending a replacement-forecast email when high-risk assets exist.
    ///
    /// Runs once every 24 hours with an initial 30-second warm-up delay.
    /// Uses <see cref="IServiceScopeFactory"/> to resolve scoped services safely.
    /// </summary>
    public class AssetHealthBackgroundService : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan Interval     = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AssetHealthBackgroundService> _logger;

        public AssetHealthBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AssetHealthBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AssetHealthBackgroundService started — first run in {Delay}s", InitialDelay.TotalSeconds);
            await Task.Delay(InitialDelay, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCycleAsync(stoppingToken);
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task RunCycleAsync(CancellationToken ct)
        {
            _logger.LogInformation("AssetHealthBackgroundService: starting daily cycle at {Time}", DateTime.UtcNow);

            using var scope = _scopeFactory.CreateScope();

            var prediction = scope.ServiceProvider.GetRequiredService<IPredictionService>();
            var warranty   = scope.ServiceProvider.GetRequiredService<IWarrantyAlertService>();
            var email      = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var templates  = scope.ServiceProvider.GetRequiredService<IEmailTemplateService>();

            // 1. Recalculate all predictions / health scores
            try
            {
                var results = (await prediction.RecalculateAllPredictionsAsync(ct)).ToList();
                _logger.LogInformation("Health scores updated for {Count} assets", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health-score recalculation failed");
            }

            // 2. Send warranty-expiry alerts
            try
            {
                await warranty.SendWarrantyExpiryAlertsAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Warranty alert dispatch failed");
            }

            // 3. Send replacement-forecast digest for high-risk assets
            try
            {
                await SendReplacementForecastAsync(prediction, email, templates, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Replacement forecast email failed");
            }

            _logger.LogInformation("AssetHealthBackgroundService: cycle complete");
        }

        private async Task SendReplacementForecastAsync(
            IPredictionService prediction,
            IEmailService email,
            IEmailTemplateService templates,
            CancellationToken ct)
        {
            var adminEmail = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL");
            if (string.IsNullOrWhiteSpace(adminEmail)) return;

            var highRisk = (await prediction.GetHighRiskAssetsAsync(50m, ct)).ToList();
            if (!highRisk.Any()) return;

            var assets = highRisk
                .Where(p => p.RecommendedReplacementDate.HasValue)
                .Select(p => (
                    MaterielName:     p.Materiel?.MaterielName ?? "Unknown",
                    SerialNumber:     p.Materiel?.SerialNumber ?? "N/A",
                    ReplacementDate:  p.RecommendedReplacementDate!.Value,
                    Cost:             p.EstimatedReplacementCost))
                .OrderBy(a => a.ReplacementDate)
                .ToList();

            if (!assets.Any()) return;

            var subject = $"[IT Stock] Replacement Forecast — {assets.Count} asset(s) at risk";
            var body    = templates.BuildAssetReplacementForecast(assets);

            await email.SendEmailAsync(adminEmail, subject, body);
            _logger.LogInformation("Replacement forecast sent for {Count} high-risk assets", assets.Count);
        }
    }
}
