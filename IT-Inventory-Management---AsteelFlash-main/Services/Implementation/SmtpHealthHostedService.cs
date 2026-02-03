using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ITStockM.Services.Implementation;

namespace ITStockM.Services.Implementation
{
    public class SmtpHealthHostedService : IHostedService
    {
        private readonly ILogger<SmtpHealthHostedService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly SmtpHealthChecker _checker;
        private readonly IServiceProvider _serviceProvider;

        public SmtpHealthHostedService(ILogger<SmtpHealthHostedService> logger, IConfiguration configuration, IWebHostEnvironment env, SmtpHealthChecker checker, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
            _checker = checker;
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Run only in Development AND when explicitly enabled
            var enabled = _configuration["HealthChecks:Smtp:Enabled"] ?? _configuration["SMTP_HEALTH_CHECK_ON_STARTUP"];
            if (!(_env.IsDevelopment() || (_configuration["DEV_ALLOW_TEST_EMAIL"] == "true")) || string.IsNullOrEmpty(enabled) || enabled.ToLower() != "true")
            {
                _logger.LogInformation("SMTP health check on startup is disabled.");
                return;
            }

            _logger.LogInformation("Running SMTP health check on startup...");
            var connectivity = await _checker.CheckConnectivityAsync(cancellationToken);
            if (!connectivity.Connected)
            {
                _logger.LogWarning("SMTP connectivity check failed at startup: {msg}", connectivity.Message);
            }
            else
            {
                _logger.LogInformation("SMTP connectivity OK: {msg}", connectivity.Message);
                var sendIfAllowed = (_configuration["SMTP_HEALTH_CHECK_SEND"] ?? "false").ToLower() == "true";
                if (sendIfAllowed)
                {
                    var admin = _configuration["EmailSettings:AdminEmail"] ?? _configuration["EmailSettings__AdminEmail"];
                    if (!string.IsNullOrEmpty(admin))
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<Services.Interfaces.IEmailService>();
                        var sendRes = await _checker.SendTestEmailAsync(emailService, admin, cancellationToken);
                        if (sendRes.Connected)
                        {
                            _logger.LogInformation("SMTP startup send succeeded");
                        }
                        else
                        {
                            _logger.LogWarning("SMTP startup send failed: {msg}", sendRes.Message);
                        }
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}