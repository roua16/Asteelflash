# AI Phase 3 Operations Runbook

## Purpose

This runbook explains how to operate, tune, and verify the AI model lifecycle features added in Phase 3.

## Production Readiness Documents

For strict production-governance completion, use these companion documents:
1. [AI_PRODUCTION_READINESS_CHECKLIST.md](AI_PRODUCTION_READINESS_CHECKLIST.md)
2. [AI_MODEL_ACCEPTANCE_CRITERIA.md](AI_MODEL_ACCEPTANCE_CRITERIA.md)
3. [AI_GOVERNANCE_MONITORING_PLAN.md](AI_GOVERNANCE_MONITORING_PLAN.md)

## Runtime Controls

Configuration section:
- AiModels:Retraining

Primary options:
1. Enabled
2. InitialDelayMinutes
3. IntervalMinutes
4. MinimumHardwareTrainingSamples
5. MinimumTicketTrainingSamples
6. ActivationMetricTolerance
7. MaxModelVersionsToKeep
8. DriftMediumThreshold
9. DriftHighThreshold

## Current Tuned Values

### Production profile

File:
- src/ITStockM.WebApi/appsettings.json

Configured values:
1. Enabled: true
2. InitialDelayMinutes: 15
3. IntervalMinutes: 240
4. MinimumHardwareTrainingSamples: 20
5. MinimumTicketTrainingSamples: 40
6. ActivationMetricTolerance: 0.015
7. MaxModelVersionsToKeep: 10
8. DriftMediumThreshold: 35
9. DriftHighThreshold: 60

### Development profile

File:
- src/ITStockM.WebApi/appsettings.Development.json

Configured values:
1. Enabled: true
2. InitialDelayMinutes: 2
3. IntervalMinutes: 120
4. MinimumHardwareTrainingSamples: 8
5. MinimumTicketTrainingSamples: 16
6. ActivationMetricTolerance: 0.03
7. MaxModelVersionsToKeep: 6
8. DriftMediumThreshold: 35
9. DriftHighThreshold: 60

## Retraining Scheduler

Service:
- src/ITStockM.Infrastructure/Services/AiModelRetrainingBackgroundService.cs

Registration:
- src/ITStockM.Infrastructure/DependencyInjection/InfrastructureLayerServiceCollectionExtensions.cs

Scheduler behavior:
1. Waits initial delay.
2. Executes retraining cycle for hardware and ticket services.
3. Repeats on configured interval.
4. Logs completion or failure.

## Model Versioning and Rollover

Services:
- src/ITStockM.Infrastructure/Services/HardwareRecommendationService.cs
- src/ITStockM.Infrastructure/Services/TicketPrioritizationService.cs

Operational behavior:
1. New model artifact is saved with a versioned timestamped filename.
2. Manifest stores active and previous version metadata.
3. Candidate activation uses validation metric tolerance.
4. If candidate is below tolerance, service keeps previous model (rollover policy).
5. If active artifact is missing on load, service attempts previous model path fallback.

## Retention Cleanup Policy

Policy is controlled by MaxModelVersionsToKeep.

How cleanup works:
1. Always protect active model artifact.
2. Always protect previous model artifact.
3. Keep newest N model artifacts.
4. Delete older zip artifacts best-effort.

Important:
- Cleanup failures do not block predictions or service startup.

## Drift Indicators

Drift is computed in both AI services and exposed in status output.

Levels are controlled by:
1. DriftMediumThreshold
2. DriftHighThreshold

Fleet drift level is derived from the maximum drift score across models.

## Observability Endpoint

Controller:
- src/ITStockM.WebApi/Controllers/HealthController.cs

Endpoint:
- GET /api/v1/health/ai-models

Use this endpoint to monitor:
1. Last training timestamp.
2. Active and previous versions.
3. Validation metric.
4. Drift score and drift level.
5. Last retraining status.
6. Fleet drift summary.

## Test and Verification

Test file:
- src/ITStockM.Tests/Api/HealthControllerIntegrationTests.cs

Focused command used:
- dotnet test src/ITStockM.Tests/ITStockM.Tests.csproj --filter "FullyQualifiedName~HealthControllerIntegrationTests|FullyQualifiedName~HardwareRecommendationServiceTests|FullyQualifiedName~TicketPrioritizationServiceTests"

Last observed result:
1. Total: 4
2. Passed: 4
3. Failed: 0

## Recommended Production Cadence Review

Revisit monthly:
1. IntervalMinutes based on ticket/request volume and drift trends.
2. Minimum sample thresholds based on data quality.
3. ActivationMetricTolerance based on regression sensitivity.
4. MaxModelVersionsToKeep based on storage policy and audit requirements.

## Fast Rollback Procedure

If model quality degrades:
1. Disable scheduler by setting Enabled to false.
2. Keep service running with current active/previous manifest state.
3. Inspect ai-models endpoint for retrain status and drift spikes.
4. Lower tolerance conservatively only after data diagnostics.
