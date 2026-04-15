# API Testing Guide - curl Examples

## 🌐 Base URL
```
http://localhost:8080/api/v1
```

---

## ✅ Health & Status Endpoints (No Authentication)

### 1. Health Check
```bash
curl http://localhost:8080/health
# Returns: "Healthy"
```

### 2. API Health Status
```bash
curl http://localhost:8080/api/v1/health
# Returns JSON with status, timestamp, version, environment
```

### 3. API Version Information
```bash
curl http://localhost:8080/api/v1/health/version
# Returns: apiVersion, applicationName, description
```

### 4. Detailed Application Info
```bash
curl http://localhost:8080/api/v1/health/info
# Returns: Full application metadata
```

---

## 🔄 Asset Lifecycle Endpoints

### 1. Get Assets by Stage
```bash
# Get all "Active" assets
curl http://localhost:8080/api/v1/assetlifecycle/by-stage/Active

# Get all "Retired" assets
curl http://localhost:8080/api/v1/assetlifecycle/by-stage/Retired

# Possible stages: Active, InRepair, Retired, Archived, etc.
```

### 2. Get Lifecycle History for an Asset
```bash
# Get history for asset with ID 1
curl http://localhost:8080/api/v1/assetlifecycle/1/history

# Response includes all state transitions and dates
```

### 3. Get Assets Nearing End of Life
```bash
# Get assets ending within 6 months (default)
curl http://localhost:8080/api/v1/assetlifecycle/nearing-end-of-life

# Get assets ending within 12 months
curl http://localhost:8080/api/v1/assetlifecycle/nearing-end-of-life?withinMonths=12

# Get assets ending within 3 months
curl http://localhost:8080/api/v1/assetlifecycle/nearing-end-of-life?withinMonths=3
```

### 4. Transition Asset to New Stage
```bash
# Transition asset 1 to "Retired" stage
curl -X POST http://localhost:8080/api/v1/assetlifecycle/1/transition \
  -H "Content-Type: application/json" \
  -d '{
    "stage": "Retired",
    "notes": "End of life - replaced with new model"
  }'

# Response: Updated AssetLifecycleRecord
```

---

## 🔧 Maintenance Endpoints

### 1. Get All Maintenance Tickets
```bash
curl http://localhost:8080/api/v1/maintenance

# Response: Array of all maintenance tickets
```

### 2. Get Specific Maintenance Ticket
```bash
# Get ticket with ID 5
curl http://localhost:8080/api/v1/maintenance/5

# Response: Single MaintenanceTicket object or 404 if not found
```

### 3. Get Tickets for Specific Equipment
```bash
# Get all tickets for equipment/material ID 3
curl http://localhost:8080/api/v1/maintenance/equipment/3

# Response: Array of MaintenanceTicket objects for that equipment
```

### 4. Get Open Ticket Count
```bash
curl http://localhost:8080/api/v1/maintenance/count/open

# Response: { "openTickets": 5 }
```

### 5. Create New Maintenance Ticket
```bash
curl -X POST http://localhost:8080/api/v1/maintenance \
  -H "Content-Type: application/json" \
  -d '{
    "id": 0,
    "materielId": 3,
    "description": "Printer not responding",
    "status": "Open",
    "priority": "High",
    "createdDate": "2026-04-14T10:00:00Z",
    "resolvedDate": null
  }'

# Response: 201 Created with MaintenanceTicket object
# Location header: /api/v1/maintenance/{ticketId}
```

### 6. Update Maintenance Ticket
```bash
# Update ticket 5
curl -X PUT http://localhost:8080/api/v1/maintenance/5 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 5,
    "materielId": 3,
    "description": "Printer not responding - PARTIALLY FIXED",
    "status": "InProgress",
    "priority": "High",
    "createdDate": "2026-04-14T10:00:00Z",
    "resolvedDate": null
  }'

# Response: 200 OK with updated MaintenanceTicket
```

### 7. Close Maintenance Ticket
```bash
# Close ticket 5 with resolution
curl -X POST http://localhost:8080/api/v1/maintenance/5/close \
  -H "Content-Type: application/json" \
  -d '{
    "resolution": "Replaced faulty print head - now working normally",
    "cost": 150.00
  }'

# Response: 200 OK with closed MaintenanceTicket
```

---

## 🎯 Predictions Endpoints

### 1. Get Latest Asset Predictions
```bash
curl http://localhost:8080/api/v1/predictions/latest

# Response: Array of AssetPrediction objects with health scores
```

### 2. Get High-Risk Assets
```bash
# Get assets with health score <= 50 (default)
curl http://localhost:8080/api/v1/predictions/high-risk

# Get assets with health score <= 30
curl http://localhost:8080/api/v1/predictions/high-risk?maxScore=30

# Get assets with health score <= 70
curl http://localhost:8080/api/v1/predictions/high-risk?maxScore=70

# Response: Array of high-risk AssetPrediction objects
```

### 3. Recalculate All Predictions
```bash
curl -X POST http://localhost:8080/api/v1/predictions/recalculate

# Response: Array of recalculated AssetPrediction objects
```

---

## 📊 Response Format Examples

### Asset Lifecycle Response
```json
{
  "id": 1,
  "materielId": 5,
  "stage": "Active",
  "transitionDate": "2026-04-14T10:00:00Z",
  "notes": "Asset deployed to office",
  "createdBy": "admin@asteelflash.com"
}
```

### Maintenance Ticket Response
```json
{
  "id": 1,
  "materielId": 3,
  "description": "Printer not responding",
  "status": "Open",
  "priority": "High",
  "createdDate": "2026-04-14T10:00:00Z",
  "resolvedDate": null,
  "resolution": null,
  "cost": null
}
```

### Prediction Response
```json
{
  "id": 1,
  "materielId": 5,
  "healthScore": 45.5,
  "riskLevel": "High",
  "predictedFailureDate": "2026-06-14T00:00:00Z",
  "recommendedAction": "Replace within 60 days",
  "lastUpdated": "2026-04-14T17:30:00Z"
}
```

---

## 🔍 Testing with Pretty JSON

### Install jq (if not installed)
```bash
# macOS
brew install jq

# Ubuntu/Debian
sudo apt-get install jq

# Fedora
sudo dnf install jq
```

### Test with Formatted Output
```bash
# Get health with pretty JSON
curl http://localhost:8080/api/v1/health | jq

# Get maintenance tickets with pretty JSON
curl http://localhost:8080/api/v1/maintenance | jq

# Get specific fields
curl http://localhost:8080/api/v1/predictions/latest | jq '.[].healthScore'

# Count objects
curl http://localhost:8080/api/v1/maintenance | jq 'length'
```

---

## 📈 Batch Testing Script

```bash
#!/bin/bash

echo "=== IT Stock Management API Test Suite ==="
echo ""

# Health Check
echo "1. Health Check:"
curl -s http://localhost:8080/health
echo ""

# API Version
echo "2. API Version:"
curl -s http://localhost:8080/api/v1/health/version | jq -r '.apiVersion'
echo ""

# Get all maintenance tickets
echo "3. Maintenance Tickets Count:"
curl -s http://localhost:8080/api/v1/maintenance | jq 'length'
echo ""

# Get open tickets
echo "4. Open Tickets:"
curl -s http://localhost:8080/api/v1/maintenance/count/open | jq '.openTickets'
echo ""

# Get high-risk assets
echo "5. High-Risk Assets:"
curl -s http://localhost:8080/api/v1/predictions/high-risk | jq 'length'
echo ""

# Get assets nearing end-of-life
echo "6. Assets Nearing End-of-Life:"
curl -s "http://localhost:8080/api/v1/assetlifecycle/nearing-end-of-life?withinMonths=12" | jq 'length'
echo ""

echo "=== Test Complete ==="
```

---

## 🔐 Authentication (When Required)

### Get Bearer Token
```bash
# Login endpoint (if available)
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@asteelflash.com",
    "password": "admin123"
  }'

# Save token
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@asteelflash.com","password":"admin123"}' | jq -r '.token')

echo "Token: $TOKEN"
```

### Use Token in Requests
```bash
# Get tickets with authentication
curl -H "Authorization: Bearer $TOKEN" \
  http://localhost:8080/api/v1/maintenance

# Create ticket with authentication
curl -X POST http://localhost:8080/api/v1/maintenance \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

---

## 🧪 Common Test Scenarios

### Scenario 1: Full Asset Lifecycle
```bash
# 1. Get all active assets
curl http://localhost:8080/api/v1/assetlifecycle/by-stage/Active | jq '. | length' assets

# 2. Get specific asset history
curl http://localhost:8080/api/v1/assetlifecycle/1/history | jq

# 3. Transition asset to InRepair
curl -X POST http://localhost:8080/api/v1/assetlifecycle/1/transition \
  -H "Content-Type: application/json" \
  -d '{"stage":"InRepair","notes":"Maintenance in progress"}'

# 4. Create maintenance ticket
curl -X POST http://localhost:8080/api/v1/maintenance \
  -H "Content-Type: application/json" \
  -d '{"materielId":1,"description":"Repair hinges","status":"Open","priority":"Medium"}'

# 5. Close maintenance ticket
curl -X POST http://localhost:8080/api/v1/maintenance/1/close \
  -H "Content-Type: application/json" \
  -d '{"resolution":"Hinges replaced and tested","cost":75.00}'

# 6. Transition back to Active
curl -X POST http://localhost:8080/api/v1/assetlifecycle/1/transition \
  -H "Content-Type: application/json" \
  -d '{"stage":"Active","notes":"Repair completed - returned to service"}'
```

### Scenario 2: Monitor High-Risk Assets
```bash
# Get high-risk assets
HIGH_RISK=$(curl -s http://localhost:8080/api/v1/predictions/high-risk?maxScore=40 | jq -r '.[].materielId' )

# For each high-risk asset
for ASSET_ID in $HIGH_RISK; do
  echo "Asset $ASSET_ID - Getting lifecycle history..."
  curl -s http://localhost:8080/api/v1/assetlifecycle/$ASSET_ID/history | jq
done
```

### Scenario 3: Maintenance Dashboard
```bash
#!/bin/bash

echo "=== Maintenance Status Dashboard ==="
echo "Total Tickets: $(curl -s http://localhost:8080/api/v1/maintenance | jq 'length')"
echo "Open Tickets: $(curl -s http://localhost:8080/api/v1/maintenance/count/open | jq '.openTickets')"
echo "High-Risk Assets: $(curl -s http://localhost:8080/api/v1/predictions/high-risk | jq 'length')"
echo "Assets Ending Soon: $(curl -s 'http://localhost:8080/api/v1/assetlifecycle/nearing-end-of-life?withinMonths=3' | jq 'length')"
```

---

## 🚨 Error Handling

### 404 - Resource Not Found
```bash
curl http://localhost:8080/api/v1/maintenance/99999
# Response: 404 Not Found
```

### 400 - Bad Request
```bash
curl -X POST http://localhost:8080/api/v1/assetlifecycle/999/transition \
  -H "Content-Type: application/json" \
  -d '{"stage":"","notes":"empty stage"}'
# Response: 400 Bad Request (validation errors)
```

### 500 - Server Error
```bash
# Check logs for errors
docker-compose logs app | tail -50
```

---

## 📊 Performance Metrics

### Request Count
```bash
# Get count of open maintenance tickets
curl -s http://localhost:8080/api/v1/maintenance/count/open

# Get count of all maintenance tickets
curl -s http://localhost:8080/api/v1/maintenance | jq 'length'

# Get count of high-risk assets
curl -s http://localhost:8080/api/v1/predictions/high-risk | jq 'length'
```

### Response Time
```bash
# Check response time for endpoint
curl -w "Time: %{time_total}s\n" http://localhost:8080/api/v1/maintenance
```

---

## 🔗 API Endpoint Summary

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/health` | Basic health check |
| GET | `/api/v1/health` | Health status with metadata |
| GET | `/api/v1/health/version` | API version info |
| GET | `/api/v1/health/info` | Detailed app info |
| GET | `/api/v1/assetlifecycle/by-stage/{stage}` | Get assets by stage |
| GET | `/api/v1/assetlifecycle/{materielId}/history` | Get lifecycle history |
| GET | `/api/v1/assetlifecycle/nearing-end-of-life` | Get end-of-life assets |
| POST | `/api/v1/assetlifecycle/{materielId}/transition` | Transition asset |
| GET | `/api/v1/maintenance` | Get all tickets |
| GET | `/api/v1/maintenance/{ticketId}` | Get specific ticket |
| GET | `/api/v1/maintenance/equipment/{materielId}` | Get tickets for equipment |
| GET | `/api/v1/maintenance/count/open` | Get open ticket count |
| POST | `/api/v1/maintenance` | Create ticket |
| PUT | `/api/v1/maintenance/{ticketId}` | Update ticket |
| POST | `/api/v1/maintenance/{ticketId}/close` | Close ticket |
| GET | `/api/v1/predictions/latest` | Get latest predictions |
| GET | `/api/v1/predictions/high-risk` | Get high-risk assets |
| POST | `/api/v1/predictions/recalculate` | Recalculate predictions |

---

**Total Endpoints**: 18  
**Auth Required**: Most endpoints (except /health, /api/v1/health/*)  
**Format**: JSON  
**API Version**: 1.0.0  
**Status**: ✅ Production Ready
