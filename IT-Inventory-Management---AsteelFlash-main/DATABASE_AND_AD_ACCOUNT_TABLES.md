# Database and AD Account Tables

Date: 2026-06-07

## Important Clarification

Active Directory accounts are not stored as AD accounts inside this application database.

Instead:

- AD is the external source of truth for credentials when enabled.
- The app stores a local business profile in `dbo.Employee`.
- For AD-authenticated users, the app may also create a local Identity profile in `dbo.AspNetUsers`.
- Role membership is stored in `dbo.AspNetRoles` and `dbo.AspNetUserRoles`.

## Account-Related Tables

| Table | Purpose | Key Columns |
|---|---|---|
| `dbo.Employee` | Business user profile used by the app UI and services | `Id`, `FullName`, `Email`, `Post`, `PhoneNumber`, `Service`, `Role` |
| `dbo.AspNetUsers` | ASP.NET Identity account mirror | `Id`, `UserName`, `Email`, `FullName`, `Post`, `Service`, `Role`, `EmployeeId`, `CreatedAt`, `UpdatedAt`, `IsDeleted` |
| `dbo.AspNetRoles` | Identity role catalog | `Id`, `Name`, `NormalizedName` |
| `dbo.AspNetUserRoles` | User-to-role membership | `UserId`, `RoleId` |
| `dbo.AspNetUserClaims` | Optional user claims | `Id`, `UserId`, `ClaimType`, `ClaimValue` |
| `dbo.AspNetRoleClaims` | Optional role claims | `Id`, `RoleId`, `ClaimType`, `ClaimValue` |
| `dbo.AspNetUserLogins` | External login provider links | `LoginProvider`, `ProviderKey`, `UserId` |
| `dbo.AspNetUserTokens` | Identity tokens | `UserId`, `LoginProvider`, `Name`, `Value` |

## Business Domain Tables

| Table | Purpose |
|---|---|
| `dbo.Employee` | Employees/users and app roles |
| `dbo.Project` | Projects used for assignments |
| `dbo.Supplier` | Suppliers/vendors |
| `dbo.Materiel` | IT/PDR stock material |
| `dbo.Assignment` | Asset assignments to employees |
| `dbo.AssignmentMateriel` | Materials attached to assignments |
| `dbo.Request` | Purchase/material requests |
| `dbo.Offer` | Supplier offers for requests |
| `dbo.DeliveryOrder` | Delivery orders |
| `dbo.DeliveryOrderMateriel` | Materials attached to delivery orders |
| `dbo.MaintenanceTicket` | Asset maintenance issues |
| `dbo.AssetLifecycleRecord` | Lifecycle history per material |
| `dbo.AssetPrediction` | Predictive health/replacement data |

## LanSweeper Demo Tables

These are created in the separate `lansweeperdb` database when `LanSweeperConnection` is configured.

| Table | Purpose |
|---|---|
| `dbo.tblAssets` | Demo/network-discovered assets |
| `dbo.tblAssetCustom` | Asset hardware/OS metadata |

## Seeded Accounts

Default local Docker seed behavior:

- `SEED_ENABLED=true`
- `SEED_DEMO_DATA=true`
- `ACTIVE_DIRECTORY_ENABLED=false`

Default seeded admin:

| Email | Role | Source |
|---|---|---|
| `admin@asteelflash.com` | `Admin` | `Seed:AdminEmail` / `Seed:AdminPassword` |

Default demo users:

| Email | Role |
|---|---|
| `john.smith@asteelflash.com` | `PDR` |
| `sarah.johnson@asteelflash.com` | `Purchasing` |
| `mike.davis@asteelflash.com` | `IT` |
| `alice.brown@asteelflash.com` | `Infrastructure` |
| `paul.green@asteelflash.com` | `Purchasing` |
| `emma.white@asteelflash.com` | `PDR` |
| `thomas.miller@asteelflash.com` | `Employee` |

Fallback password behavior:

- Admin user uses `Seed:AdminPassword`.
- Demo/non-admin users use `Seed:DemoPassword` when configured.
- If `Seed:DemoPassword` is missing, demo/non-admin users use `Seed:AdminPassword`.
- Legacy fallback passwords are no longer accepted.

## AD Provisioning Behavior

When AD is enabled and a user authenticates successfully:

1. The app searches for `Employee.Email`.
2. If no employee exists, a new `dbo.Employee` row is inserted.
3. The `Role` is resolved from AD group mappings.
4. If no mapping matches, the role defaults to `Employee`.
5. The app ensures Identity roles exist in `dbo.AspNetRoles`.
6. The app creates or updates a matching `dbo.AspNetUsers` row.
7. The app inserts role membership into `dbo.AspNetUserRoles`.

## Useful SQL Checks

### Count All Account Rows

```sql
SELECT COUNT(*) AS EmployeeCount FROM dbo.Employee;
SELECT COUNT(*) AS IdentityUserCount FROM dbo.AspNetUsers;
SELECT COUNT(*) AS IdentityRoleCount FROM dbo.AspNetRoles;
SELECT COUNT(*) AS IdentityMembershipCount FROM dbo.AspNetUserRoles;
```

### List Business Users by Role

```sql
SELECT Role, COUNT(*) AS UserCount
FROM dbo.Employee
GROUP BY Role
ORDER BY Role;
```

### Find AD-Provisioned/Mirrored Identity Users

```sql
SELECT
    e.Id AS EmployeeId,
    e.Email AS EmployeeEmail,
    e.FullName AS EmployeeName,
    e.Role AS EmployeeRole,
    u.Id AS IdentityUserId,
    u.Email AS IdentityEmail,
    u.Role AS IdentityRole
FROM dbo.Employee e
LEFT JOIN dbo.AspNetUsers u ON u.EmployeeId = e.Id
ORDER BY e.Email;
```

### List Identity Role Membership

```sql
SELECT
    u.Email,
    u.EmployeeId,
    r.Name AS RoleName
FROM dbo.AspNetUsers u
JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
ORDER BY u.Email, r.Name;
```

### Find Employees Without Identity Mirror

```sql
SELECT e.Id, e.Email, e.FullName, e.Role
FROM dbo.Employee e
LEFT JOIN dbo.AspNetUsers u ON u.EmployeeId = e.Id
WHERE u.Id IS NULL
ORDER BY e.Email;
```

This is expected for fallback/demo users unless they authenticated through AD provisioning.

### Check Required Domain Tables

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo'
ORDER BY TABLE_NAME;
```

## Docker SQL Server Connection

Default app connection in Docker:

```text
Server=sqlserver,1433;Database=ITStockM;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True
```

From the host machine, use:

```text
Server=localhost,1433;Database=ITStockM;User Id=sa;Password=<your MSSQL_SA_PASSWORD>;TrustServerCertificate=True;Encrypt=False
```

## Required AD Environment Variables

```bash
ACTIVE_DIRECTORY_ENABLED=true
ACTIVE_DIRECTORY_LDAP_PATH=LDAP://your.domain.local
ACTIVE_DIRECTORY_DOMAIN=your.domain.local
ACTIVE_DIRECTORY_SEARCH_BASE=DC=your,DC=domain,DC=local
ACTIVE_DIRECTORY_TIMEOUT_SECONDS=5
SEED_ENABLED=false
SEED_DEMO_DATA=false
```

Role mappings are currently configured in `appsettings.json` / `appsettings.Development.json`.
