This folder contains helpful scripts to inspect and migrate the `ITStockM` database.

Files:

 - `auto_init_and_run.sh` — Full automation: starts SQL Server, scaffolds/applies migrations if needed, builds and runs the app.

Basic steps to initialize the DB (development):

1) Build the solution:

```bash
dotnet build ITStockM.sln
```

2) Automatic full initialization and run (recommended for dev):

macOS / Linux:
```bash
chmod +x scripts/auto_init_and_run.sh
./scripts/auto_init_and_run.sh
```

Windows PowerShell:
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
./scripts/auto_init_and_run.ps1
```

This will start SQL Server, scaffold `InitialCreate` migration if none exists, apply migrations, build and run the Web API.

3) Alternatively run the SQL script directly (backup first):

```bash
sqlcmd -S localhost,1433 -U sa -P 'AsteelFlash@2026' -d ITStockM -i scripts/fix_AspNetUsers.sql
```

Notes:
