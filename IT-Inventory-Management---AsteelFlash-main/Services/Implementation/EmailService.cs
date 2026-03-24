using ITStockM.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ITStockM.Services.Implementation
{
    /// <summary>
    /// Delivers email via MailKit.
    /// SRP: only responsible for transport — HTML bodies come from <see cref="IEmailTemplateService"/>.
    /// Uses <see cref="EmailOptions"/> (IOptions pattern) for configuration.
    /// Includes exponential-backoff retry for transient SMTP failures.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _opts;
        private readonly IEmailTemplateService _templates;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailOptions> options,
            IEmailTemplateService templates,
            ILogger<EmailService> logger)
        {
            _opts      = options.Value;
            _templates = templates;
            _logger    = logger;

            _logger.LogInformation(
                "EmailService ready — server={Server}:{Port} from={From}",
                _opts.SmtpServer, _opts.SmtpPort, _opts.FromEmail);
        }

        // ─── Public interface ────────────────────────────────────────────────────

        public Task SendEmailAsync(string toEmail, string subject, string body)
            => SendWithRetryAsync(BuildMessage([toEmail], subject, body), subject);

        public Task SendEmailToMultipleAsync(List<string> toEmails, string subject, string body)
            => SendWithRetryAsync(BuildMessage(toEmails, subject, body), subject);

        public async Task SendAdminNotificationAsync(string action, string details, string performedBy)
        {
            if (string.IsNullOrWhiteSpace(_opts.AdminEmail))
            {
                _logger.LogWarning("AdminEmail is not configured — skipping admin notification for action='{Action}'", action);
                return;
            }

            var subject = $"[IT Stock Management] {action}";
            var body    = _templates.BuildActionNotification(action, details, performedBy);
            await SendEmailAsync(_opts.AdminEmail, subject, body);
        }

        // ─── Private helpers ─────────────────────────────────────────────────────

        private MimeMessage BuildMessage(IEnumerable<string> recipients, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress("IT Stock Management", _opts.FromEmail));
            foreach (var r in recipients)
                msg.To.Add(MailboxAddress.Parse(r));
            msg.Subject = subject;
            msg.Body    = new TextPart("html") { Text = htmlBody };
            return msg;
        }

        private async Task SendWithRetryAsync(MimeMessage message, string subjectForLog)
        {
            var attempt = 0;
            var delay   = _opts.RetryDelayMs;

            while (true)
            {
                attempt++;
                try
                {
                    await DeliverAsync(message);
                    _logger.LogInformation(
                        "Email delivered on attempt {Attempt}: subject='{Subject}' to=[{To}]",
                        attempt, subjectForLog, string.Join(',', message.To));
                    return;
                }
                catch (Exception ex) when (attempt < _opts.MaxRetryAttempts)
                {
                    _logger.LogWarning(ex,
                        "SMTP delivery failed (attempt {Attempt}/{Max}) — retrying in {Delay}ms. Subject='{Subject}'",
                        attempt, _opts.MaxRetryAttempts, delay, subjectForLog);
                    await Task.Delay(delay);
                    delay *= 2; // exponential back-off
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "SMTP delivery failed after {Max} attempts. Subject='{Subject}'",
                        _opts.MaxRetryAttempts, subjectForLog);
                    throw;
                }
            }
        }

        /// <summary>Opens a connection, sends, disconnects. One responsibility only.</summary>
        private async Task DeliverAsync(MimeMessage message)
        {
            using var client = new SmtpClient();

            var ssl = _opts.SmtpPort switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                25  => SecureSocketOptions.None,
                _   => SecureSocketOptions.StartTls
            };

            await client.ConnectAsync(_opts.SmtpServer, _opts.SmtpPort, ssl);

            if (!string.IsNullOrEmpty(_opts.Password))
                await client.AuthenticateAsync(_opts.FromEmail, _opts.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
