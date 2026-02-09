#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BASE_URL="${UI_BASE_URL:-http://localhost:8080}"

cd "$ROOT_DIR"

dotnet build ITStockM.Tests/ITStockM.Tests.csproj -c Release

PLAYWRIGHT_SH="$ROOT_DIR/ITStockM.Tests/bin/Release/net10.0/playwright.sh"
PLAYWRIGHT_PS1="$ROOT_DIR/ITStockM.Tests/bin/Release/net10.0/playwright.ps1"

if [[ -f "$PLAYWRIGHT_SH" ]]; then
  "$PLAYWRIGHT_SH" install
elif [[ -f "$PLAYWRIGHT_PS1" && -x "$(command -v pwsh || true)" ]]; then
  pwsh "$PLAYWRIGHT_PS1" install
else
  echo "Playwright install script not found or pwsh missing. Skipping install; the test fixture will attempt install." 
fi

RUN_UI_TESTS=true PLAYWRIGHT_CHANNEL=chrome UI_BASE_URL="$BASE_URL" dotnet test ITStockM.Tests/ITStockM.Tests.csproj -c Release --no-build --filter "Category=UI"
