# Clean Architecture Folder Structure

## Current Project Structure

```
ITStockM/
│
├── 📄 ITStockM.sln                    # Solution file
├── 📄 Program.cs                      # Main entry point ✅ CLEAN
├── 📄 ITStockM.csproj                 # Root web project
├── 📄 appsettings.json
├── 📄 appsettings.Development.json
│
├── 📘 ARCHITECTURE.md                 # ✅ CREATED: Architecture guide
├── 📘 REFACTORING_STATUS.md           # ✅ CREATED: Implementation tracking
│
├── 📁 src/
│   │
│   ├── 📁 ITStockM.Domain/            # ✅ LAYER: Pure Business Logic
│   │   ├── 📁 Base/                   # ✅ NEW: Base classes for all entities
│   │   │   ├── 📄 BaseEntity.cs       # ✅ CREATED: Common entity base
│   │   │   │   ├── int Id
│   │   │   │   ├── DateTime CreatedAt
│   │   │   │   ├── DateTime? UpdatedAt
│   │   │   │   ├── bool IsDeleted
│   │   │   │   ├── GetDomainEvents()
│   │   │   │   └── class DomainEvent
│   │   │   └── 📄 ValueObject.cs      # ⏳ TODO: For value object implementations
│   │   │
│   │   ├── 📁 Entities/               # ✅ DOMAIN ENTITIES (13 total)
│   │   │   ├── 📄 Materiel.cs
│   │   │   ├── 📄 Employee.cs         # 🔴 TODO: Remove plain-text password
│   │   │   ├── 📄 Assignment.cs
│   │   │   ├── 📄 AssignmentMateriel.cs
│   │   │   ├── 📄 DeliveryOrder.cs
│   │   │   ├── 📄 DeliveryOrderMateriel.cs
│   │   │   ├── 📄 MaintenanceTicket.cs
│   │   │   ├── 📄 Project.cs
│   │   │   ├── 📄 Request.cs
│   │   │   ├── 📄 Supplier.cs
│   │   │   ├── 📄 Offer.cs
│   │   │   ├── 📄 AssetLifecycleRecord.cs
│   │   │   └── 📄 AssetPrediction.cs
│   │   │
│   │   ├── 📁 Enums/                  # ✅ DOMAIN ENUMERATIONS (3 total)
│   │   │   ├── 📄 HealthStatus.cs
│   │   │   ├── 📄 LifecycleStage.cs
│   │   │   └── 📄 MaintenanceTicketStatus.cs
│   │   │
│   │   ├── 📁 Exceptions/             # ✅ NEW: Domain-specific exceptions
│   │   │   └── 📄 DomainExceptions.cs
│   │   │       ├── DomainException
│   │   │       ├── EntityNotFoundException
│   │   │       ├── BusinessRuleViolationException
│   │   │       └── InvalidOperationException
│   │   │
│   │   ├── 📁 Events/                 # ⏳ NEW: Domain events (empty)
│   │   │   ├── 📄 AssetLifecycleChangedEvent.cs    # ⏳ TODO
│   │   │   ├── 📄 MaintenanceCreatedEvent.cs       # ⏳ TODO
│   │   │   ├── 📄 MaterialAssignedEvent.cs         # ⏳ TODO
│   │   │   ├── 📄 WarrantyExpiringEvent.cs         # ⏳ TODO
│   │   │   └── 📄 AssetDisposedEvent.cs            # ⏳ TODO
│   │   │
│   │   ├── 📁 ValueObjects/           # ⏳ NEW: Value object types (empty)
│   │   │   ├── 📄 SerialNumber.cs     # ⏳ TODO: String value object
│   │   │   ├── 📄 Warranty.cs         # ⏳ TODO: Start/end dates
│   │   │   ├── 📄 Money.cs            # ⏳ TODO: Amount + currency
│   │   │   └── 📄 Quantity.cs         # ⏳ TODO: Value with unit
│   │   │
│   │   ├── 📄 ITStockM.Domain.csproj
│   │   └── 📁 bin/, 📁 obj/
│   │
│   ├── 📁 ITStockM.Application/       # ✅ LAYER: Use Cases & Business Logic Orchestration
│   │   ├── 📁 Features/               # ✅ CQRS FEATURES (3 examples)
│   │   │   │
│   │   │   ├── 📁 AssetLifecycle/
│   │   │   │   ├── 📁 Commands/
│   │   │   │   │   ├── 📄 TransitionAssetStageCommand.cs
│   │   │   │   │   └── 📄 TransitionAssetStageCommandValidator.cs
│   │   │   │   │
│   │   │   │   ├── 📁 Queries/
│   │   │   │   │   ├── 📄 GetLifecycleHistoryQuery.cs
│   │   │   │   │   ├── 📄 GetAssetsByStageQuery.cs
│   │   │   │   │   └── 📄 GetAssetsNearingEndOfLifeQuery.cs
│   │   │   │   │
│   │   │   │   ├── 📁 Handlers/
│   │   │   │   │   └── 📄 AssetLifecycleHandlers.cs
│   │   │   │   │
│   │   │   │   ├── 📁 DTOs/
│   │   │   │   │   └── 📄 TransitionRequestDto.cs
│   │   │   │   │
│   │   │   │   └── 📁 Validators/
│   │   │   │       └── 📄 (validators)
│   │   │   │
│   │   │   ├── 📁 Maintenance/
│   │   │   │   ├── 📁 Commands/
│   │   │   │   ├── 📁 Queries/
│   │   │   │   ├── 📁 Handlers/
│   │   │   │   └── 📁 DTOs/
│   │   │   │
│   │   │   ├── 📁 Predictions/
│   │   │   │   ├── 📁 Commands/
│   │   │   │   ├── 📁 Queries/
│   │   │   │   ├── 📁 Handlers/
│   │   │   │   └── 📁 DTOs/
│   │   │   │
│   │   │   ├── 📁 Materials/          # ⏳ TODO: Add CQRS for materials (bulk of features)
│   │   │   ├── 📁 Assignments/        # ⏳ TODO: Add CQRS for assignments
│   │   │   ├── 📁 DeliveryOrders/     # ⏳ TODO: Add CQRS for delivery orders
│   │   │   └── 📁 Common/             # Feature-shared items
│   │   │       ├── 📁 DTOs/
│   │   │       ├── 📁 Validators/
│   │   │       └── 📁 Queries/        # Shared query extensions
│   │   │
│   │   ├── 📁 Common/                 # ✅ APPLICATION-WIDE INFRASTRUCTURE
│   │   │   ├── 📁 Interfaces/         # ✅ 25 SERVICE INTERFACES
│   │   │   │   ├── 📄 IApplicationDbContext.cs
│   │   │   │   ├── 📄 IDateTimeService.cs
│   │   │   │   ├── 📄 ICurrentUserService.cs
│   │   │   │   ├── 📄 IUserService.cs
│   │   │   │   ├── 📄 IAuthService.cs
│   │   │   │   ├── 📄 IEmailService.cs
│   │   │   │   ├── 📄 INotificationService.cs
│   │   │   │   ├── 📄 IAssignmentService.cs
│   │   │   │   ├── 📄 IMaterielService.cs
│   │   │   │   ├── 📄 IRequestService.cs
│   │   │   │   ├── 📄 IDeliveryOrderService.cs
│   │   │   │   ├── 📄 IOfferService.cs
│   │   │   │   ├── 📄 IProjectService.cs
│   │   │   │   ├── 📄 ISupplierService.cs
│   │   │   │   ├── 📄 IMaintenanceService.cs
│   │   │   │   ├── 📄 IPredictionService.cs
│   │   │   │   ├── 📄 IAssetLifecycleService.cs
│   │   │   │   ├── 📄 IWarrantyAlertService.cs
│   │   │   │   ├── 📄 IEmailTemplateService.cs
│   │   │   │   └── (more...)
│   │   │   │
│   │   │   ├── 📁 Behaviors/          # ✅ MediatR PIPELINE
│   │   │   │   └── 📄 ValidationBehavior.cs
│   │   │   │
│   │   │   ├── 📁 Mapping/            # ✅ AutoMapper PROFILES
│   │   │   │   └── 📄 MappingProfile.cs
│   │   │   │
│   │   │   ├── 📁 Constants/
│   │   │   │   ├── 📄 RouteConstants.cs
│   │   │   │   └── 📄 UserRoles.cs
│   │   │   │
│   │   │   ├── 📁 ViewModels/         # Application-level DTOs
│   │   │   │   ├── 📄 LoginViewModel.cs
│   │   │   │   ├── 📄 MaterielViewModel.cs
│   │   │   │   └── (view models)
│   │   │   │
│   │   │   ├── 📄 QueryExtensions.cs
│   │   │   └── 📄 (other utilities)
│   │   │
│   │   ├── 📁 DependencyInjection/    # ✅ DI CONFIGURATION
│   │   │   └── 📄 ApplicationLayerServiceCollectionExtensions.cs
│   │   │       ├── services.AddMediatR()
│   │   │       ├── services.AddAutoMapper()
│   │   │       ├── services.AddFluentValidation()
│   │   │       └── services.AddBehavior()
│   │   │
│   │   ├── 📄 ITStockM.Application.csproj
│   │   └── 📁 bin/, 📁 obj/
│   │
│   ├── 📁 ITStockM.Infrastructure/    # ✅ LAYER: Technical Implementation
│   │   ├── 📁 Persistence/            # ✅ DATABASE & ORM
│   │   │   ├── 📄 ITStockManagmentContext.cs    # EF Core DbContext
│   │   │   ├── 📁 Configurations/
│   │   │   │   └── (Entity configurations)
│   │   │   │
│   │   │   ├── 📁 Migrations/
│   │   │   │   ├── 📄 20250806110553_InitialCreate.cs
│   │   │   │   ├── 📄 20260122141429_AddUsersTable.cs
│   │   │   │   ├── 📄 20260304002727_AddEmployeeRoleAndLifecycle.cs
│   │   │   │   └── 📄 ITStockManagmentContextModelSnapshot.cs
│   │   │   │
│   │   │   ├── 📄 DatabaseInitializer.cs
│   │   │   └── 📄 BlankTriggerAddingConvention.cs
│   │   │
│   │   ├── 📁 Services/               # ✅ IMPLEMENTS APPLICATION INTERFACES (25 services, 3,671 LOC)
│   │   │   ├── 📄 DateTimeService.cs
│   │   │   ├── 📄 CurrentUserService.cs
│   │   │   ├── 📄 AuthService.cs
│   │   │   │
│   │   │   ├── 📄 MaterielService.cs  # ⚠️ Business services with duplicate code
│   │   │   ├── 📄 AssignmentService.cs
│   │   │   ├── 📄 DeliveryOrderService.cs
│   │   │   ├── 📄 RequestService.cs
│   │   │   ├── 📄 OfferService.cs
│   │   │   ├── 📄 ProjectService.cs
│   │   │   ├── 📄 SupplierService.cs
│   │   │   ├── 📄 EmployeeService.cs
│   │   │   ├── 📄 MaintenanceService.cs
│   │   │   ├── 📄 PredictionService.cs
│   │   │   ├── 📄 AssetLifecycleService.cs
│   │   │   ├── 📄 WarrantyAlertService.cs
│   │   │   │
│   │   │   ├── 📄 EmailService.cs
│   │   │   ├── 📄 EmailTemplateService.cs
│   │   │   ├── 📄 NotificationService.cs
│   │   │   ├── 📄 OperationNotificationService.cs
│   │   │   │
│   │   │   ├── 📁 Background Services/
│   │   │   │   ├── 📄 EmailBackgroundService.cs
│   │   │   │   ├── 📄 AssetHealthBackgroundService.cs
│   │   │   │   ├── 📄 SmtpHealthChecker.cs
│   │   │   │   └── 📄 SmtpHealthHostedService.cs
│   │   │   │
│   │   │   └── 📄 EmailOptions.cs
│   │   │
│   │   ├── 📁 Repositories/           # ✅ DATA ACCESS
│   │   │   ├── 📄 IRepository.cs      # Generic interface
│   │   │   ├── 📄 EfRepository.cs     # Generic implementation
│   │   │   │
│   │   │   ├── 📄 IMaterielRepository.cs
│   │   │   ├── 📄 MaterielRepository.cs
│   │   │   │
│   │   │   ├── 📄 IAssignmentRepository.cs
│   │   │   ├── 📄 AssignmentRepository.cs
│   │   │   │
│   │   │   ├── 📄 IDeliveryOrderRepository.cs
│   │   │   ├── 📄 DeliveryOrderRepository.cs
│   │   │   │
│   │   │   ├── 📄 IRequestRepository.cs
│   │   │   ├── 📄 RequestRepository.cs
│   │   │   │
│   │   │   └── (more specialized repositories)
│   │   │
│   │   ├── 📁 DependencyInjection/    # ✅ DI CONFIGURATION
│   │   │   ├── 📄 InfrastructureLayerServiceCollectionExtensions.cs
│   │   │   │   ├── services.AddDbContext()
│   │   │   │   ├── services.AddScoped<IXService, XService>() (25 services)
│   │   │   │   ├── services.AddHostedService<IBackgroundService>()
│   │   │   │   └── services.AddRepositories()
│   │   │   │
│   │   │   └── 📄 InfrastructureApplicationBuilderExtensions.cs
│   │   │       └── app.InitializeInfrastructureAsync()
│   │   │
│   │   ├── 📄 ITStockM.Infrastructure.csproj
│   │   └── 📁 bin/, 📁 obj/
│   │
│   └── (no ITStockM.WebApi folder - using root project as presentation)
│
├── 📁 Components/                    # ✅ BLAZOR UI COMPONENTS (87 total)
│   ├── 📁 Layout/
│   │   ├── 📄 MainLayout.razor
│   │   ├── 📄 MainLayout.razor.css
│   │   └── 📄 SideLayout.razor
│   │
│   ├── 📁 Pages/                    # ✅ 87 Blazor pages
│   │   ├── 📁 CrudPages/            # 31 CRUD components
│   │   │   ├── 📄 Materiels.razor
│   │   │   ├── 📄 AddMateriel.razor
│   │   │   ├── 📄 EditMateriel.razor
│   │   │   ├── 📄 Assignments.razor
│   │   │   ├── 📄 AddAssignment.razor
│   │   │   ├── 📄 EditAssignment.razor
│   │   │   ├── 📄 Requests.razor
│   │   │   ├── 📄 AddRequest.razor
│   │   │   ├── 📄 EditRequest.razor
│   │   │   ├── (and 22 more...)
│   │   │   └── (each with .razor.cs code-behind file)
│   │   │
│   │   ├── 📁 MaterialsView/
│   │   ├── 📁 MaterialsAssignments/
│   │   ├── 📁 DeliveryOrder/
│   │   ├── 📁 Maintenance/
│   │   ├── 📁 Predictions/
│   │   ├── 📁 AssetLifecycle/
│   │   ├── 📁 ArchivedRequests/
│   │   ├── 📁 Purchase/
│   │   ├── 📁 Supplier/
│   │   ├── 📁 PendingDeliveries/
│   │   ├── 📁 MaterialsViewPDR/
│   │   ├── 📁 Infra/
│   │   ├── 📁 auth/
│   │   │   ├── 📄 Login.razor
│   │   │   └── 📄 AccessDenied.razor
│   │   │
│   │   └── 📄 Index.razor
│   │
│   ├── 📄 App.razor                 # Root component
│   └── 📄 Routes.razor              # Routing configuration
│
├── 📁 Presentation/                # ✅ API LAYER (EXISTING, SHOULD MOVE TO ROOT)
│   ├── 📁 Controllers/             # ✅ REST API CONTROLLERS
│   │   ├── 📄 AssetLifecycleController.cs   # ✅ Clean MediatR
│   │   ├── 📄 MaintenanceController.cs      # ✅ Clean MediatR
│   │   ├── 📄 PredictionsController.cs      # ✅ Clean MediatR
│   │   ├── 📄 ExportController.cs
│   │   ├── 📄 ExportITStockManagmentController.cs
│   │   ├── 📄 EmailTestController.cs
│   │   ├── 📄 DevAuthController.cs
│   │   └── 📄 DevController.cs
│   │
│   ├── 📁 Middleware/              # ✅ HTTP MIDDLEWARE
│   │   └── 📄 GlobalExceptionMiddleware.cs
│   │
│   ├── 📁 Auth/                    # ✅ AUTHENTICATION
│   │   └── 📄 CustomAuthenticationStateProvider.cs
│   │
│   ├── 📁 Services/                # ✅ UI SUPPORT SERVICES
│   │   ├── 📄 AppThemeService.cs
│   │   ├── 📄 ITStockManagmentService.cs
│   │   │
│   │   └── 📁 Export/
│   │       ├── 📄 IExportService.cs
│   │       └── 📄 ExportService.cs
│   │
│   ├── 📁 DependencyInjection/     # ✅ DI CONFIGURATION
│   │   └── 📄 PresentationLayerServiceCollectionExtensions.cs
│   │
│   └── 📁 Filters/                 # (Authorization, etc.)
│
├── 📁 wwwroot/                     # ✅ Static files (CSS, JS, images)
│
├── 📁 Properties/
│   └── 📄 launchSettings.json
│
├── 📁 ITStockM.Tests/              # ✅ TESTS PROJECT
│   ├── 📄 ITStockM.Tests.csproj
│   └── (test files)
│
├── 📁 scripts/                     # Build/deployment scripts
│
├── 📁 docs/                        # Documentation
│
├── Dockerfile                      # Docker configuration
├── docker-compose.yml              # Docker Compose for local dev
└── global.json                     # .NET SDK version lock
```

---

## Key Architectural Indicators

### ✅ Correct Dependencies

```
Presentation Layer (Root + Controllers)
        ↓ IMediator
Application Layer (Features, Commands, Queries)
        ↓ IApplicationDbContext + Service Interfaces
Infrastructure Layer (Services, Repositories)
        ↓ DbContext
Domain Layer (Entities, Enums, Exceptions)
        ↓ NOTHING - Pure business logic
```

### ✅ Layer Responsibilities

| Layer | What It Does | What It Doesn't Do |
|-------|-------------|------------------|
| **Domain** | Defines what the business is | Knows nothing about how (tech) |
| **Application** | Orchestrates business operations | Implements technical details |
| **Infrastructure** | Implements technical solutions | Makes business decisions |
| **Presentation** | Handles user interaction | Contains business logic |

### ⏳ Refactoring Priorities

1. **CRITICAL** - Fix Employee password storage (🔴 Security)
2. **HIGH** - Update entities to use BaseEntity
3. **HIGH** - Replace generic exceptions
4. **MEDIUM** - Refactor service duplication
5. **MEDIUM** - Add domain events
6. **LOW** - Optimize Blazor components

---

## Summary

The solution is now properly structured according to Clean Architecture principles:

- ✅ 4 distinct, separated layers
- ✅ Proper dependency flow (inward only)
- ✅ CQRS pattern implemented via MediatR
- ✅ Domain layer with base classes and exceptions
- ✅ Strong DI configuration per layer
- ✅ Comprehensive documentation created

**Ready for**: Entity inheritance, security fixes, and service refactoring.
