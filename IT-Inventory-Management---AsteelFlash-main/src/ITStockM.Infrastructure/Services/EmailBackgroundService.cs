using ITStockM.Domain.Entities;
using ITStockM.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ITStockM.Services.Assignments;
using ITStockM.Services.Materiels;
using ITStockM.Services.Interfaces;

namespace ITStockM.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public EmailBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                var materielService = scope.ServiceProvider.GetRequiredService<IMaterielService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailBackgroundService>>();

                try
                {
                    // Alert 1: Low stock — quantity <= 10
                    var allMateriels = (await materielService.GetMateriels()).ToList();
                    var lowStockItems = allMateriels
                        .Where(m => m.QuantityITStock <= 10)
                        .ToList();

                    if (lowStockItems.Count > 0)
                    {
                        logger.LogInformation("Low stock check: {Count} material(s) at or below 10 units", lowStockItems.Count);
                        await SendLowStockAlertAsync(lowStockItems, emailService, logger, stoppingToken);
                    }

                    // Alert 2: Return reminder — RestoreDateLimit within 3 days
                    var allAssignments = (await assignmentService.GetAssignments(
                        new QueryOptions { Expand = "AssignmentMateriels, AssignedEmployee" })).ToList();

                    var upcomingReturns = allAssignments
                        .Where(a => a.RestoreDate == null
                            && a.RestoreDateLimit.HasValue
                            && a.RestoreDateLimit.Value.Date <= DateTime.Today.AddDays(3)
                            && a.RestoreDateLimit.Value.Date >= DateTime.Today)
                        .ToList();

                    if (upcomingReturns.Count > 0)
                    {
                        logger.LogInformation("Return reminder check: {Count} assignment(s) due within 3 days", upcomingReturns.Count);
                        await SendReturnReminderAsync(upcomingReturns, emailService, logger, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during background email processing cycle");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private static async Task SendLowStockAlertAsync(
            List<Materiel> items,
            IEmailService emailService,
            ILogger<EmailBackgroundService> logger,
            CancellationToken cancellationToken)
        {
            var subject = "⚠️ Low Stock Alert — IT Inventory";
            var html = BuildLowStockEmail(items);
            var toEmail = "helpdesk@asteel.com";

            var sw = Stopwatch.StartNew();
            try
            {
                await emailService.SendEmailAsync(toEmail, subject, html);
                sw.Stop();
                logger.LogInformation("Low stock alert email sent ({Count} items) in {ElapsedMs}ms",
                    items.Count, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogWarning(ex, "Failed to send low stock alert email in {ElapsedMs}ms", sw.ElapsedMilliseconds);
            }
        }

        private static async Task SendReturnReminderAsync(
            List<Assignment> assignments,
            IEmailService emailService,
            ILogger<EmailBackgroundService> logger,
            CancellationToken cancellationToken)
        {
            foreach (var assignment in assignments)
            {
                var toEmail = assignment.AssignedEmployee?.Email ?? "helpdesk@asteel.com";
                var subject = "🔔 Material Return Reminder — Due in 3 Days or Less";
                var html = BuildReturnReminderEmail(assignment);
                var recipientDomain = toEmail.Contains('@') ? toEmail.Split('@').Last() : toEmail;

                var sw = Stopwatch.StartNew();
                try
                {
                    await emailService.SendEmailAsync(toEmail, subject, html);
                    sw.Stop();
                    logger.LogInformation("Return reminder sent for AssignmentId={Id} to {Domain} in {ElapsedMs}ms",
                        assignment.Id, recipientDomain, sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    logger.LogWarning(ex, "Failed to send return reminder for AssignmentId={Id} in {ElapsedMs}ms",
                        assignment.Id, sw.ElapsedMilliseconds);
                }
            }
        }

        #region Email Templates

        private static string BuildLowStockEmail(List<Materiel> items)
        {
            var rows = string.Join("", items.Select(m =>
                $"<tr><td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;'>{m.MaterielName ?? "Unknown"}</td>" +
                $"<td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:center;'>{m.Type ?? "—"}</td>" +
                $"<td style='padding:8px 12px;border-bottom:1px solid #e5e7eb;text-align:center;font-weight:600;color:{(m.QuantityITStock == 0 ? "#dc2626" : "#d97706")};'>{m.QuantityITStock}</td></tr>"));

            return $@"<!DOCTYPE html>
<html>
<head><meta charset=""UTF-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""></head>
<body style=""font-family:Arial,sans-serif;background:#f9fafb;margin:0;padding:20px;"">
  <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,.08);overflow:hidden;"">
    <div style=""background:linear-gradient(135deg,#0055a4,#003b75);color:#fff;padding:24px;text-align:center;"">
      <h1 style=""margin:0;font-size:22px;"">⚠️ Low Stock Alert</h1>
      <p style=""margin:8px 0 0;opacity:.9;"">The following items have 10 or fewer units in IT stock</p>
    </div>
    <div style=""padding:24px;"">
      <p style=""color:#374151;margin-top:0;"">Please review and reorder as needed to avoid stockout.</p>
      <table style=""width:100%;border-collapse:collapse;font-size:14px;"">
        <thead>
          <tr style=""background:#f3f4f6;"">
            <th style=""padding:10px 12px;text-align:left;font-weight:600;color:#374151;"">Material</th>
            <th style=""padding:10px 12px;text-align:center;font-weight:600;color:#374151;"">Type</th>
            <th style=""padding:10px 12px;text-align:center;font-weight:600;color:#374151;"">Qty in Stock</th>
          </tr>
        </thead>
        <tbody>{rows}</tbody>
      </table>
      <p style=""color:#6b7280;font-size:12px;margin-top:24px;"">This alert was generated automatically by IT Stock Management. Do not reply to this email.</p>
    </div>
  </div>
</body>
</html>";
        }

        private static string BuildReturnReminderEmail(Assignment assignment)
        {
            var employeeName = assignment.AssignedEmployee?.FullName ?? "Team Member";
            var dueDate = assignment.RestoreDateLimit?.ToString("dddd, dd MMMM yyyy") ?? "—";
            var daysLeft = assignment.RestoreDateLimit.HasValue
                ? Math.Max(0, (int)(assignment.RestoreDateLimit.Value.Date - DateTime.Today).TotalDays)
                : 0;

            var materialRows = string.Join("", (assignment.AssignmentMateriels ?? [])
                .Select(am =>
                    $"<li style='padding:4px 0;'>{am.Materiel?.MaterielName ?? $"Material #{am.MaterielId}"} × {am.Qte}</li>"));

            return $@"<!DOCTYPE html>
<html>
<head><meta charset=""UTF-8""><meta name=""viewport"" content=""width=device-width,initial-scale=1""></head>
<body style=""font-family:Arial,sans-serif;background:#f9fafb;margin:0;padding:20px;"">
  <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:8px;box-shadow:0 2px 8px rgba(0,0,0,.08);overflow:hidden;"">
    <div style=""background:linear-gradient(135deg,#0055a4,#003b75);color:#fff;padding:24px;text-align:center;"">
      <h1 style=""margin:0;font-size:22px;"">🔔 Material Return Reminder</h1>
    </div>
    <div style=""padding:24px;"">
      <p style=""font-size:16px;color:#111827;"">Hello <strong>{employeeName}</strong>,</p>
      <p style=""color:#374151;"">This is a reminder that you have assigned materials due for return in <strong style='color:{(daysLeft == 0 ? "#dc2626" : "#d97706")};'>{daysLeft} day(s)</strong>.</p>
      <div style=""background:#fff7ed;border-left:4px solid #f59e0b;padding:12px 16px;border-radius:4px;margin:16px 0;"">
        <strong>Return deadline:</strong> {dueDate}
      </div>
      <p style=""color:#374151;font-weight:600;"">Items to return:</p>
      <ul style=""color:#374151;margin:0;padding-left:20px;"">
        {materialRows}
      </ul>
      <p style=""color:#6b7280;font-size:12px;margin-top:24px;"">Please contact IT helpdesk if you need an extension. Do not reply to this email.</p>
    </div>
  </div>
</body>
</html>";
        }

        #endregion
    }
}
