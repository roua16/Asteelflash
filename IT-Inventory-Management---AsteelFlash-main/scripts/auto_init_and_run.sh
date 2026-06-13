#!/usr/bin/env bash
set -euo pipefail

# Automated initialization and run script for development.
# - Starts local SQL Server (Docker)
# - Exports connection string
# - Scaffolds InitialCreate migration if none exist
# - Applies migrations to the database
# - Builds and runs the WebApi project

ROOT_DIR=$(cd "$(dirname "$0")/.." && pwd)
cd "$ROOT_DIR"

echo "Starting local SQL Server (Docker)..."
chmod +x ./scripts/start_mssql_docker.sh
./scripts/start_mssql_docker.sh itstockm-mssql AsteelFlash@2026

export ConnectionStrings__ITStockManagmentConnection="Server=localhost,1433;Database=ITStockM;User Id=sa;Password=AsteelFlash@2026;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=True"
echo "Exported ConnectionStrings__ITStockManagmentConnection"

# Ensure dotnet-ef is available for scaffolding migrations
if ! command -v dotnet-ef >/dev/null 2>&1; then
  echo "dotnet-ef not found. Installing global tool..."
  dotnet tool install --global dotnet-ef || true
  export PATH="$PATH:$HOME/.dotnet/tools"
fi

MIGRATIONS_DIR=src/ITStockM.Infrastructure/Migrations
PROJECT="src/ITStockM.Infrastructure/ITStockM.Infrastructure.csproj"
STARTUP_PROJECT="src/ITStockM.WebApi/ITStockM.WebApi.csproj"

if [ -d "$MIGRATIONS_DIR" ] && [ "$(ls -A $MIGRATIONS_DIR)" ]; then
  echo "Existing migrations found — applying to database..."
  dotnet ef database update --project "$PROJECT" --startup-project "$STARTUP_PROJECT"
else
  echo "No migrations found — creating 'InitialCreate' migration and applying..."
  dotnet ef migrations add InitialCreate --project "$PROJECT" --startup-project "$STARTUP_PROJECT"
  dotnet ef database update --project "$PROJECT" --startup-project "$STARTUP_PROJECT"
fi

echo "Building solution..."
dotnet build ITStockM.sln

echo "Running WebApi project..."
dotnet run --project $STARTUP_PROJECT
