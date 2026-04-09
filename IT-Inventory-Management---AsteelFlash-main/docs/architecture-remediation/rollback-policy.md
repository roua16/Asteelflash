# Rollback Policy

## Rule

If a remediation batch fails build/tests or breaks smoke checks, revert only that batch and keep unrelated validated batches.

## Procedure

1. Identify failing batch scope (files changed in current batch).
2. Revert scoped files only.
3. Re-run build and tests.
4. Re-apply fix in smaller increments.

## Stop Conditions

- Repeated failure of same batch 3 times.
- Data migration risk without tested fallback.

## Recovery

- Restore previous known-good commit for affected files.
- Log failure cause in remediation-gap-report.md.
- Create follow-up task with reduced scope and explicit acceptance checks.
