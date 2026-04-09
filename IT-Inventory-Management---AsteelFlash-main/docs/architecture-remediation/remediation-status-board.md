# Professional Architecture Remediation - Actual Status Board

Date: 2026-04-09

This board reflects actual repository state, not claimed state.

## Master Status (Corrected)

| ID | Step | Correct Status | Notes |
|---|---|---|---|
| 0 | Setup and Safety | In Progress | Added scripts/docs in this pass; branch governance still process-dependent. |
| 1 | Fix export format bug | Pending Verification | Requires retest against export endpoints and tests in current branch state. |
| 2 | Remove async void usage | Completed | Guard scan now reports zero `async void` in core paths. |
| 3 | Replace Task.Run fire-and-forget | Completed | Guard scan now reports zero `_ = Task.Run(...)` in `Services`. |
| 4 | Remove dynamic repository context access | Completed | Guard scan now reports zero dynamic context cast usage in `Services`. |
| 5 | Define and enforce feature service contracts | In Progress | Contracts exist, but page usage still includes monolith injection. |
| 6 | Migrate UI dependencies to feature interfaces | In Progress | Reduced direct page injections from 57 to 16; continue page-by-page migration for remaining dashboards/workflows. |
| 7 | Move query composition from pages to services | In Progress | Mixed state; requires page-by-page validation. |
| 8 | Canonical naming dictionary | In Progress | Naming dictionary exists, canonicalization incomplete in code surface. |
| 9 | Controlled folder/file rename batches | In Progress | DeliveryOrder folder normalized; many typo symbols remain. |
| 10 | Normalize model/service naming | Not Started (truthful) | Public model/service names still include typo-form tokens. |
| 11 | Consolidate repetitive export endpoints | In Progress | Needs current-code verification and endpoint diff review. |
| 12 | Harden CSV output security | Pending Verification | Requires test confirmation in current state. |
| 13 | Stabilize solution-level test build | In Progress | Build scripts added in this pass; CI execution pending. |
| 14 | Warning budget + reduction policy | In Progress | Added baseline + checker in this pass; tune baseline from measured run. |
| 15 | CI architecture regression checks | In Progress | Guard script + CI wiring added in this pass; first run expected to fail until violations are fixed. |
| 16 | Dashboard data-load optimization | Pending Verification | Requires code and runtime profiling confirmation. |

## Missing Items Checklist (Actionable)

- [x] Remove remaining _ = Task.Run(...) in Services/ITStockManagmentService.cs.
- [ ] Remove all direct ITStockManagmentService page injections by moving to feature interfaces (16 remain).
- [ ] Finish canonical naming migration for DeliveryOrder/Description/Management symbols.
- [ ] Verify and fix all architecture-guard failures until scripts/check-architecture-regressions.sh passes.
- [ ] Recompute warning baseline from clean build and update .ci/warning-budget.txt.
- [ ] Run full build and test matrix and record evidence in this folder.

## Verification Commands

- rg -n "async void" Components Services Controllers Repositories
- rg -n "_ = Task\.Run\(" Services
- rg -n "as dynamic\)\?\._context|dynamic\)\?\._context|_context\) as dynamic|as dynamic" Services
- rg -n "public ITStockManagmentService ITStockManagmentService" Components/Pages --glob "**/*.razor.cs"
- bash ./scripts/check-architecture-regressions.sh
- dotnet build ITStockM.csproj -v minimal
- dotnet test ITStockM.Tests/ITStockM.Tests.csproj -v minimal
