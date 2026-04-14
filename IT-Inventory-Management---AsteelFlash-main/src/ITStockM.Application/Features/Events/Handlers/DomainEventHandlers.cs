using ITStockM.Application.Features.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ITStockM.Application.Features.Events.Handlers;

/// <summary>
/// Handles AssetLifecycleChanged events.
/// Triggered when an asset transitions through lifecycle stages.
/// </summary>
public class AssetLifecycleEventHandler : INotificationHandler<AssetLifecycleChangedNotification>
{
    private readonly ILogger<AssetLifecycleEventHandler> _logger;

    public AssetLifecycleEventHandler(ILogger<AssetLifecycleEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AssetLifecycleChangedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✓ AssetLifecycleChanged: Asset {MaterielId} ({OldStage} → {NewStage}) - {Reason}",
            notification.MaterielId,
            notification.OldStage,
            notification.NewStage,
            notification.Reason ?? "No reason");

        if (notification.NewStage == "Disposed" || notification.NewStage == "EndOfLife")
        {
            _logger.LogWarning("Asset {MaterielId} reaching end-of-life", notification.MaterielId);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles MaintenanceTicketCreated events.
/// Triggered when a maintenance ticket is created.
/// </summary>
public class MaintenanceTicketEventHandler : INotificationHandler<MaintenanceTicketCreatedNotification>
{
    private readonly ILogger<MaintenanceTicketEventHandler> _logger;

    public MaintenanceTicketEventHandler(ILogger<MaintenanceTicketEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(MaintenanceTicketCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✓ MaintenanceTicketCreated: Ticket {TicketId} for Asset {MaterielId} - Priority: {Priority}",
            notification.TicketId,
            notification.MaterielId,
            notification.Priority);

        if (notification.Priority.Equals("High", StringComparison.OrdinalIgnoreCase) ||
            notification.Priority.Equals("Critical", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("⚠ High-priority maintenance ticket created: {TicketId}", notification.TicketId);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles AssetAssigned events.
/// Triggered when an asset is assigned to an employee.
/// </summary>
public class AssetAssignmentEventHandler : INotificationHandler<AssetAssignedNotification>
{
    private readonly ILogger<AssetAssignmentEventHandler> _logger;

    public AssetAssignmentEventHandler(ILogger<AssetAssignmentEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AssetAssignedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "✓ AssetAssigned: {AssetName} ({MaterielId}) → Employee {EmployeeId} on {AssignmentDate:yyyy-MM-dd}",
            notification.AssetName,
            notification.MaterielId,
            notification.EmployeeId,
            notification.AssignmentDate);

        if (!string.IsNullOrEmpty(notification.AssignmentLocation))
        {
            _logger.LogInformation("  Location: {Location}", notification.AssignmentLocation);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles WarrantyExpiring events.
/// Triggered when an asset warranty is approaching expiration.
/// </summary>
public class WarrantyEventHandler : INotificationHandler<WarrantyExpiringNotification>
{
    private readonly ILogger<WarrantyEventHandler> _logger;

    public WarrantyEventHandler(ILogger<WarrantyEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(WarrantyExpiringNotification notification, CancellationToken cancellationToken)
    {
        var daysUntilExpiration = (notification.WarrantyExpirationDate - DateTime.UtcNow).Days;

        if (daysUntilExpiration <= 7 && daysUntilExpiration > 0)
        {
            _logger.LogError(
                "🔴 CRITICAL: Asset {MaterielId} warranty expiring in {Days} days - IMMEDIATE ACTION REQUIRED",
                notification.MaterielId,
                daysUntilExpiration);
        }
        else if (daysUntilExpiration <= 30)
        {
            _logger.LogWarning(
                "🟠 URGENT: Asset {MaterielId} warranty expiring in {Days} days",
                notification.MaterielId,
                daysUntilExpiration);
        }
        else
        {
            _logger.LogInformation(
                "✓ WarrantyExpiring: Asset {MaterielId} ({AssetName}) expires in {Days} days - Provider: {Provider}",
                notification.MaterielId,
                notification.AssetName,
                daysUntilExpiration,
                notification.WarrantyProvider);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Handles AssetDisposed events.
/// Triggered when an asset is disposed, sold, or decommissioned.
/// </summary>
public class AssetDisposalEventHandler : INotificationHandler<AssetDisposedNotification>
{
    private readonly ILogger<AssetDisposalEventHandler> _logger;

    public AssetDisposalEventHandler(ILogger<AssetDisposalEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AssetDisposedNotification notification, CancellationToken cancellationToken)
    {
        var depreciationAmount = notification.OriginalValue - notification.SalvageValue;

        _logger.LogInformation(
            "✓ AssetDisposed: {AssetName} ({MaterielId}) - Method: {Method}, Reason: {Reason}",
            notification.AssetName,
            notification.MaterielId,
            notification.DisposalMethod,
            notification.DisposalReason);

        _logger.LogInformation(
            "  Depreciation: {Original:C} - {Salvage:C} = {Depreciation:C}",
            notification.OriginalValue,
            notification.SalvageValue,
            depreciationAmount);

        if (notification.OriginalValue > 1000)
        {
            _logger.LogWarning(
                "⚠ High-value asset disposal: {MaterielId} (Original value: {Value:C})",
                notification.MaterielId,
                notification.OriginalValue);
        }

        return Task.CompletedTask;
    }
}
