using Microsoft.AspNetCore.Mvc;
using ITStockM.Services.Interfaces;

namespace ITStockM.Controllers
{
    /// <summary>
    /// Test controller for email functionality (development only)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;
        private readonly IWebHostEnvironment _env;

        public EmailTestController(
            IEmailService emailService,
            ILogger<EmailTestController> logger,
            IWebHostEnvironment env)
        {
            _emailService = emailService;
            _logger = logger;
            _env = env;
        }

        /// <summary>
        /// Send a test email (development only)
        /// </summary>
        [HttpGet("send-test")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string? to = null)
        {
            // Only allow in development
            if (!_env.IsDevelopment())
            {
                return NotFound();
            }

            var adminEmail = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL") ?? "admin@example.com";
            var recipient = to ?? adminEmail;

            _logger.LogInformation("Sending test email to {Recipient}", recipient);

            try
            {
                var subject = "IT Stock Management - Test Email";
                var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Email Configuration Test</h2>
                    <p>This is a test email from the IT Stock Management system.</p>
                    <p><strong>Timestamp:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                    <p><strong>Server:</strong> {Environment.GetEnvironmentVariable("SMTP_SERVER")}</p>
                    <p><strong>Port:</strong> {Environment.GetEnvironmentVariable("SMTP_PORT")}</p>
                    <hr/>
                    <p style='color: green;'>✓ If you're seeing this, email is working correctly!</p>
                </body>
                </html>";

                await _emailService.SendEmailAsync(recipient, subject, body);

                _logger.LogInformation("Test email sent successfully to {Recipient}", recipient);
                return Ok(new { success = true, message = $"Test email sent to {recipient}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test email to {Recipient}", recipient);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}
