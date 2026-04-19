# Quick Start Guide - 30 Seconds to Running

## 🚀 Start the Application

```bash
# Navigate to project directory
cd /path/to/IT-Inventory-Management---AsteelFlash-main

# Start all services
docker-compose up -d

# Wait 30 seconds for startup
sleep 30

# Check status
docker-compose ps
```

---

## ✅ Verify Services Are Running

```bash
# Health check
curl http://localhost:8080/health
# Should return: "Healthy"

# API status
curl http://localhost:8080/api/v1/health
# Should return JSON with status

# SMTP4Dev
# Open browser: http://localhost:5080
```

---

## 📚 Access Everything

| What | Where | Link |
|------|-------|------|
| **API Health** | Browser/Terminal | `http://localhost:8080/health` |
| **API Docs** | Browser | `http://localhost:8080/swagger/v1/swagger.json` |
| **API Status** | Browser | `http://localhost:8080/api/v1/health` |
| **SMTP4Dev** | Browser | `http://localhost:5080` |
| **Logs** | Terminal | `docker-compose logs app` |

---

## 🔧 Common Commands

```bash
# View all services
docker-compose ps

# View application logs
docker-compose logs app

# View SMTP logs
docker-compose logs smtpdev

# Real-time logs
docker-compose logs -f app

# Stop services
docker-compose stop

# Stop and remove everything
docker-compose down

# Stop and delete volumes/data
docker-compose down -v
```

---

## 📊 Quick API Tests

```bash
# Get health
curl http://localhost:8080/health

# Get API version
curl http://localhost:8080/api/v1/health/version

# Pretty print with jq
curl http://localhost:8080/api/v1/health/version | jq
```

---

## 📖 Full Documentation

1. **ACCESSIBILITY.md** - Complete guide to all services and endpoints
2. **API_TESTING.md** - Detailed curl examples for all endpoints
3. **PORTS_AND_NAVIGATION.md** - Port configuration and navigation
4. **README.md** - Project overview

---

## 🎯 What's Running

✅ **Application** (Port 8080)
- REST API with 15+ endpoints
- Health checks
- Asset lifecycle management
- Maintenance tracking
- Asset predictions

✅ **SMTP4Dev** (Port 5080)
- Email testing dashboard
- View all emails sent by app

✅ **SQLite Database**
- Persisted to Docker volume
- Automatic schema creation
- Demo data seeded

---

## 🟢 Status Check

```bash
# One-liner status check
curl -s http://localhost:8080/health && \
docker-compose ps | grep -E "app|smtpdev"
```

Expected output:
```
Healthy
itstockm-app       ... Up (healthy)
itstockm-smtpdev   ... Up (healthy)
```

---

## 🆘 Troubleshooting

**Port already in use?**
```bash
# Check what's using the port
lsof -i :8080

# Change port in docker-compose.yml
# ports:
#   - "9080:8080"
```

**Services not starting?**
```bash
# Check logs
docker-compose logs app

# Rebuild image
docker-compose build --no-cache

# Start fresh
docker-compose down -v
docker-compose up -d
```

**Can't access endpoints?**
```bash
# Verify container is running
docker-compose ps

# Check firewall
curl -v http://localhost:8080/health
```

---

## 🔗 Key URLs

| Purpose | URL |
|---------|-----|
| Health | http://localhost:8080/health |
| API Status | http://localhost:8080/api/v1/health |
| SMTP Dashboard | http://localhost:5080 |
| API Spec | http://localhost:8080/swagger/v1/swagger.json |

---

## 📝 Next Steps

1. ✅ Start services (`docker-compose up -d`)
2. ✅ Verify health (`curl http://localhost:8080/health`)
3. 📖 Read ACCESSIBILITY.md for complete guide
4. 🧪 Test API with examples in API_TESTING.md
5. 🛠️ Configure ports in PORTS_AND_NAVIGATION.md if needed

---

**Everything Ready!** 🎉
