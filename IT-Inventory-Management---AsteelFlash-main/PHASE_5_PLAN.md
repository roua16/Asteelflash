# Phase 5 Plan - CQRS & Event Handling

**Status**: Ready to Start  
**Date**: April 14, 2026  
**Duration**: 2-3 hours  
**Goal**: Implement event-driven architecture using MediatR

---

## 📊 Phase 5 Overview

### Objectives
1. ✅ Implement 5 domain event handlers
2. ✅ Add MediatR notification publishing
3. ✅ Create event publishing infrastructure
4. ✅ Test event flow end-to-end
5. ✅ Document event architecture

### Current Foundation (From Phases 1-4)
- ✅ 5 Domain Events defined (AssetLifecycleChanged, MaintenanceTicketCreated, AssetAssigned, WarrantyExpiring, AssetDisposed)
- ✅ BaseEntity with event infrastructure
- ✅ MediatR pre-configured in Application layer
- ✅ Clean Architecture (4 layers) established
- ✅ 12/25 services consolidated with BaseCrudService

---

## 🎯 Implementation Plan

### Task 1: Create Event Handler Classes (1 hour)

#### 1.1 AssetLifecycleEventHandler
**Location**: `src/ITStockM.Application/Features/Events/Handlers/AssetLifecycleEventHandler.cs`

```csharp
public class AssetLifecycleEventHandler : 
    INotificationHandler<AssetLifecycleChangedEvent>
{
    private readonly ILogger<AssetLifecycleEventHandler> _logger;
    private readonly IOperationNotificationService _notificationService;
    
    public async Task Handle(AssetLifecycleChangedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("Asset {MaterielId} lifecycle changed to {NewStage}", 
            notification.MaterielId, notification.NewStage);
        
        // 1. Update reports
        // 2. Send notifications
        // 3. Log audit trail
        // 4. Trigger downstream processes
        
        await _notificationService.NotifyAssetLifecycleChanged(notification);
    }
}
```

**Responsibilities**:
- Log lifecycle transitions
- Update related reports
- Send notifications to stakeholders
- Trigger downstream workflows

---

#### 1.2 MaintenanceEventHandler
**Location**: `src/ITStockM.Application/Features/Events/Handlers/MaintenanceEventHandler.cs`

```csharp
public class MaintenanceEventHandler : 
    INotificationHandler<MaintenanceTicketCreatedEvent>
{
    public async Task Handle(MaintenanceTicketCreatedEvent notification, CancellationToken ct)
    {
        // 1. Send alerts to maintenance team
        // 2. Create notifications
        // 3. Log event
        // 4. Update dashboards
    }
}
```

**Responsibilities**:
- Alert maintenance team
- Create notifications
- Update maintenance dashboards
- Log event for auditing

---

#### 1.3 AssetAssignmentEventHandler
**Location**: `src/ITStockM.Application/Features/Events/Handlers/AssetAssignmentEventHandler.cs`

```csharp
public class AssetAssignmentEventHandler : 
    INotificationHandler<AssetAssignedEvent>
{
    public async Task Handle(AssetAssignedEvent notification, CancellationToken ct)
    {
        // 1. Send notification to employee
        // 2. Update asset allocation reports
        // 3. Log assignment
        // 4. Update dashboard
    }
}
```

**Responsibilities**:
- Notify assigned employee
- Update allocation reports
- Log assignment for audit trail
- Trigger related workflows

---

#### 1.4 WarrantyEventHandler
**Location**: `src/ITStockM.Application/Features/Events/Handlers/WarrantyEventHandler.cs`

```csharp
public class WarrantyEventHandler : 
    INotificationHandler<WarrantyExpiringEvent>
{
    public async Task Handle(WarrantyExpiringEvent notification, CancellationToken ct)
    {
        // 1. Alert procurement team
        // 2. Create remediation tasks
        // 3. Update asset risk profile
        // 4. Log warranty issue
    }
}
```

**Responsibilities**:
- Alert procurement team
- Create remediation tasks
- Update risk assessments
- Log warranty issues

---

#### 1.5 AssetDisposalEventHandler
**Location**: `src/ITStockM.Application/Features/Events/Handlers/AssetDisposalEventHandler.cs`

```csharp
public class AssetDisposalEventHandler : 
    INotificationHandler<AssetDisposedEvent>
{
    public async Task Handle(AssetDisposedEvent notification, CancellationToken ct)
    {
        // 1. Archive asset records
        // 2. Generate disposal report
        // 3. Update inventory
        // 4. Log disposal
    }
}
```

**Responsibilities**:
- Archive asset records
- Generate disposal reports
- Update inventory counts
- Log disposal for compliance

---

### Task 2: Event Publishing Infrastructure (45 min)

#### 2.1 Update Domain Events to Include Required Data

All 5 event classes need to include:
- Event timestamp
- Source entity ID
- Event description
- Relevant context data

Example:
```csharp
public class AssetLifecycleChangedEvent : INotification
{
    public int MaterielId { get; init; }
    public string NewStage { get; init; }
    public string? OldStage { get; init; }
    public string? Reason { get; init; }
    public DateTime OccurredAt { get; init; }
}
```

#### 2.2 Update Service Layer to Publish Events

In refactored services (those inheriting BaseCrudService):
```csharp
protected override async Task OnEntityCreated(Materiel entity)
{
    var @event = new AssetAssignedEvent
    {
        MaterielId = entity.Id,
        // ... other data
    };
    
    await _mediator.Publish(@event);
}
```

#### 2.3 Register Event Handlers in DependencyInjection

In `Infrastructure/DependencyInjection.cs`:
```csharp
// Auto-register all MediatR handlers
services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
```

---

### Task 3: Testing Event Flow (45 min)

#### 3.1 Unit Tests for Event Handlers
**Location**: `ITStockM.Tests/Features/Events/`

```csharp
[Fact]
public async Task AssetLifecycleEventHandler_PublishesNotification_WhenAssetLifecycleChanges()
{
    // Arrange
    var handler = new AssetLifecycleEventHandler(_logger, _notificationService);
    var @event = new AssetLifecycleChangedEvent 
    { 
        MaterielId = 1, 
        NewStage = "Retired" 
    };
    
    // Act
    await handler.Handle(@event, CancellationToken.None);
    
    // Assert
    _notificationService.Verify(x => x.NotifyAssetLifecycleChanged(@event), Times.Once);
}
```

#### 3.2 Integration Tests for Event Publishing
- Test that services properly raise events
- Test that MediatR publishes to correct handlers
- Test end-to-end event flow

#### 3.3 E2E Tests for Workflows
- Test complete asset lifecycle scenarios
- Test event cascading
- Test side effects (notifications, reports)

---

### Task 4: Documentation Updates (30 min)

#### 4.1 Update ARCHITECTURE.md
Add new section: "Event-Driven Architecture"
- Event design pattern
- Event flow diagram
- Handler registration
- Integration points

#### 4.2 Update README_CLEAN_ARCHITECTURE.md
Add Phase 5 achievements

#### 4.3 Update IMPLEMENTATION.md
Mark Phase 5 as In Progress → Complete

#### 4.4 Create PHASE_5_STATUS.md
Document event handler implementation details

---

## 📁 File Structure After Phase 5

```
src/ITStockM.Application/Features/Events/
├── Handlers/
│   ├── AssetLifecycleEventHandler.cs (NEW)
│   ├── MaintenanceEventHandler.cs (NEW)
│   ├── AssetAssignmentEventHandler.cs (NEW)
│   ├── WarrantyEventHandler.cs (NEW)
│   └── AssetDisposalEventHandler.cs (NEW)
└── README.md (NEW - Event architecture guide)

ITStockM.Tests/Features/Events/
├── AssetLifecycleEventHandlerTests.cs (NEW)
├── MaintenanceEventHandlerTests.cs (NEW)
├── AssetAssignmentEventHandlerTests.cs (NEW)
├── WarrantyEventHandlerTests.cs (NEW)
└── AssetDisposalEventHandlerTests.cs (NEW)
```

---

## 🔄 Event Flow Architecture

### Current Flow (Phase 4)
```
Entity Operation (Create/Update/Delete)
    ↓
Repository.SaveChangesAsync()
    ↓
BaseEntity.DomainEvents collected
```

### After Phase 5
```
Entity Operation (Create/Update/Delete)
    ↓
Repository.SaveChangesAsync()
    ↓
BaseEntity.DomainEvents collected
    ↓
Service publishes via MediatR (IMediator.Publish)
    ↓
Event Handlers triggered (INotificationHandler<T>)
    ↓
Side effects executed
    ├── Notifications sent
    ├── Reports updated
    ├── Dashboards refreshed
    └── Audit trails logged
```

### Integration Points
1. **Domain Layer**: Events raised in entities
2. **Infrastructure Layer**: Services publish events via MediatR
3. **Application Layer**: Handlers implement business logic
4. **Presentation Layer**: Blazor components subscribe to updates

---

## ✅ Success Criteria

### Code Quality
- ✅ 5 event handlers implemented
- ✅ All handlers tested (unit + integration)
- ✅ No breaking changes
- ✅ 100% backward compatible

### Functionality
- ✅ Events published correctly
- ✅ Handlers execute in correct order
- ✅ Side effects work as expected
- ✅ Error handling robust

### Documentation
- ✅ Event architecture documented
- ✅ Handler responsibilities clear
- ✅ Integration examples provided
- ✅ Team onboarding guide included

---

## 🚀 Phase 6 Readiness

After Phase 5 completion, Phase 6 will:
1. Add comprehensive integration tests
2. Create E2E workflow tests
3. Achieve 80%+ test coverage
4. Generate final documentation

---

## 📝 Commits Expected

```
1. feat: Add event handler infrastructure
   - Create 5 event handler classes
   - Register handlers in DI
   - Add handler interfaces

2. feat: Implement event publishing
   - Update service layer
   - Publish events on entity changes
   - Test event flow

3. test: Add comprehensive event tests
   - Unit tests for handlers
   - Integration tests
   - E2E tests

4. docs: Update documentation for Phase 5
   - ARCHITECTURE.md update
   - Event flow diagrams
   - Integration examples
```

---

## ⏱️ Timeline

| Task | Duration | Start | Complete |
|------|----------|-------|----------|
| Create 5 handlers | 1 hour | T+0 | T+1h |
| Event publishing | 45 min | T+1h | T+1h45m |
| Testing | 45 min | T+1h45m | T+2h30m |
| Documentation | 30 min | T+2h30m | T+3h |
| **Total** | **3 hours** | **Start** | **+3h** |

---

## 🎯 Next Phase (Phase 6)

After Phase 5 completes:
- Full test coverage (target: 80%+)
- E2E API tests
- Integration tests
- Final verification
- Production readiness

---

**Status**: Ready to implement  
**Expected Completion**: April 14, 2026 (est. 3 hours after Phase 5 start)
