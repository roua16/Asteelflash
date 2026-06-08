# AI Governance and Monitoring Plan (Project-Specific)

Date: 2026-06-08

## 1. Governance scope and ownership

This plan governs:
1. Hardware recommendation retraining lifecycle.
2. Ticket prioritization retraining lifecycle.
3. Health/readiness API observability.
4. Gemini provider operational reliability.

Ownership model (target):
1. Product Owner: business KPI signoff.
2. AI/Engineering Owner: model gate thresholds and retraining quality.
3. Platform Owner: deployment and observability pipeline.
4. Security Owner: provider key handling and audit controls.
5. Operations Owner: incident response and escalation.

Current status:
1. Technical controls implemented.
2. Named individuals for all roles pending org assignment.

## 2. Implemented monitoring controls

### Available runtime endpoints
1. `GET /api/v1/health/ai-models`
2. `GET /api/v1/health/ai-readiness`

### Signals currently available
1. Drift score and drift level by model.
2. Last trained timestamp.
3. Active and previous model versions.
4. Validation metric.
5. Last retrain status including blocked gate statuses.
6. Fleet readiness boolean and per-model operational flag.

### Log-based signals currently available
1. Warning logs when retraining is blocked by data quality gates.
2. Warning logs when retraining is blocked by acceptance gates.
3. Structured Gemini API failure handling in chat service/controller.

## 3. Alert policy for this architecture

Status: In Progress (policy defined, channel routing pending)

Severity matrix:
| Severity | Condition | Target response |
|---|---|---|
| P1 | AI endpoint unavailable or major provider outage | 15 minutes |
| P2 | Retraining blocked by acceptance/quality gates repeatedly | 1 hour |
| P3 | Drift trend increases without immediate failure | 1 business day |

Recommended concrete alert triggers:
1. P1 when 5xx rate for AI endpoints > 5% for 5 minutes.
2. P1 when Gemini request failures > 20% for 10 minutes.
3. P2 when readiness endpoint reports `ReadyForProduction=false` for 2 consecutive checks.
4. P2 when model status is `blocked_acceptance_min_metric` or `blocked_data_quality_*` for 2 retraining cycles.
5. P3 when drift remains in high band for 24 hours.

## 4. Operational playbooks (kept)

### A. Model degradation
1. Confirm issue in ai-models and ai-readiness endpoints.
2. Identify affected model and last retrain status.
3. Keep previous active model (rollover) or trigger manual rollback if needed.
4. Record incident and gate failure reason.

### B. Retraining blocked/failure
1. Check blocked status code (`blocked_data_quality_*` or `blocked_acceptance_min_metric`).
2. Diagnose dataset composition and config thresholds.
3. Re-run retraining after corrective action.
4. Track closure in incident log.

### C. Gemini provider outage
1. Confirm provider failure category (auth, rate limit, timeout, 5xx).
2. Keep API responsive with graceful error response.
3. Notify operations channel.
4. Re-verify provider health before closing incident.

## 5. Audit requirements

Implemented:
1. Model version lineage (active/previous).
2. Validation metric persistence.
3. Dataset snapshot ID persistence.
4. Training commit and config hash persistence.

Pending:
1. Centralized audit sink/dashboard for governance review.
2. Signed promotion records with assigned owners.

## 6. Cadence

Practical cadence for this project:
1. Daily: check ai-readiness endpoint and critical logs.
2. Weekly: review drift and retrain statuses.
3. Monthly: review thresholds and blocked-status frequency.
4. Quarterly: run incident drill for one blocked-gate scenario and one Gemini outage scenario.

## 7. Success criteria for this plan

This governance plan is considered fully operational when all are true:
1. Readiness and model endpoints are continuously monitored.
2. Alert routing is connected to on-call channel.
3. Blocked-gate events open and close tracked incidents.
4. Owners are formally assigned for Product, AI, Platform, Security, Operations.
