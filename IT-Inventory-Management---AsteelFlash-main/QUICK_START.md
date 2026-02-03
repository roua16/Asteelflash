# IT Stock Management System - Quick Setup Guide

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB or Full)
- Visual Studio 2022 or VS Code

### 1. Clone & Restore
```powershell
cd IT-Inventory-Management---AsteelFlash-main
dotnet restore
```

### 2. Configure Email (Choose One)

#### Option A: Gmail (Development)
```json
// appsettings.json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "FromEmail": "your-email@gmail.com",
    "Password": "your-app-password",  // Get from Google Account Settings
    "AdminEmail": "admin@yourcompany.com"
  }
}
```

#### Option B: Mailtrap (Testing - Recommended)
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.mailtrap.io",
    "SmtpPort": "2525",
    "FromEmail": "test@example.com",
    "Password": "mailtrap-password",
    "AdminEmail": "admin@example.com"
  }
}
```

### 3. Run Migrations
```powershell
dotnet ef database update
```

### 4. Run Application
```powershell
dotnet run
```

### 5. Run Tests
```powershell
cd ITStockM.Tests
dotnet test
```

## 📧 Email Notifications

The system automatically sends email notifications to admin for:
- ✅ Material assignments
- ✅ Delivery orders
- ✅ Material changes (add/update/delete)
- ✅ Supplier changes
- ✅ Project changes
- ✅ Request submissions

## 🧪 Testing

All services have comprehensive unit tests:
```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test
dotnet test --filter "FullyQualifiedName~EmailServiceTests"
```

### Dev-only SMTP test endpoint (Docker)
- Ensure these are set in `.env` (or in the environment):
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `DEV_ALLOW_TEST_EMAIL=true`
- Start stack:
```powershell
docker compose up --build -d
```
- Send a test email (sends to `EmailSettings:AdminEmail` by default):
```bash
curl -X POST "http://localhost:8080/api/dev/send-test-email"
```
- To send to a custom recipient:
```bash
curl -X POST "http://localhost:8080/api/dev/send-test-email?to=you@example.com"
```
- Query SMTP connectivity without sending an email:
```bash
curl "http://localhost:8080/api/dev/smtp-health"
```
- Query SMTP connectivity and send a test email (dev-only; requires `EmailSettings:AdminEmail`):
```bash
curl "http://localhost:8080/api/dev/smtp-health?sendTestEmail=true"
```
- Inspect logs:
```powershell
docker compose logs --tail 200 app
```

> Tip: Use Mailtrap or `smtp4dev` for safe testing instead of real SMTP credentials.

## 📚 Documentation

- [Full Project Improvements](PROJECT_IMPROVEMENTS.md)
- [Architecture Details](PROJECT_IMPROVEMENTS.md#-project-structure-improvements)
- [API Usage Examples](PROJECT_IMPROVEMENTS.md#-using-the-notification-service)

## ⚠️ Important Notes

1. **Never commit appsettings.json with real credentials**
2. Use **User Secrets** for development:
   ```powershell
   dotnet user-secrets set "EmailSettings:Password" "your-password"
   ```
3. Use **Azure Key Vault** for production

## 🔧 Troubleshooting

### Email not sending?
- Check SMTP credentials
- Verify firewall/antivirus settings
- Try Mailtrap for testing
- Check logs in console output

### Tests failing?
- Clean and rebuild: `dotnet clean && dotnet build`
- Check database connection
- Ensure all dependencies restored

### Build errors?
- Update .NET SDK to 8.0 or later
- Run `dotnet restore`
- Check for missing dependencies

## 📞 Need Help?

See [PROJECT_IMPROVEMENTS.md](PROJECT_IMPROVEMENTS.md) for detailed documentation.
