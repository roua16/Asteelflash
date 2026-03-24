# IT Inventory Management System — AsteelFlash
## Full Technical Documentation

> **Version:** 1.0.0 · **Framework:** .NET 10 · **Last Updated:** March 4, 2026

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Tech Stack](#2-tech-stack)
3. [Project Structure](#3-project-structure)
4. [Domain Models](#4-domain-models)
5. [Database & Migrations](#5-database--migrations)
6. [Roles & Privilege System](#6-roles--privilege-system)
7. [Authentication System](#7-authentication-system)
8. [Seeding System](#8-seeding-system)
9. [Repository Layer](#9-repository-layer)
10. [Service Layer](#10-service-layer)
11. [API Controllers](#11-api-controllers)
12. [Export Module](#12-export-module)
13. [Email & Notification Module](#13-email--notification-module)
14. [Asset Lifecycle Module](#14-asset-lifecycle-module)
15. [Maintenance Module](#15-maintenance-module)
16. [Predictions & Health Scoring Module](#16-predictions--health-scoring-module)
17. [Background Services](#17-background-services)
18. [Blazor Pages & UI](#18-blazor-pages--ui)
19. [DTOs & AutoMapper](#19-dtos--automapper)
20. [Configuration & Environment Variables](#20-configuration--environment-variables)
21. [Docker & Deployment](#21-docker--deployment)
22. [Testing](#22-testing)
23. [Enums & Constants Reference](#23-enums--constants-reference)

---

## 1. Project Overview

**IT Stock Management (ITStockM)** is an internal enterprise web application built for **AsteelFlash** to manage:

- IT equipment inventory (materiels / PDR stock)
- Employee asset assignments
- Material purchase requests and delivery orders
- Supplier and project tracking
- Asset lifecycle tracking (Purchased → InStock → Assigned → UnderMaintenance → Retired)
- Maintenance ticketing (fault reporting, repair workflow, cost tracking)
- Predictive health scoring and replacement forecasting
- Role-based access control for 6 distinct department roles
- Email notifications for key system events

The application is deployed as a **Dockerized Blazor Server** application backed by **SQL Server 2022**.

---

## 2. Tech Stack

| Layer | Technology |
|-------|-----------|
| **Runtime** | .NET 10 (`net10.0`) |
| **Web Framework** | ASP.NET Core + Blazor Server (Interactive Server Components) |
| **UI Component Library** | [Radzen.Blazor](https://blazor.radzen.com/) (latest) |
| **ORM** | Entity Framework Core 8.0.10 (`Microsoft.EntityFrameworkCore.SqlServer`) |
| **Micro-ORM** | Dapper 2.1.66 (used in `AuthService` for login queries) |
| **Database** | SQL Server 2022 (Docker: `mcr.microsoft.com/mssql/server:2022-latest`) |
| **Email** | MailKit 4.14.1 (SMTP with retry + exponential back-off) |
| **Exports** | DocumentFormat.OpenXml 3.3.0 (Excel .xlsx), built-in CSV streaming |
| **Mapping** | AutoMapper 12.0.1 (`AutoMapper.Extensions.Microsoft.DependencyInjection`) |
| **Dynamic Queries** | `System.Linq.Dynamic.Core` 1.7.1 |
| **AD Integration** | `System.DirectoryServices` 8.0.0 (Windows-only LDAP auth) |
| **API Docs** | Swashbuckle / Swagger (`/swagger` in Development) |
| **Auth** | Cookie Authentication (`Microsoft.AspNetCore.Authentication.Cookies`) |
| **Auth State** | Custom `AuthenticationStateProvider` + `ProtectedLocalStorage` |
| **SMTP Dev** | smtp4dev (Docker container `rnwood/smtp4dev`) |
| **Testing** | xUnit + FluentAssertions + Moq |
| **Containerization** | Docker + Docker Compose |

---

## 3. Project Structure

```
ITStockM.sln
├── ITStockM.csproj                    # Main application
│   ├── Program.cs                     # DI root, middleware pipeline
│   ├── appsettings.json               # Base configuration
│   ├── appsettings.Development.json   # Dev overrides
│   ├── Dockerfile
│   ├── docker-compose.yml
│   │
│   ├── Components/                    # Blazor components
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   ├── _Imports.razor
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor(.cs)  # Root layout, theme
│   │   │   └── SideLayout.razor(.cs)  # Sidebar with nav links + user session
│   │   └── Pages/
│   │       ├── Index.razor(.cs)       # Dashboard
│   │       ├── auth/                  # Login, access-denied, redirect
│   │       ├── CRUDpages/             # Generic material CRUD
│   │       ├── MaterialsView/         # IT stock grid
│   │       ├── MaterialsViewPDR/      # PDR stock grid
│   │       ├── MaterialsAssignments/  # Assignment management
│   │       ├── DeleveryOrder/         # Delivery order management
│   │       ├── PendingDeliveries/     # Pending delivery details
│   │       ├── Purchase/              # Purchase request flow
│   │       ├── Infra/                 # Infrastructure requests
│   │       ├── ArchivedRequests/      # Archived request history
│   │       ├── Supplier/              # Supplier management
│   │       ├── AssetLifecycle/        # ← NEW: Lifecycle tracking page
│   │       ├── Maintenance/           # ← NEW: Maintenance ticketing page
│   │       └── Predictions/           # ← NEW: Health predictions page
│   │
│   ├── Controllers/                   # ASP.NET MVC API controllers
│   │   ├── ExportController.cs        # Base export helper
│   │   ├── ExportITStockManagmentController.cs
│   │   ├── AssetLifecycleController.cs
│   │   ├── MaintenanceController.cs
│   │   ├── PredictionsController.cs
│   │   ├── DevAuthController.cs       # Dev-only auth shortcuts
│   │   ├── DevController.cs
│   │   └── EmailTestController.cs
│   │
│   ├── Data/
│   │   ├── ITStockManagmentContext.cs  # EF Core DbContext
│   │   ├── DatabaseInitializer.cs     # MigrateAsync + seeding
│   │   └── BlankTriggerAddingConvention.cs
│   │
│   ├── Models/
│   │   ├── Constants/
│   │   │   ├── UserRoles.cs           # Role string constants + groupings
│   │   │   └── RouteConstants.cs
│   │   ├── Enums/
│   │   │   ├── LifecycleStage.cs
│   │   │   ├── HealthStatus.cs
│   │   │   └── MaintenanceTicketStatus.cs
│   │   ├── ITStockManagment/          # Domain entities (EF models)
│   │   └── ViewModels/                # AppUser, etc.
│   │
│   ├── DTOs/                          # Request/response transfer objects
│   ├── Repositories/                  # Data access abstractions + EF implementations
│   ├── Services/                      # Business logic layer
│   │   ├── Interfaces/                # Service contracts
│   │   ├── Implementation/            # Email, notification, SMTP health
│   │   ├── AssetLifecycle/            # AssetLifecycleService
│   │   ├── Maintenance/               # MaintenanceService
│   │   ├── Prediction/                # PredictionService
│   │   ├── Assignments/
│   │   ├── DeliveryOrders/
│   │   ├── Export/
│   │   ├── Materiels/
│   │   ├── Requests/
│   │   └── Utilities/
│   │
│   ├── Migrations/                    # EF Core migration history
│   └── wwwroot/                       # Static files (CSS, JS, images)
│
└── ITStockM.Tests/                    # xUnit test project
    ├── Services/
    │   ├── EmailServiceTests.cs
    │   ├── PredictionServiceTests.cs
    │   └── MaintenanceServiceTests.cs
    ├── Repositories/
    └── Ui/
```

---

## 4. Domain Models

All entities map to the `dbo` SQL Server schema.

### 4.1 Materiel
**Table:** `dbo.Materiel`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | int PK | Auto-increment identity |
| `MaterielName` | nvarchar | Equipment name |
| `Type` | nvarchar | Asset type category |
| `SerialNumber` | nvarchar? | Optional unique serial |
| `QuantityITStock` | int | Current IT stock count |
| `QuantityPDRStock` | int | Current PDR stock count |
| `IrreparableQuantity` | int | Scrapped/written-off units |
| `Repairing_Quantity` | int | Units currently in repair |
| `Warranty` | datetime2 | Warranty expiry date |
| `PurchaseDate` | datetime2? | Date asset entered system |
| `ExpectedLifetimeMonths` | int? | Useful life (default: 48 months) |
| `CurrentHealthScore` | decimal(5,2)? | Latest daily score 0–100 |
| `LifecycleStatus` | nvarchar(50)? | Current stage string |

**Navigation Collections:** `MaintenanceTickets`, `AssetLifecycleRecords`

---

### 4.2 Employee
**Table:** `dbo.Employee`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | int PK | Auto-increment identity |
| `FullName` | nvarchar | Display name |
| `Email` | nvarchar | Login email (unique) |
| `Password` | nvarchar | Plaintext (internal system) |
| `Post` | nvarchar | Job title / post |
| `PhoneNumber` | nvarchar | Contact number |
| `Service` | nvarchar | Department name |
| `Role` | nvarchar | Role string (see §6) |

**Navigation Collections:** `Assignments`, `Requests`, `DeliveryOrders`

---

### 4.3 Assignment
**Table:** `dbo.Assignment`

Links an employee to one or more materiels. Tracks assignment date, project, and return.

---

### 4.4 AssignmentMateriel
**Table:** `dbo.AssignmentMateriel`

Junction table for the many-to-many between `Assignment` and `Materiel`.

---

### 4.5 DeliveryOrder
**Table:** `dbo.DeliveryOrder`

Purchase delivery tracking with status workflow. Linked to employee and materiels.

---

### 4.6 DeliveryOrderMateriel
Junction table for `DeliveryOrder ↔ Materiel`.

---

### 4.7 Request
**Table:** `dbo.Request`

Infrastructure/IT material request submitted by employees, with approval workflow.

---

### 4.8 Supplier
**Table:** `dbo.Supplier`

Vendor records linked to delivery orders and materiels.

---

### 4.9 Project
**Table:** `dbo.Project`

Internal projects to which assignments can be linked.

---

### 4.10 Offer
**Table:** `dbo.Offer`

Purchase offers / quotes referenced by delivery orders.

---

### 4.11 AssetLifecycleRecord *(New)*
**Table:** `dbo.AssetLifecycleRecord`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | int PK | Auto-increment |
| `MaterielId` | int FK | Links to `Materiel` |
| `Stage` | nvarchar(50) | `LifecycleStage` value |
| `StartDate` | datetime2 | When stage began |
| `EndDate` | datetime2? | `null` = still in this stage |
| `Notes` | nvarchar(1000)? | Optional transition notes |

One row per stage transition. The open record (`EndDate IS NULL`) represents the current stage.

---

### 4.12 MaintenanceTicket *(New)*
**Table:** `dbo.MaintenanceTicket`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | int PK | Auto-increment |
| `MaterielId` | int FK | Asset under repair |
| `ProblemDescription` | nvarchar(2000) | Fault description |
| `ReportedByEmployeeId` | int? FK | Optional reporter |
| `Status` | nvarchar(50) | `Open` / `InProgress` / `Closed` |
| `Cost` | decimal(18,2)? | Repair cost in EUR |
| `ReportedAt` | datetime2 | Ticket creation time |
| `ResolvedAt` | datetime2? | Resolution timestamp |
| `Resolution` | nvarchar(2000)? | Resolution notes |

---

### 4.13 AssetPrediction *(New)*
**Table:** `dbo.AssetPrediction`

| Column | Type | Description |
|--------|------|-------------|
| `Id` | int PK | Auto-increment |
| `MaterielId` | int FK | One prediction per asset |
| `CalculatedAt` | datetime2 | Last calculation time |
| `HealthScore` | decimal(5,2) | 0–100 (higher = healthier) |
| `HealthStatus` | nvarchar(20) | `Good` / `Fair` / `Poor` |
| `PredictedFailureDate` | datetime2? | Estimated failure date |
| `RecommendedReplacementDate` | datetime2? | Procurement lead-time adjusted |
| `RecommendationReason` | nvarchar(1000)? | Human-readable rationale |
| `EstimatedReplacementCost` | decimal(18,2)? | Budget forecast |

---

## 5. Database & Migrations

### DbContext
**Class:** `ITStockM.Data.ITStockManagmentContext`  
Registered as `AddDbContext<ITStockManagmentContext>` with `UseSqlServer`.

### Startup Initialization
`DatabaseInitializer.InitializeAsync()` is called at startup inside a service scope:
1. `context.Database.MigrateAsync()` — applies all pending EF migrations
2. Checks `Seed:Enabled` config flag
3. Runs `SeedAdminAsync` on first run (no employees in DB)
4. Optionally runs `SeedDemoDataAsync` when `Seed:DemoData = true`

### Migration History

| Migration | Date | Description |
|-----------|------|-------------|
| `20250806110553_InitialCreate` | 2025-08-06 | Initial schema: Materiel, Employee, Assignment, DeliveryOrder, Request, Supplier, Project |
| `20260122141429_AddUsersTable` | 2026-01-22 | Users/session table additions |
| `20260122142346_NewMigration` | 2026-01-22 | Additional schema refinements |
| `20260208181002_AddEmployeeRoleField` | 2026-02-08 | Added `Role` column to Employee |
| `20260304002727_AddEmployeeRoleAndLifecycle` | 2026-03-04 | Added `AssetLifecycleRecord`, `MaintenanceTicket`, `AssetPrediction` tables + lifecycle fields on `Materiel` |

### Connection Strings

| Environment | Value |
|-------------|-------|
| Local dev | `Server=(localdb)\MSSQLLocalDB;Database=ITStockManagmentContext-...` |
| Docker | `Server=db,1433;Database=ITStockManagment;User Id=sa;Password=...` |

---

## 6. Roles & Privilege System

### Role Definitions
**File:** `Models/Constants/UserRoles.cs`

| Role | Constant | Description |
|------|----------|-------------|
| `Admin` | `UserRoles.Admin` | Full system access — all features, all CRUD |
| `IT` | `UserRoles.IT` | Manages IT stock, assignments, maintenance, lifecycle, predictions |
| `PDR` | `UserRoles.PDR` | Manages PDR stock and material requests |
| `Purchasing` | `UserRoles.Purchasing` | Manages procurement, delivery orders, suppliers |
| `Infrastructure` | `UserRoles.Infrastructure` | Manages infrastructure requests and approvals |
| `Employee` | `UserRoles.Employee` | Read-limited access, can submit requests |

### Role Groups

| Group | Roles |
|-------|-------|
| `UserRoles.Administrative` | Admin, PDR, Purchasing, IT, Infrastructure |
| `UserRoles.MaterialManagers` | Admin, PDR, IT |
| `UserRoles.RequestApprovers` | Admin, Infrastructure, Purchasing |

### Page-Level Authorization

| Page / Feature | Required Roles |
|----------------|---------------|
| Dashboard (Index) | All authenticated |
| Materials — IT View | Admin, IT |
| Materials — PDR View | Admin, PDR |
| Assignments | Admin, IT |
| Delivery Orders | Admin, Purchasing, IT |
| Infrastructure Requests | Admin, Infrastructure |
| Purchase Requests | Admin, Purchasing |
| Suppliers | Admin, Purchasing |
| Asset Lifecycle | Admin, IT |
| Maintenance Tickets | Admin, IT |
| Health Predictions | Admin, IT |
| Archived Requests | Admin, IT, Infrastructure |

Authorization is enforced in Blazor via:
```razor
<AuthorizeView Roles="Admin,IT">
    <Authorized>...</Authorized>
    <NotAuthorized>
        <ITStockM.Components.Pages.auth.RedirectToAccessDenied />
    </NotAuthorized>
</AuthorizeView>
```

New-ticket and close-ticket actions inside Maintenance page additionally check `userRole == "Admin" || userRole == "IT"` before rendering action buttons.

---

## 7. Authentication System

### Cookie Authentication
Configured in `Program.cs`:
```
Cookie name:        IT
Login path:         /login
Access denied:      /access-denied
Session lifetime:   60 minutes
SameSite:           Strict
Secure policy:      SameAsRequest (HTTP-safe in Docker)
```

### AuthService (`Services/AuthService.cs`)
Uses **Dapper** to query `Employee` by email, then compares plaintext password.

```
POST-login flow:
1. AuthService.Authenticate(email, password) → AppUser
2. CustomAuthenticationStateProvider sets claims principal
3. Cookie written with role claim
4. ProtectedLocalStorage stores UserSession (name, role, email)
```

### Active Directory (Windows Only)
`AuthService.ADAuthenticateUser(username, password)` connects to `LDAP://asteelflash.europe.lan` via `System.DirectoryServices`. Only runs on Windows; gracefully skips on Linux/macOS containers.

### CustomAuthenticationStateProvider
**File:** `Services/CustomAuthenticationStateProvider.cs`  
Reads the authentication cookie to build `ClaimsPrincipal`. Injected as scoped alongside `AuthenticationStateProvider`.

### ProtectedLocalStorage Rule
**Critical:** `ProtectedLocalStorage` reads **must only occur** in `OnAfterRenderAsync(firstRender: true)` — never in `OnInitializedAsync`. Pre-render has no JS circuit.

---

## 8. Seeding System

Controlled entirely by `appsettings.json` `Seed` section or environment variables.

### Configuration Flags

| Key | Default | Description |
|-----|---------|-------------|
| `Seed:Enabled` | `true` (dev) | Master switch for all seeding |
| `Seed:DemoData` | `true` (dev) | Seeds demo employees, materiels, etc. |
| `Seed:ClearOldData` | `false` | Destructive clear — only dev + first run |
| `Seed:AdminEmail` | `admin@asteelflash.com` | Seeded admin email |
| `Seed:AdminPassword` | `admin123` | Seeded admin password |
| `Seed:AdminFullName` | `Admin User` | Admin display name |
| `Seed:AdminPhoneNumber` | `+0000000000` | Admin phone |
| `Seed:AdminService` | `IT` | Admin department |

### Seed Behavior
1. **Always:** Runs `MigrateAsync()` regardless of seed flags.
2. **First-run detection:** Checks `Employees.Any()` — if false, seeds apply.
3. **Admin seed:** Creates or upgrades an employee to `Admin` role.
4. **Demo seed (when `DemoData=true`):** Seeds:
   - 7+ demo employees covering all roles (PDR, Purchasing, IT, Infrastructure, Employee)
   - Sample materiels (laptops, monitors, keyboards, servers, network switches)
   - Sample assignments, delivery orders, suppliers, requests
   - Sample lifecycle records, maintenance tickets, asset predictions

### Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@asteelflash.com` | `admin123` |
| PDR | `john.smith@asteelflash.com` | `password123` |
| Purchasing | `sarah.johnson@asteelflash.com` | `password123` |
| IT | `mike.davis@asteelflash.com` | `password123` |
| Infrastructure | `alice.brown@asteelflash.com` | `password123` |

---

## 9. Repository Layer

**Pattern:** Generic Repository + Specialized Repositories  
**Registration:** `ServiceCollectionExtensions.AddApplicationServices()`

### Generic Repository

```csharp
IRepository<T>         → EfRepository<T>
```

Provides: `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`

### Specialized Repositories

| Interface | Implementation | Purpose |
|-----------|---------------|---------|
| `IMaterielRepository` | `MaterielRepository` | Materiel-specific queries (low stock, by type) |
| `IAssignmentRepository` | `AssignmentRepository` | Assignment queries with employee/materiel joins |
| `IRequestRepository` | `RequestRepository` | Request queries with status filtering |
| `IDeliveryOrderRepository` | `DeliveryOrderRepository` | Delivery order queries with status/date filters |

---

## 10. Service Layer

### 10.1 ITStockManagmentService *(Core)*
**File:** `Services/ITStockManagmentService.cs`  
Scoped service that aggregates most DB operations. Used directly by Blazor pages and export controllers.

Key methods:
- `GetMateriels()`, `GetAssignments()`, `GetEmployees()`
- `GetDeliveryOrders()`, `GetDeliveryOrdersList()`
- `GetRequests()`, `GetSuppliers()`, `GetProjects()`
- `GetAssignmentMateriels()`, `GetDeliveryOrderMateriels()`
- All corresponding `Create`, `Update`, `Delete` variants

### 10.2 Domain Services

| Interface | Implementation | Namespace |
|-----------|---------------|-----------|
| `IAssignmentService` | `AssignmentService` | `Services.Assignments` |
| `IMaterielService` | `MaterielService` | `Services.Materiels` |
| `IRequestService` | `RequestService` | `Services.Requests` |
| `IDeliveryOrderService` | `DeliveryOrderService` | `Services.DeliveryOrders` |

### 10.3 Asset Lifecycle Service *(New)*
**Interface:** `IAssetLifecycleService`  
**Implementation:** `Services/AssetLifecycle/AssetLifecycleService.cs`

| Method | Description |
|--------|-------------|
| `GetCurrentStagesAsync()` | All open lifecycle records (EndDate = null) |
| `GetLifecycleHistoryAsync(materielId)` | Full stage history for one asset |
| `TransitionStageAsync(materielId, newStage, notes)` | Closes current record, opens new one, updates `Materiel.LifecycleStatus` |
| `GetAssetsByStageAsync(stage)` | All materiels in a given stage |
| `GetAssetsNearingEndOfLifeAsync(withinMonths)` | Assets whose warranty or calculated EOL is within N months |

### 10.4 Maintenance Service *(New)*
**Interface:** `IMaintenanceService`  
**Implementation:** `Services/Maintenance/MaintenanceService.cs`

| Method | Description |
|--------|-------------|
| `GetAllTicketsAsync()` | All tickets with Materiel and Employee navigation |
| `GetTicketsByMaterielAsync(materielId)` | Tickets for a specific asset |
| `GetTicketByIdAsync(id)` | Single ticket lookup |
| `CreateTicketAsync(ticket)` | Creates ticket, transitions asset to `UnderMaintenance` |
| `UpdateTicketAsync(id, ticket)` | Full ticket update |
| `CloseTicketAsync(id, resolution, cost)` | Sets Status=Closed, records resolution, transitions asset back to InStock |
| `GetOpenTicketCountAsync()` | Count of non-closed tickets |
| `GetTicketCountPerMaterielAsync()` | Dictionary used by health scoring |

### 10.5 Prediction Service *(New)*
**Interface:** `IPredictionService`  
**Implementation:** `Services/Prediction/PredictionService.cs`

| Method | Description |
|--------|-------------|
| `CalculateAndSavePredictionAsync(materielId)` | Computes and persists prediction for one asset |
| `RecalculateAllPredictionsAsync()` | Processes all non-retired assets in bulk |
| `GetLatestPredictionsAsync()` | Latest persisted prediction per asset |
| `GetHighRiskAssetsAsync(maxScore)` | Assets with health score ≤ maxScore (default 50) |
| `ComputeHealthScore(...)` | **Pure function** — testable without DB |

**Health Score Algorithm** (inputs → 0–100):

| Factor | Weight | Impact |
|--------|--------|--------|
| Age vs expected lifetime | High | Score decreases as asset ages |
| Open maintenance tickets | High | Each open ticket penalizes score |
| Closed maintenance tickets | Medium | History of repairs lowers confidence |
| Total assignments | Low | Frequent reassignments indicate wear |

Score bands: `Good ≥ 75`, `Fair ≥ 45`, `Poor < 45`

### 10.6 Warranty Alert Service *(New)*
**Interface:** `IWarrantyAlertService`  
**Implementation:** `Services/Implementation/WarrantyAlertService.cs`

Scans all materiels; sends email alerts for any whose `Warranty` date or calculated end-of-life falls within the configured look-ahead window (default: 30 days). Called daily by `AssetHealthBackgroundService`.

### 10.7 Notification Services

| Interface | Implementation | Purpose |
|-----------|---------------|---------|
| `INotificationService` | `NotificationService` | Assignment, request, material, supplier, project events |
| `IOperationNotificationService` | `OperationNotificationService` | Generic operational notifications |

### 10.8 Email Service *(SOLID Refactored)*
**Interface:** `IEmailService`  
**Implementation:** `Services/Implementation/EmailService.cs`

| Method | Description |
|--------|-------------|
| `SendEmailAsync(to, subject, body)` | Send to single recipient |
| `SendEmailToMultipleAsync(toList, subject, body)` | Batch send |
| `SendAdminNotificationAsync(action, details, performedBy)` | Admin event notification |

**SOLID Design:**
- `IOptions<EmailOptions>` injection — no hard-coded config
- Exponential back-off retry (`MaxRetryAttempts` × `RetryDelayMs` doubling)
- `SmtpHealthChecker` singleton validates SMTP connectivity at startup
- `SmtpHealthHostedService` monitors SMTP health continuously

### 10.9 Email Template Service *(New)*
**Interface:** `IEmailTemplateService`  
**Implementation:** `Services/Implementation/EmailTemplateService.cs`  
Registered as **Singleton** (stateless HTML builder).

Generates branded HTML email bodies for: warranty expiry, high-risk asset forecast, maintenance opened/closed, assignment notifications.

---

## 11. API Controllers

All REST controllers return JSON. Swagger UI available at `/swagger` in Development.

### 11.1 AssetLifecycleController
**Route prefix:** `api/assets/lifecycle`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/{materielId}/history` | Full stage-transition history |
| POST | `/{materielId}/transition` | Transition to a new stage |
| GET | `/by-stage/{stage}` | Assets currently in a stage |
| GET | `/nearing-eol?withinMonths=6` | Assets approaching end-of-life |

**Transition request body:**
```json
{ "stage": "Assigned", "notes": "Issued to employee" }
```

---

### 11.2 MaintenanceController
**Route prefix:** `api/assets/maintenance`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | All maintenance tickets |
| GET | `/{id}` | Single ticket by ID |
| GET | `/materiel/{materielId}` | Tickets for an asset |
| POST | `/` | Create new ticket |
| PUT | `/{id}` | Update ticket |
| POST | `/{id}/close` | Close ticket with resolution |
| GET | `/count/open` | Count of open tickets |

**Close ticket body:**
```json
{ "resolution": "Replaced HDD", "cost": 120.00 }
```

---

### 11.3 PredictionsController
**Route prefix:** `api/assets/predictions`

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/` | Latest prediction for all assets |
| GET | `/high-risk?maxScore=50` | Assets below health threshold |
| POST | `/{materielId}/recalculate` | Force-recalculate one asset |
| POST | `/recalculate-all` | Trigger full recalculation batch |

---

### 11.4 DevAuthController *(Development Only)*
**Route prefix:** `api/devauth`  
Provides shortcut login endpoints for development/testing without going through the login form.

---

### 11.5 DevController *(Development Only)*
Miscellaneous dev-only diagnostic endpoints.

---

### 11.6 EmailTestController *(Development Only)*
Allows manually triggering test emails to verify SMTP connectivity in dev/Docker.

---

## 12. Export Module

**Base class:** `Controllers/ExportController.cs`  
**Main controller:** `Controllers/ExportITStockManagmentController.cs`

### Available Export Endpoints

| Entity | CSV Route | Excel Route |
|--------|-----------|-------------|
| Assignments | `/export/ITStockManagment/assignments/csv` | `.../excel` |
| Assignment Materiels | `/export/ITStockManagment/assignmentmateriels/csv` | `.../excel` |
| Delivery Orders | `/export/ITStockManagment/deliveryorders/csv` | `.../excel` |
| Delivery Order Materiels | `/export/ITStockManagment/deliveryordermateriels/csv` | `.../excel` |
| Employees | `/export/ITStockManagment/employees/csv` | `.../excel` |
| Materiels | `/export/ITStockManagment/materiels/csv` | `.../excel` |
| Requests | `/export/ITStockManagment/requests/csv` | `.../excel` |
| Suppliers | `/export/ITStockManagment/suppliers/csv` | `.../excel` |
| Projects | `/export/ITStockManagment/projects/csv` | `.../excel` |
| Maintenance Tickets *(New)* | `/export/ITStockManagment/maintenance/csv` | `.../excel` |
| Asset Predictions *(New)* | `/export/ITStockManagment/predictions/csv` | `.../excel` |
| Lifecycle Records *(New)* | `/export/ITStockManagment/lifecycle/csv` | `.../excel` |

All routes support an optional `fileName` parameter:
```
GET /export/ITStockManagment/materiels/csv(fileName='my-export')
```

OData-style query parameters (`$filter`, `$orderby`, `$top`, `$skip`) are supported via `ApplyQuery`.

---

## 13. Email & Notification Module

### EmailOptions Configuration
**Section:** `EmailSettings` in `appsettings.json`

| Property | Description | Default |
|----------|-------------|---------|
| `SmtpServer` | SMTP hostname | `smtp.gmail.com` |
| `SmtpPort` | SMTP port | `587` |
| `FromEmail` | Sender address | *(required)* |
| `Password` | SMTP password | *(required)* |
| `AdminEmail` | Admin alert recipient | Same as `FromEmail` |
| `MaxRetryAttempts` | Retry count on failure | `3` |
| `RetryDelayMs` | Base retry delay (doubles per attempt) | `1500` ms |

### Environment Variable Overrides

| Variable | Maps To |
|----------|---------|
| `SMTP_SERVER` | `SmtpServer` |
| `SMTP_PORT` | `SmtpPort` |
| `SMTP_FROM_EMAIL` | `FromEmail` |
| `SMTP_PASSWORD` | `Password` |
| `SMTP_ADMIN_EMAIL` | `AdminEmail` |

### Notification Events

| Event | Trigger | Recipients |
|-------|---------|-----------|
| Material Assignment | Employee assigned equipment | Admin |
| Delivery Order Created | New delivery order | Admin |
| Request Submitted | Employee submits request | Admin |
| Material Change | Add / update material | Admin |
| Supplier Change | Add / update supplier | Admin |
| Project Change | Add / update project | Admin |
| Warranty Expiry | Asset warranty within 30 days | Admin (daily) |
| High-Risk Asset Forecast | Asset health score ≤ 50 | Admin (daily) |
| Maintenance Opened | New ticket created | Admin |
| Maintenance Closed | Ticket resolved | Admin |

---

## 14. Asset Lifecycle Module

### Overview
Tracks each IT asset through a defined set of lifecycle stages. Every stage transition writes an audit record with timestamps and optional notes.

### Lifecycle Stages

```
Purchased → InStock → Assigned → UnderMaintenance → Retired
               ↑_____________________|
```

| Stage | Constant | Description |
|-------|----------|-------------|
| `Purchased` | `LifecycleStage.Purchased` | Asset acquired, not yet in inventory |
| `InStock` | `LifecycleStage.InStock` | In the warehouse/storage |
| `Assigned` | `LifecycleStage.Assigned` | Issued to an employee or project |
| `UnderMaintenance` | `LifecycleStage.UnderMaintenance` | In repair / out of service |
| `Retired` | `LifecycleStage.Retired` | Decommissioned — excluded from health scoring |

### Automatic Stage Transitions

| Action | From | To |
|--------|------|----|
| `CreateTicketAsync` | Any | `UnderMaintenance` |
| `CloseTicketAsync` | `UnderMaintenance` | `InStock` |

### Blazor Page
**Route:** `/lifecycle`  
**File:** `Components/Pages/AssetLifecycle/LifecyclePage.razor`  
**Roles:** Admin, IT  

Features: stage filter dropdown, search by name/serial, `RadzenDataGrid` with paging/sorting/filtering, stage badges with `--rz-*` color vars.

---

## 15. Maintenance Module

### Ticket Workflow

```
Created (Open) → InProgress → Closed
```

### Blazor Page
**Route:** `/maintenance`  
**File:** `Components/Pages/Maintenance/MaintenancePage.razor`  
**Roles:** Admin, IT  

Features:
- KPI cards: Open (danger), InProgress (warning), Closed (success)
- Status filter + search bar
- **New Ticket** inline modal (Admin/IT only): select asset, enter problem description
- **Close Ticket** inline modal (Admin/IT only): enter resolution notes + cost
- CSV export via `/export/ITStockManagment/maintenance/csv`
- `RadzenDataGrid` showing: Asset name, Serial #, Problem, Status badge, Reported date, Cost (€), Actions

---

## 16. Predictions & Health Scoring Module

### Health Score Formula

```
score = 100
      - (ageMonths / expectedLifetimeMonths) × 40   [age factor]
      - (openTickets × 15)                           [active faults]
      - (closedTickets × 5)                          [repair history]
      - (totalAssignments × 0.5)                     [usage wear]
      clamped to [0, 100]
```

### Health Bands

| Status | Score Range | Badge Color |
|--------|-------------|-------------|
| `Good` | ≥ 75 | `--rz-success-lighter / dark` |
| `Fair` | 45 – 74 | `--rz-warning-lighter / dark` |
| `Poor` | < 45 | `--rz-danger-lighter / dark` |

### Replacement Forecasting
When `HealthScore < 75`, the prediction calculates:
- `PredictedFailureDate` = today + remaining-months based on score decay
- `RecommendedReplacementDate` = PredictedFailureDate − 90 days (procurement lead time)
- `EstimatedReplacementCost` = sourced from asset records or a category default

### Blazor Page
**Route:** `/asset-predictions`  
**File:** `Components/Pages/Predictions/PredictionsPage.razor`  
**Roles:** Admin, IT  

Features:
- KPI cards: Good (success), Fair (warning), Poor/High Risk (danger), Avg Score (primary)
- Health status filter + search bar
- "High risk only" checkbox toggle
- **Recalculate** button (force-triggers `RecalculateAllPredictionsAsync`)
- Per-row recalculate button
- Score bar visual (width proportional to score)
- CSV + Excel export
- `RadzenDataGrid` showing: Asset, Serial #, Health Score (bar), Status badge, Est. Failure, Replace By, Cost (€), Updated

---

## 17. Background Services

### EmailBackgroundService
**File:** `Services/EmailBackgroundService.cs`  
**Type:** `BackgroundService` (registered as IHostedService)

Processes queued outbound emails on a background thread to avoid blocking Blazor circuit threads.

### AssetHealthBackgroundService *(New)*
**File:** `Services/AssetHealthBackgroundService.cs`  
**Type:** `BackgroundService`

| Property | Value |
|----------|-------|
| Initial warm-up delay | 30 seconds |
| Repeat interval | 24 hours |

**Daily cycle steps:**
1. `IPredictionService.RecalculateAllPredictionsAsync()` — updates health scores for all non-Retired assets
2. `IWarrantyAlertService.SendWarrantyExpiryAlertsAsync()` — emails admin for assets with warranty expiring within 30 days
3. If any `Poor` predictions exist → sends replacement-forecast email to admin via `IEmailTemplateService`

Uses `IServiceScopeFactory` to resolve scoped services safely from the singleton-lifetime hosted service.

### SmtpHealthHostedService
**File:** `Services/Implementation/SmtpHealthHostedService.cs`  
Periodically checks SMTP connectivity. Logs warnings if SMTP becomes unreachable without crashing the application.

---

## 18. Blazor Pages & UI

### Layout
- **MainLayout.razor** — root shell, theme switcher, top bar
- **SideLayout.razor** — sidebar navigation, user display, notification badges

**SideLayout JS Rule:** `ProtectedLocalStorage` reads occur exclusively in `OnAfterRenderAsync(firstRender: true)`, not `OnInitializedAsync`.

### Navigation Links (SideLayout)

| Section | Route | Roles |
|---------|-------|-------|
| Dashboard | `/` | All |
| IT Materials | `/materiels` | Admin, IT |
| PDR Materials | `/materielsPDR` | Admin, PDR |
| Assignments | `/assignments` | Admin, IT |
| Delivery Orders | `/deliveryorders` | Admin, Purchasing, IT |
| Pending Deliveries | `/pendingdeliveries` | Admin, IT, Purchasing |
| Purchase | `/purchase` | Admin, Purchasing |
| Infrastructure | `/infra` | Admin, Infrastructure |
| Archived Requests | `/archivedrequests` | Admin, IT, Infrastructure |
| Suppliers | `/suppliers` | Admin, Purchasing |
| Asset Lifecycle | `/lifecycle` | Admin, IT |
| Maintenance | `/maintenance` | Admin, IT |
| Health Predictions | `/asset-predictions` | Admin, IT |

### UI Style System

All pages use **Radzen CSS custom properties** — no hardcoded color values:

| CSS Variable | Usage |
|-------------|-------|
| `--rz-base-background-color` | Card, modal backgrounds |
| `--rz-base-200` | Table header, hover row |
| `--rz-base-300` | Row borders, score bar background |
| `--rz-text-title-color` | Page titles, column headers |
| `--rz-text-secondary-color` | Table cell text |
| `--rz-success-lighter / dark` | Good / Closed /  Active badges |
| `--rz-warning-lighter / dark` | Fair / InProgress badges |
| `--rz-danger-lighter / dark` | Poor / Open / High-risk badges |
| `--rz-info-lighter / dark` | Informational badges |

### Standard Page Structure
```razor
<RadzenCard class="mb-4 rz-shadow-2" Style="border-radius:12px;border:none">
  <div class="p-2">
    <RadzenText TextStyle="TextStyle.H4" ... />   <!-- Page title -->
    
    <RadzenRow class="mb-4">                        <!-- KPI cards -->
      <RadzenColumn ...>
        <div class="dashboard-card card-primary|card-warning|card-stock|card-alert">
          <div class="card-header">
            <div><div class="card-title">...</div><div class="card-value">@count</div></div>
            <div class="kpi-icon bg-primary|bg-success|bg-warning|bg-danger">
              <RadzenIcon Icon="..." />
            </div>
          </div>
        </div>
      </RadzenColumn>
    </RadzenRow>

    <div class="search-export-container">           <!-- Search + export bar -->
      <div class="search-container"><RadzenTextBox .../></div>
      <div class="export-container"><RadzenButton .../></div>
    </div>

    <div class="datagrid-container">                <!-- Data grid -->
      <RadzenDataGrid AllowPaging AllowSorting AllowFiltering
                      PageSizeOptions="@(new int[]{10,15,25,50})" ...>
    </div>
  </div>
</RadzenCard>
```

---

## 19. DTOs & AutoMapper

### DTOs (`DTOs/`)

| File | Purpose |
|------|---------|
| `AssignmentDto.cs` | Assignment transfer object for API responses |
| `DeliveryOrderDto.cs` | Delivery order transfer object |
| `MaterielDto.cs` | Materiel transfer object (flattened for grids) |
| `RequestDto.cs` | Request transfer object |

### MappingProfile
**File:** `Services/MappingProfile.cs`  
Registered via `AddAutoMapper(typeof(MappingProfile).Assembly)`.

Defines entity ↔ DTO mappings using AutoMapper's fluent API.

---

## 20. Configuration & Environment Variables

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "ITStockManagmentConnection": "..."
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "FromEmail": "",
    "Password": "",
    "AdminEmail": "",
    "MaxRetryAttempts": 3,
    "RetryDelayMs": 1500
  },
  "Seed": {
    "Enabled": true,
    "DemoData": true,
    "ClearOldData": false,
    "AdminEmail": "admin@asteelflash.com",
    "AdminPassword": "admin123",
    "AdminFullName": "Admin User",
    "AdminPhoneNumber": "+0000000000",
    "AdminService": "IT"
  },
  "Logging": { ... }
}
```

### Environment Variables (Docker / Production)

| Variable | Description |
|----------|-------------|
| `SA_PASSWORD` | SQL Server SA password |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |
| `ConnectionStrings__ITStockManagmentConnection` | Full connection string override |
| `SMTP_SERVER` | SMTP hostname |
| `SMTP_PORT` | SMTP port (integer) |
| `SMTP_FROM_EMAIL` | Sender email address |
| `SMTP_PASSWORD` | SMTP password |
| `SMTP_ADMIN_EMAIL` | Admin alert recipient |
| `SEED_ENABLED` | `true`/`false` |
| `SEED_DEMO_DATA` | `true`/`false` |
| `SEED_CLEAR_OLD_DATA` | `true`/`false` (destructive!) |
| `SEED_ADMIN_EMAIL` | Admin login email |
| `SEED_ADMIN_PASSWORD` | Admin login password |
| `DEV_ALLOW_TEST_EMAIL` | Allow email test endpoints |

---

## 21. Docker & Deployment

### docker-compose.yml Services

| Service | Image | Ports | Purpose |
|---------|-------|-------|---------|
| `db` | `mcr.microsoft.com/mssql/server:2022-latest` | `1433:1433` | SQL Server 2022 Express |
| `smtpdev` | `rnwood/smtp4dev:latest` | `5025:25`, `5080:80` | Dev SMTP + web UI |
| `app` | Built from `Dockerfile` | `8080:8080` | Blazor Server app |

### Startup Dependencies
`app` waits for:
- `db` → `service_healthy` (sqlcmd check every 10s, 20 retries)
- `smtpdev` → `service_started`

### DB Lifecycle Note
The database container uses **no named volume** by default — data is ephemeral per `docker compose down`. To persist data, add a named volume:
```yaml
volumes:
  sqldata:
services:
  db:
    volumes:
      - sqldata:/var/opt/mssql
```

### Quick Start Commands

```bash
# Start all services
docker compose up -d --build

# View app logs
docker compose logs -f app

# Force clean restart (re-applies migrations + re-seeds)
docker compose down && docker compose up -d --build

# Stop without removing volumes
docker compose stop

# Access SMTP dev web UI
open http://localhost:5080
```

### Dockerfile
Multi-stage build:
1. **Build stage** (`mcr.microsoft.com/dotnet/sdk:10.0`) — `dotnet publish`
2. **Runtime stage** (`mcr.microsoft.com/dotnet/aspnet:10.0`) — copies publish output

---

## 22. Testing

**Test project:** `ITStockM.Tests/ITStockM.Tests.csproj`  
**Framework:** xUnit + FluentAssertions + Moq

### Test Coverage

| Test File | Area | Count |
|-----------|------|-------|
| `Services/EmailServiceTests.cs` | EmailService: retry logic, single/multi send, admin notify | ~8 |
| `Services/PredictionServiceTests.cs` | `ComputeHealthScore` pure function, band classification | ~10 |
| `Services/MaintenanceServiceTests.cs` | Ticket CRUD, open count, per-materiel dictionary | ~7 |
| `Repositories/` | Repository integration tests | TBD |
| `Ui/` | Blazor component tests | TBD |

### Key Test Patterns

**PredictionService — pure function test:**
```csharp
var score = _service.ComputeHealthScore(
    ageMonths: 12, expectedLifetimeMonths: 48,
    totalAssignments: 2, closedTickets: 1, openTickets: 0);
score.Should().BeGreaterThan(75);
```

**Maintenance concurrency safety:**
All service results are materialized to `List<T>` before being bound to Radzen components to prevent EF "second operation on context" errors.

---

## 23. Enums & Constants Reference

### LifecycleStage
```csharp
LifecycleStage.Purchased        = "Purchased"
LifecycleStage.InStock          = "InStock"
LifecycleStage.Assigned         = "Assigned"
LifecycleStage.UnderMaintenance = "UnderMaintenance"
LifecycleStage.Retired          = "Retired"
LifecycleStage.All              → IReadOnlyList<string>
```

### HealthStatus
```csharp
HealthStatus.Good  = "Good"     // score >= 75
HealthStatus.Fair  = "Fair"     // score >= 45
HealthStatus.Poor  = "Poor"     // score <  45
HealthStatus.FromScore(decimal) → string
```

### MaintenanceTicketStatus
```csharp
MaintenanceTicketStatus.Open       = "Open"
MaintenanceTicketStatus.InProgress = "InProgress"
MaintenanceTicketStatus.Closed     = "Closed"
MaintenanceTicketStatus.All        → IReadOnlyList<string>
```

### UserRoles
```csharp
UserRoles.Admin          = "Admin"
UserRoles.PDR            = "PDR"
UserRoles.Purchasing     = "Purchasing"
UserRoles.IT             = "IT"
UserRoles.Infrastructure = "Infrastructure"
UserRoles.Employee       = "Employee"

UserRoles.All                → string[]  (all 6)
UserRoles.Administrative     → string[]  (excludes Employee)
UserRoles.MaterialManagers   → string[]  (Admin, PDR, IT)
UserRoles.RequestApprovers   → string[]  (Admin, Infrastructure, Purchasing)
UserRoles.NormalizeRole(str) → string    (resolves legacy Post values)
```

---

*Document generated: March 4, 2026 — IT Inventory Management System, AsteelFlash*
