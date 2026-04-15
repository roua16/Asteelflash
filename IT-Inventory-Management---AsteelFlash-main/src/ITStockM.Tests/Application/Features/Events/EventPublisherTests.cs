using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediatR;
using ITStockM.Application.Features.Events;
using ITStockM.Domain.Events;
using ITStockM.Domain.Base;
using ITStockM.Infrastructure.Services;

namespace ITStockM.Tests.Application.Features.Events;

public class EventPublisherTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<EventPublisher>> _loggerMock;
    private readonly EventPublisher _eventPublisher;

    public EventPublisherTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<EventPublisher>>();
        _eventPublisher = new EventPublisher(_mediatorMock.Object, _loggerMock.Object);
    }

    #region Single Event Publishing

    [Fact]
    public async Task PublishAsync_SingleAssetLifecycleEvent_PublishesSuccessfully()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent(1, "Active", "Maintenance", "Scheduled maintenance");
        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.True(@event.IsPublished);
    }

    [Fact]
    public async Task PublishAsync_SingleMaintenanceTicketEvent_PublishesSuccessfully()
    {
        // Arrange
        var @event = new MaintenanceTicketCreatedEvent(1, 5, "Replace screen");
        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.True(@event.IsPublished);
    }

    [Fact]
    public async Task PublishAsync_SingleAssetAssignedEvent_PublishesSuccessfully()
    {
        // Arrange
        var @event = new AssetAssignedEvent(1, 2, 3, 4);
        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.True(@event.IsPublished);
    }

    [Fact]
    public async Task PublishAsync_SingleWarrantyExpiringEvent_PublishesSuccessfully()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(30);
        var @event = new WarrantyExpiringEvent(1, futureDate);
        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.True(@event.IsPublished);
    }

    [Fact]
    public async Task PublishAsync_SingleAssetDisposedEvent_PublishesSuccessfully()
    {
        // Arrange
        var @event = new AssetDisposedEvent(1, "Obsolete", "Recycling");
        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.True(@event.IsPublished);
    }

    #endregion

    #region Multiple Events Publishing

    [Fact]
    public async Task PublishAsync_MultipleEvents_PublishesAllSuccessfully()
    {
        // Arrange
        var events = new DomainEvent[]
        {
            new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null),
            new MaintenanceTicketCreatedEvent(1, 5, "Test"),
            new AssetAssignedEvent(2, 3, 4, 5)
        };

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(events);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        foreach (var @event in events)
        {
            Assert.True(@event.IsPublished);
        }
    }

    [Fact]
    public async Task PublishAsync_EmptyEventList_CompletesSuccessfully()
    {
        // Arrange
        var events = new DomainEvent[] { };

        // Act
        await _eventPublisher.PublishAsync(events);

        // Assert - Should not publish anything
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task PublishAsync_MixedEventTypes_PublishesCorrectly()
    {
        // Arrange
        var lifecycleEvent = new AssetLifecycleChangedEvent(1, "Active", "Disposed", "EOL");
        var disposalEvent = new AssetDisposedEvent(1, "Obsolete", "Recycling");

        var events = new DomainEvent[] { lifecycleEvent, disposalEvent };

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(events);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task PublishAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        DomainEvent? nullEvent = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _eventPublisher.PublishAsync(nullEvent!));
    }

    [Fact]
    public async Task PublishAsync_NullEventList_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<DomainEvent>? nullEvents = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _eventPublisher.PublishAsync(nullEvents!));
    }

    [Fact]
    public async Task PublishAsync_MediatorThrowsException_PropagatesException()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null);
        var exception = new InvalidOperationException("MediatR error");

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _eventPublisher.PublishAsync(@event));
    }

    [Fact]
    public async Task PublishAsync_PartialFailure_StopsAtFirstError()
    {
        // Arrange
        var events = new DomainEvent[]
        {
            new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null),
            new MaintenanceTicketCreatedEvent(1, 5, "Test"),
        };

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Callback<INotification, CancellationToken>((notification, token) =>
            {
                if (notification is MaintenanceTicketCreatedNotification)
                {
                    throw new InvalidOperationException("Handler failed");
                }
            })
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _eventPublisher.PublishAsync(events));
    }

    #endregion

    #region CancellationToken Handling

    [Fact]
    public async Task PublishAsync_WithCancellationToken_PassesToMediator()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null);
        var cts = new CancellationTokenSource();

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event, cts.Token);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithDefaultCancellationToken_Succeeds()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null);

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event, default);

        // Assert
        _mediatorMock.Verify(
            x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Logging Verification

    [Fact]
    public async Task PublishAsync_LogsEventPublishing()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null);

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(@event);

        // Assert - Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task PublishAsync_LogsEventCount()
    {
        // Arrange
        var events = new DomainEvent[]
        {
            new AssetLifecycleChangedEvent(1, "Active", "Maintenance", null),
            new MaintenanceTicketCreatedEvent(1, 5, "Test"),
        };

        _mediatorMock.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventPublisher.PublishAsync(events);

        // Assert - Verify count logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeast(3)); // At least initial log + 2 event logs
    }

    #endregion

    #region Notification Creation

    [Fact]
    public void NotificationCreation_AssetLifecycleChanged_MapsCorrectly()
    {
        // Arrange
        var @event = new AssetLifecycleChangedEvent(1, "Old", "New", "Reason");

        // Act - Via reflection or direct instantiation
        var notification = new AssetLifecycleChangedNotification(@event);

        // Assert
        Assert.Equal("Old", notification.OldStage);
        Assert.Equal("New", notification.NewStage);
    }

    [Fact]
    public void NotificationCreation_WarrantyExpiring_IncludesDaysUntilExpiry()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(15);
        var @event = new WarrantyExpiringEvent(1, futureDate);

        // Act
        var notification = new WarrantyExpiringNotification(@event);

        // Assert
        Assert.InRange(notification.DaysUntilExpiry, 14, 15);
    }

    #endregion
}
