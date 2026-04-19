using ITStockM.Data;
using ITStockM.Domain.Enums;
using ITStockM.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Sends warranty-expiry alerts.
    /// Single responsibility: only concerned with the warranty-alert email workflow.
    /// </summary>
    public class WarrantyAlertService : IWarrantyAlertService
    {
        private readonly ITStockManagmentContext _context;
        private readonly IEmailService _email;
        private readonly IEmailTemplateService _templates;
        private readonly ILogger<WarrantyAlertService> _logger;
        private readonly int _lookAheadDays;
        private readonly string _adminEmail;

        public WarrantyAlertService(
            ITStockManagmentContext context,
            IEmailService email,
            IEmailTemplateService templates,
            IConfiguration configuration,
            ILogger<WarrantyAlertService> logger)
        {
            _context      = context;
            _email        = email;
            _templates    = templates;
            _logger       = logger;
            _lookAheadDays = int.TryParse(configuration["WarrantyAlerts:LookAheadDays"], out var days) ? days : 30;
            _adminEmail   = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL")
                         ?? configuration["EmailSettings:AdminEmail"]
                         ?? string.Empty;
        }

        public async Task SendWarrantyExpiryAlertsAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_adminEmail))
            {
                _logger.LogWarning("WarrantyAlertService: AdminEmail is not configured, skipping alerts");
                return;
            }

            var cutoff = DateTime.UtcNow.AddDays(_lookAheadDays);
            var assets = await _context.Materiels
                .Where(m => m.Warranty <= cutoff && m.Warranty >= DateTime.UtcNow
                         && m.LifecycleStatus != LifecycleStage.Retired)
                .ToListAsync(ct);

            if (!assets.Any())
            {
                _logger.LogInformation("WarrantyAlertService: no warranties expiring within {Days} days", _lookAheadDays);
                return;
            }

            _logger.LogInformation("WarrantyAlertService: sending alerts for {Count} asset(s)", assets.Count);

            foreach (var asset in assets)
            {
                var daysRemaining = (int)(asset.Warranty - DateTime.UtcNow).TotalDays;
                var subject = $"[IT Stock] Warranty Expiry Alert — {asset.MaterielName} ({daysRemaining}d)";
                var body    = _templates.BuildWarrantyExpiryAlert(
                    asset.MaterielName,
                    asset.SerialNumber ?? "N/A",
                    asset.Warranty,
                    daysRemaining);

                try
                {
                    await _email.SendEmailAsync(_adminEmail, subject, body);
                    _logger.LogInformation("Warranty alert sent for MaterielId={Id}, daysRemaining={Days}", asset.Id, daysRemaining);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send warranty alert for MaterielId={Id}", asset.Id);
                }
            }
        }
    }
}
