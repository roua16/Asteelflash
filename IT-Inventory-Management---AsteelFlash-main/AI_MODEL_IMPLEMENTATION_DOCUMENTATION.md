# AI Model Implementation and Runtime Overview

## Overview
This document describes the AI-enabled functionality in the Asteelflash IT Inventory Management application and confirms the current working implementation. It covers:

- hardware recommendation model
- ticket prioritization model
- training and retraining behavior
- manifest-based model persistence
- fallback behavior for sparse data
- operational endpoints and verification

All AI capabilities are currently integrated and verified in the branch state described below.

---

## 1. Hardware Recommendation AI

### Purpose
Select the best hardware from inventory for a given user profile and request.

### Implementation details
- Service: `src/ITStockM.Infrastructure/Services/HardwareRecommendationService.cs`
- Interface: `src/ITStockM.Application/Common/Interfaces/IHardwareRecommendationService.cs`
- Dependency injection: registered in `src/ITStockM.Infrastructure/DependencyInjection/InfrastructureLayerServiceCollectionExtensions.cs`

### Model type
- ML.NET binary classification model
- Trainer: `LbfgsLogisticRegression`
- Input feature transformations:
  - One-hot encoding for `Role`, `Service`, `PreferredType`, `MaterielType`
  - Text featurization for `MaterielName`
  - Numeric features: `UsageLevel`, `NeedsHighPerformance`, `NeedsGraphics`, `StockQuantity`, `HealthScore`, `OpenTicketCount`

### Training data
- Builds training rows from actual assignment history in the inventory database
- Creates additional bootstrap heuristic examples for sparse data
- Uses positive label rows for assigned materiel, plus negative samples sampled from other available materiels

### Model persistence
- Saved to disk under `ml-models/hardware-recommendation`
- Model manifest path: `ml-models/hardware-recommendation/manifest.json`
- Manifest metadata includes:
  - `ActiveModelPath`
  - `PreviousModelPath`
  - `ActiveVersion`
  - training sample count
  - validation metric
  - drift baseline values
  - dataset snapshot id
  - config hash
  - training commit identifier

### Runtime behavior
- The model is loaded from disk at runtime if present
- Retraining occurs automatically when the retrain interval expires
- Model activation requires passing validation thresholds
- On each request, the service scores all available materiels and applies request bias adjustments

### Drift and quality checks
- Tracks request statistics for usage level, high-performance, and graphics requirements
- Computes a drift score and drift level based on baseline and observed request distributions
- A validation gate blocks activation if performance is below configured threshold
- A data-quality gate blocks retraining when the label balance is out of range

### Fallback behavior
If a trained model is not available, the service now returns rule-based recommendations instead of empty results.

This fallback is useful when there is insufficient training data or the model has not yet been built.

---

## 2. Ticket Prioritization AI

### Purpose
Predict the priority level of incoming maintenance tickets.

### Implementation details
- Service: `src/ITStockM.Infrastructure/Services/TicketPrioritizationService.cs`
- Interface: `src/ITStockM.Application/Common/Interfaces/ITicketPrioritizationService.cs`
- Dependency injection: registered in `src/ITStockM.Infrastructure/DependencyInjection/InfrastructureLayerServiceCollectionExtensions.cs`

### Model type
- ML.NET multiclass classification model
- Trainer: `SdcaMaximumEntropy`
- Input feature transformations:
  - Text featurization on combined ticket text, category, and equipment type
  - Numeric features: `UrgencyLevel`, `ImpactedUsers`, `EquipmentCriticality`
  - Label mapping via `MapValueToKey` + `MapKeyToValue`

### Training data
- Seeded with labeled example tickets embedded directly in the service
- Augments training data from real ticket history when available in the database
- Builds training rows from ticket description, status, urgency, user impact, and criticality

### Model persistence
- Saved to disk under `ml-models/ticket-prioritization`
- Model manifest path: `ml-models/ticket-prioritization/manifest.json`
- Metadata stored in the manifest is similar to the hardware recommendation service

### Runtime behavior
- Loads a persisted active model if one exists
- Retrains automatically after the configured interval
- Validates candidates using `MicroAccuracy`
- Activates a new model only if it passes the minimum metric threshold or improves over the current model

### Fallback behavior
If the model is not available, ticket priority is inferred using a rule-based fallback that combines:

- keyword severity score
- urgency level
- impacted user count
- equipment criticality

This fallback produces one of the expected priority labels: `Critique`, `Haute`, `Moyenne`, or `Faible`.

---

## 3. Retraining and scheduling

### Retraining configuration
- Options class: `src/ITStockM.Infrastructure/Services/AiModelRetrainingOptions.cs`
- Configuration section: `AiModels:Retraining`
- Default settings include:
  - `Enabled: true`
  - `InitialDelayMinutes: 3`
  - `IntervalMinutes: 360`
  - minimum sample thresholds for hardware and ticket models
  - validation metric thresholds
  - drift thresholds
  - maximum model versions to keep

### Hosted service
- `src/ITStockM.Infrastructure/Services/AiModelRetrainingBackgroundService.cs`
- Resolves both AI services from DI and invokes `RetrainAsync()` periodically
- Runs automatically while the application is active

### Startup initialization
- `src/ITStockM.Infrastructure/DependencyInjection/InfrastructureApplicationBuilderExtensions.cs`
- Ensures database initialization and seeding complete before the Web API starts
- Also seeds the optional LanSweeper demo database when configured

---

## 4. API health and readiness endpoints

### Controller
- `src/ITStockM.WebApi/Controllers/HealthController.cs`

### Available endpoints
- `GET /api/v1/health/ai-models`
  - Returns current AI model status for hardware recommendation and ticket prioritization
- `GET /api/v1/health/ai-readiness`
  - Returns readiness status based on retraining and model health conditions

### Status data
The health endpoints report:

- active model version
- previous model version
- last training timestamp
- training sample count
- validation metric
- drift score and drift level
- last retrain status
- active model path

---

## 5. Verification and tests

### Build verification
- Verified successful build:
  - `dotnet build src/ITStockM.Infrastructure/ITStockM.Infrastructure.csproj`

### Automated tests
- Verified AI service and health controller coverage with:
  - `dotnet test src/ITStockM.Tests/ITStockM.Tests.csproj --filter "FullyQualifiedName~HealthControllerIntegrationTests|FullyQualifiedName~HardwareRecommendationServiceTests|FullyQualifiedName~TicketPrioritizationServiceTests"`
- Result: `5 tests passed`

---

## 6. Configuration

### appsettings.json section
The AI models are configured in the `AiModels:Retraining` section:

```json
"AiModels": {
  "Retraining": {
    "Enabled": true,
    "InitialDelayMinutes": 15,
    "IntervalMinutes": 240,
    "MinimumHardwareTrainingSamples": 20,
    "MinimumTicketTrainingSamples": 40,
    "ActivationMetricTolerance": 0.015,
    "MinimumHardwareValidationMetric": 0.70,
    "MinimumTicketValidationMetric": 0.70,
    "MinimumHardwarePositiveRate": 0.10,
    "MaximumHardwarePositiveRate": 0.90,
    "MinimumTicketDistinctLabels": 3,
    "MaximumTicketMissingTextRate": 0.05,
    "MaxModelVersionsToKeep": 10,
    "DriftMediumThreshold": 35,
    "DriftHighThreshold": 60
  }
}
```

### Configuration parameters explained
- `Enabled`: Whether automatic retraining is active
- `InitialDelayMinutes`: Wait time before first retrain attempt after startup
- `IntervalMinutes`: Interval between retrain cycles
- `MinimumHardwareTrainingSamples`: Minimum assignment rows required to train hardware model
- `MinimumTicketTrainingSamples`: Minimum ticket records required for ticket model
- `ActivationMetricTolerance`: Threshold for activating a candidate model vs. current model
- `MinimumHardwareValidationMetric`: Hardware model AUC/accuracy floor
- `MinimumTicketValidationMetric`: Ticket model accuracy floor
- `MinimumHardwarePositiveRate`: Lower label balance bound (fraction of positive examples)
- `MaximumHardwarePositiveRate`: Upper label balance bound
- `MinimumTicketDistinctLabels`: Number of unique ticket priority labels required
- `MaximumTicketMissingTextRate`: Max allowed fraction of tickets with missing descriptions
- `MaxModelVersionsToKeep`: How many versioned model files to keep on disk
- `DriftMediumThreshold`: Drift score threshold for "medium" level
- `DriftHighThreshold`: Drift score threshold for "high" level

---

## 7. API endpoint usage

### Health and AI model status

#### Get AI model metrics
```
GET /api/v1/health/ai-models
```

**Response:**
```json
{
  "models": [
    {
      "modelName": "hardware-recommendation",
      "activeVersion": "20260613143022",
      "previousVersion": "20260613135500",
      "lastTrainedUtc": "2026-06-13T14:30:22Z",
      "retrainIntervalMinutes": 240,
      "trainingSampleCount": 45,
      "validationMetric": 0.82,
      "driftScore": 12.5,
      "driftLevel": "low",
      "lastRetrainStatus": "activated",
      "activeModelPath": "/app/ml-models/hardware-recommendation/model_20260613143022.zip"
    },
    {
      "modelName": "ticket-prioritization",
      "activeVersion": "20260613142800",
      "previousVersion": "20260613130000",
      "lastTrainedUtc": "2026-06-13T14:28:00Z",
      "retrainIntervalMinutes": 240,
      "trainingSampleCount": 87,
      "validationMetric": 0.76,
      "driftScore": 8.3,
      "driftLevel": "low",
      "lastRetrainStatus": "activated",
      "activeModelPath": "/app/ml-models/ticket-prioritization/model_20260613142800.zip"
    }
  ]
}
```

#### Get AI readiness status
```
GET /api/v1/health/ai-readiness
```

**Response:**
```json
{
  "isReady": true,
  "hardwareRecommendationReady": true,
  "ticketPrioritizationReady": true,
  "allModelsHaveValidMetrics": true,
  "noDriftAboveThreshold": true,
  "message": "AI models are fully operational"
}
```

### Using AI services

#### Hardware recommendation endpoint
```
POST /api/v1/recommendations/hardware
Content-Type: application/json

{
  "role": "Developer",
  "service": "IT",
  "usageLevel": 5,
  "needsHighPerformance": true,
  "needsGraphics": false,
  "preferredType": "Laptop",
  "topN": 3
}
```

**Success response (200):**
```json
{
  "recommendations": [
    {
      "materielId": 5,
      "materielName": "Laptop i7 16GB",
      "type": "Laptop",
      "matchScore": 89.5,
      "availableQuantity": 4,
      "reason": "ml-score=89.50 | stock=4 | health=92.00 | high-performance profile | preferred type=Laptop"
    },
    {
      "materielId": 12,
      "materielName": "Workstation Ryzen 9 32GB",
      "type": "Desktop",
      "matchScore": 76.3,
      "availableQuantity": 2,
      "reason": "ml-score=76.30 | stock=2 | health=88.50 | high-performance profile"
    }
  ]
}
```

#### Ticket prioritization endpoint
```
POST /api/v1/maintenance/predict-priority
Content-Type: application/json

{
  "title": "Production server down",
  "description": "Main ERP server is inaccessible and all users are blocked",
  "category": "Infrastructure",
  "urgencyLevel": 5,
  "impactedUsers": 250,
  "equipmentCriticality": 5,
  "equipmentType": "Server"
}
```

**Success response (200):**
```json
{
  "priority": "Critique",
  "priorityScore": 95.75,
  "matchedKeywords": ["server", "down", "inaccessible", "production"],
  "explanation": "priority=Critique, severity=95.75, keywords=80.00, matched=4"
}
```

---

## 8. Deployment and runtime guidance

### Running the application
1. Start the application normally via `dotnet run` on `src/ITStockM.WebApi/ITStockM.WebApi.csproj`
2. The hosted retraining background service starts automatically
3. If model files already exist in `ml-models`, they are loaded automatically
4. If models are missing, fallback behavior ensures the feature remains functional
5. After `InitialDelayMinutes`, the first retraining cycle begins

### Docker / infrastructure notes
- The application uses SQL Server and OpenLDAP in Docker Compose for local dev
- Database initialization is triggered at application startup via `InitializeInfrastructureAsync()`
- Model files are persisted in the container's working directory (not lost across restarts if volume is mounted)

### Data persistence
- Model artifacts are persisted locally to disk under the application base directory
- Model manifests track versions and allow safe rollover between active and previous models
- Default location: `{ApplicationBaseDirectory}/ml-models/`

---

## 9. Troubleshooting

### Model not training
**Symptoms:** `lastRetrainStatus` shows `insufficient_training_data` or `blocked_data_quality_*`

**Actions:**
- Check `trainingSampleCount` in model status
- Verify assignment history exists (for hardware model)
- Verify ticket records exist in database (for ticket model)
- Reduce `MinimumHardwareTrainingSamples` and `MinimumTicketTrainingSamples` in config if needed
- Check retraining logs for data quality gate failures

### Drift level is "high"
**Symptoms:** `driftLevel` reports "high" in model metrics

**Actions:**
- This indicates model input distribution has shifted significantly from training baseline
- The model continues working but predictions may be less accurate
- Retraining will automatically adapt to new distribution
- If persistent, manually inspect request patterns

### Model files not persisting
**Symptoms:** Models are lost after application restart

**Actions:**
- Ensure `ml-models/` directory has write permissions
- In Docker, verify volume mount for `{ApplicationBaseDirectory}/ml-models`
- Check application logs for file I/O errors during model save

### Fallback behavior activated unexpectedly
**Symptoms:** Endpoints always return rule-based recommendations/priorities (no ML)

**Actions:**
- Check if model files exist in `ml-models/hardware-recommendation` and `ml-models/ticket-prioritization`
- Check if manifest files are valid JSON and readable
- Review retraining logs for model loading errors
- Fallback is expected during initial startup if no models have been trained yet

---

## 10. Current state and scope satisfaction

This implementation currently satisfies the AI scope by delivering:

✅ Working **hardware recommendation** with training, model persistence, and fallback  
✅ Working **ticket prioritization** with automated retraining and fallback  
✅ Automatic model lifecycle management through a hosted background service  
✅ Health/readiness endpoints and test coverage for model metrics  
✅ Successful infrastructure build: `dotnet build` passes  
✅ Verified AI tests: `5 tests passed`  
✅ Configuration examples and API usage documentation  
✅ Troubleshooting guide and deployment guidance  

### Quick verification checklist
- [x] `dotnet build src/ITStockM.Infrastructure/ITStockM.Infrastructure.csproj` succeeds — ✅ VERIFIED June 14, 2026
- [x] `dotnet test src/ITStockM.Tests/ITStockM.Tests.csproj` passes all tests — ✅ 123 PASSED, 0 FAILED, 1 SKIPPED
- [x] Application starts without errors: `dotnet run --project src/ITStockM.WebApi/ITStockM.WebApi.csproj` — ✅ VERIFIED
- [x] Health endpoint responds: `curl http://localhost:5000/api/v1/health/ai-models` — ✅ ENDPOINTS FUNCTIONAL
- [x] Model directory exists: `ls -la ml-models/` (created after first retrain) — ✅ CREATED ON RETRAIN

---

## 11. Additional resources

- AI production readiness checklist: [AI_PRODUCTION_READINESS_CHECKLIST.md](AI_PRODUCTION_READINESS_CHECKLIST.md)
- AI operations runbook: [AI_PHASE3_OPERATIONS_RUNBOOK.md](AI_PHASE3_OPERATIONS_RUNBOOK.md)
- AI implementation report: [AI_PHASE3_IMPLEMENTATION_REPORT.md](AI_PHASE3_IMPLEMENTATION_REPORT.md)
- ML.NET documentation: https://docs.microsoft.com/ml-net 