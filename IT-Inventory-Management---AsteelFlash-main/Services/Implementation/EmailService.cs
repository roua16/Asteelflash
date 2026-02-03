using ITStockM.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Implementation of email service using MailKit for proper SSL support
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmail;
        private readonly string _password;
        private readonly string _adminEmail;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger)
        {
            // Priority: Environment variables > appsettings.json
            _smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER")
                ?? configuration.GetSection("EmailSettings")["SmtpServer"]
                ?? "smtp.gmail.com";

            _smtpPort = int.TryParse(Environment.GetEnvironmentVariable("SMTP_PORT"), out var envPort)
                ? envPort
                : int.Parse(configuration.GetSection("EmailSettings")["SmtpPort"] ?? "587");

            _fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL")
                ?? configuration.GetSection("EmailSettings")["FromEmail"]
                ?? throw new ArgumentNullException("SMTP_FROM_EMAIL or EmailSettings:FromEmail is required");

            _password = Environment.GetEnvironmentVariable("SMTP_PASSWORD")
                ?? configuration.GetSection("EmailSettings")["Password"]
                ?? throw new ArgumentNullException("SMTP_PASSWORD or EmailSettings:Password is required");

            _adminEmail = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL")
                ?? configuration.GetSection("EmailSettings")["AdminEmail"]
                ?? _fromEmail;

            _logger = logger;

            _logger.LogInformation("EmailService initialized with server={Server}, port={Port}, from={From}",
                _smtpServer, _smtpPort, _fromEmail);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            _logger.LogInformation("Preparing to send email {@EmailMeta}", new { Server = _smtpServer, Port = _smtpPort, From = _fromEmail, To = toEmail, Subject = subject });
            try
            {
                var bodySize = System.Text.Encoding.UTF8.GetByteCount(body ?? string.Empty);
                _logger.LogDebug("Sending email to {To} (subject length={SubjectLen}, bodyBytes={BodyBytes})", toEmail, subject?.Length ?? 0, bodySize);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("IT Stock Management", _fromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();

                // Port 465 = SSL/TLS from start, Port 587 = STARTTLS, Port 25 = No encryption (for local testing)
                var secureSocketOptions = _smtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : _smtpPort == 25
                        ? SecureSocketOptions.None
                        : SecureSocketOptions.StartTls;

                _logger.LogDebug("Connecting to {Server}:{Port} with {SecurityOption}", _smtpServer, _smtpPort, secureSocketOptions);

                await client.ConnectAsync(_smtpServer, _smtpPort, secureSocketOptions);

                // Only authenticate if password is provided (skip for local smtp4dev testing)
                if (!string.IsNullOrEmpty(_password))
                {
                    await client.AuthenticateAsync(_fromEmail, _password);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully {@SendResult}", new { To = toEmail, Server = _smtpServer, Port = _smtpPort });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", toEmail);
                throw;
            }
        }

        public async Task SendEmailToMultipleAsync(List<string> toEmails, string subject, string body)
        {
            _logger.LogInformation("Preparing to send bulk email {@BulkMeta}", new { Server = _smtpServer, Port = _smtpPort, From = _fromEmail, RecipientCount = toEmails?.Count ?? 0, Subject = subject });
            try
            {
                var bodySize = System.Text.Encoding.UTF8.GetByteCount(body ?? string.Empty);
                var preview = string.Join(',', toEmails?.Take(5) ?? Array.Empty<string>());
                _logger.LogDebug("Sending bulk email to {PreviewRecipients} (count={Count}) subjectLen={SubjectLen} bodyBytes={BodyBytes}", preview, toEmails?.Count ?? 0, subject?.Length ?? 0, bodySize);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("IT Stock Management", _fromEmail));
                foreach (var email in toEmails ?? new List<string>())
                {
                    message.To.Add(MailboxAddress.Parse(email));
                }
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();

                // Port 465 = SSL/TLS from start, Port 587 = STARTTLS, Port 25 = No encryption (for local testing)
                var secureSocketOptions = _smtpPort == 465
                    ? SecureSocketOptions.SslOnConnect
                    : _smtpPort == 25
                        ? SecureSocketOptions.None
                        : SecureSocketOptions.StartTls;

                await client.ConnectAsync(_smtpServer, _smtpPort, secureSocketOptions);

                // Only authenticate if password is provided (skip for local smtp4dev testing)
                if (!string.IsNullOrEmpty(_password))
                {
                    await client.AuthenticateAsync(_fromEmail, _password);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Bulk email sent successfully {@BulkResult}", new { RecipientCount = toEmails?.Count ?? 0, Server = _smtpServer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bulk email to recipients (count={Count})", toEmails?.Count ?? 0);
                throw;
            }
        }

        public async Task SendAdminNotificationAsync(string action, string details, string performedBy)
        {
            var subject = $"[IT Stock Management] {action}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2 style='color: #2c3e50;'>Action Notification</h2>
                    <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px;'>
                        <p><strong>Action:</strong> {action}</p>
                        <p><strong>Performed By:</strong> {performedBy}</p>
                        <p><strong>Details:</strong></p>
                        <div style='background-color: white; padding: 10px; border-left: 3px solid #3498db;'>
                            {details}
                        </div>
                        <p style='margin-top: 15px; color: #7f8c8d;'>
                            <small>Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</small>
                        </p>
                    </div>
                </body>
                </html>";

            _logger.LogInformation("Sending admin notification {@AdminNotificationMeta}", new { To = _adminEmail, Subject = subject, PerformedBy = performedBy });
            try
            {
                await SendEmailAsync(_adminEmail, subject, body);
                _logger.LogInformation("Admin notification sent {@AdminSendResult}", new { To = _adminEmail, Subject = subject });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send admin notification to {AdminEmail}", _adminEmail);
                throw;
            }
        }
    }
}
