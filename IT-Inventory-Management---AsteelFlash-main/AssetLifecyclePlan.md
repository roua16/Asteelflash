# Asset Lifecycle & Predictive Maintenance Feature Plan

This document outlines a step-by-step plan for implementing the proposed asset lifecycle and predictive maintenance module in the IT Inventory Management system. The goal is to evolve from basic inventory tracking to strategic asset management with analytics, alerts and recommendations.

---

## 1. Database Changes

1.1. **Extend `Materiels` table**
- Add columns: `PurchaseDate`, `WarrantyEndDate`, `ExpectedLifetime` (in months/years), `CurrentHealthScore` (decimal), `LifecycleStatus` (enum/string).

1.2. **New tables**
- `AssetLifecycle` (if more detailed per-asset records are needed)
    ```sql
    Id (PK)
    MaterielId (FK)
    Stage (Purchased, InStock, Assigned, UnderMaintenance, Retired)
    StartDate
    EndDate (nullable)
    Notes
    ```

- `MaintenanceTicket`
    ```sql
    Id (PK)
    MaterielId (FK)
    ProblemDescription
    ReportedByEmployeeId
    AssignedTechnicianId (FK to new Technician table)
    Status (Open, InProgress, Closed)
    Cost
    ReportedAt
    ResolvedAt (nullable)
    ```

- `MaintenanceHistory` (optional, for completed tickets)
    duplicate of ticket fields + additional metadata.

- `Technician`
    ```sql
    Id (PK)
    FullName
    Email
    Phone
    ```

Add EF Core migrations for these changes.

---

## 2. Domain & Services

2.1. **Interfaces**
- `IAssetLifecycleService` (CRUD, status updates, queries by stage)
- `IMaintenanceService` (create ticket, update status, get history)
- `IPredictionService` (health scoring and replacement recommendation)

2.2. **Implementations**
- Add classes in `Services/` folder; use `ITStockManagmentContext` or new dedicated context for performance.
- Hook up repository pattern if needed.

2.3. **DTOs/ViewModels**
- Add mapping profiles for new entities.
- Maintain health score, prediction results, warranty alerts.

---

## 3. Background Job & Health Scoring

3.1. **AssetHealthBackgroundService**
- Runs daily (configure hosted service) using `IHostedService`/`BackgroundService`.
- For each material compute:
    - Age = (today - PurchaseDate) / ExpectedLifetime
    - Usage frequency (could derive from `AssignmentMateriels` count/duration)
    - Past failures = count of `MaintenanceTicket` where Status=Closed
    - Maintenance frequency = number of tickets per time window
    - Score = weighted formula (25% age + 25% usage + 30% failures + 20% maint.)
- Update `CurrentHealthScore` & set `LifecycleStatus` thresholds (e.g. >80 Good, 50‑80 Fair, <50 Poor).
- Generate alerts for warranties approaching expiration or low health using existing email queue (`EmailBackgroundService` or new job).
- Compute predictions via `IPredictionService` and persist or cache results.

3.2. **Prediction logic**
- Use simple heuristics initially (e.g. linear regression on past failure intervals) or integrate ML later.
- Store recommendation entries with `MaterielId`, `PredictedFailureDate`, `RecommendedReplacementDate`.

---

## 4. API Endpoints / Controllers

4.1. Extend existing service controllers or add new ones:
- `/api/assets/lifecycle` – GET/POST/PUT for lifecycle records.
- `/api/assets/maintenance` – ticket management.
- `/api/assets/predictions` – health scores & replacement recommendations.

4.2. Add export endpoints to `ExportITStockManagmentController` for lifecycle/maintenance/prediction data.

4.3. Add Swagger documentation via comments.

---

## 5. UI (Blazor Pages)

5.1. **Assets Lifecycle Pages**
- `/assets/lifecycle` grid showing stage, purchase date, health score, warranty, etc.
- Detail/edit page to update status and dates.

5.2. **Maintenance Module**
- `/assets/maintenance` grid for open/closed tickets.
- Forms to create new ticket, assign technician, close ticket.
- History view per asset.

5.3. **Predictions Dashboard**
- Tab showing assets with low health or predicted failures.
- Charts: replacements by month, failure rates by model/brand.

5.4. **Warranty Alerts**
- Notification panel or email alerts when expiration is near.

All pages use Radzen DataGrid/Chart components; leverage existing service methods.

---

## 6. Exports & Reporting

6.1. Add navigation and service methods for:
- `/export/maintenance/csv|excel`
- `/export/lifecycle/csv|excel`
- `/export/predictions/csv|excel`

6.2. Include filters (by status, dates).

---

## 7. Testing

7.1. **Unit tests** for new services:
- Lifecycle transitions, health score calculations, ticket CRUD.

7.2. **Integration tests** similar to existing ones: in `ITStockM.Tests/Services`.
- Simulate background job run and verify score updates & email alerts.

7.3. **UI tests** (shell script `run-ui-tests.sh` can be extended) to cover pages.

---

## 8. Deployment & Configuration

- Add new connection strings or settings for prediction parameters.
- Update `appsettings.json` and `appsettings.Development.json` with sample data.

---

## 9. Future Enhancements

- ML model using past data (could export to Python/R or use ML.NET).
- Mobile-friendly pages or push notifications.
- Supplier contract integration.

---

✅ **Outcome**: With these steps, the system evolves from inventory CRUD to a strategic asset-management platform providing predictive insights, warranty tracking, and maintenance workflows. This makes it enterprise‑grade, opens the door to budgeting forecasts, and adds significant real‑world value for users.