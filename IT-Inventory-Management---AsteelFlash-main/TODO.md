# TODO

## Initialization + Docker compose fixes
- [ ] Standardize SQL Server container naming between compose and scripts (use existing `itstockm-mssql` name).
- [ ] Ensure SQL Server is reachable externally on localhost:1433 consistently.
- [ ] Locate app seeding + Employee→AspNetUsers mirroring logic.
- [ ] Make seeding idempotent (no duplication across restarts) and deterministic.
- [ ] Move any `fix_AspNetUsers.sql`-type schema corrections into EF migrations / startup schema initialization.
- [ ] Verify/adjust healthchecks so app starts only after schema is ready.
- [ ] Add a host-side verification script/command to query key table counts.

