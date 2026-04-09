# IT Inventory Management System - AsteelFlash

## 📋 Project Overview

IT Stock Management system for AsteelFlash with complete Docker containerization, email notifications, and CRUD operations.

---

## 🐳 Docker Setup & Build

### Quick Start
```bash
# Build and run all containers
docker-compose up -d --build

# Stop containers
docker-compose down

# View logs
docker logs itstockm-app --tail 50
```

### Container Architecture
| Container | Service | Port | Description |
|-----------|---------|------|-------------|
| `itstockm-app` | Blazor Server | 8080 | Main application |
| `itstockm-db` | SQL Server 2022 | 1433 | Database |
| `itstockm-smtpdev` | smtp4dev | 5080 (UI), 5025 (SMTP) | Email testing |

### Access URLs
- **Application**: http://localhost:8080
- **smtp4dev Inbox**: http://localhost:5080
- **Database**: localhost:1433

---

## 🔧 Improvements & Corrections (February 2026)

### ✅ Email Service Fixes
| Issue | Solution |
|-------|----------|
| STARTTLS error with smtp4dev | Added `SecureSocketOptions.None` for port 25 |
| Authentication fails on local SMTP | Skip authentication when password is empty |
| Shell env variables override .env | Clear PowerShell variables before Docker run |

### ✅ Configuration Updates
- **appsettings.Development.json**: Added `EmailSettings` section for local `dotnet run`
- **.env file**: Configured for smtp4dev (`SMTP_SERVER=smtpdev`, `SMTP_PORT=25`)

- **Low stock notifications**: The system will send a low-stock email when a material's total quantity (IT + PDR) drops below a configurable threshold. Set `LOW_STOCK_THRESHOLD` (default 10) to change the threshold. Recipients are taken from `SMTP_PDR_EMAIL` / `SMTP_IT_EMAIL` if set; otherwise seeded users with roles `PDR`, `IT`, or `Admin` will be used.
- **EmailService.cs**: Dynamic SSL/TLS based on port (465=SSL, 587=STARTTLS, 25=None)

### ✅ Docker Corrections
- Fixed port conflict (moved smtp4dev UI from 3000 to 5080)
- Proper container networking (app uses `smtpdev:25` internally)
- Health checks for SQL Server before app startup

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Network                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  itstockm-  │  │  itstockm-  │  │   itstockm-smtpdev  │ │
│  │     app     │──│     db      │  │   (smtp4dev)        │ │
│  │  :8080      │  │  :1433      │  │   :5080 (UI)        │ │
│  │  Blazor     │  │  SQL Server │  │   :5025 (SMTP)      │ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Tech Stack
- **.NET 8** / Blazor Server
- **Entity Framework Core** / SQL Server 2022
- **MailKit 4.14.1** for SMTP
- **Radzen Components** for UI
- **Docker Compose** for orchestration

---

## 📧 Email Notification System

### Supported Operations
- Assignment Created/Updated/Deleted
- Materiel Created/Updated/Deleted  
- Delivery Order operations
- Request status changes

### Configuration
```json
// appsettings.Development.json (for dotnet run)
"EmailSettings": {
  "SmtpServer": "localhost",
  "SmtpPort": 5025,
  "FromEmail": "system@asteelflash.com",
  "Password": "",
  "AdminEmail": "admin@asteelflash.com"
}
```

```env
# .env (for Docker)
SMTP_SERVER=smtpdev
SMTP_PORT=25
SMTP_FROM_EMAIL=system@asteelflash.com
SMTP_PASSWORD=
ADMIN_EMAIL=admin@asteelflash.com
```

---

## 📝 Today's Changes (04/02/2026)

1. **Fixed SMTP STARTTLS error** - EmailService now uses `SecureSocketOptions.None` for port 25
2. **Skip authentication for local testing** - No auth required for smtp4dev
3. **Rebuilt Docker containers** with corrected EmailService
4. **Verified all containers running** - app, db, smtpdev
5. **Refined legacy project architecture** - Refactored services and modules for improved separation of concerns and maintainability (modular services, clearer DI registrations)
6. **Fixed runtime & build errors** - Resolved environment variable conflicts, added `EmailSettings` to `appsettings.Development.json` for local `dotnet run`, and cleared overriding PowerShell env vars
7. **Added email services and tests** - Implemented robust `EmailService` (MailKit) with local smtp4dev support and added unit/integration tests for email sending and notification flows


---

## 🎯 Original Project Goals

### Principales Améliorations
- **Refinement de la structure** : Réorganisation du code pour une meilleure maintenabilité et scalabilité.
- **Dockerisation** : Conteneurisation de l'application pour éviter les erreurs d'environnement.
- **Correction des erreurs** : Résolution des bugs et problèmes identifiés.
- **Système d'email avancé** : Notifications automatiques pour toutes les actions du système.
- **Alertes de matériels manquants** : Détection et alerte pour les équipements non retournés après mission.
- **Module d'ajout de matériel avec OCR** : Upload de bons d'achat avec extraction automatique de données via reconnaissance optique de caractères (OCR).

## Checklist d'Avancement sur 2 Semaines

### Semaine 1 : Fondation et Corrections
- [ ] **Analyse et correction des erreurs existantes**
  - Identifier les bugs dans le code actuel (Entity Framework, Blazor components, etc.).
  - Corriger les erreurs de compilation et runtime.
  - Tester les fonctionnalités de base (CRUD, authentification).
- [ ] **Refactorisation de la structure**
  - Réorganiser les services, modèles et contrôleurs pour une architecture plus modulaire.
  - Implémenter des patterns comme Repository et Unit of Work pour la scalabilité.
  - Améliorer la séparation des préoccupations (UI, logique métier, données).
- [ ] **Dockerisation de l'application**
  - Créer un Dockerfile pour l'application .NET 8.
  - Configurer Docker Compose pour inclure la base de données (SQL Server ou PostgreSQL).
  - Tester le déploiement en conteneurs locaux.

### Semaine 2 : Nouvelles Fonctionnalités
- [ ] **Implémentation du système d'email avancé**
  - Intégrer un service d'email (SendGrid, SMTP) pour les notifications.
  - Configurer des templates d'email pour les actions (ajout, modification, suppression).
  - Ajouter des emails pour les événements importants (nouvelles demandes, livraisons).
- [ ] **Alertes pour matériels manquants**
  - Développer une logique pour détecter les matériels non retournés après mission.
  - Implémenter des notifications (email, dashboard) pour les alertes.
  - Ajouter une interface pour gérer les alertes et les rappels.
- [ ] **Module d'ajout de matériel avec OCR**
  - Créer une interface d'upload pour les images de bons d'achat.
  - Intégrer une API OCR (comme Azure Computer Vision ou Tesseract) pour extraire le texte.
  - Automatiser l'ajout de références, quantités et détails des matériels à partir du texte extrait.
  - Valider et corriger manuellement si nécessaire.

## Suggestions d'Améliorations Futures
Pour rendre le système encore meilleur à ce stade :
- **Tests automatisés** : Ajouter des tests unitaires et d'intégration pour garantir la stabilité.
- **Sécurité renforcée** : Implémenter l'authentification à deux facteurs et chiffrer les données sensibles.
- **Performance** : Optimiser les requêtes EF Core et ajouter du caching.
- **UI/UX** : Améliorer l'interface avec des composants plus intuitifs et responsives.
- **Intégrations** : Connecter avec des APIs externes (fournisseurs, ERP) pour automatiser davantage.
- **Monitoring** : Ajouter des logs et des métriques pour surveiller la santé du système.
- **Documentation** : Compléter la documentation technique et utilisateur.
