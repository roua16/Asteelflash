# IT Stock Management System - Accessibility Guide

## 📋 Overview
Complete guide to accessing all services, APIs, and dashboards for the IT Stock Management System running in Docker Compose.

---

## 🚀 Quick Access Summary

| Service | URL | Port | Purpose | Auth |
|---------|-----|------|---------|------|
| **Dashboard & Frontend** | http://localhost:8080 | 8080 | Main landing page with system info | No |
| **Swagger UI** | http://localhost:8080/swagger | 8080 | Interactive API documentation | No |
| **Swagger JSON** | http://localhost:8080/swagger/v1/swagger.json | 8080 | OpenAPI 3.0 specification | No |
| **Health Check** | http://localhost:8080/health | 8080 | Simple health status | No |
| **API Status** | http://localhost:8080/api/v1/health | 8080 | Detailed API health (JSON) | No |
| **SMTP4Dev UI** | http://localhost:5080 | 5080 | Email testing & debugging | No |
| **SMTP Server** | localhost:5025 | 5025 | SMTP service (internal) | No |

---

## 🔌 Service Details

### 1. Application Service (Port 8080)

**Environment**: Development  
**Status**: Running in container `itstockm-app`

#### Key Endpoints:

```bash
# Health Check
curl http://localhost:8080/health
# Returns: "Healthy"

# Application Info
GET http://localhost:8080/api/v1/health
# Response:
{
  "status": "healthy",
  "timestamp": "2026-04-14T17:35:42Z",
  "version": "1.0.0",
  "environment": "Development"
}

# API Version Info
GET http://localhost:8080/api/v1/health/version
# Response:
{
  "apiVersion": "1.0.0",
  "applicationName": "IT Stock Management API",
  "description": "Enterprise-level IT asset inventory management system"
}

# Application Details
GET http://localhost:8080/api/v1/health/info
# Returns application metadata and status
```

---

## 📡 RESTful API Documentation

### Base URL
```
http://localhost:8080/api/v1
```

### Authentication
- **Status**: Identity-based (Seeded admin user available)
- **Default Admin Credentials** (if seeding enabled):
  - Email: `admin@asteelflash.com`
  - Password: `admin123`

---

## 🛠️ API Endpoints

### Health & Info Endpoints (No Auth Required)

```http
# Application Health
GET /api/v1/health
Authorization: (None Required)
Response: 200 OK
{
  "status": "healthy",
  "timestamp": "2026-04-14T17:35:42.8023913Z",
  "version": "1.0.0",
  "environment": "Development"
}

# Version Information
GET /api/v1/health/version
Authorization: (None Required)
Response: 200 OK
{
  "apiVersion": "1.0.0",
  "applicationName": "IT Stock Management API",
  "description": "Enterprise-level IT asset inventory management system"
}

# Detailed Application Info
GET /api/v1/health/info
Authorization: (None Required)
Response: 200 OK
{
  "applicationName": "IT Stock Management System",
  "version": "1.0.0",
  "description": "Enterprise-level IT asset inventory management system",
  "organization": "Asteelflash",
  "environment": "Development",
  "timestamp": "2026-04-14T17:35:42.8023913Z",
  "uptime": "See logs for detailed uptime"
}
```

### Asset Lifecycle Management

```http
# Get Assets by Stage
GET /api/v1/assetlifecycle/by-stage/{stage}
Authorization: Bearer {token}
Example: /api/v1/assetlifecycle/by-stage/Active
Response: 200 OK [Materiel[]]

# Get Lifecycle History
GET /api/v1/assetlifecycle/{materielId}/history
Authorization: Bearer {token}
Response: 200 OK [AssetLifecycleRecord[]]

# Get Assets Nearing End of Life
GET /api/v1/assetlifecycle/nearing-end-of-life?withinMonths=6
Authorization: Bearer {token}
Response: 200 OK [Materiel[]]

# Transition Asset Stage
POST /api/v1/assetlifecycle/{materielId}/transition
Authorization: Bearer {token}
Content-Type: application/json
Body:
{
  "stage": "Retired",
  "notes": "Optional transition notes"
}
Response: 200 OK (AssetLifecycleRecord)
```

### Maintenance Ticket Management

```http
# Get All Maintenance Tickets
GET /api/v1/maintenance
Authorization: Bearer {token}
Response: 200 OK [MaintenanceTicket[]]

# Get Specific Ticket
GET /api/v1/maintenance/{ticketId}
Authorization: Bearer {token}
Response: 200 OK (MaintenanceTicket) | 404 Not Found

# Get Tickets by Equipment
GET /api/v1/maintenance/equipment/{materielId}
Authorization: Bearer {token}
Response: 200 OK [MaintenanceTicket[]]

# Get Open Ticket Count
GET /api/v1/maintenance/count/open
Authorization: Bearer {token}
Response: 200 OK
{
  "openTickets": 5
}

# Create Maintenance Ticket
POST /api/v1/maintenance
Authorization: Bearer {token}
Content-Type: application/json
Body: (MaintenanceTicket object)
Response: 201 Created

# Update Maintenance Ticket
PUT /api/v1/maintenance/{ticketId}
Authorization: Bearer {token}
Content-Type: application/json
Body: (MaintenanceTicket object)
Response: 200 OK

# Close Maintenance Ticket
POST /api/v1/maintenance/{ticketId}/close
Authorization: Bearer {token}
Content-Type: application/json
Body:
{
  "resolution": "Fixed defective component",
  "cost": 250.50
}
Response: 200 OK (MaintenanceTicket)
```

### Asset Predictions

```http
# Get Latest Predictions
GET /api/v1/predictions/latest
Authorization: Bearer {token}
Response: 200 OK [AssetPrediction[]]

# Get High-Risk Assets
GET /api/v1/predictions/high-risk?maxScore=50
Authorization: Bearer {token}
Response: 200 OK [AssetPrediction[]]

# Recalculate All Predictions
POST /api/v1/predictions/recalculate
Authorization: Bearer {token}
Response: 200 OK [AssetPrediction[]]
```

---

## 📚 API Documentation UI (Swagger)

### Access Swagger UI
```
http://localhost:8080/swagger
```

### Features:
- ✅ Interactive API exploration
- ✅ Request/Response examples
- ✅ Authentication integration
- ✅ Real-time testing of endpoints
- ✅ XML documentation comments

### View OpenAPI JSON:
```
http://localhost:8080/swagger/v1/swagger.json
```

**Expected Response**: Complete OpenAPI 3.0.1 specification with 15+ endpoints

---

## 📧 Email Testing (SMTP4Dev)

### Access SMTP4Dev Dashboard
```
http://localhost:5080
```

### Features:
- **Email Inbox**: View all emails sent by the application
- **Email Details**: Inspect email headers, body, and attachments
- **SMTP Configuration**: Email settings for development testing
- **No Authentication Required**: Open access for development

### Configuration Details:
- **SMTP Host**: `smtpdev` (internal container networking)
- **SMTP Port**: `25` (no authentication)
- **From Address**: `system@asteelflash.com`
- **Admin Email**: `admin@asteelflash.com`

### Usage Example:
When the application sends emails (e.g., maintenance reminders, asset alerts), they automatically appear in the SMTP4Dev inbox at http://localhost:5080.

---

## 🗄️ Database

### Database Type
- **Engine**: SQLite
- **Location**: `/app/data/ITStockManagement.db` (inside container)
- **Persistence**: Docker named volume `itstockm-data`

### Connection String
```
Data Source=/app/data/ITStockManagement.db
```

### Data Protection Keys
- **Location**: `/root/.aspnet/DataProtection-Keys` (inside container)
- **Persistence**: Docker named volume `itstockm-dpkeys`

---

## 🐳 Docker Compose Services

### Running Services

```bash
# View all running services
docker-compose ps

# Expected Output:
# NAME               SERVICE    STATUS          PORTS
# itstockm-app      app        Up (healthy)    0.0.0.0:8080->8080/tcp
# itstockm-smtpdev  smtpdev    Up (healthy)    0.0.0.0:5080->80/tcp, 0.0.0.0:5025->25/tcp
```

### Service Management

```bash
# Start Services
docker-compose up -d

# Stop Services
docker-compose stop

# Restart Services
docker-compose restart

# View Logs
docker-compose logs app       # Application logs
docker-compose logs smtpdev   # SMTP service logs
docker-compose logs -f app    # Real-time app logs

# Stop and Remove Everything
docker-compose down

# Remove Everything + Delete Volumes
docker-compose down -v
```

---

## 🔐 Security Headers

All responses include security headers:
```
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
```

---

## 📊 Configuration

### Environment Variables (docker-compose.yml)

```yaml
# Database
ConnectionStrings__ITStockManagmentConnection: Data Source=/app/data/ITStockManagement.db

# Seeding
Seed__Enabled: true              # Enable auto-seeding
Seed__DemoData: true             # Load demo data
Seed__AdminEmail: admin@asteelflash.com
Seed__AdminPassword: admin123

# SMTP
SMTP_SERVER: smtpdev             # Container name
SMTP_PORT: 25                    # SMTP port
SMTP_FROM_EMAIL: system@asteelflash.com
SMTP_ADMIN_EMAIL: admin@asteelflash.com

# Application
ASPNETCORE_ENVIRONMENT: Development
ASPNETCORE_URLS: http://+:8080
```

---

## ✅ Health Check Status

### Application Health Endpoint
```bash
# Get health status
curl http://localhost:8080/health

# Expected Response: "Healthy"
# Status Code: 200 OK
```

### Database Health Check
The health endpoint automatically checks:
- ✅ SQLite database connectivity
- ✅ Database schema presence
- ✅ Core tables available

### SMTP Health Check
- ✅ Connected to SMTP server
- ✅ Email service ready

---

## 🧪 Testing the API

### Using curl

```bash
# Get health status
curl http://localhost:8080/health

# Get API version
curl http://localhost:8080/api/v1/health/version | jq

# Get Swagger spec
curl http://localhost:8080/swagger/v1/swagger.json | jq '.paths | keys'

# Get assets by lifecycle stage
curl http://localhost:8080/api/v1/assetlifecycle/by-stage/Active

# Get maintenance tickets
curl http://localhost:8080/api/v1/maintenance
```

### Using Postman

1. **Import Swagger**: 
   - Paste: `http://localhost:8080/swagger/v1/swagger.json`
   - Import as OpenAPI spec

2. **Authenticate**:
   - Obtain Bearer token through identity endpoints
   - Add to `Authorization` header: `Bearer {token}`

3. **Test Endpoints**:
   - All endpoints now available in Postman collection

---

## 📱 API Status Dashboard

Create a simple monitoring dashboard:

```bash
#!/bin/bash
echo "=== IT Stock Management System Status ==="
echo "Timestamp: $(date)"
echo ""
echo "Health: $(curl -s http://localhost:8080/health)"
echo "API Version: $(curl -s http://localhost:8080/api/v1/health/version | jq -r '.apiVersion')"
echo "Environment: $(curl -s http://localhost:8080/api/v1/health/version | jq -r '.applicationName')"
echo ""
echo "Docker Status:"
docker-compose ps | tail -2
```

---

## 🐛 Troubleshooting

### Port Already in Use
```bash
# Check what's using port 8080
lsof -i :8080

# Change port in docker-compose.yml
# ports:
#   - "9080:8080"  # Change from 8080 to 9080
```

### Containers Not Starting
```bash
# Check logs
docker-compose logs app

# Rebuild image
docker-compose build --no-cache

# Restart
docker-compose up -d
```

### API Not Responding
```bash
# Verify container is running
docker-compose ps

# Check application logs
docker-compose logs app --tail 50

# Test connectivity
curl -v http://localhost:8080/health
```

### Database Issues
```bash
# Check database file
docker exec itstockm-app ls -la /app/data/

# Reset database
docker-compose down -v
docker-compose up -d
```

---

## 📈 Performance Monitoring

### Monitor Application
```bash
# Watch logs in real-time
docker-compose logs -f app

# Check container resource usage
docker stats itstockm-app itstockm-smtpdev

# Check health at regular intervals
watch -n 5 'curl -s http://localhost:8080/health'
```

---

## 🎯 Common Use Cases

### Check If System Is Running
```bash
curl http://localhost:8080/health
# Returns: "Healthy" (200 OK)
```

### View All Maintenance Tickets
```bash
curl http://localhost:8080/api/v1/maintenance
```

### Send Test Email
The system automatically sends emails via SMTP4Dev. Check inbox at:
```
http://localhost:5080
```

### Monitor Asset Health
```bash
curl http://localhost:8080/api/v1/predictions/high-risk
```

### Get System Version
```bash
curl http://localhost:8080/api/v1/health/version | jq
```

---

## 📞 Support

### For Issues:
1. Check logs: `docker-compose logs app`
2. Verify containers: `docker-compose ps`
3. Test health: `curl http://localhost:8080/health`
4. Review documentation: This file

### Database Inspection
```bash
# Check volumes
docker volume ls

# Inspect data volume
docker volume inspect it-inventory-management---asteelflash-main_itstockm-data
```

---

## ✨ Summary

| Component | Access Point | Status |
|-----------|--------------|--------|
| **Main Application** | http://localhost:8080 | ✅ Running |
| **Swagger API Docs** | http://localhost:8080/swagger | ✅ Available |
| **Health Check** | http://localhost:8080/health | ✅ Responsive |
| **SMTP4Dev Email** | http://localhost:5080 | ✅ Available |
| **SMTP Server** | localhost:5025 | ✅ Connected |
| **API Endpoints** | /api/v1/* | ✅ 15+ paths |
| **Database** | SQLite (persisted) | ✅ Initialized |

---

**Last Updated**: April 14, 2026  
**API Version**: 1.0.0  
**Status**: Production Ready ✅
