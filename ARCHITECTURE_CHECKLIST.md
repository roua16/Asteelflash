# ITStockM Strict Clean-Architecture Checklist

This is the **master completion checklist** for taking ITStockM to a strict, production-grade clean architecture with SOLID compliance.

---

## A. Non-Negotiable Architecture Rules

1. **Domain**: business rules only (entities, value objects, enums, domain events/exceptions).  
   No UI/EF/ASP.NET dependencies.
2. **Application**: use-cases, DTO contracts, handlers, validators, mapping, interfaces.
3. **Infrastructure**: persistence/auth/email/external integrations and concrete implementations.
4. **WebApi/Presentation**: controllers, Blazor UI, middleware, composition root.
5. **Dependency direction only inward**: `WebApi -> Application -> Domain`, `Infrastructure -> Application + Domain`.

---

## B. Status Snapshot (Current)

## Completed
- [x] `Radzen.Query` decoupled from Application/Infrastructure (`QueryOptions` introduced).
- [x] WebApi direct Domain project reference removed.
- [x] Maintenance API migrated to DTO request/response contracts.
- [x] AssetLifecycle + Predictions APIs migrated to DTO contracts (`AssetLifecycleRecordDto`, `MaterielDto`, `AssetPredictionDto`).
- [x] Global exception middleware maps domain + validation exceptions.
- [x] CORS moved from wildcard to configured allowlist.
- [x] Global authenticated controller policy enforced (with explicit `[AllowAnonymous]` exceptions).
- [x] Swagger XML comment loading fixed with assembly-aware include.
- [x] AssetLifecycle + Predictions controller Swagger responses now use explicit DTO response schemas.
- [x] Legacy junk removed (`download.js`, `txtData`, unused loading component).

## In progress / remaining
- [ ] Namespace standardization (`ITStockM.Services.*` -> explicit Application contract namespaces).
- [ ] Split oversized orchestration services (notably `OperationNotificationService`).
- [ ] Full Swagger response metadata parity across all remaining controllers (future work).

---

## C. File-Oriented Strict Checklist

## C1. Domain Layer (src/ITStockM.Domain)
- [ ] Keep Domain free of framework concerns (incrementally reduce persistence annotations where possible).
- [ ] Confirm domain event flow is coordinated from Application/Infrastructure boundaries only.
- [ ] Maintain one-way dependency: Domain never references Application/Infrastructure/WebApi.

## C2. Application Layer (src/ITStockM.Application)
- [x] `Common/Models/QueryOptions.cs` established as UI-agnostic query contract.
- [x] `Features/Maintenance/DTOs/MaintenanceTicketDtos.cs` added and wired.
- [x] Migrated feature contracts away from domain return types:
  - `Features/AssetLifecycle/Commands/*`
  - `Features/AssetLifecycle/Queries/*`
  - `Features/Predictions/Commands/*`
  - `Features/Predictions/Queries/*`
- [x] Added validators for DTO commands/queries (AssetLifecycle, Predictions feature validators).
- [ ] Consolidate contract namespaces to make Application intent explicit and consistent.

## C3. Infrastructure Layer (src/ITStockM.Infrastructure)
- [ ] Split `Services/OperationNotificationService.cs` into feature-specific notification handlers (SRP).
- [ ] Break up monolithic `DependencyInjection/InfrastructureLayerServiceCollectionExtensions.cs` into feature extension modules:
  - `AddMaintenanceServices()`
  - `AddAssetLifecycleServices()`
  - `AddPredictionServices()`
  - `AddPurchasingServices()`
  - `AddAuthServices()`
- [ ] Keep infrastructure services focused on data/integration concerns; push orchestration into application handlers.

## C4. WebApi + Blazor Presentation (src/ITStockM.WebApi)
- [x] `Controllers/MaintenanceController.cs` now DTO-based (no direct domain entity binding).
- [x] `Middleware/GlobalExceptionMiddleware.cs` now maps domain + FluentValidation exceptions.
- [x] `Program.cs` now enforces allowlisted CORS and global auth policy.
- [x] All sidebar routes now have corresponding Razor page components (24 pages created as placeholders).
- [x] All pages implement proper `@attribute [Authorize(Roles = "...")]` guards.
- [x] Controllers remain thin (request mapping + MediatR dispatch + HTTP response shaping only).

---

## D. Swagger / API Contract Completion Matrix

## Done
- [x] Swagger endpoint enabled (`/swagger`, `/swagger/v1/swagger.json`).
- [x] XML docs generation enabled in WebApi csproj.
- [x] Maintenance endpoint response metadata improved (401/422 included).
- [x] AssetLifecycle + Predictions endpoints now publish typed DTO response schemas.

## Must complete
- [x] Applied `ProducesResponseType` coverage to Maintenance, AssetLifecycle, and Predictions endpoints.
- [x] Ensure all API inputs/outputs use DTO contracts (no domain entity schemas exposed).
- [ ] Future: Add integration smoke tests for:
  - `/swagger`
  - `/swagger/v1/swagger.json`
  - key controller routes

---

## E. Junk / Legacy / Noise Control

## Must never be committed
- `**/bin/**`
- `**/obj/**`
- runtime outputs (`*.dll`, `*.pdb`, generated bundles)

## Cleanup command
```bash
find src -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
```

## Removed in this refactor stream
- `src/ITStockM.WebApi/wwwroot/js/download.js`
- `src/ITStockM.WebApi/wwwroot/txtData/*`
- `src/ITStockM.WebApi/Components/Pages/LoadingScreen.razor*`
- Radzen UI framework dependency from Application layer

## Added in this refactor stream (Pages)
- 24 Blazor page components for sidebar navigation routes
- All pages have proper role-based authorization attributes
- All pages follow consistent placeholder pattern (ready for full CRUD implementation)

---

## F. Merge / Split Strategy

## Merge / standardize
- [ ] Standard DTO naming everywhere:
  - `CreateXxxDto`
  - `UpdateXxxDto`
  - `XxxDto` (response)
- [ ] Consolidate repetitive mapping + response shaping patterns in handlers.

## Split for SRP
- [ ] Split oversized notification/orchestration services by bounded context.
- [ ] Split Infrastructure DI registrations by feature.
- [ ] Split large Blazor code-behind classes where state + data + orchestration are mixed.

---

## G. Final Definition of Done (Strict Pro-Level)

Project is complete when all are true:
1. No WebApi endpoint accepts/returns domain entities.
2. Application public contracts are DTO-first for all use-cases.
3. Infrastructure details are hidden behind Application interfaces.
4. Blazor/UI uses stable application boundaries, not implementation details.
5. Swagger fully documents endpoint contracts and error models.
6. Build artifacts and junk folders are absent from versioned source.
7. Build succeeds and test baseline is stable.
