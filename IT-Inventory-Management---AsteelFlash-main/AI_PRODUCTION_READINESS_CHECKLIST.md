# AI Production Readiness Checklist (Project-Specific)

Date: 2026-06-08
Project: IT Stock Management (Asteelflash)

## Scope kept for this project
- Included:
1. Hardware recommendation model (ML.NET, retrained)
2. Ticket prioritization model (ML.NET, retrained)
3. Asset prediction service (heuristic score, no model promotion lifecycle)
4. Gemini chat integration (external provider reliability/security controls)

- Removed as not needed for current architecture:
1. Champion/challenger deployment
2. Canary traffic shifting for model endpoints
3. Fairness cohort policy requiring protected-attribute datasets (not available in this project)

## Status legend
- Done: implemented and verified in code/tests
- In Progress: partially implemented, operationalization still needed
- Blocked: pending org/process decision outside code

## 1. Runtime Acceptance Gates

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| Hardware min validation gate | Done | Prod >= 0.70, Dev >= 0.60 | `AiModels:Retraining:MinimumHardwareValidationMetric` |
| Ticket min validation gate | Done | Prod >= 0.70, Dev >= 0.60 | `AiModels:Retraining:MinimumTicketValidationMetric` |
| Activation tolerance gate | Done | Prod 0.015, Dev 0.03 | `ActivationMetricTolerance` |
| Retrain blocked status emitted | Done | `blocked_acceptance_*` and `blocked_data_quality_*` | retraining services + background logs |

## 2. Data Quality Gates

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| Hardware label balance | Done | Positive rate in [0.10, 0.90] | `MinimumHardwarePositiveRate`, `MaximumHardwarePositiveRate` |
| Ticket label diversity | Done | Distinct labels >= 3 | `MinimumTicketDistinctLabels` |
| Ticket missing-text ratio | Done | Prod <= 0.05, Dev <= 0.10 | `MaximumTicketMissingTextRate` |
| Retrain blocked on quality failure | Done | `blocked_data_quality_*` | retraining services |

## 3. Reproducibility and Traceability

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| Dataset snapshot ID persisted | Done | SHA-256 snapshot hash in manifest | both model manifests |
| Training commit persisted | Done | `GIT_COMMIT_SHA`/`SOURCE_VERSION`/`BUILD_SOURCEVERSION` fallback | both model manifests |
| Config hash persisted | Done | SHA-256 hash of active retrain config | both model manifests |
| Active/previous versions persisted | Done | versioned zip + manifest rollover | existing phase 3 model storage |
| Feature pipeline version tag | In Progress | Explicit semantic pipeline version not yet added | pending next code slice |

## 4. Monitoring and Health

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| AI models health endpoint | Done | `/api/v1/health/ai-models` | `HealthController` |
| AI readiness endpoint | Done | `/api/v1/health/ai-readiness` | `HealthController` + tests |
| Drift thresholding from config | Done | Medium/High from appsettings | `DriftMediumThreshold`, `DriftHighThreshold` |
| Background warning logs on blocked retrain | Done | Warning log for quality/acceptance blocks | `AiModelRetrainingBackgroundService` |
| Alert routing to pager/email | In Progress | Not yet wired to alert manager | operational task |

## 5. Tests and CI Gates

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| Health controller model metrics test | Done | passes | `HealthControllerIntegrationTests` |
| Health controller readiness test | Done | passes | `HealthControllerIntegrationTests` |
| Full AI regression suite in CI | In Progress | not yet mandatory pipeline gate | CI workflow update pending |
| Load/failure injection tests | In Progress | not implemented yet | pending performance/chaos stage |

## 6. Security and Compliance (for this scope)

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| No hardcoded Gemini key in appsettings | Done | empty key in config | appsettings files |
| Model artifact retention policy | Done | `MaxModelVersionsToKeep` | retraining options + cleanup |
| Provider error handling (Gemini) | Done | structured handling for auth/rate limit/network | `GeminiChatController` + `GeminiChatService` |
| PII retention/handling policy doc | In Progress | technical controls exist, formal policy pending | ops/compliance document step |

## 7. Operations Readiness (practical)

| Control | Status | Value / Rule | Evidence |
|---|---|---|---|
| Model degradation playbook | Done | documented | `AI_GOVERNANCE_MONITORING_PLAN.md` |
| Retrain failure playbook | Done | documented | `AI_GOVERNANCE_MONITORING_PLAN.md` |
| Gemini outage playbook | Done | documented | `AI_GOVERNANCE_MONITORING_PLAN.md` |
| RACI ownership finalized | Blocked | needs business owner assignment | org decision |
| On-call escalation policy finalized | Blocked | needs operations signoff | org decision |

## 8. Production Decision (current)

- ML gate implementation status: Done
- Runtime readiness observability: Done
- Reproducibility metadata: Done (except explicit pipeline semantic version)
- Process/governance completion: In Progress

### Decision
- APPROVED FOR TECHNICAL STAGING: Yes
- APPROVED FOR STRICT PRODUCTION GOVERNANCE: Not yet

### Remaining required items before strict signoff
1. Finalize owners (Product, AI/ML, Ops, Security) and RACI.
2. Add CI pipeline hard gate for readiness tests and threshold checks.
3. Add explicit `FeaturePipelineVersion` to manifests.
4. Wire readiness warnings to alerting channel (email/Teams/Pager).


