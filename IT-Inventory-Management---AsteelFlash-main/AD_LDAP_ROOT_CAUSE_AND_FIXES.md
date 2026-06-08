# AD/LDAP Authentication - Root Cause and Fix Summary (v6 -> v7)

Date: 2026-06-08

## Scope of this report

This document is based on the Git branch diff between `v6` and `v7`.

Diff snapshot:
- 44 files changed
- 3689 insertions, 160 deletions

Primary AD/auth-related changes are in:
1. [docker-compose.yml](docker-compose.yml)
2. [Dockerfile](Dockerfile)
3. [ldap/seed.ldif](ldap/seed.ldif)
4. [ldap/memberof-overlay.ldif](ldap/memberof-overlay.ldif)
5. [src/ITStockM.Infrastructure/Services/ActiveDirectory/ActiveDirectoryService.cs](src/ITStockM.Infrastructure/Services/ActiveDirectory/ActiveDirectoryService.cs)
6. [src/ITStockM.Infrastructure/Services/AuthService.cs](src/ITStockM.Infrastructure/Services/AuthService.cs)
7. [src/ITStockM.WebApi/Components/Pages/MaterialsViewPdr.razor](src/ITStockM.WebApi/Components/Pages/MaterialsViewPdr.razor)
8. [src/ITStockM.WebApi/Components/Layout/SideLayout.razor](src/ITStockM.WebApi/Components/Layout/SideLayout.razor)
9. [src/ITStockM.Tests/Ui/RoleAccessTests.cs](src/ITStockM.Tests/Ui/RoleAccessTests.cs)
10. [src/ITStockM.Tests/Ui/UiTheoryAttribute.cs](src/ITStockM.Tests/Ui/UiTheoryAttribute.cs)

## What was the problem in v6

### 1) LDAP bootstrap was not robust
- `v7` adds dedicated `ldap` and `ldap-seed` services in [docker-compose.yml](docker-compose.yml), which indicates startup/seeding gaps in `v6`.
- `v7` introduces RootDSE health probing and idempotent seed checks before import.

### 2) Linux runtime LDAP compatibility was missing
- `v7` updates [Dockerfile](Dockerfile) to install `libldap2` and `libsasl2-2` and add compatibility symlinks (`libldap-2.5.so.0`, `liblber-2.5.so.0`).
- This addresses Linux runtime failures for LDAP protocol usage.

### 3) AD auth path needed strict AD-only enforcement when enabled
- In `v7`, [src/ITStockM.Infrastructure/Services/AuthService.cs](src/ITStockM.Infrastructure/Services/AuthService.cs) explicitly blocks fallback password login when AD is enabled and AD authentication fails.
- This closes the mismatch where fallback behavior could interfere with AD-only intent.

### 4) AD/LDAP implementation required provider-safe behavior
- In `v7`, [src/ITStockM.Infrastructure/Services/ActiveDirectory/ActiveDirectoryService.cs](src/ITStockM.Infrastructure/Services/ActiveDirectory/ActiveDirectoryService.cs) switches from `System.DirectoryServices` to `System.DirectoryServices.Protocols` and adds:
1. Multi-strategy bind (user DN pattern, AD principal candidates, service-account search fallback).
2. User profile/group resolution for role mapping.
3. Safer LDAP filter handling.

### 5) Authorization and UI role visibility were inconsistent for PDR flow
- In `v7`, [src/ITStockM.WebApi/Components/Pages/MaterialsViewPdr.razor](src/ITStockM.WebApi/Components/Pages/MaterialsViewPdr.razor) adds `PDR` in the `[Authorize(Roles=...)]` list.
- In `v7`, [src/ITStockM.WebApi/Components/Layout/SideLayout.razor](src/ITStockM.WebApi/Components/Layout/SideLayout.razor) updates role visibility and labels (including PDR standby/provisional loans path alignment).

### 6) UI role test matrix was outdated
- In `v7`, [src/ITStockM.Tests/Ui/RoleAccessTests.cs](src/ITStockM.Tests/Ui/RoleAccessTests.cs) is refactored to scenario-driven allow/deny route checks using seeded accounts.
- [src/ITStockM.Tests/Ui/UiTheoryAttribute.cs](src/ITStockM.Tests/Ui/UiTheoryAttribute.cs) is added for gated UI theory execution via `RUN_UI_TESTS`.

## What we changed in v7 to make AD work

## Docker + LDAP startup hardening
1. Added local LDAP container and one-shot seeding workflow in [docker-compose.yml](docker-compose.yml).
2. Added RootDSE health checks and wait-until-ready logic before seeding.
3. Added idempotent import behavior to avoid repeated destructive seed actions.
4. Added persistent LDAP data/config volumes.

## LDAP seed data and directory structure
1. Added/fixed baseline entries and users/groups in [ldap/seed.ldif](ldap/seed.ldif).
2. Added optional memberOf overlay support in [ldap/memberof-overlay.ldif](ldap/memberof-overlay.ldif).

## Auth service behavior and provisioning
1. Enforced AD-first authentication when AD is enabled in [src/ITStockM.Infrastructure/Services/AuthService.cs](src/ITStockM.Infrastructure/Services/AuthService.cs).
2. Added AD-authenticated user provisioning path and identity-role ensure path.
3. Kept fallback login only for non-AD mode.

## LDAP protocol implementation update
1. Migrated to `System.DirectoryServices.Protocols` in [src/ITStockM.Infrastructure/Services/ActiveDirectory/ActiveDirectoryService.cs](src/ITStockM.Infrastructure/Services/ActiveDirectory/ActiveDirectoryService.cs).
2. Implemented bind/search logic compatible with AD-style and OpenLDAP-style directories.
3. Added richer group extraction and resolved app role mapping behavior.

## Role/access consistency
1. Added `PDR` to protected standby/materials page authorization in [src/ITStockM.WebApi/Components/Pages/MaterialsViewPdr.razor](src/ITStockM.WebApi/Components/Pages/MaterialsViewPdr.razor).
2. Synchronized sidebar navigation role visibility in [src/ITStockM.WebApi/Components/Layout/SideLayout.razor](src/ITStockM.WebApi/Components/Layout/SideLayout.razor) and [src/ITStockM.WebApi/Components/Layout/SideLayout.razor.cs](src/ITStockM.WebApi/Components/Layout/SideLayout.razor.cs).

## Tests and validation
1. Reworked UI role tests to current seeded LDAP identities in [src/ITStockM.Tests/Ui/RoleAccessTests.cs](src/ITStockM.Tests/Ui/RoleAccessTests.cs).
2. Added environment-gated UI theory attribute in [src/ITStockM.Tests/Ui/UiTheoryAttribute.cs](src/ITStockM.Tests/Ui/UiTheoryAttribute.cs).
3. Recorded passing role matrix and AD access outcomes in [AUTH_AD_ACCESS_TEST_REPORT.md](AUTH_AD_ACCESS_TEST_REPORT.md).

## Outcome (based on v6 -> v7)

From branch comparison, `v7` introduces the missing LDAP infrastructure, Linux runtime dependencies, AD-auth strictness, role-consistency fixes, and updated role smoke validation that were needed to make AD/LDAP authentication and authorization operational in this project setup.

## Operational notes

For AD mode reproduction, keep these configured:
- `ACTIVE_DIRECTORY_ENABLED=true`
- `ACTIVE_DIRECTORY_LDAP_PATH`
- `ACTIVE_DIRECTORY_DOMAIN`
- `ACTIVE_DIRECTORY_SEARCH_BASE`
- `ACTIVE_DIRECTORY_BIND_DN`
- `ACTIVE_DIRECTORY_BIND_PASSWORD`

Keep LDAP seeding enabled in compose startup so required users/groups exist before app login tests.
