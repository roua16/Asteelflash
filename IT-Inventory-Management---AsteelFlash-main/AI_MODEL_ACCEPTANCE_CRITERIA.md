# AI Model Acceptance Criteria (Project-Specific)

Date: 2026-06-08
Applies to: hardware recommendation and ticket prioritization retraining in this repository

## Scope and exclusions

Included in this policy:
1. Offline acceptance and quality gates executed during retraining.
2. Candidate-versus-active comparison with tolerance.
3. Reproducibility metadata required in model manifests.

Excluded for current architecture:
1. Canary traffic rollout policy.
2. Champion/challenger online split policy.
3. Protected-attribute fairness cohort policy (data unavailable).

## 1. Runtime acceptance gates (implemented)

| Model | Environment | Minimum validation metric | Tolerance vs active |
|---|---|---|---|
| Hardware recommendation | Production | 0.70 | 0.015 |
| Hardware recommendation | Development | 0.60 | 0.03 |
| Ticket prioritization | Production | 0.70 | 0.015 |
| Ticket prioritization | Development | 0.60 | 0.03 |

Acceptance rule:
1. Candidate metric must be above minimum threshold.
2. Candidate metric must not be lower than active metric by more than tolerance.
3. Candidate must pass model-specific data quality gates.

## 2. Data quality gates (implemented)

### Hardware recommendation
1. Training samples >= configured minimum.
2. Positive label ratio must stay in [0.10, 0.90].
3. If failed, retraining status must be `blocked_data_quality_label_imbalance`.

### Ticket prioritization
1. Training samples >= configured minimum.
2. Distinct labels >= 3.
3. Missing text rate <= 0.05 in production and <= 0.10 in development.
4. If failed, retraining status must be `blocked_data_quality_low_label_diversity` or `blocked_data_quality_missing_text`.

## 3. Rejection conditions (strict)

Reject candidate immediately when any condition is true:
1. Validation metric below minimum threshold.
2. Data quality gate failure.
3. Reproducibility metadata missing in manifest.
4. Model artifact write failed.

Required blocked statuses:
1. `blocked_acceptance_min_metric`
2. `blocked_data_quality_label_imbalance`
3. `blocked_data_quality_low_label_diversity`
4. `blocked_data_quality_missing_text`

## 4. Required evidence artifacts per accepted model

Each accepted version must include:
1. Active model version ID.
2. Previous model version ID.
3. Validation metric used for acceptance.
4. Dataset snapshot ID.
5. Training commit identifier.
6. Config hash.
7. Last retrain status.

## 5. Operational acceptance check API

Operational acceptance is verified through:
1. `GET /api/v1/health/ai-models`
2. `GET /api/v1/health/ai-readiness`

Readiness rule:
1. `ReadyForProduction=true` only when each tracked model is operational (`IsOperational=true`) and not blocked by quality or acceptance gates.

## 6. Promotion record template

| Field | Value |
|---|---|
| Model | hardware-recommendation / ticket-prioritization |
| Candidate version | |
| Active version before promotion | |
| Validation metric | |
| Minimum threshold | |
| Dataset snapshot ID | |
| Training commit | |
| Config hash | |
| Decision | Approve / Reject |
| Reviewer(s) | |
| Timestamp (UTC) | |
| Notes | |
