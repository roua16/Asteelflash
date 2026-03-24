using FluentAssertions;
using ITStockM.Services.Implementation;
using ITStockM.Services.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using MimeKit;
using Xunit;

using EmailServiceImpl = ITStockM.Services.Implementation.EmailService;

namespace ITStockM.Tests.Services
{
    public class EmailServiceTests
    {
        private static EmailServiceImpl CreateService(
            string adminEmail    = "admin@example.com",
            string fromEmail     = "sender@example.com",
            string smtpServer    = "localhost",
            int    smtpPort      = 25,
            Mock<IEmailTemplateService>? templatesMock = null)
        {
            var opts = Options.Create(new EmailOptions
            {
                AdminEmail  = adminEmail,
                FromEmail   = fromEmail,
                SmtpServer  = smtpServer,
                SmtpPort    = smtpPort,
                Password    = "",
                MaxRetryAttempts = 1,
                RetryDelayMs     = 0
            });

            templatesMock ??= new Mock<IEmailTemplateService>();
            return new EmailServiceImpl(opts, templatesMock.Object, NullLogger<EmailServiceImpl>.Instance);
        }

        // ─── AdminEmail skipping ──────────────────────────────────────────────────

        [Fact]
        public async Task SendAdminNotificationAsync_Skips_WhenAdminEmailIsEmpty()
        {
            var templatesMock = new Mock<IEmailTemplateService>();
            var svc = CreateService(adminEmail: "", templatesMock: templatesMock);

            // No SMTP connection attempted — since AdminEmail is empty the method returns early
            await svc.SendAdminNotificationAsync("action", "details", "user");

            templatesMock.Verify(
                t => t.BuildActionNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never,
                "template should not be built when AdminEmail is not configured");
        }

        // ─── Template delegation ──────────────────────────────────────────────────

        [Fact]
        public async Task SendAdminNotificationAsync_CallsTemplate_WithCorrectArgs()
        {
            var templatesMock = new Mock<IEmailTemplateService>();
            templatesMock
                .Setup(t => t.BuildActionNotification("Deploy", "v2.0", "Alice"))
                .Returns("<html>body</html>");

            // Service will try to deliver but fail on connection — that is fine for this test
            var svc = CreateService(templatesMock: templatesMock);

            // swallow any SmtpCommandException/SocketException — we only care the template was called
            try { await svc.SendAdminNotificationAsync("Deploy", "v2.0", "Alice"); }
            catch { /* SMTP delivery expected to fail in unit test */ }

            templatesMock.Verify(
                t => t.BuildActionNotification("Deploy", "v2.0", "Alice"),
                Times.Once);
        }

        // ─── Invalid recipient address ────────────────────────────────────────────

        [Fact]
        public async Task SendAdminNotificationAsync_Throws_WhenAdminEmailIsInvalidFormat()
        {
            var templatesMock = new Mock<IEmailTemplateService>();
            templatesMock
                .Setup(t => t.BuildActionNotification(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns("<html/>");

            var svc = CreateService(adminEmail: "not-an-email@@", templatesMock: templatesMock);

            Func<Task> act = () => svc.SendAdminNotificationAsync("action", "detail", "user");

            await act.Should().ThrowAsync<ParseException>("an invalid To address must cause a ParseException");
        }
    }
}
