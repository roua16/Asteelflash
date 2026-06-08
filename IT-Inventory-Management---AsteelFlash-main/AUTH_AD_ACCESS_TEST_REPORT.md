# Auth, Role, Active Directory Access Test Report

Date: 2026-06-07

## Scope

This report covers the current authentication, role authorization, Active Directory (AD) integration, seed login behavior, Docker startup configuration, and local test results for the IT Stock Management app.

## Executive Summary

- Local build of the web project passes.
- Non-UI test execution runs, but 3 existing service tests fail outside auth/AD.
- AD live authentication was not executed because the local `.env` does not define AD connection variables and no AD test account/password was provided.
- Docker is configured for local demo/bootstrap access by default: AD disabled, seed/demo enabled.
- AD-provisioned app accounts are stored in the business `dbo.Employee` table and optionally mirrored into Identity tables such as `dbo.AspNetUsers`, `dbo.AspNetRoles`, and `dbo.AspNetUserRoles`.
- A Google/Gemini-style API key was visible in the IDE; rotate that key and keep it out of committed files.

## Fresh UI Role Smoke Matrix (2026-06-08)

Command executed:

```bash
RUN_UI_TESTS=true UI_BASE_URL=http://localhost:8080 PLAYWRIGHT_CHANNEL=chrome dotnet test src/ITStockM.Tests/ITStockM.Tests.csproj --filter Category=UI -v minimal
```

Overall result:

- Total role scenarios: 6
- Passed: 6
- Failed: 0
- Duration: ~44s

Role scenario outcomes:

| Role | Seeded Account | Result | Notes |
|---|---|---|---|
| Admin | `admin@asteelflash.com` | PASS | Expected allow/deny checks succeeded. |
| Purchasing | `purchasing@asteelflash.com` | PASS | Expected allow/deny checks succeeded. |
| IT | `it@asteelflash.com` | PASS | Expected allow/deny checks succeeded. |
| Infrastructure | `infrastructure@asteelflash.com` | PASS | Expected allow/deny checks succeeded. |
| Employee | `employee@asteelflash.com` | PASS | Expected allow/deny checks succeeded. |
| PDR | `pdr@asteelflash.com` | PASS | `/standby` and other expected routes are accessible. |

Interpretation:

- Test suite is now aligned with current seeded LDAP credentials.
- Full UI role matrix is passing after rebuilding the app container with latest authorization changes.

## Tested Locally

### Build

Command:

```bash
dotnet build src/ITStockM.WebApi/ITStockM.WebApi.csproj --no-restore -v:minimal -m:1 /p:UseSharedCompilation=false
```

Result:

- Passed.
- `0 Error(s)`.
- Existing warnings remain for package advisories, nullable annotations in non-nullable projects, and Windows-only `System.DirectoryServices` calls.

### Non-UI Tests

Command:

```bash
dotnet test src/ITStockM.Tests/ITStockM.Tests.csproj --no-restore --filter "FullyQualifiedName!~Ui" -v:minimal -m:1 /p:UseSharedCompilation=false
```

Result:

- Total: 122
- Passed: 119
- Failed: 3
- Skipped: 0

Failures observed:

- `TicketPrioritizationServiceTests.PredictPriorityAsync_CriticalSignals_ReturnsCritique`
  - Expected `Critique`, actual `Moyenne`.
- `TicketPrioritizationServiceTests.PredictPriorityAsync_MinorIssue_ReturnsFaible`
  - Expected `Faible`, actual `Moyenne`.
- `HardwareRecommendationServiceTests.RecommendAsync_DeveloperProfile_PrefersHighPerformanceAsset`
  - Expected 2 recommendations, actual 0.

These failures are service/AI recommendation behavior issues, not direct AD authentication failures.

## AD Live Access Status

AD live bind was not executed in this environment.

Reason:

- `.env` does not currently include:
  - `ACTIVE_DIRECTORY_ENABLED`
  - `ACTIVE_DIRECTORY_LDAP_PATH`
  - `ACTIVE_DIRECTORY_DOMAIN`
  - `ACTIVE_DIRECTORY_SEARCH_BASE`
- No test AD username/password was provided.
- The current AD implementation uses `System.DirectoryServices`, which is reported by the compiler as Windows-only. Docker/Linux AD mode is therefore risky until this is replaced or verified in the deployment OS.

## AD Authentication Flow

1. User submits e-mail and password on the Blazor login page.
2. `AuthService.Authenticate` trims the e-mail and derives the `sAMAccountName` from the e-mail prefix.
3. `ActiveDirectoryService.AuthenticateAsync` tries LDAP bind/search when `ActiveDirectory:Enabled=true`.
4. If AD is enabled and LDAP auth fails, login fails immediately.
5. If AD is disabled, fallback login checks the `Employee` row and configured seed password.
6. If AD auth succeeds and the employee does not exist, the app creates an `Employee` row.
7. AD group membership is mapped to app roles using `ActiveDirectory:RoleMappings`.
8. Identity roles and user membership are ensured for AD-authenticated users.
9. The app issues a claims cookie containing `NameIdentifier`, `Email`, `Name`, and `Role`.

## Role Access Matrix

| Area | Roles |
|---|---|
| Dashboard | `Admin`, `PDR`, `Purchasing`, `IT`, `Infrastructure`, `Employee` |
| Employees / Projects / Admin CRUD | `Admin` |
| Assignments interface | `Admin`, `IT`, `Infrastructure`, `Employee` |
| Materials view interface | `Admin`, `Purchasing`, `IT` |
| Materials PDR page | `Admin`, `PDR`, `IT`, `Infrastructure`, `Employee` |
| Delivery orders | `Admin`, `Purchasing`, `PDR` |
| Suppliers | `Admin`, `Purchasing`, `PDR` |
| Purchasing pages | `Admin`, `Purchasing` |
| Infrastructure / LanSweeper / Maintenance / Predictions | `Admin`, `IT`, `Infrastructure` |
| Guest | Central role exists, but no protected pages currently allow it |

## Docker Auth Defaults

Local Docker defaults:

```yaml
ActiveDirectory__Enabled: "${ACTIVE_DIRECTORY_ENABLED:-false}"
Seed__Enabled: "${SEED_ENABLED:-true}"
Seed__DemoData: "${SEED_DEMO_DATA:-true}"
```

Implication:

- Fresh local Docker startup should seed admin/demo users.
- For AD-only environments, set:

```bash
ACTIVE_DIRECTORY_ENABLED=true
SEED_ENABLED=false
SEED_DEMO_DATA=false
ACTIVE_DIRECTORY_LDAP_PATH=LDAP://your.domain.local
ACTIVE_DIRECTORY_DOMAIN=your.domain.local
ACTIVE_DIRECTORY_SEARCH_BASE=DC=your,DC=domain,DC=local
```

## Manual AD Smoke Test Checklist

Run this only with a real AD test user.

1. Add AD variables to `.env` or shell.
2. Start SQL Server and app:

   ```bash
   docker compose up -d --build
   ```

3. Open:

   ```text
   http://localhost:8080/login
   ```

4. Login with an AD user in each mapped group:
   - Admin group → should access Employees and Projects admin pages.
   - PDR group → should access delivery/supplier PDR pages.
   - Purchasing group → should access purchasing pages.
   - IT group → should access infrastructure/material pages.
   - Infrastructure group → should access infrastructure pages.
   - Unmapped AD user → should become `Employee` unless mapping provides another role.

5. Confirm the account was provisioned:

   ```sql
   SELECT Id, Email, FullName, Post, Service, Role
   FROM dbo.Employee
   WHERE Email = 'user@domain.local';
   ```

6. Confirm Identity mirror/role membership for AD-authenticated users:

   ```sql
   SELECT u.Id, u.Email, u.FullName, u.EmployeeId, u.Role
   FROM dbo.AspNetUsers u
   WHERE u.Email = 'user@domain.local';

   SELECT u.Email, r.Name AS RoleName
   FROM dbo.AspNetUsers u
   JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
   JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
   WHERE u.Email = 'user@domain.local';
   ```

## Known Risks

- `System.DirectoryServices` is Windows-only according to build warnings; AD in Linux Docker may fail when enabled.
- AD group matching is substring-based, so short mappings such as `IT` can overmatch unintended group names.
- UI role tests currently require a live app/browser and should be updated to use current configured seed passwords.
- The application uses `EnsureCreatedAsync` instead of EF migrations; schema evolution remains fragile.

## Recommended Next Fixes

1. Replace `System.DirectoryServices` with `System.DirectoryServices.Protocols` for container-friendly LDAP.
2. Add unit tests for:
   - AD disabled + fallback login.
   - AD enabled + failed bind rejects fallback.
   - AD success provisions `Employee` and Identity role.
3. Update UI tests to use `Seed:AdminPassword` / `Seed:DemoPassword` instead of hardcoded legacy passwords.
4. Move schema management to EF migrations.
5. Rotate the exposed API key and store only placeholder values in `.env.example`.
