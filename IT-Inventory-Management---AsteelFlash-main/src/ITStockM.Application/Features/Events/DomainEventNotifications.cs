using ITStockM.Domain.Base;
using ITStockM.Domain.Events;
using MediatR;

namespace ITStockM.Application.Features.Events;

/// <summary>
/// MediatR notification adapter for domain events.
/// Allows domain events to be published through MediatR without violating dependency rules.
/// Domain layer has no dependency on MediatR or Application layer.
/// </summary>
public class DomainEventNotification : INotification
{
    public DomainEventNotification(DomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public DomainEvent DomainEvent { get; }
}

/// <summary>
/// Notification wrapper for AssetLifecycleChangedEvent.
/// </summary>
public class AssetLifecycleChangedNotification : INotification
{
    public AssetLifecycleChangedNotification(AssetLifecycleChangedEvent @event)
    {
        Event = @event;
    }

    public AssetLifecycleChangedEvent Event { get; }
    public int MaterielId => Event.MaterielId;
    public string OldStage => Event.PreviousStage;
    public string NewStage => Event.NewStage;
    public string? Reason => Event.Reason;
}

/// <summary>
/// Notification wrapper for MaintenanceTicketCreatedEvent.
/// </summary>
public class MaintenanceTicketCreatedNotification : INotification
{
    public MaintenanceTicketCreatedNotification(MaintenanceTicketCreatedEvent @event)
    {
        Event = @event;
    }

    public MaintenanceTicketCreatedEvent Event { get; }
    public int TicketId => Event.TicketId;
    public int MaterielId => Event.MaterielId;
    public string Description => Event.Description ?? "No description";
    public string Priority => "Normal"; // Default, could be enhanced
}

/// <summary>
/// Notification wrapper for AssetAssignedEvent.
/// </summary>
public class AssetAssignedNotification : INotification
{
    public AssetAssignedNotification(AssetAssignedEvent @event)
    {
        Event = @event;
    }

    public AssetAssignedEvent Event { get; }
    public int MaterielId => Event.MaterielId;
    public int EmployeeId => Event.AssignedToEmployeeId;
    public string AssetName => "Unknown Asset"; // Would need DB lookup
    public DateTime AssignmentDate => Event.AssignmentDate;
    public string? AssignmentLocation => null; // Would need DB lookup
}

/// <summary>
/// Notification wrapper for WarrantyExpiringEvent.
/// </summary>
public class WarrantyExpiringNotification : INotification
{
    public WarrantyExpiringNotification(WarrantyExpiringEvent @event)
    {
        Event = @event;
    }

    public WarrantyExpiringEvent Event { get; }
    public int MaterielId => Event.MaterielId;
    public string AssetName => "Unknown Asset"; // Would need DB lookup
    public DateTime WarrantyExpirationDate => Event.WarrantyEndDate;
    public string WarrantyProvider => "Unknown"; // Would need DB lookup
    public int DaysUntilExpiry => Event.DaysUntilExpiry;
}

/// <summary>
/// Notification wrapper for AssetDisposedEvent.
/// </summary>
public class AssetDisposedNotification : INotification
{
    public AssetDisposedNotification(AssetDisposedEvent @event)
    {
        Event = @event;
    }

    public AssetDisposedEvent Event { get; }
    public int MaterielId => Event.MaterielId;
    public string AssetName => "Unknown Asset"; // Would need DB lookup
    public DateTime DisposalDate => Event.DisposalDate;
    public string DisposalMethod => Event.DisposalMethod ?? "Unknown";
    public string DisposalReason => Event.Reason ?? "Not specified";
    public decimal OriginalValue => 0m; // Would need DB lookup
    public decimal SalvageValue => 0m; // Would need DB lookup
}
