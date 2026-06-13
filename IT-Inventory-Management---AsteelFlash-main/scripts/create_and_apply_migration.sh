#!/usr/bin/env bash
set -euo pipefail

# Helper script to create and apply an EF Core migration for ITStockM
# Run from repository root. Requires dotnet-ef tool installed.

MIGRATION_NAME=${1:-InitialCreate}
PROJECT="src/ITStockM.Infrastructure/ITStockM.Infrastructure.csproj"
STARTUP_PROJECT="src/ITStockM.WebApi/ITStockM.WebApi.csproj"

echo "Creating migration: $MIGRATION_NAME"
# Create migration
dotnet ef migrations add "$MIGRATION_NAME" --project "$PROJECT" --startup-project "$STARTUP_PROJECT"

echo "Applying migrations to the database"
dotnet ef database update --project "$PROJECT" --startup-project "$STARTUP_PROJECT"

echo "Done."
