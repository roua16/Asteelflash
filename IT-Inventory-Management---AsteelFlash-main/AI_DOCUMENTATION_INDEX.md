# AI & Machine Learning Documentation Index

**Complete reference for the ITStockM AI implementation (v7)**

---

## 📋 Documentation Map

### For Quick Reference
- **Start here:** [AI_COMPLETION_SUMMARY.md](AI_COMPLETION_SUMMARY.md) — Status, verification results, scope completion
- **For operators:** [AI_PHASE3_OPERATIONS_RUNBOOK.md](AI_PHASE3_OPERATIONS_RUNBOOK.md) — Day-to-day operations
- **For developers:** [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md) — API usage, configuration, troubleshooting

### For Deployments
- **Pre-flight checks:** [AI_PRODUCTION_READINESS_CHECKLIST.md](AI_PRODUCTION_READINESS_CHECKLIST.md)
- **Sign-off documentation:** [AI_PRODUCTION_SIGNOFF.md](AI_PRODUCTION_SIGNOFF.md)

### For Deep Dives
- **Technical implementation:** [AI_PHASE3_IMPLEMENTATION_REPORT.md](AI_PHASE3_IMPLEMENTATION_REPORT.md)
- **Acceptance criteria:** [AI_MODEL_ACCEPTANCE_CRITERIA.md](AI_MODEL_ACCEPTANCE_CRITERIA.md)
- **Governance & monitoring:** [AI_GOVERNANCE_MONITORING_PLAN.md](AI_GOVERNANCE_MONITORING_PLAN.md)

---

## 🎯 Use This Document For

| Need | Document | Time |
|------|----------|------|
| "What's the status?" | [AI_COMPLETION_SUMMARY.md](AI_COMPLETION_SUMMARY.md) | 5 min |
| "How do I run this?" | [AI_PHASE3_OPERATIONS_RUNBOOK.md](AI_PHASE3_OPERATIONS_RUNBOOK.md) | 10 min |
| "How do I call the API?" | [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md#7-api-endpoint-usage) | 10 min |
| "How do I debug an issue?" | [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md#9-troubleshooting) | 15 min |
| "Is this ready for production?" | [AI_PRODUCTION_READINESS_CHECKLIST.md](AI_PRODUCTION_READINESS_CHECKLIST.md) | 20 min |
| "I need the full technical story" | [AI_PHASE3_IMPLEMENTATION_REPORT.md](AI_PHASE3_IMPLEMENTATION_REPORT.md) | 30 min |
| "What are the acceptance criteria?" | [AI_MODEL_ACCEPTANCE_CRITERIA.md](AI_MODEL_ACCEPTANCE_CRITERIA.md) | 20 min |
| "How should we govern this?" | [AI_GOVERNANCE_MONITORING_PLAN.md](AI_GOVERNANCE_MONITORING_PLAN.md) | 25 min |

---

## ✅ Verification Status

### Build & Tests
- ✅ **Build:** `dotnet build ITStockM.sln` - SUCCESSFUL
- ✅ **AI Tests:** 5/5 passing (HealthController, HardwareRecommendation, TicketPrioritization)
- ✅ **Integration:** All dependencies properly wired in DI container

### APIs
- ✅ `GET /api/v1/health/ai-models` - Model metrics and status
- ✅ `GET /api/v1/health/ai-readiness` - Overall AI readiness
- ✅ `POST /api/v1/recommendations/hardware` - Hardware recommendation service
- ✅ `POST /api/v1/maintenance/predict-priority` - Ticket prioritization service

### Infrastructure
- ✅ Database initialization with identity seeding
- ✅ LDAP/AD integration
- ✅ Docker Compose stack (SQL Server, LDAP, CloudBeaver, smtp4dev)
- ✅ Model persistence and versioning

### Documentation
- ✅ 8 comprehensive markdown documents
- ✅ Code examples and API usage
- ✅ Configuration reference
- ✅ Troubleshooting guide
- ✅ Operations runbook
- ✅ Production checklist

---

## 🚀 Quick Start

### Local Development
```bash
# Build
dotnet build ITStockM.sln

# Start infrastructure
docker-compose up -d

# Run application
dotnet run --project src/ITStockM.WebApi/ITStockM.WebApi.csproj

# Check AI status
curl http://localhost:5000/api/v1/health/ai-models
```

### Test AI Features
```bash
# Hardware recommendation
curl -X POST http://localhost:5000/api/v1/recommendations/hardware \
  -H "Content-Type: application/json" \
  -d '{"role": "Developer", "usageLevel": 5, "needsHighPerformance": true}'

# Ticket prioritization
curl -X POST http://localhost:5000/api/v1/maintenance/predict-priority \
  -H "Content-Type: application/json" \
  -d '{"title": "Server down", "urgencyLevel": 5, "impactedUsers": 100}'
```

---

## 📚 Implementation Overview

### Hardware Recommendation Model
- **Type:** Binary classification (ML.NET LbfgsLogisticRegression)
- **Inputs:** Role, service, usage level, performance needs, stock status, health score
- **Output:** Top N ranked hardware recommendations with match scores
- **Retraining:** Automatic when new assignment data available
- **Fallback:** Rule-based recommendations when ML unavailable

**See:** [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md § Hardware Recommendation](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md)

### Ticket Prioritization Model
- **Type:** 4-class classification (ML.NET SdcaMaximumEntropy)
- **Classes:** Critique (critical), Haute (high), Moyenne (medium), Faible (low)
- **Inputs:** Title, description, category, urgency level, impacted users, equipment criticality
- **Output:** Priority classification with confidence score
- **Training:** 15 seeded examples + real ticket history
- **Fallback:** Keyword-based severity scoring + numeric features

**See:** [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md § Ticket Prioritization](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md)

### Model Lifecycle Management
- **Retraining:** Hosted background service on configurable intervals
- **Persistence:** JSON manifest versioning under `ml-models/`
- **Drift Tracking:** Input distribution monitoring with drift scoring
- **Quality Gates:** Validation metrics, sample counts, label balance checks

**See:** [AI_PHASE3_IMPLEMENTATION_REPORT.md § Model Management](AI_PHASE3_IMPLEMENTATION_REPORT.md)

---

## 🔧 Configuration

Both production and development configurations are present:

| Setting | Dev | Production | Purpose |
|---------|-----|-----------|---------|
| `Enabled` | true | true | Enable/disable retraining |
| `InitialDelayMinutes` | 2 | 15 | Startup delay before first retrain |
| `IntervalMinutes` | 120 | 240 | Retraining frequency |
| `MinimumHardwareTrainingSamples` | 8 | 20 | Minimum data for hardware model |
| `MinimumTicketTrainingSamples` | 16 | 40 | Minimum data for ticket model |
| `MinimumHardwareValidationMetric` | 0.60 | 0.70 | Hardware model quality floor |
| `MinimumTicketValidationMetric` | 0.60 | 0.70 | Ticket model quality floor |

**See:** [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md § Configuration](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md#6-configuration)

---

## 🐛 Troubleshooting

### Common Issues

**Q: Models not training**  
A: Check training data availability in database. See [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md § Troubleshooting](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md#9-troubleshooting)

**Q: Health endpoints returning errors**  
A: Verify models exist in `ml-models/` directory. Check application logs for initialization errors.

**Q: Fallback behavior always active**  
A: Model files missing or corrupt. Run manual retrain or delete manifest to reset.

**Q: Drift level "high"**  
A: Input distribution has shifted. Normal behavior — model will adapt on next retrain.

**Full guide:** [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md § Troubleshooting](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md#9-troubleshooting)

---

## 📞 Support Matrix

| Role | Document | Primary Resource |
|------|----------|------------------|
| **Developer** | API usage, configuration | [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md) |
| **DevOps/SRE** | Operations, monitoring, deployment | [AI_PHASE3_OPERATIONS_RUNBOOK.md](AI_PHASE3_OPERATIONS_RUNBOOK.md) + [AI_PRODUCTION_READINESS_CHECKLIST.md](AI_PRODUCTION_READINESS_CHECKLIST.md) |
| **QA/Tester** | Acceptance criteria, test strategy | [AI_MODEL_ACCEPTANCE_CRITERIA.md](AI_MODEL_ACCEPTANCE_CRITERIA.md) |
| **Architecture** | Technical design, governance | [AI_PHASE3_IMPLEMENTATION_REPORT.md](AI_PHASE3_IMPLEMENTATION_REPORT.md) + [AI_GOVERNANCE_MONITORING_PLAN.md](AI_GOVERNANCE_MONITORING_PLAN.md) |
| **Project Lead** | Status, completion, sign-off | [AI_COMPLETION_SUMMARY.md](AI_COMPLETION_SUMMARY.md) + [AI_PRODUCTION_SIGNOFF.md](AI_PRODUCTION_SIGNOFF.md) |

---

## 🎓 Learning Path

### If you're new to the AI system (Start here)
1. Read: [AI_COMPLETION_SUMMARY.md](AI_COMPLETION_SUMMARY.md) (5 min)
2. Review: [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md) (20 min)
3. Try: Run the API examples (10 min)
4. Reference: Return to specific docs as needed

### If you're deploying to production
1. Review: [AI_PRODUCTION_READINESS_CHECKLIST.md](AI_PRODUCTION_READINESS_CHECKLIST.md) (20 min)
2. Configure: Update appsettings.json for production thresholds (10 min)
3. Verify: Run checklist validation steps (15 min)
4. Sign-off: [AI_PRODUCTION_SIGNOFF.md](AI_PRODUCTION_SIGNOFF.md) (5 min)

### If you're operating the system
1. Start with: [AI_PHASE3_OPERATIONS_RUNBOOK.md](AI_PHASE3_OPERATIONS_RUNBOOK.md) (10 min)
2. Reference: [AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md § Troubleshooting](AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md#9-troubleshooting) (as needed)
3. Monitor: `/api/v1/health/ai-models` endpoint (ongoing)

---

## 📄 File Manifest

```
AI_COMPLETION_SUMMARY.md                    ← Status & verification
AI_MODEL_IMPLEMENTATION_DOCUMENTATION.md    ← Technical reference (MOST COMPREHENSIVE)
AI_PHASE3_OPERATIONS_RUNBOOK.md             ← Operational procedures
AI_PHASE3_IMPLEMENTATION_REPORT.md          ← Technical deep dive
AI_PRODUCTION_READINESS_CHECKLIST.md        ← Pre-production verification
AI_PRODUCTION_SIGNOFF.md                    ← Sign-off documentation
AI_GOVERNANCE_MONITORING_PLAN.md            ← Governance & monitoring strategy
AI_MODEL_ACCEPTANCE_CRITERIA.md             ← Acceptance test criteria
AI_DOCUMENTATION_INDEX.md                   ← This file (you are here)
```

---

## 🏆 Quality Metrics

| Metric | Target | Achieved |
|--------|--------|----------|
| Build Status | ✅ Success | ✅ Success |
| AI Tests Passing | ✅ 5/5 | ✅ 5/5 (100%) |
| Documentation Completeness | ✅ All sections | ✅ 8 comprehensive docs |
| Health Endpoints | ✅ Functional | ✅ Both endpoints working |
| Model Persistence | ✅ Versioned | ✅ JSON manifest system |
| Configuration | ✅ Prod + Dev | ✅ Both configured |
| Fallback Behavior | ✅ Implemented | ✅ Both services have fallback |

---

## 🔗 Related Documentation

- **Infrastructure:** [QUICK_START.md](QUICK_START.md), [docker-compose.yml](docker-compose.yml)
- **Architecture:** [ARCHITECTURE_CHECKLIST.md](ARCHITECTURE_CHECKLIST.md)
- **Authentication:** [LDAP_ACCESS_ROLES_AND_CREDENTIALS.md](LDAP_ACCESS_ROLES_AND_CREDENTIALS.md)
- **Database:** [DATABASE_AND_AD_ACCOUNT_TABLES.md](DATABASE_AND_AD_ACCOUNT_TABLES.md)
- **APIs:** [SWAGGER_API_DOCUMENTATION.md](SWAGGER_API_DOCUMENTATION.md)

---

## 📝 Document Revision History

| Version | Date | Status | Notes |
|---------|------|--------|-------|
| 1.0 | June 13, 2026 | ✅ Final | Initial completion of AI implementation |

---

**This index is your map to the complete AI implementation. Choose your starting document based on your role and needs above.**
