using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ITStockM.Services.Implementation
{
    public class SmtpHealthResult
    {
        public bool Connected { get; set; }
        public string Message { get; set; }
    }

    public class SmtpHealthChecker
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpHealthChecker> _logger;

        public SmtpHealthChecker(IConfiguration configuration, ILogger<SmtpHealthChecker> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SmtpHealthResult> CheckConnectivityAsync(CancellationToken cancellationToken = default)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? _configuration["EmailSettings__SmtpServer"];
            var smtpPortRaw = _configuration["EmailSettings:SmtpPort"] ?? _configuration["EmailSettings__SmtpPort"] ?? "25";
            if (!int.TryParse(smtpPortRaw, out var smtpPort)) smtpPort = 25;

            if (string.IsNullOrEmpty(smtpServer))
            {
                _logger.LogWarning("SMTP server not configured");
                return new SmtpHealthResult { Connected = false, Message = "Smtp server not configured" };
            }

            _logger.LogDebug("Checking SMTP connectivity to {Server}:{Port}", smtpServer, smtpPort);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                using var tcp = new TcpClient();
                var connectTask = tcp.ConnectAsync(smtpServer, smtpPort);
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var completed = await Task.WhenAny(connectTask, Task.Delay(TimeSpan.FromSeconds(5), cts.Token));

                if (completed != connectTask || !tcp.Connected)
                {
                    sw.Stop();
                    var msg = $"Unable to connect to {smtpServer}:{smtpPort} within timeout ({sw.ElapsedMilliseconds} ms)";
                    _logger.LogWarning(msg);
                    return new SmtpHealthResult { Connected = false, Message = msg };
                }

                sw.Stop();
                var success = new SmtpHealthResult { Connected = true, Message = $"Connected to {smtpServer}:{smtpPort} in {sw.ElapsedMilliseconds} ms" };
                _logger.LogInformation("SMTP connectivity OK: {msg}", success.Message);
                return success;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "SMTP connectivity check failed after {Ms} ms", sw.ElapsedMilliseconds);
                return new SmtpHealthResult { Connected = false, Message = ex.Message };
            }
        }

        public async Task<SmtpHealthResult> SendTestEmailAsync(Services.Interfaces.IEmailService emailService, string to, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Attempting to send SMTP test email to {Recipient}", to);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await emailService.SendEmailAsync(to, "[SMTP Test] ITStockM", "This is a test message from ITStockM SMTP health check.");
                sw.Stop();
                _logger.LogInformation("SMTP test email sent to {Recipient} in {Ms} ms", to, sw.ElapsedMilliseconds);
                return new SmtpHealthResult { Connected = true, Message = "Test email sent" };
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogWarning(ex, "SMTP test send failed to {Recipient} after {Ms} ms", to, sw.ElapsedMilliseconds);
                return new SmtpHealthResult { Connected = false, Message = ex.Message };
            }
        }
    }
}