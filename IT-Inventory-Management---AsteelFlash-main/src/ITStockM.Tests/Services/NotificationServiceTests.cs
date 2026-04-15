using FluentAssertions;
using ITStockM.Services.Implementation;
using ITStockM.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ITStockM.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ILogger<NotificationService>> _mockLogger;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _mockEmailService = new Mock<IEmailService>();
            _mockLogger = new Mock<ILogger<NotificationService>>();
            _notificationService = new NotificationService(_mockEmailService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task NotifyMaterialAssignmentAsync_ShouldCallEmailService()
        {
            // Arrange
            var assignmentId = 1;
            var assignedBy = "User1";
            var assignedTo = "User2";

            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifyMaterialAssignmentAsync(assignmentId, assignedBy, assignedTo);

            // Assert
            _mockEmailService.Verify(
                x => x.SendAdminNotificationAsync(
                    "Material Assignment Created",
                    It.Is<string>(s => s.Contains(assignmentId.ToString()) && s.Contains(assignedTo)),
                    assignedBy),
                Times.Once);
        }

        [Fact]
        public async Task NotifyDeliveryOrderCreatedAsync_ShouldCallEmailService()
        {
            // Arrange
            var orderNumber = "DO-001";
            var createdBy = "User1";

            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifyDeliveryOrderCreatedAsync(orderNumber, createdBy);

            // Assert
            _mockEmailService.Verify(
                x => x.SendAdminNotificationAsync(
                    "Delivery Order Created",
                    It.Is<string>(s => s.Contains(orderNumber)),
                    createdBy),
                Times.Once);
        }

        [Fact]
        public async Task NotifyRequestSubmittedAsync_ShouldCallEmailService()
        {
            // Arrange
            var requestId = 1;
            var requestedBy = "User1";

            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifyRequestSubmittedAsync(requestId, requestedBy);

            // Assert
            _mockEmailService.Verify(
                x => x.SendAdminNotificationAsync(
                    "New Request Submitted",
                    It.Is<string>(s => s.Contains(requestId.ToString())),
                    requestedBy),
                Times.Once);
        }

        [Fact]
        public async Task NotifyMaterialChangeAsync_ShouldCallEmailService()
        {
            // Arrange
            var action = "Added";
            var materialId = 1;
            var performedBy = "User1";

            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifyMaterialChangeAsync(action, materialId, performedBy);

            // Assert
            _mockEmailService.Verify(
                x => x.SendAdminNotificationAsync(
                    $"Material {action}",
                    It.Is<string>(s => s.Contains(materialId.ToString()) && s.Contains(action)),
                    performedBy),
                Times.Once);
        }

        [Fact]
        public async Task NotifySupplierChangeAsync_ShouldCallEmailService()
        {
            // Arrange
            var action = "Updated";
            var supplierName = "Supplier1";
            var performedBy = "User1";

            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifySupplierChangeAsync(action, supplierName, performedBy);

            // Assert
            _mockEmailService.Verify(
                x => x.SendAdminNotificationAsync(
                    $"Supplier {action}",
                    It.Is<string>(s => s.Contains(supplierName) && s.Contains(action)),
                    performedBy),
                Times.Once);
        }

        [Fact]
        public async Task NotifyProjectChangeAsync_ShouldCallEmailService()
        {
            // Arrange
            var action = "Created";
            var projectId = 1;
            var performedBy = "User1";

            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            await _notificationService.NotifyProjectChangeAsync(action, projectId, performedBy);

            // Assert
            _mockEmailService.Verify(
                x => x.SendAdminNotificationAsync(
                    $"Project {action}",
                    It.Is<string>(s => s.Contains(projectId.ToString()) && s.Contains(action)),
                    performedBy),
                Times.Once);
        }

        [Fact]
        public async Task NotificationService_ShouldHandleExceptions_GracefullyWithLogging()
        {
            // Arrange
            _mockEmailService
                .Setup(x => x.SendAdminNotificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Email service error"));

            // Act
            await _notificationService.NotifyMaterialAssignmentAsync(1, "User1", "User2");

            // Assert - Should not throw, but should log error
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception?>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
