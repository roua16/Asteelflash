using ITStockM.Services.Interfaces;

namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Builds HTML email bodies without knowing anything about delivery (SRP).
    /// All templates use the AsteelFlash house style.
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private const string PrimaryColor = "#0055a4";
        private const string DangerColor  = "#e74c3c";
        private const string WarningColor = "#f39c12";
        private const string SuccessColor = "#27ae60";

        // ─── Shared wrapper ──────────────────────────────────────────────────────
        private static string Wrap(string title, string bodyHtml, string accentColor)
        => $@"<!DOCTYPE html>
<html>
<head><meta charset=""UTF-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1.0""></head>
<body style=""font-family:'Segoe UI',Arial,sans-serif;background:#f4f6f9;margin:0;padding:20px;"">
  <div style=""max-width:600px;margin:0 auto;background:#ffffff;border-radius:8px;
              box-shadow:0 2px 12px rgba(0,0,0,.1);overflow:hidden;"">
    <div style=""background:{accentColor};padding:20px 28px;"">
      <h1 style=""color:#fff;margin:0;font-size:20px;font-weight:700;"">IT Stock Management</h1>
      <p  style=""color:rgba(255,255,255,.85);margin:4px 0 0;font-size:14px;"">{title}</p>
    </div>
    <div style=""padding:24px 28px;"">
      {bodyHtml}
    </div>
    <div style=""background:#f8f9fa;padding:14px 28px;border-top:1px solid #e9ecef;font-size:12px;color:#888;"">
      AsteelFlash — IT Stock Management System &nbsp;|&nbsp; {DateTime.Now:yyyy-MM-dd HH:mm}
    </div>
  </div>
</body></html>";

        private static string Row(string label, string value)
            => $@"<tr>
          <td style=""padding:6px 0;color:#555;font-size:14px;width:40%;"">{label}</td>
          <td style=""padding:6px 0;font-weight:600;font-size:14px;"">{value}</td>
        </tr>";

        private static string Table(string rowsHtml)
            => $@"<table style=""width:100%;border-collapse:collapse;margin:16px 0;"">{rowsHtml}</table>";

        private static string Badge(string text, string color)
            => $@"<span style=""background:{color};color:#fff;padding:2px 8px;border-radius:4px;font-size:13px;font-weight:600;"">{text}</span>";

        // ─── IEmailTemplateService ───────────────────────────────────────────────

        public string BuildActionNotification(string action, string details, string performedBy)
        {
            var body = $@"
              <p style=""font-size:15px;margin-bottom:16px;"">The following action was performed in the system:</p>
              {Table(Row("Action", action) + Row("Performed By", performedBy))}
              <div style=""background:#f8f9fa;border-left:4px solid {PrimaryColor};padding:12px 16px;
                           border-radius:4px;margin-top:12px;"">
                {details}
              </div>";
            return Wrap(action, body, PrimaryColor);
        }

        public string BuildWarrantyExpiryAlert(string materielName, string serialNumber, DateTime warrantyEnd, int daysRemaining)
        {
            var urgencyColor = daysRemaining <= 7 ? DangerColor : daysRemaining <= 14 ? WarningColor : WarningColor;
            var body = $@"
              <p style=""font-size:15px;"">The following asset's warranty is expiring soon:</p>
              {Table(
                  Row("Asset Name",    materielName)    +
                  Row("Serial Number", serialNumber)    +
                  Row("Warranty Ends", warrantyEnd.ToString("dd MMM yyyy")) +
                  Row("Days Remaining", Badge($"{daysRemaining} days", urgencyColor))
              )}
              <p style=""color:#555;margin-top:16px;"">
                Please arrange a renewal or start the replacement process to avoid coverage gaps.
              </p>";
            return Wrap("Warranty Expiry Alert", body, urgencyColor);
        }

        public string BuildLowHealthScoreAlert(string materielName, string serialNumber, decimal healthScore, string healthStatus, string? recommendation)
        {
            var color = healthStatus == "Poor" ? DangerColor : WarningColor;
            var body = $@"
              <p style=""font-size:15px;"">An asset has been flagged with a low health score:</p>
              {Table(
                  Row("Asset Name",    materielName) +
                  Row("Serial Number", serialNumber) +
                  Row("Health Score",  Badge($"{healthScore:F0} / 100", color)) +
                  Row("Health Status", Badge(healthStatus, color))
              )}
              {(recommendation != null ? $@"<div style=""background:#fff5f5;border-left:4px solid {color};padding:12px 16px;border-radius:4px;margin-top:12px;"">
                <strong>Recommendation:</strong> {recommendation}
              </div>" : "")}";
            return Wrap("Low Asset Health Score", body, color);
        }

        public string BuildMaintenanceTicketCreated(int ticketId, string materielName, string problem, string reportedBy)
        {
            var body = $@"
              <p style=""font-size:15px;"">A new maintenance ticket has been opened:</p>
              {Table(
                  Row("Ticket #",     $"#{ticketId}")  +
                  Row("Asset",        materielName)     +
                  Row("Reported By",  reportedBy)       +
                  Row("Status",       Badge("Open", WarningColor))
              )}
              <div style=""background:#f8f9fa;border-left:4px solid {WarningColor};padding:12px 16px;border-radius:4px;margin-top:12px;"">
                <strong>Problem Description:</strong><br/>{problem}
              </div>";
            return Wrap($"Maintenance Ticket #{ticketId} Opened", body, WarningColor);
        }

        public string BuildMaintenanceTicketClosed(int ticketId, string materielName, string resolution, decimal? costEur)
        {
            var costRow = costEur.HasValue ? Row("Repair Cost", $"€ {costEur.Value:F2}") : "";
            var body = $@"
              <p style=""font-size:15px;"">A maintenance ticket has been resolved:</p>
              {Table(
                  Row("Ticket #",  $"#{ticketId}") +
                  Row("Asset",     materielName)    +
                  Row("Status",    Badge("Closed", SuccessColor)) +
                  costRow
              )}
              <div style=""background:#f0fff4;border-left:4px solid {SuccessColor};padding:12px 16px;border-radius:4px;margin-top:12px;"">
                <strong>Resolution:</strong><br/>{resolution}
              </div>";
            return Wrap($"Maintenance Ticket #{ticketId} Closed", body, SuccessColor);
        }

        public string BuildAssetReplacementForecast(IEnumerable<(string MaterielName, string SerialNumber, DateTime ReplacementDate, decimal? Cost)> assets)
        {
            var rows = string.Join("", assets.Select(a =>
                $@"<tr style=""border-bottom:1px solid #f0f0f0;"">
                  <td style=""padding:8px 4px;font-size:13px;"">{a.MaterielName}</td>
                  <td style=""padding:8px 4px;font-size:13px;color:#666;"">{a.SerialNumber}</td>
                  <td style=""padding:8px 4px;font-size:13px;"">{a.ReplacementDate:dd MMM yyyy}</td>
                  <td style=""padding:8px 4px;font-size:13px;"">{(a.Cost.HasValue ? $"€ {a.Cost.Value:F0}" : "—")}</td>
                </tr>"));

            var body = $@"
              <p style=""font-size:15px;"">The following assets are predicted to need replacement soon:</p>
              <table style=""width:100%;border-collapse:collapse;margin-top:14px;"">
                <thead>
                  <tr style=""background:{PrimaryColor};color:#fff;"">
                    <th style=""padding:10px 4px;text-align:left;font-size:13px;"">Asset</th>
                    <th style=""padding:10px 4px;text-align:left;font-size:13px;"">Serial</th>
                    <th style=""padding:10px 4px;text-align:left;font-size:13px;"">Replace By</th>
                    <th style=""padding:10px 4px;text-align:left;font-size:13px;"">Est. Cost</th>
                  </tr>
                </thead>
                <tbody>{rows}</tbody>
              </table>
              <p style=""margin-top:16px;color:#555;"">
                Budget forecast based on current health-score analysis.  Please review and plan procurement accordingly.
              </p>";
            return Wrap("Asset Replacement Forecast", body, DangerColor);
        }
    }
}
