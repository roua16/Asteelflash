#!/usr/bin/env bash
set -euo pipefail

SOLUTION_PATH="${1:-ITStockM.sln}"
TEST_PROJECT="ITStockM.Tests/ITStockM.Tests.csproj"

echo "Cleaning test project to avoid recursive bin growth: ${TEST_PROJECT}"
dotnet clean "${TEST_PROJECT}" -v minimal

echo "Building solution: ${SOLUTION_PATH}"
dotnet build "${SOLUTION_PATH}" -v minimal
