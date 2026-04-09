# Architecture Remediation Gap Report

Date: 2026-04-09

This file records what is still missing versus the Professional Architecture Remediation Execution Playbook.

## Current Gap Summary

- Step 0 (Setup and Safety): branch/process governance remains process-dependent.
- Step 6: page code-behind still injects monolith ITStockManagmentService in 2 files (reduced from 57).
- Step 10: canonical naming not fully applied (DeleveryOrder/Descriptoin/Managment family remains).
- Step 15: architecture regression guard still fails due outstanding naming and dependency-injection migration work.

## Remediation Started In This Pass

- Converted first-wave async void handlers to async Task in:
  - Components/Pages/DeliveryOrder/AddDelayedMaterials.razor.cs
  - Components/Pages/MaterialsView/ConfirmConditionChange.razor.cs
  - Components/Pages/MaterialsViewPDR/AddMaterialsAssignmentsPDR.razor.cs
- Removed Task.Run fire-and-forget and dynamic context access in feature services:
  - Services/Assignments/AssignmentService.cs
  - Services/DeliveryOrders/DeliveryOrderService.cs
  - Services/Materiels/MaterielService.cs
  - Services/Requests/RequestService.cs
- Removed remaining `Task.Run` fire-and-forget from `Services/ITStockManagmentService.cs` by replacing with awaited, exception-safe notification dispatch.
- Migrated page-level dependency from monolith service to `IMaterielService` in:
  - Components/Pages/MaterialsView/ConfirmConditionChange.razor.cs
  - Components/Pages/CrudPages/AddMateriel.razor.cs
  - Components/Pages/CrudPages/EditMateriel.razor.cs
- Migrated CRUD list pages to bounded services + export abstraction:
  - Components/Pages/CrudPages/Materiels.razor.cs (`IMaterielService` + `IExportService`)
  - Components/Pages/CrudPages/Assignments.razor.cs (`IAssignmentService` + `IExportService`)
  - Components/Pages/CrudPages/Requests.razor.cs (`IRequestService` + `IExportService`)
- Added bounded services for missing domains and migrated broad CRUD flows:
  - New domain services: Employees, Projects, Suppliers, Offers.
  - New focused services: AssignmentMateriels, DeliveryOrderMateriels.
  - Migrated purchase, pending-deliveries, supplier, and multiple CRUD add/edit/list pages to bounded services and `IExportService`.
- Reduced direct monolith page injections from 57 to 2.
- Added missing remediation artifacts and scripts:
  - scripts/build-app.sh
  - scripts/build-tests.sh
  - scripts/build-solution.sh
  - scripts/check-warning-budget.sh
  - scripts/check-architecture-regressions.sh
  - .ci/warning-budget.txt
  - docs/architecture-remediation/smoke-tests.md
  - docs/architecture-remediation/rollback-policy.md

## Next Priorities

1. Remove remaining monolith page injections by moving to bounded-context interfaces.
2. Execute canonical naming migration safely with compatibility mapping.
3. Verify architecture guard reaches full pass state.
4. Run full build/test and keep warning baseline aligned with measured output.
