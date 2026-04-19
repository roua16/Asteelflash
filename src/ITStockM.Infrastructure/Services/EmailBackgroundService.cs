using ITStockM.Domain.Entities;
using ITStockM.Application.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using ITStockM.Services.Assignments;
using ITStockM.Services.DeliveryOrders;
using ITStockM.Services.Requests;
using ITStockM.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

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
                using (var scope = _serviceProvider.CreateScope())
                {
                    var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                    var deliveryOrderService = scope.ServiceProvider.GetRequiredService<IDeliveryOrderService>();
                    var requestService = scope.ServiceProvider.GetRequiredService<IRequestService>();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailBackgroundService>>();

                    try
                    {
                        var assignments = (await assignmentService.GetAssignments(new QueryOptions { Expand = "AssignmentMateriels, AssignedEmployee, Employee" }))
                            .Where(a => a.OnMission && a.RestoreDate == null && a.RestoreDateLimit > DateTime.Today).ToList();
                        var deliveryorders = (await deliveryOrderService.GetDeliveryOrders()).Where(dlo => dlo.OrderNumber == null).ToList();
                        var requests = (await requestService.GetRequests(new QueryOptions
                        {
                            Filter = $@"i => i.Status == @0",
                            FilterParameters = new object[] { "Done" },
                            Expand = "Employee, Offers"
                        })).Where(request => request.Offers.Any(offer => offer.Selected == true && offer.DeliveryDate > DateTime.Today && offer.DeliveryDate <= DateTime.Today.AddDays(1))).ToList();

                        logger.LogInformation("Background email check: {AssignmentCount} assignments, {DeliveryOrderCount} unfilled orders, {RequestCount} pending deliveries",
                            assignments.Count, deliveryorders.Count, requests.Count);

                        if (!requests.IsNullOrEmpty())
                        {
                            await PendingDeliveriesAsync(requests, emailService, logger, stoppingToken);
                        }

                        if (!deliveryorders.IsNullOrEmpty())
                        {
                            await OrderNumbersNotFilledAsync(deliveryorders, emailService, logger, stoppingToken);
                        }

                        if (!assignments.IsNullOrEmpty())
                        {
                            await ExpiredMissionsAsync(assignments, emailService, logger, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error during background email processing cycle");
                    }

                    await Task.Delay(3600000, stoppingToken);
                }
            }
        }

        protected async Task ExpiredMissionsAsync(List<Assignment> assignments, IEmailService emailService, ILogger<EmailBackgroundService> logger, CancellationToken cancellationToken = default)
        {
            foreach (var assignment in assignments)
            {
                if (assignment == null) continue;

                string? subject = null;
                string? htmlEmail = null;

                if (assignment.RestoreDateLimit <= DateTime.Today.AddDays(3))
                {
                    subject = "Final Reminder: Material Return Due Soon";
                    htmlEmail = BuildFinalReminderEmail(assignment);
                }
                else if (assignment.RestoreDateLimit <= DateTime.Today.AddDays(7))
                {
                    subject = "Material Return Reminder";
                    htmlEmail = BuildWeeklyReminderEmail(assignment);
                }
                else
                {
                    continue;
                }

                var toEmail = assignment.AssignedEmployee?.Email ?? "helpdesk@asteel.com";
                var recipientDomain = toEmail.Contains('@') ? toEmail.Split('@').Last() : toEmail;

                logger.LogInformation("Preparing reminder email '{Subject}' for AssignmentId={AssignmentId} to {RecipientDomain}",
                    subject, assignment.Id, recipientDomain);

                var sw = Stopwatch.StartNew();
                try
                {
                    await emailService.SendEmailAsync(toEmail, subject, htmlEmail);
                    sw.Stop();
                    logger.LogInformation("Reminder email sent successfully for AssignmentId={AssignmentId} in {ElapsedMs}ms",
                        assignment.Id, sw.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    logger.LogWarning(ex, "Failed to send reminder email for AssignmentId={AssignmentId} in {ElapsedMs}ms",
                        assignment.Id, sw.ElapsedMilliseconds);
                }
            }
        }

        protected async Task OrderNumbersNotFilledAsync(List<DeliveryOrder> deliveryorders, IEmailService emailService, ILogger<EmailBackgroundService> logger, CancellationToken cancellationToken = default)
        {
            var subject = "Reminder: Unfilled Delivery Orders";
            var htmlEmail = BuildUnfilledOrdersEmail(deliveryorders);
            var toEmail = "helpdesk@asteel.com";
            var recipientDomain = toEmail.Contains('@') ? toEmail.Split('@').Last() : toEmail;

            logger.LogInformation("Preparing unfilled orders email '{Subject}' to {RecipientDomain} (Count={OrderCount})",
                subject, recipientDomain, deliveryorders.Count);

            var sw = Stopwatch.StartNew();
            try
            {
                await emailService.SendEmailAsync(toEmail, subject, htmlEmail);
                sw.Stop();
                logger.LogInformation("Unfilled orders email sent in {ElapsedMs}ms (Count={OrderCount})",
                    sw.ElapsedMilliseconds, deliveryorders.Count);
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogWarning(ex, "Failed to send unfilled orders email in {ElapsedMs}ms (Count={OrderCount})",
                    sw.ElapsedMilliseconds, deliveryorders.Count);
            }
        }

        protected async Task PendingDeliveriesAsync(List<Request> requests, IEmailService emailService, ILogger<EmailBackgroundService> logger, CancellationToken cancellationToken = default)
        {
            var subject = "Reminder: Upcoming Delivery Orders (Arriving Tomorrow)";
            var htmlEmail = BuildPendingDeliveriesEmail(requests);
            var toEmail = "helpdesk@asteel.com";
            var recipientDomain = toEmail.Contains('@') ? toEmail.Split('@').Last() : toEmail;

            logger.LogInformation("Preparing pending deliveries email '{Subject}' to {RecipientDomain} (Count={RequestCount})",
                subject, recipientDomain, requests.Count);

            var sw = Stopwatch.StartNew();
            try
            {
                await emailService.SendEmailAsync(toEmail, subject, htmlEmail);
                sw.Stop();
                logger.LogInformation("Pending deliveries email sent in {ElapsedMs}ms (Count={RequestCount})",
                    sw.ElapsedMilliseconds, requests.Count);
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogWarning(ex, "Failed to send pending deliveries email in {ElapsedMs}ms (Count={RequestCount})",
                    sw.ElapsedMilliseconds, requests.Count);
            }
        }

        #region Email Templates

        private string BuildFinalReminderEmail(Assignment assignment)
        {
            var employeeName = assignment.AssignedEmployee?.FullName ?? "Team Member";
            var materielItems = string.Join("", assignment.AssignmentMateriels
                .Where(assm => assm.AssignmentId == assignment.Id)
                .Select(m => $"<div class=\"materiel-item\">• {m.Materiel?.MaterielName ?? "Unknown"}</div>"));

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: 'Roboto', Arial, sans-serif; line-height: 1.6; color: #333333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .container {{ background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #0055a4, #003b75); color: white; padding: 20px; text-align: center; }}
        .header h1 {{ font-size: 24px; margin: 0; font-weight: 700; }}
        .content {{ padding: 25px; }}
        .greeting {{ font-size: 18px; margin-bottom: 20px; }}
        .materiel-list {{ background-color: #f8f9fa; padding: 15px; border-left: 4px solid #e74c3c; border-radius: 4px; margin: 15px 0; }}
        .materiel-item {{ padding: 8px 0; border-bottom: 1px dashed #e9ecef; }}
        .materiel-item:last-child {{ border-bottom: none; }}
        .urgent {{ color: #e74c3c; font-weight: bold; background-color: #fff5f5; padding: 10px 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #e74c3c; }}
        .due-date {{ background-color: #e74c3c; color: white; font-weight: bold; padding: 5px 10px; border-radius: 4px; display: inline-block; margin: 10px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef; }}
        .signature {{ font-weight: bold; margin-top: 20px; color: #0055a4; }}
        .contact-info {{ background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #0055a4; }}
        .contact-info a {{ color: #0055a4; text-decoration: none; font-weight: 500; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Final Reminder: Material Return Due Soon</h1>
        </div>
        <div class=""content"">
            <p class=""greeting"">Hi {employeeName},</p>
            <p class=""urgent"">This is a final reminder that the following borrowed materials are due in <strong>3 days</strong>:</p>
            <div class=""materiel-list"">
                {materielItems}
            </div>
            <p>Please ensure these items are returned by:</p>
            <div class=""due-date"">{assignment.RestoreDateLimit:MMMM dd, yyyy}</div>
            <p>Returning the material on time is crucial as others may be waiting to use these resources.</p>
            <div class=""contact-info"">
                <p>If you've already returned these items, please disregard this message. If you need an extension or have any questions, please contact us immediately at 
                <a href=""mailto:helpdesk@asteel.com"">helpdesk@asteel.com</a>.</p>
            </div>
            <p class=""urgent"">Failure to return materials by the due date may result in follow-up actions.</p>
        </div>
        <div class=""footer"">
            <p>Thank you for your prompt attention to this matter.</p>
            <p class=""signature"">Best regards,<br>The AsteelFlash Team</p>
        </div>
    </div>
</body>
</html>";
        }

        private string BuildWeeklyReminderEmail(Assignment assignment)
        {
            var employeeName = assignment.AssignedEmployee?.FullName ?? "Team Member";
            var materielItems = string.Join("", assignment.AssignmentMateriels
                .Where(assm => assm.AssignmentId == assignment.Id)
                .Select(m => $"<div class=\"materiel-item\">• {m.Materiel?.MaterielName ?? "Unknown"}</div>"));

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: 'Roboto', Arial, sans-serif; line-height: 1.6; color: #333333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .container {{ background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #0055a4, #003b75); color: white; padding: 20px; text-align: center; }}
        .header h1 {{ font-size: 24px; margin: 0; font-weight: 700; }}
        .content {{ padding: 25px; }}
        .greeting {{ font-size: 18px; margin-bottom: 20px; }}
        .materiel-list {{ background-color: #f8f9fa; padding: 15px; border-left: 4px solid #0055a4; border-radius: 4px; margin: 15px 0; }}
        .materiel-item {{ padding: 8px 0; border-bottom: 1px dashed #e9ecef; }}
        .materiel-item:last-child {{ border-bottom: none; }}
        .reminder {{ color: #0055a4; font-weight: bold; background-color: #e6f7ff; padding: 10px 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #0055a4; }}
        .due-date {{ background-color: #0055a4; color: white; font-weight: bold; padding: 5px 10px; border-radius: 4px; display: inline-block; margin: 10px 0; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef; }}
        .signature {{ font-weight: bold; margin-top: 20px; color: #0055a4; }}
        .contact-info {{ background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #0055a4; }}
        .contact-info a {{ color: #0055a4; text-decoration: none; font-weight: 500; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Material Return Reminder</h1>
        </div>
        <div class=""content"">
            <p class=""greeting"">Hi {employeeName},</p>
            <p class=""reminder"">Just a friendly follow-up regarding the material you borrowed:</p>
            <div class=""materiel-list"">
                {materielItems}
            </div>
            <p>This is a reminder that you have one week remaining to return these items. We would appreciate it if you could bring them back by:</p>
            <div class=""due-date"">{assignment.RestoreDateLimit:MMMM dd, yyyy}</div>
            <p>Returning the material on time ensures that it's available for others who may need it.</p>
            <div class=""contact-info"">
                <p>If you've already returned the material, please disregard this email. If you anticipate needing a little more time or have any questions, please let me know as soon as possible at 
                <a href=""mailto:helpdesk@asteel.com"">helpdesk@asteel.com</a> or contact the helpdesk.</p>
            </div>
        </div>
        <div class=""footer"">
            <p>Thanks again for your cooperation!</p>
            <p class=""signature"">Best regards,<br>The AsteelFlash Team</p>
        </div>
    </div>
</body>
</html>";
        }

        private string BuildUnfilledOrdersEmail(List<DeliveryOrder> deliveryorders)
        {
            var orderItems = string.Join("", deliveryorders.Select(dlo =>
                $"<div class=\"order-item\">• Delivery Order: {dlo.DeliveryOrderNumber}</div>"));

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: 'Roboto', Arial, sans-serif; line-height: 1.6; color: #333333; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .container {{ background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #dc3545, #c82333); color: white; padding: 20px; text-align: center; }}
        .header h1 {{ font-size: 24px; margin: 0; font-weight: 700; }}
        .content {{ padding: 25px; }}
        .greeting {{ font-size: 18px; margin-bottom: 20px; }}
        .order-list {{ background-color: #f8f9fa; padding: 15px; border-left: 4px solid #dc3545; border-radius: 4px; margin: 15px 0; }}
        .order-item {{ padding: 8px 0; border-bottom: 1px dashed #e9ecef; }}
        .order-item:last-child {{ border-bottom: none; }}
        .alert {{ color: #dc3545; font-weight: bold; background-color: #fff5f5; padding: 10px 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #dc3545; }}
        .count {{ background-color: #dc3545; color: white; font-weight: bold; padding: 2px 8px; border-radius: 50%; display: inline-block; margin: 0 5px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef; }}
        .signature {{ font-weight: bold; margin-top: 20px; color: #0055a4; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Reminder: Unfilled Delivery Orders</h1>
        </div>
        <div class=""content"">
            <p class=""greeting"">Hello Sabrine,</p>
            <p class=""alert"">This is a reminder that you have <span class=""count"">{deliveryorders.Count}</span> delivery orders that have not been filled:</p>
            <div class=""order-list"">
                {orderItems}
            </div>
            <p>Please don't forget to update the delivery order number.</p>
            <p>If these orders have been fulfilled, please record the completion in the system.</p>
        </div>
        <div class=""footer"">
            <p>Thank you for your prompt attention to this matter.</p>
            <p class=""signature"">Best regards,<br>ITStockM<br>AsteelFlash</p>
        </div>
    </div>
</body>
</html>";
        }

        private string BuildPendingDeliveriesEmail(List<Request> requests)
        {
            var orderItems = string.Join("", requests.Select(r =>
            {
                var selectedOffer = r.Offers?.FirstOrDefault(o => o.Selected == true);
                var supplierName = selectedOffer?.SupplierName ?? "Unknown Supplier";
                return $@"<div class=""order-item"">
                    <div class=""order-title"">{r.Title}</div>
                    <div class=""supplier""><span class=""supplier-label"">Supplier</span> {supplierName}</div>
                </div>";
            }));

            return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        body {{ font-family: 'Roboto', Arial, sans-serif; line-height: 1.5; color: #333; max-width: 700px; margin: 0 auto; padding: 20px; background-color: #f9f9f9; }}
        .container {{ background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background: linear-gradient(135deg, #28a745, #218838); color: white; padding: 20px; text-align: center; }}
        .header h1 {{ font-size: 24px; margin: 0; font-weight: 700; }}
        .content {{ padding: 25px; }}
        .greeting {{ font-size: 18px; margin-bottom: 20px; }}
        .order-list {{ background-color: #f0f7fd; border-left: 4px solid #2a6496; padding: 15px; border-radius: 4px; margin: 15px 0; }}
        .order-item {{ padding: 10px; margin-bottom: 10px; background-color: white; border-radius: 4px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }}
        .order-title {{ font-weight: bold; margin-bottom: 5px; }}
        .supplier {{ font-size: 14px; color: #666; }}
        .supplier-label {{ background-color: #e9ecef; padding: 2px 6px; border-radius: 3px; font-size: 12px; margin-right: 5px; }}
        .highlight {{ background-color: #fff9e6; padding: 3px 5px; border-radius: 3px; font-weight: bold; }}
        .checklist {{ background-color: #f8f9fa; padding: 15px; border-radius: 4px; margin: 15px 0; border-left: 4px solid #ffc107; }}
        .checklist h3 {{ margin-top: 0; margin-bottom: 10px; color: #333; }}
        .checklist ul {{ margin: 0; padding-left: 20px; }}
        .checklist li {{ margin-bottom: 8px; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; border-top: 1px solid #e9ecef; }}
        .signature {{ font-weight: bold; margin-top: 20px; color: #0055a4; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Upcoming Delivery Orders (Arriving Tomorrow)</h1>
        </div>
        <div class=""content"">
            <p class=""greeting"">Hello Team,</p>
            <p>This is a <strong>friendly reminder</strong> that the following <span class=""highlight"">{requests.Count} delivery orders</span> are scheduled to arrive <strong>tomorrow</strong>:</p>
            <div class=""order-list"">
                {orderItems}
            </div>
            <div class=""checklist"">
                <h3>Please ensure:</h3>
                <ul>
                    <li>Your team is <strong>prepared to receive</strong> and process these orders.</li>
                    <li>Storage/workspace is <strong>ready</strong> (if applicable).</li>
                    <li>Any required documentation is <strong>available</strong>.</li>
                </ul>
            </div>
            <p>If there are any changes or delays, please notify us immediately.</p>
        </div>
        <div class=""footer"">
            <p>Thank you for your attention!</p>
            <p class=""signature"">Best regards,<br>ITSTOCKM<br>AsteelFlash</p>
        </div>
    </div>
</body>
</html>";
        }

        #endregion
    }
}
