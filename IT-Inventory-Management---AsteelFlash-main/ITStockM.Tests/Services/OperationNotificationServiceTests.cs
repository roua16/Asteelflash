using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

using ITStockM.Services.Implementation;
using ITStockM.Services.Interfaces;
using ITStockM.Models.ViewModels;
using ITStockM.Data;
using Microsoft.EntityFrameworkCore;
using ITStockM.Domain.Entities;

namespace ITStockM.Tests.Services
{
    public class OperationNotificationServiceTests
    {
        private ITStockManagmentContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ITStockManagmentContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new ITStockManagmentContext(options);
        }

        [Fact]
        public async Task NotifyTopUsedMateriels_SendsEmail_Then_IsSuppressed_OnSecondCall()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_db_1");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var items = new List<MaterielUsageViewModel>
            {
                new MaterielUsageViewModel { Id = 1, MaterielName = "A", UsageCount = 10, UsageShare = 0.5 },
                new MaterielUsageViewModel { Id = 2, MaterielName = "B", UsageCount = 6, UsageShare = 0.3 },
                new MaterielUsageViewModel { Id = 3, MaterielName = "C", UsageCount = 4, UsageShare = 0.2 }
            };

            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var to = "user@example.com";

            // Act - first call should send
            await svc.NotifyTopUsedMateriels(items, to);

            // Act - second call (same day) should be suppressed by cache
            await svc.NotifyTopUsedMateriels(items, to);

            // Assert - SendEmailAsync invoked only once
            mockEmail.Verify(e => e.SendEmailAsync(It.Is<string>(s => s == to), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task NotifyLowStockSummary_SendsEmail_Then_IsSuppressed_OnSecondCall()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_lowstock_1");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var lowMateriels = new List<Materiel>
            {
                new Materiel { Id = 1, MaterielName = "Consumable A", QuantityITStock = 2, QuantityPDRStock = 0 }
            };

            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var to = "user@example.com";

            // Act - first call should send
            await svc.NotifyLowStockSummary(lowMateriels, to);

            // Act - second call (same day) should be suppressed by cache
            await svc.NotifyLowStockSummary(lowMateriels, to);

            // Assert - SendEmailAsync invoked only once
            mockEmail.Verify(e => e.SendEmailAsync(It.Is<string>(s => s == to), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task NotifyLowStockSummary_SendsEmail_WithCorrectContent()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_lowstock_content");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var lowMateriels = new List<Materiel>
            {
                new Materiel { Id = 1, MaterielName = "Cable", QuantityITStock = 1, QuantityPDRStock = 0 },
                new Materiel { Id = 2, MaterielName = "Adapter", QuantityITStock = 0, QuantityPDRStock = 1 }
            };

            string? capturedSubject = null;
            string? capturedBody = null;

            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((to, subject, body) =>
                {
                    capturedSubject = subject;
                    capturedBody = body;
                })
                .Returns(Task.CompletedTask);

            var to = "admin@example.com";

            // Act
            await svc.NotifyLowStockSummary(lowMateriels, to);

            // Assert - verify email content
            mockEmail.Verify(e => e.SendEmailAsync(It.Is<string>(s => s == to), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

            capturedSubject.Should().Contain("Low Stock Summary");
            capturedBody.Should().Contain("Cable");
            capturedBody.Should().Contain("Adapter");
            capturedBody.Should().Contain("IT Stock");
        }

        [Fact]
        public async Task NotifyLowStockSummary_ResetsCache_NextDay()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_lowstock_next_day");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var lowMateriels = new List<Materiel>
            {
                new Materiel { Id = 1, MaterielName = "Part A", QuantityITStock = 1, QuantityPDRStock = 0 }
            };

            int callCount = 0;
            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => callCount++)
                .Returns(Task.CompletedTask);

            var to = "user@example.com";

            // Simulate first call
            await svc.NotifyLowStockSummary(lowMateriels, to);
            callCount.Should().Be(1);

            // Manually clear cache key to simulate next day
            var cacheKey = $"LowStockSummarySent:{to}:{DateTime.UtcNow:yyyyMMdd}";
            memoryCache.Remove(cacheKey);

            // Act - call again (simulating next day)
            await svc.NotifyLowStockSummary(lowMateriels, to);

            // Assert - email should be sent again
            callCount.Should().Be(2);
        }
        [Fact]
        public async Task NotifyTopUsedMateriels_DoesNotThrow_For_EmptyList()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_db_2");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            // Act / Assert - should simply return without calling email
            await svc.NotifyTopUsedMateriels(new List<MaterielUsageViewModel>(), "u@e.com");
            mockEmail.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task NotifyTopUsedMateriels_SendsEmail_WithCorrectContent()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_db_content");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var items = new List<MaterielUsageViewModel>
            {
                new MaterielUsageViewModel { Id = 1, MaterielName = "Laptop", UsageCount = 50, UsageShare = 0.5 },
                new MaterielUsageViewModel { Id = 2, MaterielName = "Mouse", UsageCount = 30, UsageShare = 0.3 },
                new MaterielUsageViewModel { Id = 3, MaterielName = "Keyboard", UsageCount = 20, UsageShare = 0.2 }
            };

            string? capturedSubject = null;
            string? capturedBody = null;

            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string, string>((to, subject, body) =>
                {
                    capturedSubject = subject;
                    capturedBody = body;
                })
                .Returns(Task.CompletedTask);

            var to = "admin@example.com";

            // Act
            await svc.NotifyTopUsedMateriels(items, to);

            // Assert - verify email content
            mockEmail.Verify(e => e.SendEmailAsync(It.Is<string>(s => s == to), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            
            capturedSubject.Should().Contain("Top 3 Most Used Materials");
            capturedBody.Should().Contain("Laptop");
            capturedBody.Should().Contain("50");
            capturedBody.Should().MatchRegex(@"50[\.,]0%");
            capturedBody.Should().Contain("Mouse");
            capturedBody.Should().MatchRegex(@"30[\.,]0%");
            capturedBody.Should().Contain("Keyboard");
            capturedBody.Should().MatchRegex(@"20[\.,]0%");
        }

        [Fact]
        public async Task NotifyTopUsedMateriels_DifferentRecipients_CanBothReceive()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_db_different_recipients");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var items = new List<MaterielUsageViewModel>
            {
                new MaterielUsageViewModel { Id = 1, MaterielName = "A", UsageCount = 10, UsageShare = 1.0 }
            };

            // Count how many times SendEmailAsync is called
            int callCount = 0;
            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => callCount++)
                .Returns(Task.CompletedTask);

            // Act - send to first user
            await svc.NotifyTopUsedMateriels(items, "admin@example.com");
            
            // Act - send to second user (different email)
            await svc.NotifyTopUsedMateriels(items, "pdr@example.com");

            // Assert - both should receive emails since they're different recipients
            callCount.Should().Be(2);
        }

        [Fact]
        public async Task NotifyTopUsedMateriels_ResetsCache_NextDay()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_db_next_day");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var items = new List<MaterielUsageViewModel>
            {
                new MaterielUsageViewModel { Id = 1, MaterielName = "A", UsageCount = 10, UsageShare = 1.0 }
            };

            int callCount = 0;
            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback(() => callCount++)
                .Returns(Task.CompletedTask);

            var to = "user@example.com";

            // Simulate first call
            await svc.NotifyTopUsedMateriels(items, to);

            // Verify first call sent email
            callCount.Should().Be(1);

            // Simulate next day by clearing the cache (in real scenario, cache expires at midnight UTC)
            var cacheKey = $"TopUsedSent:{to}:{DateTime.UtcNow:yyyyMMdd}";
            
            // Manually expire the cache entry to simulate next day
            memoryCache.Remove(cacheKey);

            // Act - call again (simulating next day)
            await svc.NotifyTopUsedMateriels(items, to);

            // Assert - email should be sent again (cache was cleared)
            callCount.Should().Be(2);
        }

        [Fact]
        public async Task NotifyTopUsedMateriels_LogsWarning_OnEmailFailure()
        {
            // Arrange
            var ctx = CreateInMemoryContext("opnotif_db_email_fail");
            var mockEmail = new Mock<IEmailService>();
            var mockLogger = new Mock<ILogger<OperationNotificationService>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            var svc = new OperationNotificationService(mockEmail.Object, mockLogger.Object, ctx, memoryCache);

            var items = new List<MaterielUsageViewModel>
            {
                new MaterielUsageViewModel { Id = 1, MaterielName = "A", UsageCount = 10, UsageShare = 1.0 }
            };

            // Setup email to throw exception
            mockEmail.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP error"));

            var to = "user@example.com";

            // Act & Assert - should not throw
            await svc.NotifyTopUsedMateriels(items, to);

            // Verify logger was called with warning
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
                Times.AtLeastOnce);
        }
    }
}
