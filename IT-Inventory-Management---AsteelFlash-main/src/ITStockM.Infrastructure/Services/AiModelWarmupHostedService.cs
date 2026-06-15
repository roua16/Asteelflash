using ITStockM.Services.Interfaces;
using ITStockM.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITStockM.Infrastructure.Services;

public sealed class AiModelWarmupHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AiModelWarmupHostedService> _logger;

    public AiModelWarmupHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<AiModelWarmupHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Warmup should be one-shot at startup.
        using var scope = _scopeFactory.CreateScope();
        var hardware = scope.ServiceProvider.GetService<IHardwareRecommendationService>();
        var tickets = scope.ServiceProvider.GetService<ITicketPrioritizationService>();

        if (hardware is null || tickets is null)
        {
            _logger.LogWarning("AI warmup skipped: missing ML services. Hardware={Hardware}, Tickets={Tickets}",
                hardware is null ? "missing" : "ok",
                tickets is null ? "missing" : "ok");
            return;
        }

        try
        {
            var hwStatus = await hardware.GetModelStatusAsync(stoppingToken);
            var tkStatus = await tickets.GetModelStatusAsync(stoppingToken);

            _logger.LogInformation("AI warmup status: Hardware={HardwareStatus} Tickets={TicketsStatus}",
                Summarize(hwStatus), Summarize(tkStatus));

            var hwNeedsTraining = ShouldWarmup(hwStatus);
            var tkNeedsTraining = ShouldWarmup(tkStatus);

            if (hwNeedsTraining)
            {
                _logger.LogInformation("AI warmup retraining hardware model (reason: {Reason})", hwStatus.LastRetrainStatus ?? "unknown");
                await hardware.RetrainAsync(stoppingToken);
            }

            if (tkNeedsTraining)
            {
                _logger.LogInformation("AI warmup retraining ticket model (reason: {Reason})", tkStatus.LastRetrainStatus ?? "unknown");
                await tickets.RetrainAsync(stoppingToken);
            }

            // Log final statuses after retraining attempt(s).
            var hwFinal = await hardware.GetModelStatusAsync(stoppingToken);
            var tkFinal = await tickets.GetModelStatusAsync(stoppingToken);

            _logger.LogInformation("AI warmup completed. Hardware={HardwareStatus} Tickets={TicketsStatus}",
                Summarize(hwFinal), Summarize(tkFinal));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI warmup failed");
        }
    }

    private static bool ShouldWarmup(AiModelStatusDto status)
    {
        if (status.LastTrainedUtc is null)
            return true;

        // If explicitly blocked due to gating, retraining may still happen later when data changes.
        // We only warmup for the common "not trained" / "insufficient training data" cases.
        var s = status.LastRetrainStatus?.ToLowerInvariant() ?? string.Empty;
        return s.Contains("not_trained")
               || s.Contains("insufficient_training_data")
               || s.Contains("blocked_data_quality")
               || s.Contains("blocked_acceptance");
    }

    private static string Summarize(AiModelStatusDto s)
        => $"{s.ModelName}: lastTrained={(s.LastTrainedUtc.HasValue ? s.LastTrainedUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null")}, status={s.LastRetrainStatus}, drift={s.DriftLevel}({s.DriftScore:0.##}), metric={s.ValidationMetric:0.##}";
}
