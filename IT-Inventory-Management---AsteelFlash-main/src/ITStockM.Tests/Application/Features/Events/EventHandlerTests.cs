using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using ITStockM.Application.Features.Events;
using ITStockM.Application.Features.Events.Handlers;
using ITStockM.Domain.Events;
using ITStockM.Domain.Base;

namespace ITStockM.Tests.Application.Features.Events;

public class EventHandlerTests
{
    private readonly Mock<ILogger<AssetLifecycleEventHandler>> _assetLifecycleLoggerMock;
    private readonly Mock<ILogger<MaintenanceTicketEventHandler>> _maintenanceLoggerMock;
    private readonly Mock<ILogger<AssetAssignmentEventHandler>> _assignmentLoggerMock;
    private readonly Mock<ILogger<WarrantyEventHandler>> _warrantyLoggerMock;
    private readonly Mock<ILogger<AssetDisposalEventHandler>> _disposalLoggerMock;

    public EventHandlerTests()
    {
        _assetLifecycleLoggerMock = new Mock<ILogger<AssetLifecycleEventHandler>>();
        _maintenanceLoggerMock = new Mock<ILogger<MaintenanceTicketEventHandler>>();
        _assignmentLoggerMock = new Mock<ILogger<AssetAssignmentEventHandler>>();
        _warrantyLoggerMock = new Mock<ILogger<WarrantyEventHandler>>();
        _disposalLoggerMock = new Mock<ILogger<AssetDisposalEventHandler>>();
    }

    #region AssetLifecycleEventHandler Tests

    [Fact]
    public async Task AssetLifecycleEventHandler_Handle_AssetTransitionEvent_Succeeds()
    {
        // Arrange
        var handler = new AssetLifecycleEventHandler(_assetLifecycleLoggerMock.Object);
        var assetLifecycleEvent = new AssetLifecycleChangedEvent(
            materielId: 1,
            previousStage: "Active",
            newStage: "Under Maintenance",
            reason: "Preventive maintenance"
        );
        var notification = new AssetLifecycleChangedNotification(assetLifecycleEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(1, notification.MaterielId);
        Assert.Equal("Active", notification.OldStage);
        Assert.Equal("Under Maintenance", notification.NewStage);
        Assert.Equal("Preventive maintenance", notification.Reason);

        _assetLifecycleLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AssetLifecycleEventHandler_Handle_WithoutReason_Succeeds()
    {
        // Arrange
        var handler = new AssetLifecycleEventHandler(_assetLifecycleLoggerMock.Object);
        var assetLifecycleEvent = new AssetLifecycleChangedEvent(
            materielId: 2,
            previousStage: "Stored",
            newStage: "Distributed",
            reason: null
        );
        var notification = new AssetLifecycleChangedNotification(assetLifecycleEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.Null(notification.Reason);
    }

    #endregion

    #region MaintenanceTicketEventHandler Tests

    [Fact]
    public async Task MaintenanceTicketEventHandler_Handle_TicketCreatedEvent_Succeeds()
    {
        // Arrange
        var handler = new MaintenanceTicketEventHandler(_maintenanceLoggerMock.Object);
        var maintenanceEvent = new MaintenanceTicketCreatedEvent(
            ticketId: 10,
            materielId: 5,
            description: "Replace broken screen"
        );
        var notification = new MaintenanceTicketCreatedNotification(maintenanceEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(10, notification.TicketId);
        Assert.Equal(5, notification.MaterielId);
        Assert.Equal("Replace broken screen", notification.Description);

        _maintenanceLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region AssetAssignmentEventHandler Tests

    [Fact]
    public async Task AssetAssignmentEventHandler_Handle_AssetAssignedEvent_Succeeds()
    {
        // Arrange
        var handler = new AssetAssignmentEventHandler(_assignmentLoggerMock.Object);
        var assignmentEvent = new AssetAssignedEvent(
            materielId: 3,
            assignmentId: 15,
            assignedToEmployeeId: 7,
            assignedByEmployeeId: 1
        );
        var notification = new AssetAssignedNotification(assignmentEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(3, notification.MaterielId);
        Assert.Equal(7, notification.EmployeeId);

        _assignmentLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    #endregion

    #region WarrantyEventHandler Tests

    [Fact]
    public async Task WarrantyEventHandler_Handle_WarrantyExpiringEvent_Succeeds()
    {
        // Arrange
        var handler = new WarrantyEventHandler(_warrantyLoggerMock.Object);
        var futureDate = DateTime.UtcNow.AddDays(30);
        var warrantyEvent = new WarrantyExpiringEvent(
            materielId: 4,
            warrantyEndDate: futureDate
        );
        var notification = new WarrantyExpiringNotification(warrantyEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(4, notification.MaterielId);
        Assert.InRange(notification.DaysUntilExpiry, 29, 30);

        _warrantyLoggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task WarrantyEventHandler_Handle_WarrantyExpiring_CriticalThreshold()
    {
        // Arrange - Test critical threshold (< 7 days)
        var handler = new WarrantyEventHandler(_warrantyLoggerMock.Object);
        var criticalDate = DateTime.UtcNow.AddDays(3);
        var criticalEvent = new WarrantyExpiringEvent(
            materielId: 8,
            warrantyEndDate: criticalDate
        );
        var notification = new WarrantyExpiringNotification(criticalEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert - Should log critical warning
        Assert.InRange(notification.DaysUntilExpiry, 2, 3);
    }

    #endregion

    #region AssetDisposalEventHandler Tests

    [Fact]
    public async Task AssetDisposalEventHandler_Handle_AssetDisposedEvent_Succeeds()
    {
        // Arrange
        var handler = new AssetDisposalEventHandler(_disposalLoggerMock.Object);
        var disposalEvent = new AssetDisposedEvent(
            materielId: 6,
            reason: "Obsolete hardware",
            disposalMethod: "Recycling"
        );
        var notification = new AssetDisposedNotification(disposalEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(6, notification.MaterielId);
        Assert.Equal("Obsolete hardware", notification.DisposalReason);
        Assert.Equal("Recycling", notification.DisposalMethod);

        _disposalLoggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task AssetDisposalEventHandler_Handle_WithoutReason_Succeeds()
    {
        // Arrange
        var handler = new AssetDisposalEventHandler(_disposalLoggerMock.Object);
        var disposalEvent = new AssetDisposedEvent(
            materielId: 7,
            reason: null,
            disposalMethod: "Donation"
        );
        var notification = new AssetDisposedNotification(disposalEvent);

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        Assert.Equal("Not specified", notification.DisposalReason);
    }

    #endregion

    #region Event Notification Adapter Tests

    [Fact]
    public void AssetLifecycleChangedNotification_CreatesCorrectly()
    {
        // Arrange & Act
        var @event = new AssetLifecycleChangedEvent(1, "Old", "New", "Test");
        var notification = new AssetLifecycleChangedNotification(@event);

        // Assert
        Assert.Equal(@event, notification.Event);
        Assert.Equal(1, notification.MaterielId);
    }

    [Fact]
    public void MaintenanceTicketCreatedNotification_CreatesCorrectly()
    {
        // Arrange & Act
        var @event = new MaintenanceTicketCreatedEvent(1, 2, "Test");
        var notification = new MaintenanceTicketCreatedNotification(@event);

        // Assert
        Assert.Equal(@event, notification.Event);
        Assert.Equal(1, notification.TicketId);
    }

    [Fact]
    public void AssetAssignedNotification_CreatesCorrectly()
    {
        // Arrange & Act
        var @event = new AssetAssignedEvent(1, 2, 3, 4);
        var notification = new AssetAssignedNotification(@event);

        // Assert
        Assert.Equal(@event, notification.Event);
        Assert.Equal(1, notification.MaterielId);
    }

    [Fact]
    public void WarrantyExpiringNotification_CreatesCorrectly()
    {
        // Arrange & Act
        var @event = new WarrantyExpiringEvent(1, DateTime.UtcNow.AddDays(30));
        var notification = new WarrantyExpiringNotification(@event);

        // Assert
        Assert.Equal(@event, notification.Event);
        Assert.Equal(1, notification.MaterielId);
    }

    [Fact]
    public void AssetDisposedNotification_CreatesCorrectly()
    {
        // Arrange & Act
        var @event = new AssetDisposedEvent(1, "Test", "Recycling");
        var notification = new AssetDisposedNotification(@event);

        // Assert
        Assert.Equal(@event, notification.Event);
        Assert.Equal(1, notification.MaterielId);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task EventHandlers_HandleNullCancellationToken_Succeeds()
    {
        // Arrange
        var handler = new AssetLifecycleEventHandler(_assetLifecycleLoggerMock.Object);
        var @event = new AssetLifecycleChangedEvent(1, "Old", "New", "Test");
        var notification = new AssetLifecycleChangedNotification(@event);

        // Act & Assert - Should not throw
        await handler.Handle(notification, default);
    }

    [Fact]
    public async Task EventHandlers_HandleCancelledToken_Completes()
    {
        // Arrange
        var handler = new AssetLifecycleEventHandler(_assetLifecycleLoggerMock.Object);
        var @event = new AssetLifecycleChangedEvent(1, "Old", "New", "Test");
        var notification = new AssetLifecycleChangedNotification(@event);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Should complete even with cancelled token
        await handler.Handle(notification, cts.Token);
    }

    #endregion
}
