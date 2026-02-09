# Project Progress Report

## Overview
- **Repository**: IT-Inventory-Management (AsteelFlash)
- **Stack**: .NET 10 (Blazor Server), EF Core 8, Radzen UI, MailKit/MimeKit, Playwright (.NET) for UI tests
- **Goal**: Stabilize app, add automated UI tests, implement email notifications, fix role-based access for all roles, unify dashboards

---

## Summary of Work Done

### Build & Test Stabilization
- Fixed compile and test failures (missing imports, EF test package mismatch, Razor syntax errors)
- Implemented Playwright UI test scaffolding, gated behind `RUN_UI_TESTS` (disabled by default)
- Unit tests pass (25 tests: 19 succeeded, 6 skipped — UI tests)

### Notifications & Email
- Implemented `OperationNotificationService` and `EmailService` for operation notifications
- Added low-stock email notifications (configurable via `LOW_STOCK_THRESHOLD`, default 10)
- Added assignment return issue detection: missing items, damaged, irreparable — sends alerts to Admin, PDR, and IT
- Added handling for returned materials not in an assignment ("Other material returned") — creates/updates inventory with defaults and sends notifications

### Role-Based Access & Dashboard Fixes
- Normalized all roles to canonical constants: Admin, PDR, Purchasing, IT, Infrastructure, Employee
- Fixed Employee dashboard: moved from `<NotAuthorized>` hack into proper `<Authorized>` block with role check
- Employee dashboard shows: KPI cards (active assignments, pending requests), recent activity (filtered to their data), recent requests grid, and Quick Actions (My Assignments, Return Materials)
- Added Employee to sidebar navigation (Dashboard + Materials Assignments links)
- Added Employee role to AssignmentsInterface page access (was Admin-only)
- Added IT role to InfraInterface page access (both `@attribute [Authorize]` and `<AuthorizeView>`)
- All dashboards use the same style/layout (unified `dashboard-container`, KPI cards, RadzenCard styling)

### Seed Data & Demo Accounts
- Expanded seed data with multiple accounts, materials, suppliers, projects, and requests
- Seeded partial-return example for notification testing

---

## Seeded Accounts & Credentials

These demo accounts are created by `DatabaseInitializer` on first run (when database is empty). Seeding is enabled via `appsettings.json` (`Seed:Enabled=true`, `Seed:DemoData=true`).

| Role | Full Name | Email | Password | Notes |
|---|---|---|---|---|
| **Admin** | Admin User | `admin@asteelflash.com` | `admin123` | Receives all operation notifications by default |
| **PDR** | John Smith | `john.smith@asteelflash.com` | `password123` | PDR Manager |
| **PDR** | Emma White | `emma.white@asteelflash.com` | `password123` | PDR Technician |
| **Purchasing** | Sarah Johnson | `sarah.johnson@asteelflash.com` | `password123` | Purchasing Manager |
| **Purchasing** | Paul Green | `paul.green@asteelflash.com` | `password123` | Purchasing Officer |
| **IT** | Mike Davis | `mike.davis@asteelflash.com` | `password123` | IT Support Specialist |
| **Infrastructure** | Alice Brown | `alice.brown@asteelflash.com` | `password123` | Infrastructure Manager |
| **Employee** | Thomas Miller | `thomas.miller@asteelflash.com` | `password123` | Generic employee account |

> **Security note**: These are demo/test accounts. Do NOT reuse real credentials in production.

---

## Roles & Access Definition

### Admin
- **Full access** to everything: all dashboards, materials, assignments, employees, suppliers, projects, offers, requests, delivery orders
- **Administration section**: Materials, Assignments, Assignment Materials, Delivery Orders, Delivery Orders Mats, Requests, Employees, Offers, Projects, Suppliers (CRUD admin pages)
- **Email notifications**: receives all operation notifications (override: `SMTP_ADMIN_EMAIL`)

### PDR
- **Dashboard**: Full IT Inventory Dashboard (KPIs, quick actions, recent activity, charts)
- **Inventory**: Materials View (PDR)
- **Purchasing**: Suppliers, Add Delivery Orders, Pending Deliveries, D.O History
- **Email notifications**: receives assignment return issue alerts

### Purchasing
- **Dashboard**: Full IT Inventory Dashboard
- **Inventory**: Materials View
- **Purchasing**: Purchase Requests, Suppliers, Add Delivery Orders, Pending Deliveries, D.O History
- **Archives**: Archived Requests

### IT
- **Dashboard**: Full IT Inventory Dashboard
- **Inventory**: Materials View, Materials Assignments, Infrastructure
- **Email notifications**: receives assignment return issue alerts

### Infrastructure
- **Dashboard**: Full IT Inventory Dashboard
- **Inventory**: Materials Assignments, Infrastructure

### Employee
- **Dashboard**: Employee Dashboard (limited view — own assignments, own requests, own recent activity, quick actions)
- **Inventory**: Materials Assignments (view own assignments)
- **Tools**: Return Materials via Mission Interface
- **No access to**: Administration, Purchase pages, Suppliers, Delivery Orders, Archives, Infrastructure

---

## Page Access Matrix

| Page | Route | Roles Allowed |
|---|---|---|
| Dashboard | `/` | Admin, PDR, Purchasing, IT, Infrastructure, Employee |
| Materials View | `/materials-view-interface` | Admin, Purchasing, IT |
| Materials View (PDR) | `/materials-view-interface-pdr` | Admin, PDR, Purchasing |
| Materials Assignments | `/assignments-interface` | Admin, IT, Infrastructure, Employee |
| Infrastructure | `/infra-interface` | Admin, IT, Infrastructure |
| Mission Interface | `/mission-interface` | All (no restriction) |
| Purchase Requests | `/purchase-page` | Admin, Purchasing |
| Suppliers | `/suppliers` | Admin, Purchasing, PDR |
| Add Delivery Orders | `/delivery-order` | Admin, Purchasing, PDR |
| Delivery Orders History | `/delivery-order-history` | Admin, Purchasing, PDR |
| Pending Deliveries | `/pending-deliveries` | Admin, Purchasing, PDR |
| Archived Requests | `/archived-requests-page` | Admin, Purchasing |
| Archived Assignments | `/assignments-archive` | Admin |
| Archived Missions | `/archived-missions` | Admin, Purchasing |
| Administration (CRUD) | `/materiels-super-admin`, etc. | Admin |

---

## Email Notification Behavior

| Event | Recipients | Details |
|---|---|---|
| Assignment creation/update/delete | Admin | Standard operation notification |
| Assignment return issues (missing, damaged, irreparable, remaining items) | Admin + PDR users + IT users | Triggered from "Confirm Material Return" dialog |
| Low-stock alert | Admin | When material quantity crosses `LOW_STOCK_THRESHOLD` (default 10) |
| Returned material not in system | Admin + PDR + IT | "Other material returned" — new material registered with defaults |

Recipient overrides: `SMTP_ADMIN_EMAIL`, `SMTP_PDR_EMAIL`, `SMTP_IT_EMAIL`

---

## Configurable Environment Variables

| Variable | Purpose | Default |
|---|---|---|
| `SMTP_SERVER` | SMTP server host | — |
| `SMTP_PORT` | SMTP server port | — |
| `SMTP_FROM_EMAIL` | Sender email address | — |
| `SMTP_PASSWORD` | SMTP password | — |
| `SMTP_ADMIN_EMAIL` | Admin notification recipient override | `admin@asteelflash.com` |
| `SMTP_PDR_EMAIL` | PDR notification recipient override | Auto from seeded PDR users |
| `SMTP_IT_EMAIL` | IT notification recipient override | Auto from seeded IT users |
| `LOW_STOCK_THRESHOLD` | Low-stock alert threshold | `10` |
| `RUN_UI_TESTS` | Enable Playwright UI tests | `false` |
| `PLAYWRIGHT_CHANNEL` | Browser channel for UI tests | — |

---

## Testing & How to Run

- **Unit tests**: `dotnet test` (UI tests are skipped by default)
- **UI tests**: set `RUN_UI_TESTS=true` and optionally `PLAYWRIGHT_CHANNEL=chrome`, then run `scripts/run-ui-tests.sh`
- **Email testing**: project uses `smtp4dev` on local dev via docker compose

---

## Key Files Changed

| File | Description |
|---|---|
| `Services/Implementation/OperationNotificationService.cs` | Notifications (assignment issues, materiel changes, low-stock alerts) |
| `Services/ITStockManagmentService.cs` | Low-stock detection on `UpdateMateriel`, `RegisterReturnedMaterial` |
| `Components/Pages/Index.razor` | Unified dashboard for all roles, Employee dashboard with filtered data |
| `Components/Pages/Index.razor.cs` | Dashboard code-behind, `userId` field for Employee filtering |
| `Components/Layout/SideLayout.razor` | Sidebar navigation with role-based menu items |
| `Components/Pages/MaterialsAssignments/AssignmentsInterface.razor` | Added Employee role access |
| `Components/Pages/Infra/InfraInterface.razor` | Added IT role access |
| `Data/DatabaseInitializer.cs` | Seeding of demo accounts, materials, partial-return example |
| `Models/Constants/UserRoles.cs` | Role constants and normalization |
| `ITStockM.Tests/` | Low-stock unit tests, Playwright UI test scaffolding, role access tests |

---

## Remaining / Next Steps

- [ ] Run full role-based UI checks across all 6 roles
- [ ] Test all CRUD flows (Create/Edit/Delete, filters, details) per role
- [ ] Confirm email delivery for return issues in smtp4dev
- [ ] Stabilize Playwright runs across CI hosts
- [ ] Optionally add per-material reorder level fields (`ReorderLevel`) for more granular thresholds
- [ ] Optionally add de-duplication (last-notified timestamp) to avoid repeated low-stock emails
- [ ] Address remaining nullability warnings if required for stricter builds

---

_Last updated: February 9, 2026_
