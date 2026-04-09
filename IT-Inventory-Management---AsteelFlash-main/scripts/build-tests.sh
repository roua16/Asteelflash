#!/usr/bin/env bash
set -euo pipefail

TEST_PROJECT="${1:-ITStockM.Tests/ITStockM.Tests.csproj}"

echo "Cleaning test project: ${TEST_PROJECT}"
dotnet clean "${TEST_PROJECT}" -v minimal

echo "Building test project: ${TEST_PROJECT}"
dotnet build "${TEST_PROJECT}" -v minimal
