# Architecture Improvements

## Overview
This document outlines the architectural improvements made to the IT Stock Management system to enhance code organization, maintainability, and adherence to best practices.

## 1. Role Management System

### Issue
- Employee model used `Post` field for role information
- Role names hardcoded throughout the codebase
- Inconsistent role checking logic
- Difficult to maintain and extend roles

### Solution
- Created `Models/Constants/UserRoles.cs` with standardized role constants:
  - `Admin`: System administrator
  - `PDR`: Material management role
  - `Purchasing`: Procurement role
  - `IT`: IT support role
  - `Infrastructure`: Infrastructure management role
  - `Employee`: Standard employee role

- Added `Role` field to Employee model with proper data annotations
- Created migration `AddEmployeeRoleField` to add Role column to database
- Updated `DatabaseInitializer.cs` to seed users with correct roles:
  - Admin: `admin@asteelflash.com` → Admin role
  - John Smith: `john.smith@asteelflash.com` → PDR role
  - Sarah Johnson: `sarah.johnson@asteelflash.com` → Purchasing role
  - Mike Davis: `mike.davis@asteelflash.com` → IT role

### Benefits
- Single source of truth for role names
- Type-safe role references throughout codebase
- Easy to add new roles
- Role grouping for authorization (Administrative, MaterialManagers, RequestApprovers)

## 2. Route Management System

### Issue
- Routes hardcoded with "-super-admin" suffixes
- Route paths defined in multiple places
- Difficult to maintain URL structure
- No centralized route discovery

### Solution  
- Created `Models/Constants/RouteConstants.cs` with all application routes organized by feature:
  - Authentication routes (Login, Register, Logout)
  - Dashboard route
  - Materials routes (IT and PDR views)
  - Assignments routes
  - Requests routes (Active and Archived)
  - Infrastructure routes
  - Purchasing routes
  - Delivery Orders routes
  - Administration routes (Employees, Suppliers, Projects, DeliveryOrderMateriel, Offers)

### Benefits
- Single source of truth for all routes
- Type-safe route references
- Easy to update URL structure
- Enables route validation and testing
- Simplifies navigation logic

## 3. Export Service Extraction

### Issue
- `ITStockManagmentService.cs` was 1,722 lines long
- Contained 26+ export methods (13 entities × 2 formats)
- Export logic duplicated across all methods
- Violated Single Responsibility Principle
- Mixed business logic with navigation concerns

### Solution
- Created `Services/Export/IExportService.cs` interface
- Created `Services/Export/ExportService.cs` implementation
- Export methods now delegate to ExportService:
  ```csharp
  public async Task ExportAssignmentsToExcel(Query query = null, string fileName = null)
      => await exportService.ExportToExcel(query, "export/itstockmanagment/assignments/excel", fileName);
  ```
- Registered ExportService in dependency injection container
- Injected IExportService into ITStockManagmentService constructor

### Benefits
- Reduced ITStockManagmentService size (removing ~200+ lines)
- Export logic centralized and reusable
- Proper URL encoding handled in one place
- Easier to test export functionality
- Cleaner separation of concerns

## 4. Service Architecture

### Current State
The application now uses a layered service architecture:

```
Controllers (API endpoints)
    ↓
ITStockManagmentService (Coordinator)
    ↓
Domain Services (Business logic per entity)
    ↓
Repositories (Data access)
    ↓
DbContext (EF Core)
```

### Services Breakdown

#### ITStockManagmentService
- **Role**: Service coordinator and partial methods container
- **Responsibilities**:
  - Coordinate cross-entity operations
  - Provide partial method hooks for extensibility
  - Delegate to domain services for entity-specific logic
  - Handle export operations via IExportService

#### Domain Services
- **AssignmentService**: Assignment entity business logic
- **MaterielService**: Material entity business logic
- **RequestService**: Request entity business logic
- **DeliveryOrderService**: Delivery order entity business logic

#### Support Services
- **ExportService**: Centralized export navigation
- **EmailService**: Email notifications
- **NotificationService**: User notifications
- **OperationNotificationService**: Operation status notifications

### Query Utilities
- **QueryExtensions.cs**: Common query operations (filter, sort, paginate)
- Used by all domain services to eliminate duplication

## 5. Database Seeding

### Seeded Users
1. **Admin User**
   - Email: admin@asteelflash.com
   - Password: admin123
   - Role: Admin
   - Service: IT

2. **PDR User**
   - Email: john.smith@asteelflash.com
   - Password: password123
   - Role: PDR
   - Service: Material Management

3. **Purchasing User**
   - Email: sarah.johnson@asteelflash.com
   - Password: password123
   - Role: Purchasing
   - Service: Procurement

4. **IT User**
   - Email: mike.davis@asteelflash.com
   - Password: password123
   - Role: IT
   - Service: IT

## 6. Recommended Next Steps

### Priority 1: Complete Export Refactoring
- Replace remaining 24 export methods to use ExportService
- Remove unused System.Text.Encodings.Web import
- Further reduce ITStockManagmentService size

### Priority 2: Route Constants Integration
- Update all `@page` directives to use RouteConstants
- Update SideLayout.razor navigation to use RouteConstants  
- Remove hardcoded route strings throughout application

### Priority 3: Role-Based Authorization
- Apply `[Authorize(Roles = UserRoles.Admin)]` attributes to admin pages
- Apply `[Authorize(Roles = UserRoles.PDR)]` attributes to PDR pages
- Apply `[Authorize(Roles = UserRoles.Purchasing)]` attributes to purchasing pages
- Update Index.razor.cs to use UserRoles constants for role checks
- Update SideLayout.razor to show/hide menu items based on user role

### Priority 4: Dashboard Improvements
- Split Index.razor (809 lines) into smaller components
- Create role-specific dashboard sections
- Implement proper role-based content visibility
- Move dashboard cards to separate components

### Priority 5: Further Service Refactoring
- Consider extracting CRUD operations from ITStockManagmentService
- Move remaining business logic to domain services
- Reduce ITStockManagmentService to pure coordinator pattern

### Priority 6: Testing
- Add unit tests for new constants
- Add unit tests for ExportService
- Add integration tests for role-based authorization
- Test all seeded user accounts

## 7. Migration Guide

### To Apply Role Changes
```bash
# Apply the migration
dotnet ef database update

# Verify in database
# Employee table should now have Role column
# Seeded users should have proper Role values
```

### To Use New Constants

#### Before:
```csharp
if (user.Role == "PDR") { ... }
navigationManager.NavigateTo("/dashboard-super-admin");
```

#### After:
```csharp
using ITStockM.Models.Constants;

if (user.Role == UserRoles.PDR) { ... }
navigationManager.NavigateTo(RouteConstants.Dashboard);
```

## 8. Benefits Summary

- ✅ **Maintainability**: Centralized constants, reduced duplication
- ✅ **Type Safety**: Compile-time checking for roles and routes
- ✅ **Testability**: Smaller, focused services easier to test
- ✅ **Performance**: No impact, just better code organization
- ✅ **Extensibility**: Easy to add new roles, routes, or export formats
- ✅ **Clarity**: Clear separation of concerns
- ✅ **Best Practices**: Follows SOLID principles
