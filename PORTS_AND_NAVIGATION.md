# Port & Navigation Configuration Guide

## 📊 Current Port Configuration

### Service Ports

| Service | External Port | Internal Port | Protocol | Status |
|---------|---------------|-----------------|----------|--------|
| **Application (WebAPI)** | 8080 | 8080 | HTTP | ✅ Running |
| **SMTP4Dev Web UI** | 5080 | 80 | HTTP | ✅ Running |
| **SMTP Server** | 5025 | 25 | SMTP | ✅ Running |

---

## 🗺️ Navigation Map

### 1. **Main Application** (Port 8080)

```
http://localhost:8080
├── /health                          ← Health check (no auth)
├── /api/v1/
│   ├── health                       ← API health status
│   ├── health/version              ← API version info
│   ├── health/info                 ← Detailed app info
│   ├── assetlifecycle/
│   │   ├── by-stage/{stage}        ← Get assets by stage
│   │   ├── {id}/history            ← Asset lifecycle history
│   │   ├── nearing-end-of-life     ← End-of-life assets
│   │   └── {id}/transition         ← Transition asset
│   ├── maintenance/
│   │   ├── (GET all)               ← All tickets
│   │   ├── {id}                    ← Get specific ticket
│   │   ├── equipment/{id}          ← Tickets for equipment
│   │   ├── count/open              ← Open tickets count
│   │   ├── (POST)                  ← Create ticket
│   │   ├── {id} (PUT)              ← Update ticket
│   │   └── {id}/close (POST)       ← Close ticket
│   └── predictions/
│       ├── latest                  ← Latest predictions
│       ├── high-risk               ← High-risk assets
│       └── recalculate (POST)      ← Recalculate predictions
└── /swagger/v1/swagger.json        ← OpenAPI specification
```

### 2. **SMTP4Dev Dashboard** (Port 5080)

```
http://localhost:5080
├── Dashboard               ← Email inbox
├── Email Details          ← View email content
├── Settings               ← SMTP configuration
└── Messages               ← Email history
```

---

## ✅ Endpoint Verification Checklist

### Health Check (No Authentication)
- ✅ `GET http://localhost:8080/health` → Returns "Healthy"
- ✅ `GET http://localhost:8080/api/v1/health` → Returns JSON with status
- ✅ `GET http://localhost:8080/api/v1/health/version` → Returns version info
- ✅ `GET http://localhost:8080/api/v1/health/info` → Returns detailed info

### API Documentation
- ✅ `GET http://localhost:8080/swagger/v1/swagger.json` → Returns OpenAPI spec
- ✅ **Total Endpoints**: 15+ documented paths

### Asset Lifecycle (Auth Required)
- ⏸️ `GET http://localhost:8080/api/v1/assetlifecycle/by-stage/{stage}` → 401 (auth required)
- ⏸️ `GET http://localhost:8080/api/v1/assetlifecycle/{id}/history` → 401
- ⏸️ `GET http://localhost:8080/api/v1/assetlifecycle/nearing-end-of-life` → 401
- ⏸️ `POST http://localhost:8080/api/v1/assetlifecycle/{id}/transition` → 401

### Maintenance (Auth Required)
- ⏸️ `GET http://localhost:8080/api/v1/maintenance` → 401
- ⏸️ `GET http://localhost:8080/api/v1/maintenance/{id}` → 401
- ⏸️ `GET http://localhost:8080/api/v1/maintenance/equipment/{id}` → 401
- ⏸️ `GET http://localhost:8080/api/v1/maintenance/count/open` → 401
- ⏸️ `POST http://localhost:8080/api/v1/maintenance` → 401
- ⏸️ `PUT http://localhost:8080/api/v1/maintenance/{id}` → 401
- ⏸️ `POST http://localhost:8080/api/v1/maintenance/{id}/close` → 401

### Predictions (Auth Required)
- ⏸️ `GET http://localhost:8080/api/v1/predictions/latest` → 401
- ⏸️ `GET http://localhost:8080/api/v1/predictions/high-risk` → 401
- ⏸️ `POST http://localhost:8080/api/v1/predictions/recalculate` → 401

### SMTP4Dev
- ✅ `http://localhost:5080` → SMTP4Dev Dashboard (200 OK)
- ✅ View all emails sent by the application

---

## 🔐 Authentication Status

### Current Implementation
- **Type**: ASP.NET Core Identity
- **Status**: Configured but endpoints marked as `[Authorize]`
- **Protected Endpoints**: Most API endpoints require Bearer token
- **Public Endpoints**: 
  - `GET /health`
  - `GET /api/v1/health*`
  - `GET /swagger/v1/swagger.json`

### Default Credentials (if seeding enabled)
```
Email:    admin@asteelflash.com
Password: admin123
```

---

## 📝 Port Configuration Management

### Changing Ports

#### Option 1: Edit docker-compose.yml (Recommended)
```yaml
services:
  app:
    ports:
      - "9080:8080"           # Change 8080 to 9080
  smtpdev:
    ports:
      - "5081:80"             # Change 5080 to 5081
      - "5026:25"             # Change 5025 to 5026
```

#### Option 2: Check Current Port Usage
```bash
# macOS/Linux
lsof -i :8080
lsof -i :5080
lsof -i :5025

# Windows
netstat -ano | findstr :8080
```

#### Option 3: Kill Process Using Port
```bash
# macOS/Linux
kill -9 <PID>

# Windows
taskkill /PID <PID> /F
```

---

## 🧭 Quick Navigation URLs

### For Development
```
Main API:        http://localhost:8080/api/v1
OpenAPI Spec:    http://localhost:8080/swagger/v1/swagger.json
Health Check:    http://localhost:8080/health
Email Testing:   http://localhost:5080
```

### Testing Endpoints
```bash
# Basic connectivity
curl http://localhost:8080/health

# API status
curl http://localhost:8080/api/v1/health

# API version
curl http://localhost:8080/api/v1/health/version
```

---

## 📊 Port Usage Summary

| Port | Service | Container | Type | Notes |
|------|---------|-----------|------|-------|
| 8080 | Application | itstockm-app | HTTP | Main API & Health checks |
| 5080 | SMTP4Dev Web | itstockm-smtpdev | HTTP | Email inbox & testing |
| 5025 | SMTP Server | itstockm-smtpdev | SMTP | Email relay (no auth) |

---

## 🔄 Port Forwarding (If Running Remotely)

### SSH Tunneling Example
```bash
# Forward remote ports to local machine
ssh -L 8080:localhost:8080 -L 5080:localhost:5080 -L 5025:localhost:5025 user@remote-server

# Then access locally
curl http://localhost:8080/health
```

### Docker Port Expose
```bash
# View exposed ports
docker-compose port app 8080
docker-compose port smtpdev 80
```

---

## 🌐 URL Structure

### Application (Port 8080)
```
Scheme:  http://
Host:    localhost
Port:    8080
Path:    /api/v1/{resource}

Example: http://localhost:8080/api/v1/maintenance
```

### SMTP4Dev (Port 5080)
```
Scheme:  http://
Host:    localhost
Port:    5080
Path:    /

Example: http://localhost:5080
```

### SMTP (Port 5025)
```
Protocol: SMTP
Host:     localhost
Port:     5025
Auth:     None (no credentials required)
```

---

## 📋 Navigation Shortcuts

### Bookmarking These URLs
```
| Label | URL |
|-------|-----|
| Health Check | http://localhost:8080/health |
| API Status | http://localhost:8080/api/v1/health |
| SMTP Inbox | http://localhost:5080 |
| API Spec | http://localhost:8080/swagger/v1/swagger.json |
```

### Browser Quick Access
```
In address bar type:
- loc:8080/health          → http://localhost:8080/health
- loc:8080/api/v1/health   → http://localhost:8080/api/v1/health
- loc:5080                 → http://localhost:5080
```

---

## 🔗 Common Navigation Patterns

### Check System Status
```
1. Visit: http://localhost:8080/health
   Expected: "Healthy"
2. Visit: http://localhost:8080/api/v1/health
   Expected: JSON response with healthy status
```

### Access API Documentation
```
1. Get OpenAPI spec: curl http://localhost:8080/swagger/v1/swagger.json
2. Import into Postman or other API tool
3. Use interactive documentation
```

### Monitor Emails
```
1. Visit: http://localhost:5080
2. Check inbox for sent emails
3. Click on emails to view details
```

### Troubleshoot Connectivity
```
Endpoint Check:  http://localhost:8080/api/v1/health
Expected:        200 OK with JSON
If 404:          Port not open or service down
If 500:          Service error - check logs
If Timeout:      Port blocked by firewall
```

---

## ✨ Current Status

### Live Services
- ✅ Application on port 8080
- ✅ SMTP4Dev on port 5080
- ✅ SMTP Server on port 5025
- ✅ Health endpoints responding
- ✅ API endpoints documented (15+ paths)

### Configuration
- ✅ Ports exposed in docker-compose.yml
- ✅ Health checks configured
- ✅ Security headers enabled
- ✅ CORS enabled for development
- ✅ Development environment active

### Documentation
- ✅ API endpoints documented
- ✅ Port mapping clear
- ✅ Navigation guide provided
- ✅ Quick access URLs available

---

**Status**: 🟢 All services running and accessible  
**Last Updated**: April 14, 2026  
**Next**: Refer to ACCESSIBILITY.md and API_TESTING.md for detailed endpoint documentation
