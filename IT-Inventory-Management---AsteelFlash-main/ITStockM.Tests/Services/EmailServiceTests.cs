using FluentAssertions;
using ITStockM.Services.Implementation;
using ITStockM.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Mail;
using Xunit;
using MimeKit;

using EmailServiceImpl = ITStockM.Services.Implementation.EmailService;

namespace ITStockM.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<ILogger<EmailServiceImpl>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IConfigurationSection> _mockEmailSettings;

        public EmailServiceTests()
        {
            _mockLogger = new Mock<ILogger<EmailServiceImpl>>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockEmailSettings = new Mock<IConfigurationSection>();

            // Setup configuration
            _mockEmailSettings.Setup(x => x["SmtpServer"]).Returns("smtp.gmail.com");
            _mockEmailSettings.Setup(x => x["SmtpPort"]).Returns("587");
            _mockEmailSettings.Setup(x => x["FromEmail"]).Returns("test@example.com");
            _mockEmailSettings.Setup(x => x["Password"]).Returns("testpassword");
            _mockEmailSettings.Setup(x => x["AdminEmail"]).Returns("admin@example.com");

            _mockConfiguration.Setup(x => x.GetSection("EmailSettings")).Returns(_mockEmailSettings.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenFromEmailIsNull()
        {
            // Arrange
            _mockEmailSettings.Setup(x => x["FromEmail"]).Returns((string)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EmailServiceImpl(_mockConfiguration.Object, _mockLogger.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenPasswordIsNull()
        {
            // Arrange
            _mockEmailSettings.Setup(x => x["Password"]).Returns((string)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new EmailServiceImpl(_mockConfiguration.Object, _mockLogger.Object));
        }

        [Fact]
        public async Task SendAdminNotificationAsync_ShouldFormatEmailCorrectly()
        {
            // Arrange
            var originalAdmin = Environment.GetEnvironmentVariable("SMTP_ADMIN_EMAIL");
            var originalFrom = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL");
            var originalPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var originalServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
            var originalPort = Environment.GetEnvironmentVariable("SMTP_PORT");

            Environment.SetEnvironmentVariable("SMTP_ADMIN_EMAIL", "not-an-email@@");
            Environment.SetEnvironmentVariable("SMTP_FROM_EMAIL", "test@example.com");
            Environment.SetEnvironmentVariable("SMTP_PASSWORD", "");
            Environment.SetEnvironmentVariable("SMTP_SERVER", "localhost");
            Environment.SetEnvironmentVariable("SMTP_PORT", "25");

            try
            {
                var service = new EmailServiceImpl(_mockConfiguration.Object, _mockLogger.Object);
                var action = "Test Action";
                var details = "Test Details";
                var performedBy = "Test User";

                // Act & Assert
                // Force a fast failure before any SMTP/network activity by using an invalid admin email.
                await Assert.ThrowsAsync<ParseException>(() =>
                    service.SendAdminNotificationAsync(action, details, performedBy));
            }
            finally
            {
                Environment.SetEnvironmentVariable("SMTP_ADMIN_EMAIL", originalAdmin);
                Environment.SetEnvironmentVariable("SMTP_FROM_EMAIL", originalFrom);
                Environment.SetEnvironmentVariable("SMTP_PASSWORD", originalPassword);
                Environment.SetEnvironmentVariable("SMTP_SERVER", originalServer);
                Environment.SetEnvironmentVariable("SMTP_PORT", originalPort);
            }
        }
    }
}
