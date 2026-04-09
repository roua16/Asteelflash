# Project Avancement - AsteelFlash IT Inventory

Date: 2026-02-08

## Completed
- Fixed Razor build errors and restored successful builds.
- Normalized roles so dashboards and menus match canonical roles (Admin, PDR, Purchasing, IT, Infrastructure, Employee).
- Aligned sidebar role gating to normalized roles.
- Fixed Dynamic LINQ filter crash in Suppliers by normalizing parameter prefixes.
- Added return-issue detection in assignment return flow and email notifications to Admin, PDR, and IT.
- Expanded seed data with multiple accounts, materials, suppliers, projects, and requests.
- Added documentation for roles and access (SEED_ROLES_AND_ACCESS.txt).
- Added seeded partial-return example for notification testing.

## In Progress
- Functional verification of dashboards and all forms/buttons by role.
- End-to-end validation of return notifications via smtpdev.

## Pending / Next Steps
- Run full role-based UI checks (Admin, PDR, Purchasing, IT, Infrastructure, Employee).
- Test all dashboards and critical CRUD flows (Create/Edit/Delete, filters, details).
- Confirm email delivery for return issues in smtpdev.
- Address remaining nullability warnings if required for stricter builds.

## Known Build Warnings (non-blocking)
- Nullability warnings across several Razor and service files (see build_output.txt).
- NuGet warning: Swashbuckle.AspNetCore 6.6.0 resolved to 6.6.1.

## Verification Checklist
- /suppliers page loads and filtering works without crashing.
- Dashboard content visible for non-admin roles.
- Materials Assignments return flow triggers notification emails for missing/damaged returns.
- CRUD pages (Suppliers, Projects, Materials, Assignments, Requests) work across roles.

## Notes
- Seed demo data is enabled in appsettings.json (Seed:Enabled=true, Seed:DemoData=true).
- Email recipients can be overridden via SMTP_ADMIN_EMAIL, SMTP_PDR_EMAIL, SMTP_IT_EMAIL.
