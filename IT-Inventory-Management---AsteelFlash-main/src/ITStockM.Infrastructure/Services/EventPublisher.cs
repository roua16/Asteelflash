using ITStockM.Application.Features.Events;
using ITStockM.Domain.Base;
using ITStockM.Domain.Events;
using MediatR;

namespace ITStockM.Infrastructure.Services;

/// <summary>
/// Publishes domain events to MediatR for processing by registered handlers.
/// This service implements the Bridge pattern between Domain and Application layers.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a single domain event.
    /// </summary>
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes multiple domain events.
    /// </summary>
    Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

public class EventPublisher : IEventPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<EventPublisher> _logger;

    public EventPublisher(IMediator mediator, ILogger<EventPublisher> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));

        await PublishAsync(new[] { domainEvent }, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        if (domainEvents == null)
            throw new ArgumentNullException(nameof(domainEvents));

        var eventsList = domainEvents.ToList();

        _logger.LogInformation("📢 Publishing {EventCount} domain event(s)", eventsList.Count);

        foreach (var domainEvent in eventsList)
        {
            var notification = CreateNotification(domainEvent);
            if (notification != null)
            {
                try
                {
                    _logger.LogInformation("🔔 Publishing {EventType}", domainEvent.GetType().Name);
                    await _mediator.Publish(notification, cancellationToken);
                    domainEvent.IsPublished = true;
                    _logger.LogInformation("✓ {EventType} published successfully", domainEvent.GetType().Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error publishing {EventType}", domainEvent.GetType().Name);
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Creates an appropriate MediatR notification wrapper for a domain event.
    /// This maps domain events to their corresponding notification classes.
    /// </summary>
    private INotification? CreateNotification(DomainEvent domainEvent)
    {
        return domainEvent switch
        {
            AssetLifecycleChangedEvent evt => new AssetLifecycleChangedNotification(evt),
            MaintenanceTicketCreatedEvent evt => new MaintenanceTicketCreatedNotification(evt),
            AssetAssignedEvent evt => new AssetAssignedNotification(evt),
            WarrantyExpiringEvent evt => new WarrantyExpiringNotification(evt),
            AssetDisposedEvent evt => new AssetDisposedNotification(evt),
            _ => null
        };
    }
}
