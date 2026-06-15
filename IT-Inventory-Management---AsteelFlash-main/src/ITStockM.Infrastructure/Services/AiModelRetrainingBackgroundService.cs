using ITStockM.Application.Common.Models;
using ITStockM.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace ITStockM.Services;

public sealed class AiModelRetrainingBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AiModelRetrainingBackgroundService> _logger;
    private readonly AiModelRetrainingOptions _options;

    public AiModelRetrainingBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<AiModelRetrainingOptions> options,
        ILogger<AiModelRetrainingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("AI model retraining scheduler is disabled.");
            return;
        }

        var initialDelay = TimeSpan.FromMinutes(Math.Max(1, _options.InitialDelayMinutes));
        var interval = TimeSpan.FromMinutes(Math.Max(15, _options.IntervalMinutes));

        _logger.LogInformation("AI model retraining scheduler started. First run in {Delay} minutes, then every {Interval} minutes.", initialDelay.TotalMinutes, interval.TotalMinutes);

        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCycleAsync(stoppingToken);
            await Task.Delay(interval, stoppingToken);
        }
    }

    private int _hardwareHighDriftConsecutive;
    private int _ticketHighDriftConsecutive;

    private async Task RunCycleAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        var recommendationService = scope.ServiceProvider.GetRequiredService<IHardwareRecommendationService>();
        var ticketService = scope.ServiceProvider.GetRequiredService<ITicketPrioritizationService>();

        try
        {
            var hardwareStatus = await recommendationService.GetModelStatusAsync(ct);
            var ticketStatus = await ticketService.GetModelStatusAsync(ct);

            var shouldForceHardware = ShouldForceRetrain(hardwareStatus, ref _hardwareHighDriftConsecutive, "hardware-recommendation");
            var shouldForceTicket = ShouldForceRetrain(ticketStatus, ref _ticketHighDriftConsecutive, "ticket-prioritization");

            if (shouldForceHardware)
            {
                _logger.LogInformation("Forcing hardware model retraining due to drift. ConsecutiveHighDrift={Count}, DriftScore={Score}", _hardwareHighDriftConsecutive, hardwareStatus.DriftScore);
                await recommendationService.RetrainAsync(ct);
            }
            else
            {
                await recommendationService.RetrainAsync(ct);
            }

            if (shouldForceTicket)
            {
                _logger.LogInformation("Forcing ticket model retraining due to drift. ConsecutiveHighDrift={Count}, DriftScore={Score}", _ticketHighDriftConsecutive, ticketStatus.DriftScore);
                await ticketService.RetrainAsync(ct);
            }
            else
            {
                await ticketService.RetrainAsync(ct);
            }

            // Refresh statuses after retraining attempt.
            hardwareStatus = await recommendationService.GetModelStatusAsync(ct);
            ticketStatus = await ticketService.GetModelStatusAsync(ct);

            LogGateStatus(hardwareStatus.ModelName, hardwareStatus.LastRetrainStatus, hardwareStatus.ValidationMetric);
            LogGateStatus(ticketStatus.ModelName, ticketStatus.LastRetrainStatus, ticketStatus.ValidationMetric);

            _logger.LogInformation("AI model retraining cycle completed successfully at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI model retraining cycle failed");
        }
    }

    private bool ShouldForceRetrain(AiModelStatusDto status, ref int consecutiveHighDrift, string modelName)
    {
        consecutiveHighDrift = string.Equals(status.DriftLevel, "high", StringComparison.OrdinalIgnoreCase)
            ? consecutiveHighDrift + 1
            : 0;

        if (!_options.ForceRetrainOnHighDrift)
        {
            return false;
        }

        var forced = consecutiveHighDrift >= Math.Max(1, _options.DriftHighConsecutiveCyclesToForceRetrain);
        if (forced)
        {
            _logger.LogWarning("Drift-based retrain trigger. Model={Model}, ConsecutiveHighDrift={Consecutive}, DriftScore={Score}", modelName, consecutiveHighDrift, status.DriftScore);
        }

        return forced;
    }

    private void LogGateStatus(string modelName, string status, double validationMetric)
    {
        if (status.StartsWith("blocked_data_quality", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "AI retraining blocked by data quality gate. Model={Model}, Status={Status}, ValidationMetric={Metric}",
                modelName,
                status,
                validationMetric);
            return;
        }

        if (status.StartsWith("blocked_acceptance", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "AI retraining blocked by acceptance gate. Model={Model}, Status={Status}, ValidationMetric={Metric}",
                modelName,
                status,
                validationMetric);
            return;
        }

        if (status.Equals("rolled_over_to_previous", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation(
                "AI retraining kept previous model. Model={Model}, Status={Status}, ValidationMetric={Metric}",
                modelName,
                status,
                validationMetric);
        }
    }
}
