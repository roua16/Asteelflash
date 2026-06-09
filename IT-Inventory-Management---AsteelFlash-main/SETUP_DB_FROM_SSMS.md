# SETUP_DB_FROM_SSMS.md

This repo can run SQL Server in a Docker container (see `docker-compose.yml`).

If you instead want to create/manage the databases in **SSMS** (directly on your machine), do the steps below.

---

## Prerequisites

1. Install/start **SQL Server** (local or remote) so you can connect from your laptop.
2. Open **SSMS** (SQL Server Management Studio).
3. Decide the login you will use (below we assume `sa`, like the docker default). If you use another login, update the connection string accordingly.

---

## Step 1 — Start SSMS and connect

1. Open **SSMS**.
2. In **Connect to Server**, enter:
   - **Server name**: the exact value you see/enter in SSMS.

     Common examples:
     - `localhost`
     - `localhost\\SQLEXPRESS`
     - `192.168.1.10\\MSSQLSERVER`

     How to know what to type:
     - If your SQL Server is installed locally using defaults, try `localhost` first.
     - If you installed a named instance (e.g., SQLEXPRESS), use `hostname\\InstanceName` (often `localhost\\SQLEXPRESS`).
     - If SSMS has connected before, check the saved entry in SSMS “Registered Servers” for the exact server string.
3. **Authentication**: SQL Server Authentication
4. **Login**: `sa` (or your chosen login)
5. **Password**: the password for that login
6. Click **Connect**.

---

## Step 2 — Create the database `ITStockM`

1. In SSMS Object Explorer, right-click **Databases** → **New Database...**
2. Name: `ITStockM`
3. Click **OK**.

> Tip: You can also run a script instead:

```sql
IF DB_ID('ITStockM') IS NULL
BEGIN
    CREATE DATABASE ITStockM;
END
GO
```

---

## Step 3 — Create the database `lansweeperdb` (optional but recommended)

The app’s initialization references a LanSweeper DB name (`lansweeperdb`) for the LanSweeper connection string.

1. Right-click **Databases** → **New Database...**
2. Name: `lansweeperdb`
3. Click **OK**.

> If you skip this database, the app may fail to connect to LanSweeper tables until it exists.

---

## Step 4 — Ensure your SQL login can access both DBs

1. In SSMS, expand **Security** → **Logins**.
2. Ensure the login you will use (e.g., `sa`) exists.
3. Ensure the login can access `ITStockM` and `lansweeperdb`:
   - Right-click the login → **Properties**
   - Or (recommended) map/grant permissions inside each DB.

> If you use `sa`, you usually don’t need extra permissions.

---

## Step 5 — Set environment variables for the app

The app reads connection strings from these environment variables:
- `ConnectionStrings__ITStockManagmentConnection`
- `ConnectionStrings__LanSweeperConnection`

You must set `Server=...` to the same server string you used in **SSMS**, except include port if needed.

> Important: The `Server=` value must be reachable from **inside Docker**. 
> - If you keep `Server=localhost,1433`, this usually works on Docker Desktop (macOS), but if it fails, replace `localhost` with your host IP.

### Option A — Put values in your shell (quick)

Run these commands **before** starting docker compose.

#### Business DB (ITStockM)
```bash
export ConnectionStrings__ITStockManagmentConnection="Server=localhost,1433;Database=ITStockM;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
```

#### LanSweeper DB (lansweeperdb)
```bash
export ConnectionStrings__LanSweeperConnection="Server=localhost,1433;Database=lansweeperdb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False"
```

### Option B — Put values in a `.env` file (preferred)

1. Create/edit `.env` in the repo root.
2. Add:

```env
ConnectionStrings__ITStockManagmentConnection=Server=localhost,1433;Database=ITStockM;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True
ConnectionStrings__LanSweeperConnection=Server=localhost,1433;Database=lansweeperdb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=False
```

---

## Step 6 — Start the app

From the repo root:

```bash
docker compose up --build
```

Wait for logs showing schema/migrations being applied (or EnsureCreated fallback) and then:
- `Database seeding complete.`

---

## Step 7 — Recreate everything cleanly (if needed)

If you need a fresh database state:

1. In SSMS, right-click `ITStockM` → **Delete**
2. Confirm deletion
3. Repeat for `lansweeperdb`
4. Run `docker compose up --build` again.

---

## Troubleshooting

### App says it can’t connect to SQL Server

- Verify SQL Server is reachable from the machine/container:
  - Port: typically **1433** (or update `Server=...,PORT`)
- Ensure login/password are correct.
- If `Server=localhost,1433` fails, replace `localhost` with your host IP.

### Seeding didn’t happen

- Check logs for `Applying EF Core migrations` / `EnsureCreated` / `Database seeding complete`.
- If you need to re-seed, you can:
  - set `Seed__ClearOldData=true` (dev only; destructive)
  - or recreate DBs in SSMS.

