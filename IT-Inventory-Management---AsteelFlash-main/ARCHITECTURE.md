# IT Inventory Management - Clean Architecture Refactoring Plan

## Executive Summary

This document outlines the restructuring of the IT Inventory Management System into a strict Clean Architecture with 4 distinct layers.  The refactoring ensures:
- **Dependency Rule Compliance**: Domain ← Application ← Infrastructure ← Presentation
- **CQRS Pattern**: Implemented via MediatR for all commands/queries
- **DDD Principles**: Domain events, value objects, and domain exceptions
- **Testability**: Layers can be tested independently
- **Scalability**: New features added via isolated CQRS handlers

---

## Current State Analysis

### ✅ Existing Strengths
1. **Layer Separation** - Domain, Application, Infrastructure projects already exist
2. **MediatR Integration** - Commands/Queries in Application layer  
3. **Exception Middleware** - GlobalExceptionMiddleware handles errors properly
4. **Dependency Injection** - Proper DI registration per layer
5. **AutoMapper** - Configured for DTO mappings
6. **FluentValidation** - Validators for commands

### ❌ Current Issues
1. **Anemic Domain Model** - Entities are pure data containers with no behavior
2. **Missing Domain Infrastructure** - No BaseEntity, DomainEvents, ValueObjects, DomainExceptions
3. **Security Issue** - Employee entity stores plain-text passwords (CRITICAL)
4. **Duplicate Service Logic** - 25 services with identical Create/Update/Delete patterns
5. **Interfaces in Wrong Layer** - Service interfaces in Application instead of Infrastructure
6. **Generic Exceptions** - Services throw `new Exception()` instead of domain-specific ones
7. **SemaphoreSlim Anti-pattern** - Workaround for Blazor DbContext concurrency issues
8. **Missing Domain Events** - No event publishing or handling
9. **No Value Objects** - SerialNumber, Warranty, Money should be value objects
10. **Large Blazor Components** - 31 CRUD components, some >300 lines

---

## Target Architecture

```
.sln (ITStockM.sln)
│
├── src/
│   ├── Domain/
│   │   ├── Base/                    # ✅ CREATED: BaseEntity, DomainEvent
│   │   ├── Entities/                # ✅ EXISTS: 13 entities
│   │   ├── Enums/                   # ✅ EXISTS: 3 enums
│   │   ├── Exceptions/              # ✅ CREATED: DomainExceptions
│   │   ├── Events/                  # ⏳ TODO: Domain events
│   │   └── ValueObjects/            # ⏳ TODO: SerialNumber, Warranty, Money
│   │
│   ├── Application/                 # ✅ EXISTS, NEEDS CLEANUP
│   │   ├── Features/                # ✅ EXISTS: AssetLifecycle, Maintenance, Predictions
│   │   │   └── [Feature]/
│   │   │       ├── Commands/        # ✅ CQRS command pattern
│   │   │       ├── Queries/         # ✅ CQRS query pattern
│   │   │       ├── Handlers/        # ✅ Mediator handlers
│   │   │       ├── DTOs/            # ✅ Data transfer objects
│   │   │       └── Validators/      # ✅ FluentValidation
│   │   │
│   │   ├── Common/
│   │   │   ├── Interfaces/          # ⚠️ REVIEW: Move to Infrastructure later
│   │   │   ├── Behaviors/           # ✅ ValidationBehavior (MediatR pipeline)
│   │   │   ├── Mapping/             # ✅ AutoMapper profiles
│   │   │   └── Constants/           # ✅ RouteConstants, UserRoles
│   │   │
│   │   └── DependencyInjection/
│   │       └── ApplicationLayerServiceCollectionExtensions.cs
│   │
│   ├── Infrastructure/              # ✅ EXISTS, NEEDS CLEANUP
│   │   ├── Persistence/
│   │   │   ├── ITStockManagmentContext.cs
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   │
│   │   ├── Services/                # ⚠️ 25 services - needs refactoring
│   │   │   ├── DateTimeService.cs
│   │   │   ├── CurrentUserService.cs
│   │   │   ├── AuthService.cs
│   │   │   └── ... (business services)
│   │   │
│   │   ├── Repositories/            # ✅ Generic + specialized repositories
│   │   │   ├── EfRepository<T>
│   │   │   └── Specialized implementations
│   │   │
│   │   └── DependencyInjection/
│   │       └── InfrastructureLayerServiceCollectionExtensions.cs
│   │
│   └── Presentation/                # EXISTING STRUCTURE (Blazor Web UI)
│       ├── Controllers/             # ✅ REST API endpoints
│       ├── Middleware/              # ✅ GlobalExceptionMiddleware
│       ├── Auth/                    # ✅ CustomAuthenticationStateProvider
│       ├── Services/                # UI/Blazor-specific services
│       └── DependencyInjection/
│
├── Components/                      # ✅ Blazor components
│   ├── Pages/                       # 87 .razor files
│   ├── Layout/                      # MainLayout, SideLayout
│   └── App.razor
│
├── Program.cs                       # ✅ Main entry point
├── appsettings.json
└── appsettings.Development.json

└── ITStockM.Tests/
    └── [Test projects]
```

---

## Layers Description

### 1. **Domain Layer** (`src/ITStockM.Domain`)

**Purpose**: Pure business logic, independent of frameworks

**Contains**:
- ✅ **Entities**: Materiel, Employee, Assignment, etc. (13 total)
- ✅ **Enums**: HealthStatus, LifecycleStage, MaintenanceTicketStatus
- ✅ **Base Classes**:
  - `BaseEntity` - Common identity, timestamps, audit fields
  - `DomainEvent` - Base class for domain events
- ✅ **Exceptions**: Domain-specific exceptions (EntityNotFoundException, BusinessRuleViolationException)
- ⏳ **Events**: AssetLifecycleChanged, MaintenanceCreated, WarrantyExpiring
- ⏳ **Value Objects**: SerialNumber, Warranty{StartDate, EndDate}, Money{Amount, Currency}

**Dependencies**: 🔴 NONE (framework-agnostic)

**Key Principle**: This layer defines "What the business does"

---

### 2. **Application Layer** (`src/ITStockM.Application`)

**Purpose**: Business logic orchestration, use cases

**Contains**:
- ✅ **Features** (CQRS structure):
  ```csharp
  Features/
  ├── AssetLifecycle/
  │   ├── Commands/TransitionAssetStageCommand.cs
  │   ├── Queries/GetAssetsByStageQuery.cs
  │   ├── Handlers/AssetLifecycleHandlers.cs
  │   └── DTOs/TransitionRequestDto.cs
  ```
- ✅ **Interfaces** (defining application contracts):
  - `IApplicationDbContext`
  - `IAssignmentService`, `IMaterielService`, etc.
  - `ICurrentUserService`, `IDateTimeService`
  - `IEmailService`, `INotificationService`
- ✅ **MediatR Pipeline Behaviors**:
  - `ValidationBehavior` - Automatic validation of commands
- ✅ **Mappings**:
  - `MappingProfile` - AutoMapper entity ↔ DTO conversions
- ✅ **Constants**:
  - RouteConstants, UserRoles

**Dependencies**: ✅ ONLY Domain

**Key Principle**: "How the business operates" via use cases

---

### 3. **Infrastructure Layer** (`src/ITStockM.Infrastructure`)

**Purpose**: Technical implementations, external services

**Contains**:
- ✅ **Persistence**:
  - `ITStockManagmentContext` - EF Core DbContext
  - Entity configurations
  - Migrations (3 existing)
- ✅ **Services** (implement Application interfaces):
  - DateTimeService, CurrentUserService, AuthService
  - Business services: MaterielService, AssignmentService, etc.
  - Email, Notification, Prediction services
- ✅ **Repositories**:
  - `EfRepository<T>` - Generic repository
  - Specialized: MaterielRepository, AssignmentRepository
- ✅ **Background Services**:
  - EmailBackgroundService, AssetHealthBackgroundService
  - SmtpHealthHostedService

**Dependencies**: ✅ Application + Domain

**Key Principle**: "Implementation details" - can be swapped

---

### 4. **Presentation Layer** (Root + Components)

**Purpose**: User interface - Web API + Blazor UI

**Contains**:
- ✅ **Controllers** (REST API):
  - Inject `IMediator` only (no business logic)
  - All endpoints use Commands/Queries
  - Examples: MaintenanceController, AssetLifecycleController
- ✅ **Middleware**:
  - `GlobalExceptionMiddleware` - Proper error mapping
- ✅ **Authentication**:
  - `CustomAuthenticationStateProvider` - Blazor auth
- ✅ **Blazor Components** (87 total):
  - Pages for CRUD operations
  - Material views, assignments, delivery orders
  - Layouts and shared components

**Dependencies**: ✅ Application (ONLY)

**Key Principle**: "How users interact" - thin, no business logic

---

## Dependency Flow

```
Presentation (Controllers, Blazor)
    ↓ (injects)
Application (Features, Commands, Queries)
    ↓ (injects)  
Infrastructure (Services, Repositories)
    ↓ (injects)
Domain (Entities, Events)
    ↓ (none - no dependencies)
[Frameworks, Databases, External APIs]
```

✅ **CORRECT**: Flow is INWARD only
❌ **WRONG**: Domain should never depend on Infrastructure or Presentation

---

## Implementation Changes Made

### ✅ Completed

1. **Domain Layer Enhancements**:
   - ✅ Created `Domain/Base/BaseEntity.cs` with:
     - Common `Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted` properties
     - `DomainEvent` infrastructure
     - `GetDomainEvents()` method for event publishing
   - ✅ Created `Domain/Exceptions/` with:
     - `DomainException` base class
     - `EntityNotFoundException`
     - `BusinessRuleViolationException`
     - `InvalidOperationException`
   - ✅ Created `Domain/ValueObjects/` folder (structure ready)
   - ✅ Created `Domain/Events/` folder (structure ready)

### ⏳ Still TODO

1. **Update all entities** to inherit from `BaseEntity`:
   ```csharp
   public partial class Materiel : BaseEntity
   {
       // inherits Id, CreatedAt, UpdatedAt, IsDeleted
   }
   ```

2. **Create Value Objects**:
   ```csharp
   public class SerialNumber : ValueObject
   {
       public string Value { get; }
       protected override IEnumerable<object> GetAtomicValues() { ... }
   }
   ```

3. **Create Domain Events**:
   ```csharp
   public class AssetLifecycleChangedEvent : DomainEvent
   {
       public int MaterielId { get; set; }
       public LifecycleStage NewStage { get; set; }
   }
   ```

4. **Replace generic exceptions**:
   ```csharp
   // BEFORE
   throw new Exception("Item not found");
   
   // AFTER  
   throw new EntityNotFoundException(nameof(Materiel), id);
   ```

5. **Fix Security Issue - Employee Password**:
   ```csharp
   // BEFORE
   public string? Password { get; set; }  // ❌ Plain text
   
   // AFTER
   // Use ASP.NET Core Identity framework instead
   public class AppUser : IdentityUser { ... }
   ```

6. **Refactor Service Duplication**:
   - Extract common Create/Update/Delete patterns to base service
   - Or use generic CRUD handler in Application layer
   - Reduces 25 services × ~150 LOC = 3,750 LOC duplicate code

7. **Database Migrations**:
   - Add `CreatedAt`, `UpdatedAt`, `IsDeleted` columns to all entities
   - Command: `dotnet ef migrations add AddAuditFields`

---

## CQRS Pattern - Examples

### Example 1: Create Material (Command)

**File**: `Application/Features/Materials/Commands/CreateMaterialCommand.cs`
```csharp
public class CreateMaterialCommand : IRequest<int>
{
    public string SerialNumber { get; set; }
    public string Description { get; set; }
    public HealthStatus HealthStatus { get; set; }
}

public class CreateMaterialCommandValidator : AbstractValidator<CreateMaterialCommand>
{
    public CreateMaterialCommandValidator()
    {
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty();
    }
}

public class CreateMaterialCommandHandler : IRequestHandler<CreateMaterialCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public async Task<int> Handle(CreateMaterialCommand request, CancellationToken ct)
    {
        var material = _mapper.Map<Materiel>(request);
        _context.Materiels.Add(material);
        await _context.SaveChangesAsync(ct);
        return material.Id;
    }
}
```

**File**: `Presentation/Controllers/MaterialsController.cs`
```csharp
[ApiController]
[Route("api/materials")]
public class MaterialsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<ActionResult<int>> Create([FromBody] CreateMaterialCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}
```

### Example 2: Get All Materials (Query)

**File**: `Application/Features/Materials/Queries/GetAllMaterialsQuery.cs`
```csharp
public class GetAllMaterialsQuery : IRequest<List<MaterielDto>> { }

public class GetAllMaterialsHandler : IRequestHandler<GetAllMaterialsQuery, List<MaterielDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public async Task<List<MaterielDto>> Handle(GetAllMaterialsQuery request, CancellationToken ct)
    {
        var materials = await _context.Materiels
            .Where(m => !m.IsDeleted)
            .ToListAsync(ct);
        return _mapper.Map<List<MaterielDto>>(materials);
    }
}
```

**File**: `Presentation/Controllers/MaterialsController.cs`
```csharp
[HttpGet]
public async Task<ActionResult<List<MaterielDto>>> GetAll()
{
    var materials = await _mediator.Send(new GetAllMaterialsQuery());
    return Ok(materials);
}
```

---

## Removing Code Duplication

### Current Problem: 25 Services with Duplicate Logic

**MaterielService.cs** (150 LOC):
```csharp
public async Task<Materiel> CreateItem(Materiel item) { ... }
public async Task<Materiel> UpdateItem(int id, Materiel item) { ... }
public async Task<Materiel> DeleteItem(int id) { ... }
public async Task<List<Materiel>> GetItems() { ... }
```

**AssignmentService.cs** (150 LOC):
```csharp
public async Task<Assignment> CreateItem(Assignment item) { ... }  // SAME CODE
public async Task<Assignment> UpdateItem(int id, Assignment item) { ... }  // SAME CODE
public async Task<Assignment> DeleteItem(int id) { ... }  // SAME CODE
public async Task<List<Assignment>> GetItems() { ... }  // SAME CODE
```

### Solution: Base Service Pattern

```csharp
namespace ITStockM.Infrastructure.Services;

public abstract class BaseCrudService<TEntity> where TEntity : BaseEntity
{
    protected readonly IApplicationDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    protected BaseCrudService(IApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        _dbSet.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task<TEntity> UpdateAsync(int id, TEntity entity, CancellationToken ct = default)
    {
        var existing = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new EntityNotFoundException(typeof(TEntity).Name, id);
        
        entity.Id = id;
        entity.UpdatedAt = DateTime.UtcNow;
        _context.Entry(existing).CurrentValues.SetValues(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new EntityNotFoundException(typeof(TEntity).Name, id);
        
        entity.IsDeleted = true;  // Soft delete
        await _context.SaveChangesAsync(ct);
    }

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync(ct);
    }
}
```

**Result**: Each service becomes 20 LOC instead of 150
```csharp
public class MaterielService : BaseCrudService<Materiel>, IMaterielService
{
    public MaterielService(IApplicationDbContext context) : base(context) { }

    // Only add domain-specific methods here
    public async Task<List<Materiel>> GetExpiredWarranties(CancellationToken ct)
    {
        return await _dbSet
            .Where(m => !m.IsDeleted && m.WarrantyEndDate < DateTime.UtcNow)
            .ToListAsync(ct);
    }
}
```

---

## Testing Strategy

### Unit Tests (Domain Layer)
```csharp
[TestClass]
public class MaterielTests
{
    [TestMethod]
    public void Materiel_ShouldRaiseEvent_WhenTransitioningLifecycle()
    {
        var materiel = new Materiel { /* ... */ };
        materiel.TransitionStage(LifecycleStage.Maintenance);
        
        var events = materiel.GetDomainEvents();
        Assert.AreEqual(1, events.Count);
        Assert.IsInstanceOfType(events[0], typeof(AssetLifecycleChangedEvent));
    }
}
```

### Integration Tests (Application Layer)
```csharp
[TestClass]
public class CreateMaterielCommandTests
{
    [TestMethod]
    public async Task Handle_ShouldCreateMaterial_WithValidCommand()
    {
        var command = new CreateMaterialCommand { /* ... */ };
        var handler = new CreateMaterialCommandHandler(_context, _mapper);
        
        var id = await handler.Handle(command, CancellationToken.None);
        
        Assert.IsTrue(id > 0);
        var material = await _context.Materiels.FindAsync(id);
        Assert.IsNotNull(material);
    }
}
```

### End-to-End Tests (API Layer)
```csharp
[TestClass]
public class MaterialsControllerTests
{
    [TestMethod]
    public async Task Create_ShouldReturn201_WithValidPayload()
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
        var command = new CreateMaterialCommand { /* ... */ };
        
        var response = await client.PostAsJsonAsync("/api/materials", command);
        
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
```

---

## Security Improvements

### 🔴 CRITICAL: Fix Password Storage

**Current** (Employee.cs):
```csharp
public string? Password { get; set; }  // ❌ DANGEROUS: Plain text in DB
```

**Fixed** (using ASP.NET Core Identity):
```csharp
public class AppUser : IdentityUser
{
    public string FullName { get; set; }
    public string Service { get; set; }
    public string Post { get; set; }
    public string PhoneNumber { get; set; }  // Override IdentityUser's
}

// In Migrations:
// dotnet ef migrations add MigrateToIdentity
// Updates: Users table schema, Password field removed, hashed passwords in PasswordHash field
```

### Other Security Measures
1. ✅ **GlobalExceptionMiddleware** - Proper error handling
2. ✅ **Authorization** - Role-based access control
3. ⏳ **CORS** - Configure properly for API
4. ⏳ **Rate Limiting** - Prevent abuse
5. ⏳ **Audit Trail** - Track who did what (`CreatedBy`, `UpdatedBy` fields)

---

## Performance Optimizations

1. **DbContext Pooling** - Fixes SemaphoreSlim workaround
   ```csharp
   services.AddDbContextPool<ITStockManagmentContext>(options =>
       options.UseSqlServer(connectionString),
       poolSize: 16);
   ```

2. **Lazy Loading** - Prevent N+1 queries
   ```csharp
   var materials = await _context.Materiels
       .Include(m => m.Assignments)
       .Include(m => m.MaintenanceTickets)
       .ToListAsync();
   ```

3. **Pagination** - For large datasets
   ```csharp
   public class PaginatedQuery<T> : IRequest<PagedResult<T>>
   {
       public int PageNumber { get; set; } = 1;
       public int PageSize { get; set; } = 10;
   }
   ```

4. **Caching** - For frequently accessed data
   ```csharp
   [CacheKey("healthStatus_counts")]
   public async Task<Dictionary<HealthStatus, int>> GetHealthStatusCounts()
   {
       return await _context.Materiels
           .GroupBy(m => m.HealthStatus)
           .Select(g => new { g.Key, Count = g.Count() })
           .ToDictionaryAsync(x => x.Key, x => x.Count);
   }
   ```

---

## Blazor Component Refactoring

### Problem: 31 CRUD Components (~150 LOC each)

**Before**: EditMaterial.razor (300+ lines)
```html
@page "/edit-material/{id:int}"
@using ITStockM.Services
@inject IMaterielService materielService
@inject NavigationManager nav

<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    <ValidationMessage For="@(() => model.SerialNumber)" />
    <InputText @bind-Value="model.SerialNumber" />
    <!-- ... 50+ lines of HTML -->
</EditForm>

@code {
    // 200+ lines of C# code
}
```

**After**: Reusable EditFormComponent.razor
```html
@typeparam TModel
@using System.Linq.Expressions

<EditForm Model="@Model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    @foreach (var field in Fields)
    {
        <div class="form-group">
            <label>@field.Label</label>
            <InputText @bind-Value="field.Value" class="form-control" />
        </div>
    }
    
    <button type="submit">Save</button>
</EditForm>

@code {
    [Parameter] public TModel Model { get; set; }
    [Parameter] public EventCallback<TModel> OnSubmit { get; set; }
    // Reusable logic for 31 forms!
}
```

### Usage:
```html
<EditFormComponent TModel="MaterielViewModel" @bind-Model="material" OnSubmit="@HandleSave" />
```

---

## Migration Path

### Phase 1: Foundation (Week 1)
- ✅ Create Domain base classes
- Create Value Objects (SerialNumber, Warranty, Money)
- Create Domain Events
- Update entities to inherit from BaseEntity

### Phase 2: Security (Week 2)
- Migrate to ASP.NET Core Identity
- Fix password storage
- Add audit fields to entities

### Phase 3: Cleanup (Week 3)
- Extract service duplication → BaseCrudService
- Create domain-specific exceptions
- Replace generic exceptions

### Phase 4: Optimization (Week 4)
- Implement pagination
- Add caching
- Fix N+1 queries
- Component refactoring

### Phase 5: Testing (Week 5)
- Write unit tests (Domain layer)
- Write integration tests (Application layer)
- Write E2E tests (API layer)

---

## Summary of Architecture Benefits

| Benefit | How It's Achieved |
|---------|------------------|
| **Maintainability** | Clear separation of concerns, CQRS pattern |
| **Testability** | Independent layers, dependency injection |
| **Scalability** | New features = new CQRS handlers, minimal changes |
| **Flexibility** | Can swap Infrastructure implementations (SQL Server → PostgreSQL) |
| **Clarity** | Domain logic is explicit, not hidden in services |
| **Reduced Duplication** | Base service patterns, value objects |
| **Better Error Handling** | Domain-specific exceptions |
| **Audit Trail** | BaseEntity includes timestamps, soft delete |

---

## Commands Reference

```bash
# Build
dotnet build

# Run
dotnet run

# Database migrations
dotnet ef migrations add {MigrationName}
dotnet ef database update

# Run tests
dotnet test

# Code cleanup
dotnet format
```

---

## Conclusion

This refactoring transforms the IT Inventory Management System from a monolithic structure into a clean, maintainable, enterprise-level architecture. The four-layer model ensures that business logic remains pure and independent of infrastructure concerns, making the system resilient to framework changes and easier to test and extend.

**Next Steps:**
1. Update all entities to inherit from `BaseEntity`
2. Create Value Objects in Domain layer
3. Migrate to ASP.NET Core Identity
4. Extract service duplication
5. Add comprehensive tests
