using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ITStockM.Services.Interfaces;
using ITStockM.Services.Implementation;

namespace ITStockM.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<DevController> _logger;
        private readonly IHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly SmtpHealthChecker _smtpHealthChecker;

        public DevController(IEmailService emailService, ILogger<DevController> logger, IHostEnvironment env, IConfiguration config, SmtpHealthChecker smtpHealthChecker)
        {
            _emailService = emailService;
            _logger = logger;
            _env = env;
            _config = config;
            _smtpHealthChecker = smtpHealthChecker;
        }

        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string? to = null)
        {
            // Allowed only in Development or when explicitly enabled via env var
            var allowInProd = _config["DEV_ALLOW_TEST_EMAIL"];
            if (!_env.IsDevelopment() && string.IsNullOrEmpty(allowInProd))
            {
                return Forbid();
            }

            var recipient = to ?? _config["EmailSettings:AdminEmail"] ?? _config["EmailSettings__AdminEmail"];
            if (string.IsNullOrEmpty(recipient))
            {
                return BadRequest("No recipient configured. Set EmailSettings:AdminEmail or pass ?to=you@example.com");
            }

            try
            {
                var subject = "[DEV] SMTP Test from ITStockM";
                var body = "This is a test email sent by DevController to verify SMTP configuration.";
                await _emailService.SendEmailAsync(recipient, subject, body);
                _logger.LogInformation("Test email sent to {recipient}", recipient);
                return Ok(new { success = true, to = recipient });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending test email");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpGet("smtp-health")]
        public async Task<IActionResult> GetSmtpHealth([FromQuery] bool sendTestEmail = false)
        {
            var allowInProd = _config["DEV_ALLOW_TEST_EMAIL"];
            if (!_env.IsDevelopment() && string.IsNullOrEmpty(allowInProd))
            {
                return Forbid();
            }

            var result = await _smtpHealthChecker.CheckConnectivityAsync();
            if (sendTestEmail)
            {
                var recipient = _config["EmailSettings:AdminEmail"] ?? _config["EmailSettings__AdminEmail"];
                if (string.IsNullOrEmpty(recipient))
                    return BadRequest("No AdminEmail configured to send test");

                var sendRes = await _smtpHealthChecker.SendTestEmailAsync(_emailService, recipient);
                return Ok(new { connectivity = result, send = sendRes });
            }

            return Ok(result);
        }
    }
}