Setup guide — initialize SQL Server and database for ITStockM

This guide walks you through setting up a local SQL Server on `localhost,1433`, preparing the project, and ensuring the database is initialized automatically at app startup.

Prerequisites
- Docker (for running local SQL Server) OR a local SQL Server instance listening on `localhost,1433`.
- .NET SDK 7+ (to build and run the app)
- `dotnet-ef` tool (optional, for scaffolding migrations):

```bash
dotnet tool install --global dotnet-ef
```

1) Start SQL Server (recommended: Docker)

```bash
chmod +x scripts/start_mssql_docker.sh
./scripts/start_mssql_docker.sh
```

This will run a SQL Server 2019 container bound to `localhost:1433` with SA password `AsteelFlash@2026`.

2) Verify connection (optional)

Using `sqlcmd` (install via Homebrew: `brew install msodbcsql17 mssql-tools` or use Azure Data Studio):

```bash
/opt/homebrew/bin/sqlcmd -S localhost,1433 -U sa -P 'AsteelFlash@2026' -Q "SELECT @@VERSION;"
```

3) Configure the app (already set for development)

The `appsettings.Development.json` already contains the connection string:

```
"ConnectionStrings": {
  "ITStockManagmentConnection": "Server=localhost,1433;Database=ITStockM;User Id=sa;Password=AsteelFlash@2026;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
}
```

If you need to set it via environment variable (useful in CI), set:

```bash
export ConnectionStrings__ITStockManagmentConnection="Server=localhost,1433;Database=ITStockM;User Id=sa;Password=AsteelFlash@2026;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
```

4) Build and run the app — automatic DB initialization

The app runs `DatabaseInitializer` during startup which will:
- Apply EF Core migrations if migrations exist in the assembly
- Otherwise fall back to `EnsureCreated` and create the schema automatically

Build and run:

```bash
dotnet build ITStockM.sln
dotnet run --project src/ITStockM.WebApi/ITStockM.WebApi.csproj
```

On first startup the database will be created and seed data will be inserted (development mode).

5) (Optional) Create and apply migrations using `dotnet-ef`

If you want to manage schema through EF migrations (recommended for production):

```bash
# From repository root
chmod +x scripts/create_and_apply_migration.sh
./scripts/create_and_apply_migration.sh InitialCreate
```

This will scaffold a migration in the `ITStockM.Infrastructure` project and apply it to the local database.

6) Inspect or fix the `AspNetUsers` table

If you previously had a mismatched `AspNetUsers` table, run the provided fixer script in `scripts/fix_AspNetUsers.sql` using SSMS, Azure Data Studio, or `sqlcmd`:

```bash
/opt/homebrew/bin/sqlcmd -S localhost,1433 -U sa -P 'AsteelFlash@2026' -d ITStockM -i scripts/fix_AspNetUsers.sql
```

Troubleshooting
- If migrations fail, check that the connection string is correct and the SQL Server container is running (`docker ps`).
- If `dotnet ef` cannot connect when creating migrations, ensure `ConnectionStrings__ITStockManagmentConnection` env var is set, or pass `--startup-project` to `dotnet ef` as done by the helper script.

Security note
- The `sa` password is for development only. Do not use this in production.

If you want, I can now:
- Generate an `InitialCreate` EF migration files in the repo.
- Or run the SQL fixer and inspect the resulting schema (you will need to run it locally and paste results).
