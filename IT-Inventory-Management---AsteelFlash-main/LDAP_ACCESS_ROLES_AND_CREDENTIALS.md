# LDAP Access Roles, Accounts, and Auto Initialization

This document describes the local Docker LDAP users, role mappings, and credentials that are auto-initialized on container startup.

## Auto Initialization Flow

When running:

```bash
docker compose up -d --build
```

the following happens automatically:

1. `ldap` container starts with base DN `dc=asteelflash,dc=com`.
2. `ldap-seed` waits for LDAP health, then imports `ldap/seed.ldif` (idempotent: skips if already seeded).
3. `app` starts with LDAP auth enabled by default in `docker-compose.yml`.
4. On first successful LDAP login, application user records are auto-provisioned/updated with mapped role.

## LDAP Admin Credentials

- LDAP admin DN: `cn=admin,dc=asteelflash,dc=com`
- LDAP admin password: `admin1234`

## LDAP User Accounts (Seeded Automatically)

| App Role | LDAP Username | Login Email | Password |
|---|---|---|---|
| Admin | `admin` | `admin@asteelflash.com` | `admin1234` |
| PDR | `pdr` | `pdr@asteelflash.com` | `pdr1234` |
| Purchasing | `purchasing` | `purchasing@asteelflash.com` | `purchasing1234` |
| IT | `it` | `it@asteelflash.com` | `it1234` |
| Infrastructure | `infrastructure` | `infrastructure@asteelflash.com` | `infrastructure1234` |
| Employee | `employee` | `employee@asteelflash.com` | `employee1234` |

## LDAP Groups to Application Roles

The seeded LDAP groups are:

- `Admin`
- `PDR`
- `Purchasing`
- `IT`
- `Infrastructure`
- `Employee`

The app maps LDAP groups to roles through `ActiveDirectory:RoleMappings`.

## Docker Environment Defaults for LDAP Auth

In `docker-compose.yml`, the app container defaults are configured to work with local OpenLDAP:

- `ActiveDirectory__Enabled=true`
- `ActiveDirectory__LdapPath=ldap://ldap:389`
- `ActiveDirectory__SearchBase=dc=asteelflash,dc=com`
- `ActiveDirectory__BindDn=cn=admin,dc=asteelflash,dc=com`
- `ActiveDirectory__BindPassword=admin1234`
- `ActiveDirectory__UserDnPattern=uid={0},ou=people,dc=asteelflash,dc=com`

## App Login Note

Use the **email** and **password** from the table above in the application login screen.
The authentication service resolves LDAP username from email prefix and applies role mapping from LDAP group membership.

## Security Note

These credentials are for local development/demo only. Rotate and externalize secrets for non-local environments.
