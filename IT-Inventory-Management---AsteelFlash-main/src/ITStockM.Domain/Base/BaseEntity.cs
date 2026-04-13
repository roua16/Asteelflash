namespace ITStockM.Domain.Base;

/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique entity identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Indicates if the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Collection of domain events raised by this entity.
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the domain events and clears the collection.
    /// </summary>
    public IReadOnlyList<DomainEvent> GetDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events.AsReadOnly();
    }

    /// <summary>
    /// Adds a domain event to the entity.
    /// </summary>
    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

/// <summary>
/// Base class for domain events.
/// </summary>
public abstract class DomainEvent
{
    public DateTime OccuredOn { get; } = DateTime.UtcNow;
    public bool IsPublished { get; set; }
}
