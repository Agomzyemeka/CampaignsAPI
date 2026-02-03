# 🎯 Campaigns API

A production-ready RESTful API for campaign management built with ASP.NET Core 6.0, Entity Framework Core, and SQLite. Demonstrates enterprise-level patterns including JWT authentication, pagination, validation, Docker containerization, and comprehensive error handling.

[![.NET](https://img.shields.io/badge/.NET-6.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)

---

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Database Schema](#-database-schema)
- [Authentication](#-authentication)
- [Docker Deployment](#-docker-deployment)
- [Interview Discussion Points](#-interview-discussion-points)
- [Performance Optimizations](#-performance-optimizations)
- [Testing](#-testing)
- [Contributing](#-contributing)

---

## ✨ Features

### Core Functionality
- ✅ **CRUD Operations** - Create, Read, Update, Delete campaigns
- ✅ **JWT Authentication** - Secure stateless authentication
- ✅ **Pagination** - Efficient data retrieval with page navigation
- ✅ **Filtering & Sorting** - Query campaigns by status, search terms
- ✅ **Soft Delete** - Data preservation for audit trails
- ✅ **Input Validation** - FluentValidation for robust request validation

### Enterprise Features
- ✅ **Global Exception Handling** - Consistent error responses
- ✅ **Logging** - Comprehensive application logging
- ✅ **Health Checks** - Monitoring endpoints for load balancers
- ✅ **CORS Configuration** - Cross-origin resource sharing
- ✅ **Swagger/OpenAPI** - Interactive API documentation
- ✅ **Docker Support** - Containerization ready

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                              │
│    (React SPA / Mobile App / Third-party Services)              │
└───────────────────────────┬─────────────────────────────────────┘
                            │ HTTPS
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      API LAYER (Controllers)                     │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │ AuthController  │  │CampaignsController│ │HealthController│ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                                 │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐ │
│  │  AuthService    │  │  TokenService   │  │   Validators    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                             │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │              ApplicationDbContext (EF Core)                  ││
│  │  • Campaign Entity     • User Entity                        ││
│  │  • Index Configuration • Relationships                      ││
│  └─────────────────────────────────────────────────────────────┘│
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER (SQLite)                       │
│  ┌────────────────┐  ┌────────────────┐                         │
│  │   Campaigns    │  │     Users      │                         │
│  └────────────────┘  └────────────────┘                         │
└─────────────────────────────────────────────────────────────────┘
```

### Design Patterns Used

| Pattern | Usage | Benefits |
|---------|-------|----------|
| **Repository Pattern** | DbContext abstraction | Testability, separation of concerns |
| **DTO Pattern** | Request/Response objects | API contract stability |
| **Dependency Injection** | Services registration | Loose coupling, testability |
| **Middleware Pipeline** | Request processing | Modular request handling |
| **Options Pattern** | Configuration | Strongly-typed settings |

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | 6.0 | Web API Framework |
| Entity Framework Core | 6.0 | ORM / Data Access |
| SQLite | 3.x | Database |
| JWT Bearer | 6.0 | Authentication |
| FluentValidation | 11.x | Input Validation |
| Swagger/OpenAPI | 6.x | API Documentation |
| BCrypt.Net | 4.x | Password Hashing |
| Docker | 20.x+ | Containerization |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Docker](https://www.docker.com/get-started) (optional)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/Agomzyemeka/CampaignsAPI.git
cd CampaignsAPI

# Restore dependencies
dotnet restore

# Run the application (database auto-migrates on startup)
dotnet run
```

### 🌐 Access the Application

| URL | Description |
|-----|-------------|
| https://localhost:7203 | **Frontend** - Campaigns Manager UI |
| https://localhost:7203/swagger | **Swagger** - API Documentation |
| https://localhost:7203/health | **Health Check** - API Status |
| http://localhost:5110 | Frontend (HTTP) |
| http://localhost:5110/swagger | Swagger (HTTP) |

### 🔑 Demo Credentials

| Email | Password | Role |
|-------|----------|------|
| `admin@campaigns.com` | `Admin@123` | Admin |
| `demo@campaigns.com` | `Admin@123` | User |

### Configuration

Update `appsettings.json` for your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=CampaignsAPI.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "CampaignsAPI",
    "Audience": "CampaignsAPIClients",
    "ExpiryMinutes": "60"
  }
}
```

---

## 📚 API Documentation

### Base URL
```
https://localhost:7203/api
```

### Interactive Documentation
Swagger UI is available at: `https://localhost:7203/swagger`

### Authentication Endpoints

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "admin@campaigns.com",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "userId": 1,
    "username": "admin",
    "email": "admin@campaigns.com",
    "role": "Admin",
    "expiresAt": "2024-01-01T12:00:00Z"
  }
}
```

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "SecurePass@123",
  "fullName": "New User"
}
```

### Campaign Endpoints

#### Get All Campaigns (Paginated)
```http
GET /api/campaigns?pageNumber=1&pageSize=10&status=Active&searchTerm=summer
Authorization: Bearer {token}
```

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Page number |
| pageSize | int | 10 | Items per page (max 100) |
| status | enum | - | Filter by status |
| searchTerm | string | - | Search in name/description |
| sortBy | string | CreatedAt | Sort field |
| sortOrder | string | desc | Sort direction |

#### Get Campaign by ID
```http
GET /api/campaigns/{id}
Authorization: Bearer {token}
```

#### Create Campaign
```http
POST /api/campaigns
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Summer Sale 2024",
  "description": "Major summer promotion",
  "budget": 50000,
  "startDate": "2024-06-01",
  "endDate": "2024-08-31",
  "status": 0
}
```

#### Update Campaign
```http
PUT /api/campaigns/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Updated Campaign Name",
  "budget": 75000,
  "status": 1
}
```

#### Delete Campaign
```http
DELETE /api/campaigns/{id}
Authorization: Bearer {token}
```

#### Get Statistics
```http
GET /api/campaigns/stats
Authorization: Bearer {token}
```

### Health Check
```http
GET /health
```

---

## 📁 Project Structure

```
CampaignsAPI/
├── Controllers/                 # API Controllers
│   ├── AuthController.cs       # Authentication endpoints
│   └── CampaignsController.cs  # Campaign CRUD endpoints
├── Data/
│   └── ApplicationDbContext.cs # EF Core DbContext
├── DTOs/
│   └── CampaignDtos.cs        # Data Transfer Objects
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs
├── Migrations/                 # EF Core Migrations
├── Models/
│   ├── Campaign.cs            # Campaign entity
│   └── User.cs                # User entity
├── Services/
│   ├── AuthService.cs         # Authentication logic
│   └── TokenService.cs        # JWT token handling
├── Validators/
│   └── CampaignValidators.cs  # FluentValidation rules
├── ClientApp/
│   └── index.html             # Simple React-like frontend
├── appsettings.json           # Configuration
├── Dockerfile                  # Container definition
├── docker-compose.yml         # Container orchestration
├── Program.cs                  # Application entry point
└── README.md                   # This file
```

---

## 🗄️ Database Schema

### Campaigns Table

| Column | Type | Constraints | Index |
|--------|------|-------------|-------|
| Id | INT | PK, Identity | Clustered |
| Name | NVARCHAR(200) | NOT NULL | IX_Campaigns_Name |
| Description | NVARCHAR(1000) | | |
| Budget | DECIMAL(18,2) | NOT NULL | |
| StartDate | DATETIME | NOT NULL | IX_Campaigns_DateRange |
| EndDate | DATETIME | NOT NULL | IX_Campaigns_DateRange |
| Status | INT | NOT NULL | IX_Campaigns_Status |
| CreatedBy | INT | FK → Users | IX_Campaigns_CreatedBy |
| CreatedAt | DATETIME | NOT NULL | |
| UpdatedAt | DATETIME | | |
| IsDeleted | BIT | NOT NULL | IX_Campaigns_IsDeleted |

### Users Table

| Column | Type | Constraints | Index |
|--------|------|-------------|-------|
| Id | INT | PK, Identity | Clustered |
| Username | NVARCHAR(100) | NOT NULL | IX_Users_Username |
| Email | NVARCHAR(255) | NOT NULL, UNIQUE | IX_Users_Email_Unique |
| PasswordHash | NVARCHAR(MAX) | NOT NULL | |
| FullName | NVARCHAR(100) | | |
| Role | NVARCHAR(50) | NOT NULL | |
| IsActive | BIT | NOT NULL | IX_Users_IsActive |
| CreatedAt | DATETIME | NOT NULL | |
| LastLoginAt | DATETIME | | |

### Index Strategy

```csharp
// Composite index for date range queries
entity.HasIndex(e => new { e.StartDate, e.EndDate })
    .HasDatabaseName("IX_Campaigns_DateRange");

// Index on Status for filtering
entity.HasIndex(e => e.Status)
    .HasDatabaseName("IX_Campaigns_Status");

// Index on IsDeleted for soft delete filtering
entity.HasIndex(e => e.IsDeleted)
    .HasDatabaseName("IX_Campaigns_IsDeleted");
```

---

## 🔐 Authentication

### JWT Flow

```
┌─────────┐          ┌─────────┐          ┌─────────┐
│  Client │          │   API   │          │   DB    │
└────┬────┘          └────┬────┘          └────┬────┘
     │                    │                    │
     │ POST /auth/login   │                    │
     │ {email, password}  │                    │
     │───────────────────>│                    │
     │                    │  Verify Password   │
     │                    │───────────────────>│
     │                    │    User Data       │
     │                    │<───────────────────│
     │                    │                    │
     │   {token, user}    │ Generate JWT       │
     │<───────────────────│                    │
     │                    │                    │
     │ GET /campaigns     │                    │
     │ Authorization:     │                    │
     │ Bearer {token}     │                    │
     │───────────────────>│                    │
     │                    │ Validate Token     │
     │                    │ Extract Claims     │
     │                    │                    │
     │                    │  Query Data        │
     │                    │───────────────────>│
     │   {campaigns}      │   Results          │
     │<───────────────────│<───────────────────│
```

### Token Structure

```javascript
// JWT Payload
{
  "sub": "1",                    // User ID
  "email": "admin@campaigns.com",
  "unique_name": "admin",
  "role": "Admin",
  "name": "System Administrator",
  "jti": "guid-here",           // Unique token ID
  "userId": "1",
  "exp": 1704067200,            // Expiration
  "iss": "CampaignsAPI",
  "aud": "CampaignsAPIClients"
}
```

---

## 🐳 Docker Deployment

### Build and Run

```bash
# Build the image
docker build -t campaigns-api .

# Run container
docker run -d -p 5000:80 --name campaigns-api campaigns-api

# Or use docker-compose (includes Redis & RabbitMQ)
docker-compose up --build -d
```

### Services Included

| Service | Port | Description |
|---------|------|-------------|
| `campaigns-api` | 5000 | ASP.NET Core API |
| `campaigns-redis` | 6379 | Redis Cache (optional) |
| `campaigns-rabbitmq` | 5672, 15672 | Message Queue + Management UI |

### After Docker Deployment

- **API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **RabbitMQ UI**: http://localhost:15672 (guest/guest)

### Production Deployment

```yaml
# docker-compose.prod.yml
services:
  api:
    image: campaigns-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - JwtSettings__SecretKey=${JWT_SECRET}
    volumes:
      - campaigns-data:/app/data
    deploy:
      replicas: 3
      restart_policy:
        condition: on-failure
```

---

## 🖥️ Frontend Application

The project includes a **Single Page Application (SPA)** built with vanilla JavaScript.

### Location
`ClientApp/index.html` - served at the root URL

### Features
- ✅ **Login/Logout** - JWT authentication with localStorage
- ✅ **Dashboard** - Statistics overview (total, active, budget)
- ✅ **Campaign Management** - Create, Edit, Delete campaigns
- ✅ **Pagination** - Navigate through campaign lists
- ✅ **Responsive Design** - Mobile-friendly UI
- ✅ **Auto-logout** - Redirects on token expiration

### How It Works

```javascript
// State management
let state = {
    token: localStorage.getItem('token'),
    user: JSON.parse(localStorage.getItem('user')),
    campaigns: [],
    pagination: { pageNumber: 1, pageSize: 10 }
};

// API calls with JWT
function getHeaders() {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${state.token}`
    };
}
```

---

## 💬 Interview Discussion Points

### 1. Architecture Decisions

**Q: Why did you choose this architecture?**

> I implemented a layered architecture with clear separation of concerns:
> - **Controllers**: Handle HTTP requests/responses
> - **Services**: Business logic
> - **Data Layer**: Database operations via EF Core
> 
> This promotes testability, maintainability, and allows independent evolution of each layer.

### 2. Database Design

**Q: Explain your indexing strategy.**

> I created indexes based on query patterns:
> - `IX_Campaigns_Status`: Filtering by status is frequent
> - `IX_Campaigns_DateRange`: Composite index for date range queries
> - `IX_Campaigns_IsDeleted`: Soft delete filtering on every query
> - `IX_Users_Email_Unique`: Fast lookups and uniqueness constraint

### 3. Performance Optimizations

**Q: How did you optimize for performance?**

> - **AsNoTracking()**: Used for read-only queries, ~30% performance improvement
> - **Pagination**: Prevents loading entire datasets
> - **Projection**: Select only needed columns in queries
> - **Indexes**: Strategic indexing based on query patterns
> - **Async/Await**: Non-blocking I/O for scalability

### 4. Security Considerations

**Q: How do you handle security?**

> - **JWT Authentication**: Stateless, scalable authentication
> - **BCrypt Password Hashing**: Industry-standard, salted hashing
> - **Input Validation**: FluentValidation for all requests
> - **Authorization**: Role-based and ownership checks
> - **HTTPS**: Required in production
> - **No Sensitive Data in Logs**: Password, tokens never logged

### 5. Scalability

**Q: How would you scale this application?**

> - **Horizontal Scaling**: Stateless JWT allows multiple instances
> - **Database**: Migrate to SQL Server/PostgreSQL for larger scale
> - **Caching**: Add Redis for frequently accessed data
> - **Message Queue**: Use RabbitMQ for async operations
> - **Read Replicas**: Separate read/write database connections

### 6. Error Handling

**Q: How do you handle errors?**

> - **Global Exception Middleware**: Catches all unhandled exceptions
> - **Consistent Error Response**: ApiResponse<T> wrapper for all responses
> - **Logging**: All errors logged with context
> - **Environment-Specific Details**: Stack traces in dev, generic messages in prod

---

## ⚡ Performance Optimizations

### 1. AsNoTracking for Read Operations

```csharp
// BAD: Tracks entities (slower, uses more memory)
var campaigns = await _context.Campaigns.ToListAsync();

// GOOD: No tracking for read-only queries
var campaigns = await _context.Campaigns
    .AsNoTracking()
    .ToListAsync();
```

### 2. Pagination

```csharp
// Efficient pagination with Skip/Take
var campaigns = await query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### 3. Projection

```csharp
// Only select needed fields
var campaigns = await _context.Campaigns
    .Select(c => new CampaignResponseDto
    {
        Id = c.Id,
        Name = c.Name,
        // ... only needed fields
    })
    .ToListAsync();
```

---

## 🧪 Testing

### Run Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Test Categories

- **Unit Tests**: Service layer, validators
- **Integration Tests**: API endpoints
- **Performance Tests**: Load testing with k6/JMeter

---

## 📝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 👤 Author

**Agomoh Emeka**
- GitHub: [@Agomzyemeka](https://github.com/Agomzyemeka)

---

## 🙏 Acknowledgments

- ASP.NET Core Team
- Entity Framework Core Team
- FluentValidation Team

---

## 📊 Campaign Status Values

| Value | Name | Description |
|-------|------|-------------|
| 0 | Draft | Campaign is being prepared |
| 1 | Active | Campaign is currently running |
| 2 | Paused | Campaign is temporarily stopped |
| 3 | Completed | Campaign has finished |
| 4 | Cancelled | Campaign was cancelled |

---

<p align="center">
  Made with ❤️ and C#
</p>
