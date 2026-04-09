#!/usr/bin/env bash
set -euo pipefail

PROJECT_PATH="${1:-ITStockM.csproj}"

echo "Building app project: ${PROJECT_PATH}"
dotnet build "${PROJECT_PATH}" -v minimal
