# IT Inventory Management System - Feature & Tech Stack Summary

## Overview
This application is an IT inventory management system built using the Microsoft ecosystem. It provides a Blazor Server UI, a REST API, and a SQL Server backend. Core functionality includes tracking of hardware/software assets, assignments, delivery orders, purchase requests and offers, suppliers and employees, with detailed reports and export capabilities.

## Main Features

- **Authentication and Authorization**
- **Role-based access** (Admin, Employee, PDR, etc.)
- **CRUD operations for:**
  - Employees
  - Suppliers
  - Materials (Materiels) with IT and PDR stock quantities
  - Projects
  - Requests (with offer management and approval workflow)
  - Offers from suppliers (selection, delivery dates, pricing)
  - Assignments of materials to employees/on‑project
  - Delivery Orders and associated materials
  - AssignmentMateriels and DeliveryOrderMateriels join tables

- **Dashboard and analytics:**
  - Inventory breakdown by type/condition
  - Monthly assignment trends and growth calculations
  - Top requests, pending requests/deliveries
  - Recent activity feed
  - Top‑used materials prediction

- **Export/Reporting:**
  - CSV/Excel export endpoints for all entities and filtered views (e.g. PDR stock, archived requests)
  - Swashbuckle/Swagger API documentation

- **Development utilities:**
  - Dev controllers for email tests and SMTP health checks
  - AD authentication stub support

- **Testing:**
  - Unit and integration tests for services, email, notifications, and UI logic

- **Miscellaneous:**
  - Background email sender service
  - Protected browser storage for session data
  - Custom authentication state provider for Blazor

## Technology Stack

- .NET 10 (ASP.NET Core) with C# and Blazor Server
- Entity Framework Core 8 (SQL Server provider)
- Dapper for lightweight querying (authentication)
- Radzen.Blazor component library for UI elements
- AutoMapper, System.Linq.Dynamic.Core, MailKit, DocumentFormat.OpenXml
- Swashbuckle.AspNetCore for API docs
- SQL Server database, migrations via EF Core
- Dependency injection, repository & service layers
- ProtectedLocalStorage and custom auth provider for Blazor
- Unit testing with xUnit, FluentAssertions

## Architectural Notes
The project follows a layered architecture: controllers expose REST endpoints, business logic resides in service classes (ITStockManagmentService and domain‑specific services), and data access is managed by EF Core DbContext and optionally Dapper repositories. Blazor pages consume the service layer via dependency injection. Many services use queryable patterns with Radzen’s Query object to allow dynamic filtering, sorting, and paging. A semaphore protects EF Core context access in the Blazor Server environment.

## Additional Components

- Background tasks (email queue)
- Data transfer objects (DTOs) and ViewModels used across layers
- Migrations folder with EF Core migration scripts
- Scripts for setup (e.g., macOS .NET install) and UI test execution

## Usage

The application can be run via `dotnet run` after configuring a SQL Server connection string in `appsettings.json`. It features Blazor pages for interactive management and a JSON API accessible via `/api/...` and export endpoints at `/export/...`. Authentication relies on email/password stored in the Employee table, with optional Active Directory fallback.
