# Smoke Test Checklist

Run after each remediation batch.

## Build

- dotnet build ITStockM.csproj -v minimal
- dotnet build ITStockM.sln -v minimal

## Unit/Integration Tests

- dotnet test ITStockM.Tests/ITStockM.Tests.csproj -v minimal

## Critical Functional Smoke

- Login with admin account.
- Open dashboard and confirm cards render.
- Create and edit a delivery order.
- Create and complete a request flow.
- Export CSV and Excel for one feature endpoint.
- Open materials assignment page and submit one assignment.

## Pass Criteria

- No compile errors.
- No failed tests in targeted suite.
- No runtime errors in browser console for tested flows.
