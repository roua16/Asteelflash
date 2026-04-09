# Seeded Accounts & Roles

These are seeded demo accounts created by `DatabaseInitializer.SeedDemoDataAsync` and `SeedAdminAsync` (used for local testing and UI tests).

| Role | Full Name | Email | Password | Notes |
|---|---|---:|---|---|
| Admin | Admin User | admin@asteelflash.com | admin123 | Admin receives operation notifications by default (can be overridden via `SMTP_ADMIN_EMAIL`).
| PDR | John Smith | john.smith@asteelflash.com | password123 | PDR Manager
| Purchasing | Sarah Johnson | sarah.johnson@asteelflash.com | password123 | Purchasing Manager
| IT | Mike Davis | mike.davis@asteelflash.com | password123 | IT Support Specialist
| Infrastructure | Alice Brown | alice.brown@asteelflash.com | password123 | Infrastructure Manager
| Purchasing | Paul Green | paul.green@asteelflash.com | password123 | Purchasing Officer
| PDR | Emma White | emma.white@asteelflash.com | password123 | PDR Technician
| Employee | Thomas Miller | thomas.miller@asteelflash.com | password123 | Generic employee account

Notes:
- The application seeds these users only if they do not exist in the database (seeding is idempotent on first run).
- For email notifications, the system uses these roles (PDR, IT, Admin) to build recipient lists when `SMTP_PDR_EMAIL` / `SMTP_IT_EMAIL` are not set.
- If you want to change seeded credentials for CI, use environment variables or modify `DatabaseInitializer`.

_Security note_: These accounts are demo/test accounts. Do NOT reuse real credentials in production.
