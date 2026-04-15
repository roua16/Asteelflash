# ITStockM Swagger API Documentation

**Complete API Reference for ITStockM Inventory Management System**

---

## Table of Contents

1. [Overview](#overview)
2. [API Endpoints](#api-endpoints)
3. [DTOs & Data Models](#dtos--data-models)
4. [Authentication](#authentication)
5. [Error Responses](#error-responses)
6. [Pagination & Filtering](#pagination--filtering)
7. [Code Examples](#code-examples)

---

## Overview

### Base URL

```
Local Development:  http://localhost:8080/api/v1
Production:         https://yourdomain.com/api/v1
```

### Swagger UI Access

- **Interactive API Explorer**: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **OpenAPI JSON Schema**: [http://localhost:8080/swagger/v1/swagger.json](http://localhost:8080/swagger/v1/swagger.json)

### Authentication

All endpoints require authentication via **Bearer Token** (JWT).

```bash
Authorization: Bearer <jwt_token>
```

---

## API Endpoints

### Health Check

**GET** `/api/v1/Health`

Check system health and database connectivity.

**Response: 200 OK**
```json
{
  "status": "Healthy",
  "timestamp": "2024-04-15T05:00:00Z",
  "database": "Connected",
  "version": "1.0.0"
}
```

---

### Maintenance Tickets

#### Get All Maintenance Tickets

**GET** `/api/v1/Maintenance`

Retrieve paginated list of maintenance tickets with optional filters.

**Query Parameters:**
- `pageNumber` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 50)
- `status` (string, optional): Filter by status (Open, InProgress, Closed)
- `priority` (string, optional): Filter by priority (Low, Medium, High)
- `sortBy` (string, optional): Sort field (CreatedAt, UpdatedAt, Title)
- `ascending` (bool, optional): Sort direction (default: false)

**Response: 200 OK**
```json
{
  "items": [
    {
      "id": "uuid",
      "title": "Replace cooling fan",
      "description": "Fan bearing making noise",
      "status": "Open",
      "priority": "High",
      "assignedTo": "John Doe",
      "createdAt": "2024-04-14T10:30:00Z",
      "updatedAt": "2024-04-14T10:30:00Z",
      "resolvedAt": null,
      "resolution": null,
      "estimatedCost": 150.00
    }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 127
}
```

**Response Codes:**
- `200` OK - Success
- `400` Bad Request - Invalid parameters
- `401` Unauthorized - Missing or invalid token
- `500` Server Error - Database or processing error

---

#### Create Maintenance Ticket

**POST** `/api/v1/Maintenance`

Create a new maintenance ticket.

**Request Body:**
```json
{
  "title": "Replace cooling fan",
  "description": "Fan bearing making noise, needs replacement",
  "priority": "High",
  "assignedTo": "John Doe",
  "estimatedCost": 150.00
}
```

**Response: 201 Created**
```json
{
  "id": "uuid",
  "title": "Replace cooling fan",
  "description": "Fan bearing making noise, needs replacement",
  "status": "Open",
  "priority": "High",
  "assignedTo": "John Doe",
  "createdAt": "2024-04-15T05:15:00Z",
  "updatedAt": "2024-04-15T05:15:00Z",
  "estimatedCost": 150.00
}
```

**Response Codes:**
- `201` Created - Success
- `400` Bad Request - Validation error
- `401` Unauthorized - Not authenticated
- `422` Unprocessable Entity - Invalid data
- `500` Server Error

---

#### Update Maintenance Ticket

**PUT** `/api/v1/Maintenance/{id}`

Update an existing maintenance ticket.

**Path Parameters:**
- `id` (string, required): Ticket ID (UUID)

**Request Body:**
```json
{
  "id": "uuid",
  "title": "Replace cooling fan (URGENT)",
  "description": "Fan bearing failing, immediate replacement needed",
  "status": "InProgress",
  "priority": "Critical",
  "assignedTo": "Jane Smith",
  "estimatedCost": 175.00
}
```

**Response: 200 OK**
```json
{
  "id": "uuid",
  "title": "Replace cooling fan (URGENT)",
  "status": "InProgress",
  "priority": "Critical",
  "updatedAt": "2024-04-15T05:20:00Z"
}
```

**Response Codes:**
- `200` OK - Success
- `400` Bad Request - Validation error
- `401` Unauthorized - Not authenticated
- `404` Not Found - Ticket not found
- `422` Unprocessable Entity - Invalid data
- `500` Server Error

---

#### Close Maintenance Ticket

**POST** `/api/v1/Maintenance/{id}/Close`

Mark a maintenance ticket as closed with resolution details.

**Path Parameters:**
- `id` (string, required): Ticket ID (UUID)

**Request Body:**
```json
{
  "resolution": "Replaced cooling fan with new unit. System temperature normalized.",
  "actualCost": 165.50
}
```

**Response: 200 OK**
```json
{
  "id": "uuid",
  "status": "Closed",
  "resolvedAt": "2024-04-15T05:25:00Z",
  "actualCost": 165.50
}
```

**Response Codes:**
- `200` OK - Success
- `404` Not Found - Ticket not found
- `400` Bad Request - Already closed or validation error
- `401` Unauthorized - Not authenticated
- `500` Server Error

---

### Asset Lifecycle

#### Get Asset Lifecycle Records

**GET** `/api/v1/AssetLifecycle`

Retrieve all asset lifecycle records.

**Query Parameters:**
- `assetId` (string, optional): Filter by asset ID
- `stage` (string, optional): Filter by stage (Active, Maintenance, Depreciated, Retired)
- `pageNumber` (int, optional): Page number
- `pageSize` (int, optional): Items per page

**Response: 200 OK**
```json
{
  "items": [
    {
      "id": "uuid",
      "assetId": "asset-uuid",
      "assetName": "Server Node 1",
      "stage": "Active",
      "purchaseDate": "2022-01-15T00:00:00Z",
      "deploymentDate": "2022-02-01T00:00:00Z",
      "maintenanceDate": null,
      "depreciationDate": null,
      "retirementDate": null,
      "notes": "Primary production server",
      "transitionHistory": [
        {
          "fromStage": "Planning",
          "toStage": "Active",
          "transitionDate": "2022-02-01T00:00:00Z",
          "notes": "Successfully deployed"
        }
      ]
    }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 24
}
```

**Response Codes:**
- `200` OK - Success
- `400` Bad Request - Invalid parameters
- `401` Unauthorized - Not authenticated
- `500` Server Error

---

#### Create Asset Lifecycle Record

**POST** `/api/v1/AssetLifecycle`

Create a new asset lifecycle record.

**Request Body:**
```json
{
  "assetId": "asset-uuid",
  "stage": "Active",
  "purchaseDate": "2024-01-01T00:00:00Z",
  "deploymentDate": "2024-02-01T00:00:00Z",
  "notes": "New server deployment"
}
```

**Response: 201 Created**
```json
{
  "id": "lifecycle-uuid",
  "assetId": "asset-uuid",
  "stage": "Active",
  "createdAt": "2024-04-15T05:30:00Z"
}
```

---

#### Update Asset Lifecycle Stage

**PUT** `/api/v1/AssetLifecycle/{id}/TransitionStage`

Transition an asset to a different lifecycle stage.

**Path Parameters:**
- `id` (string, required): Lifecycle record ID

**Request Body:**
```json
{
  "newStage": "Maintenance",
  "transitionDate": "2024-04-15T00:00:00Z",
  "notes": "Preventive maintenance scheduled"
}
```

**Response: 200 OK**
```json
{
  "id": "lifecycle-uuid",
  "stage": "Maintenance",
  "maintenanceDate": "2024-04-15T00:00:00Z"
}
```

---

### Asset Predictions

#### Get Asset Health Predictions

**GET** `/api/v1/Predictions`

Retrieve health predictions for assets.

**Query Parameters:**
- `riskLevel` (string, optional): Filter by risk (Low, Medium, High, Critical)
- `assetId` (string, optional): Filter by asset ID
- `pageNumber` (int, optional): Page number
- `pageSize` (int, optional): Items per page

**Response: 200 OK**
```json
{
  "items": [
    {
      "id": "prediction-uuid",
      "assetId": "asset-uuid",
      "assetName": "Database Server",
      "predictedFailureDate": "2024-06-15T00:00:00Z",
      "healthScore": 65,
      "riskLevel": "High",
      "indicators": [
        {
          "metric": "DiskUsage",
          "value": 87.5,
          "threshold": 85.0,
          "status": "Warning"
        },
        {
          "metric": "MemoryPressure",
          "value": 72.1,
          "threshold": 80.0,
          "status": "Healthy"
        }
      ],
      "recommendedActions": [
        "Archive old logs",
        "Upgrade storage",
        "Schedule maintenance window"
      ],
      "lastUpdated": "2024-04-15T04:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 50,
  "totalCount": 45
}
```

**Response Codes:**
- `200` OK - Success
- `401` Unauthorized - Not authenticated
- `500` Server Error

---

#### Recalculate Asset Predictions

**POST** `/api/v1/Predictions/Recalculate`

Trigger a recalculation of all asset health predictions.

**Request Body (Optional):**
```json
{
  "assetIds": ["asset-uuid-1", "asset-uuid-2"]  // Omit for all assets
}
```

**Response: 200 OK**
```json
{
  "status": "InProgress",
  "assetsQueued": 45,
  "message": "Prediction recalculation started"
}
```

**Response Codes:**
- `200` OK - Success
- `400` Bad Request - Invalid asset IDs
- `401` Unauthorized - Not authenticated
- `500` Server Error

---

---

## DTOs & Data Models

### Common DTOs

#### MaintenanceTicketDto

```typescript
{
  id: string;                    // UUID
  title: string;                 // Required, max 256 chars
  description: string;           // Optional, max 2000 chars
  status: "Open" | "InProgress" | "Closed";
  priority: "Low" | "Medium" | "High" | "Critical";
  assignedTo?: string;           // Optional, employee name
  createdAt: Date;               // ISO 8601
  updatedAt: Date;               // ISO 8601
  resolvedAt?: Date;             // ISO 8601, null if not resolved
  resolution?: string;           // Resolution notes, max 2000 chars
  estimatedCost?: number;        // Decimal(10,2)
  actualCost?: number;           // Decimal(10,2)
}
```

#### AssetLifecycleRecordDto

```typescript
{
  id: string;                    // UUID
  assetId: string;               // Reference to asset
  assetName: string;             // Denormalized for convenience
  stage: "Active" | "Maintenance" | "Depreciated" | "Retired";
  purchaseDate?: Date;           // ISO 8601
  deploymentDate?: Date;         // ISO 8601
  maintenanceDate?: Date;        // ISO 8601
  depreciationDate?: Date;       // ISO 8601
  retirementDate?: Date;         // ISO 8601
  notes?: string;                // Max 1000 chars
  transitionHistory: Array<{
    fromStage: string;
    toStage: string;
    transitionDate: Date;
    notes?: string;
  }>;
  createdAt: Date;
  updatedAt: Date;
}
```

#### AssetPredictionDto

```typescript
{
  id: string;                    // UUID
  assetId: string;               // Reference to asset
  assetName: string;             // Denormalized for convenience
  predictedFailureDate?: Date;   // ISO 8601
  healthScore: number;           // 0-100
  riskLevel: "Low" | "Medium" | "High" | "Critical";
  indicators: Array<{
    metric: string;              // e.g., "DiskUsage", "MemoryPressure"
    value: number;               // Current value
    threshold: number;            // Warning threshold
    status: "Healthy" | "Warning" | "Critical";
  }>;
  recommendedActions: string[];  // Array of actionable recommendations
  lastUpdated: Date;             // ISO 8601
}
```

---

## Authentication

### Obtaining a Token

**POST** `/auth/login`

```bash
curl -X POST http://localhost:8080/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@asteelflash.com",
    "password": "password123"
  }'
```

**Response: 200 OK**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "user": {
    "id": "user-uuid",
    "email": "user@asteelflash.com",
    "name": "John Doe",
    "roles": ["User", "Admin"]
  }
}
```

### Using the Token

Include the token in all subsequent requests:

```bash
curl -X GET http://localhost:8080/api/v1/Maintenance \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### Token Refresh

Tokens expire after 1 hour. To refresh:

**POST** `/auth/refresh`

```bash
curl -X POST http://localhost:8080/auth/refresh \
  -H "Authorization: Bearer <expired_token>"
```

---

## Error Responses

### Standard Error Format

All error responses follow this structure:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "title": ["Title is required"],
    "priority": ["Priority must be one of: Low, Medium, High, Critical"]
  }
}
```

### Common Error Codes

| Code | Meaning | Solution |
|------|---------|----------|
| 400 | Bad Request | Check request parameters and body format |
| 401 | Unauthorized | Include valid Bearer token in Authorization header |
| 403 | Forbidden | User doesn't have permission for this resource |
| 404 | Not Found | Resource doesn't exist or wrong ID provided |
| 422 | Unprocessable Entity | Validation error - see `errors` field for details |
| 500 | Server Error | Database or processing error - try again or contact support |

### Example Error Response (Validation)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Validation Error",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "title": [
      "Title must be between 3 and 256 characters"
    ],
    "estimatedCost": [
      "Estimated cost must be greater than 0"
    ]
  }
}
```

---

## Pagination & Filtering

### Pagination

All list endpoints support pagination:

**Query Parameters:**
- `pageNumber` (int, default: 1): Page to retrieve
- `pageSize` (int, default: 50, max: 500): Items per page

**Example:**
```bash
GET /api/v1/Maintenance?pageNumber=2&pageSize=25
```

**Response includes:**
```json
{
  "items": [...],
  "pageNumber": 2,
  "pageSize": 25,
  "totalCount": 127
}
```

### Filtering

Endpoints support filter parameters specific to their resource:

**Maintenance Tickets:**
```bash
GET /api/v1/Maintenance?status=Open&priority=High&pageSize=10
```

**Asset Lifecycle:**
```bash
GET /api/v1/AssetLifecycle?stage=Active&pageSize=20
```

**Asset Predictions:**
```bash
GET /api/v1/Predictions?riskLevel=Critical&pageSize=15
```

### Sorting

**Query Parameters:**
- `sortBy` (string): Field to sort by
- `ascending` (bool, default: false): Sort direction

**Example:**
```bash
GET /api/v1/Maintenance?sortBy=CreatedAt&ascending=true
```

---

## Code Examples

### C# / .NET

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class ITStockMApiClient
{
    private readonly HttpClient _client;
    private readonly string _token;

    public ITStockMApiClient(string token)
    {
        _client = new HttpClient { BaseAddress = new Uri("http://localhost:8080/api/v1") };
        _token = token;
    }

    public async Task GetMaintenanceTicketsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Maintenance?pageSize=50");
        request.Headers.Add("Authorization", $"Bearer {_token}");

        var response = await _client.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadAsAsync<dynamic>();
            Console.WriteLine($"Total tickets: {data.totalCount}");
        }
    }
}
```

### JavaScript / TypeScript

```typescript
class ITStockMApiClient {
  constructor(private token: string) {}

  async getMaintenanceTickets(pageNumber = 1, pageSize = 50) {
    const response = await fetch(
      `http://localhost:8080/api/v1/Maintenance?pageNumber=${pageNumber}&pageSize=${pageSize}`,
      {
        headers: {
          'Authorization': `Bearer ${this.token}`,
          'Content-Type': 'application/json'
        }
      }
    );
    return response.json();
  }

  async createMaintenanceTicket(ticket: MaintenanceTicketDto) {
    const response = await fetch(
      'http://localhost:8080/api/v1/Maintenance',
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${this.token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(ticket)
      }
    );
    return response.json();
  }
}
```

### Python

```python
import requests

class ITStockMApiClient:
    def __init__(self, token):
        self.base_url = "http://localhost:8080/api/v1"
        self.headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json"
        }

    def get_maintenance_tickets(self, page_number=1, page_size=50):
        response = requests.get(
            f"{self.base_url}/Maintenance",
            params={"pageNumber": page_number, "pageSize": page_size},
            headers=self.headers
        )
        return response.json()

    def create_maintenance_ticket(self, ticket_data):
        response = requests.post(
            f"{self.base_url}/Maintenance",
            json=ticket_data,
            headers=self.headers
        )
        return response.json()
```

---

## Support & Additional Resources

- **Architecture Documentation**: See [ARCHITECTURE_CHECKLIST.md](./ARCHITECTURE_CHECKLIST.md)
- **API Testing**: See [API_TESTING.md](./API_TESTING.md)
- **Accessibility**: See [ACCESSIBILITY.md](./ACCESSIBILITY.md)
- **Navigation & Ports**: See [PORTS_AND_NAVIGATION.md](./PORTS_AND_NAVIGATION.md)
- **Quick Start**: See [QUICK_START.md](./QUICK_START.md)

---

**Last Updated**: April 15, 2024  
**API Version**: 1.0  
**Status**: Production-Ready
