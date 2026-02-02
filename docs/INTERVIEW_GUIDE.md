# 🎓 Interview Preparation Guide - Campaigns API

This document provides detailed explanations of key technical decisions and concepts in the Campaigns API project. Use this guide to prepare for technical interviews.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture Deep Dive](#architecture-deep-dive)
3. [Code Walkthrough](#code-walkthrough)
4. [Common Interview Questions](#common-interview-questions)
5. [Live Demo Script](#live-demo-script)

---

## Project Overview

### What This Project Demonstrates

| Skill Area | Implementation |
|------------|----------------|
| **C# / .NET Core** | Controllers, services, middleware, async/await |
| **Database Design** | EF Core, migrations, indexing, relationships |
| **API Design** | RESTful principles, versioning, documentation |
| **Security** | JWT authentication, password hashing, authorization |
| **DevOps** | Docker, docker-compose, health checks |
| **Best Practices** | SOLID principles, clean code, error handling |

---

## Architecture Deep Dive

### 1. Dependency Injection (DI)

**What it is:** A design pattern where dependencies are provided to a class rather than created by it.

**Code Example:**
```csharp
// Registration in Program.cs
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Usage in Controller (dependency is injected via constructor)
public class CampaignsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public CampaignsController(ApplicationDbContext context)
    {
        _context = context; // Injected automatically by DI container
    }
}
```

**Interview Points:**
- **AddScoped**: One instance per HTTP request (most common for web APIs)
- **AddSingleton**: One instance for entire application lifetime
- **AddTransient**: New instance every time it's requested
- **Benefits**: Loose coupling, testability (can inject mocks), configurability

### 2. Entity Framework Core

**What it is:** An Object-Relational Mapper (ORM) that allows working with databases using C# objects.

**Key Concepts:**

```csharp
// DbContext - Unit of Work pattern
public class ApplicationDbContext : DbContext
{
    public DbSet<Campaign> Campaigns { get; set; } // Repository for campaigns
    
    // Fluent API for configuration
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.Id);                    // Primary key
            entity.HasIndex(e => e.Status);             // Index for performance
            entity.Property(e => e.Budget)
                  .HasColumnType("decimal(18,2)");       // Precision
        });
    }
}
```

**Interview Points:**
- **Lazy Loading vs Eager Loading**: `Include()` for eager loading related data
- **Tracking vs No Tracking**: `AsNoTracking()` for read-only operations
- **Migrations**: Code-first database schema management
- **Query Translation**: LINQ to SQL translation

### 3. JWT Authentication

**What it is:** JSON Web Tokens for stateless authentication.

**Token Structure:**
```
HEADER.PAYLOAD.SIGNATURE

Header:  { "alg": "HS256", "typ": "JWT" }
Payload: { "sub": "1", "email": "user@example.com", "exp": 1234567890 }
Signature: HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)
```

**Flow:**
1. User sends credentials to `/auth/login`
2. Server validates credentials, generates JWT
3. Client stores token (localStorage, sessionStorage)
4. Client sends token in `Authorization: Bearer {token}` header
5. Server validates token on each request

**Code:**
```csharp
// Token Generation
public string GenerateToken(User user)
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _issuer,
        audience: _audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

**Interview Points:**
- **Stateless**: No server-side session storage needed
- **Self-contained**: Contains all user info in the token
- **Security**: Signed to prevent tampering, but payload is NOT encrypted
- **Expiration**: Tokens should have short lifetimes
- **Refresh Tokens**: For extending sessions without re-authentication

### 4. Middleware Pipeline

**What it is:** A chain of components that process HTTP requests and responses.

```
Request  → Exception Handling → CORS → Authentication → Authorization → Controller
Response ← Exception Handling ← CORS ← Authentication ← Authorization ← Controller
```

**Order Matters:**
```csharp
// Program.cs - Middleware registration
app.UseExceptionHandling();    // 1. Catch all exceptions
app.UseHttpsRedirection();      // 2. Redirect HTTP to HTTPS
app.UseCors("AllowAll");        // 3. CORS headers
app.UseAuthentication();        // 4. Validate JWT token
app.UseAuthorization();         // 5. Check permissions
app.MapControllers();           // 6. Route to controller
```

### 5. Async/Await

**What it is:** Asynchronous programming model for non-blocking I/O operations.

```csharp
// Synchronous (BLOCKS the thread)
public Campaign GetCampaign(int id)
{
    return _context.Campaigns.Find(id); // Thread waits here
}

// Asynchronous (RELEASES the thread)
public async Task<Campaign> GetCampaignAsync(int id)
{
    return await _context.Campaigns.FindAsync(id); // Thread is free to handle other requests
}
```

**Interview Points:**
- **Thread Pool**: Async releases threads back to pool while waiting
- **Scalability**: Handle more concurrent requests with same resources
- **I/O Bound**: Best for database, network, file operations
- **All the way down**: If a method uses async, callers should be async too

---

## Code Walkthrough

### Campaign Controller - GetCampaigns Method

```csharp
[HttpGet]
public async Task<IActionResult> GetCampaigns(
    [FromQuery] int pageNumber = 1,           // Pagination
    [FromQuery] int pageSize = 10,
    [FromQuery] CampaignStatus? status = null, // Filtering
    [FromQuery] string? searchTerm = null)
{
    // 1. Validate pagination parameters
    pageNumber = Math.Max(1, pageNumber);
    pageSize = Math.Clamp(pageSize, 1, 100);

    // 2. Build base query with AsNoTracking for performance
    var query = _context.Campaigns
        .AsNoTracking()
        .Include(c => c.Creator)        // Eager load related data
        .Where(c => !c.IsDeleted);      // Soft delete filter

    // 3. Apply filters
    if (status.HasValue)
        query = query.Where(c => c.Status == status.Value);

    if (!string.IsNullOrWhiteSpace(searchTerm))
        query = query.Where(c => c.Name.Contains(searchTerm));

    // 4. Get total count BEFORE pagination
    var totalRecords = await query.CountAsync();

    // 5. Apply pagination
    var campaigns = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(c => new CampaignResponseDto { /* projection */ })
        .ToListAsync();

    // 6. Return paginated response
    return Ok(new ApiResponse<PagedResponse<CampaignResponseDto>>
    {
        Success = true,
        Data = new PagedResponse<CampaignResponseDto>
        {
            Data = campaigns,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
        }
    });
}
```

**Key Optimizations:**
1. `AsNoTracking()` - 30% faster for read operations
2. Projection with `Select()` - Only retrieves needed columns
3. Pagination - Prevents loading entire database
4. Index usage - `Where` clauses use indexed columns

---

## Common Interview Questions

### Q1: "Why did you use SQLite instead of SQL Server?"

**Answer:**
> SQLite was chosen for development speed and simplicity. It's file-based, requires no separate server installation, and is perfect for demos and development. The code is designed to easily switch to SQL Server for production - just change the connection string and provider. EF Core's abstraction means the business logic doesn't change.

### Q2: "How do you handle concurrent access to the database?"

**Answer:**
> - EF Core uses optimistic concurrency by default
> - Each request gets its own DbContext instance (Scoped lifetime)
> - For critical updates, I could add a RowVersion/Timestamp column for concurrency checking
> - Transactions wrap multi-step operations

### Q3: "Why soft delete instead of hard delete?"

**Answer:**
> - **Audit Trail**: Data is preserved for compliance/history
> - **Recovery**: Accidentally deleted data can be restored
> - **Referential Integrity**: No orphaned foreign key references
> - **Performance**: Simple boolean filter vs DELETE operation
> - **Trade-off**: Requires filtering on every query

### Q4: "How would you improve this for production?"

**Answer:**
> 1. **Database**: Switch to SQL Server/PostgreSQL
> 2. **Caching**: Add Redis for frequently accessed data
> 3. **Logging**: Integrate with ELK Stack or Application Insights
> 4. **API Gateway**: Add rate limiting, API versioning
> 5. **Message Queue**: RabbitMQ for async operations
> 6. **Monitoring**: Add Prometheus/Grafana metrics
> 7. **CI/CD**: GitHub Actions for automated deployment
> 8. **Tests**: Add comprehensive unit and integration tests

### Q5: "Explain your error handling strategy"

**Answer:**
> I use a global exception handling middleware that:
> - Catches all unhandled exceptions in one place
> - Returns consistent error response format
> - Logs errors with full context
> - Hides sensitive details in production
> - Returns appropriate HTTP status codes based on exception type

---

## Live Demo Script

### 1. Start the Application

```bash
cd CampaignsAPI
dotnet run
# Open https://localhost:7001 for Swagger UI
```

### 2. Show Swagger UI

"Here's the Swagger documentation auto-generated from our code. It shows all endpoints, their parameters, and you can even test them directly."

### 3. Login Flow

"Let me demonstrate the authentication flow..."
- POST /api/auth/login with demo credentials
- Copy the JWT token
- Click "Authorize" button, paste token
- Now all endpoints are authenticated

### 4. CRUD Operations

"Now I'll demonstrate the CRUD operations..."
- GET /api/campaigns - Show pagination
- GET /api/campaigns?status=Active - Show filtering
- POST /api/campaigns - Create new campaign
- PUT /api/campaigns/{id} - Update campaign
- DELETE /api/campaigns/{id} - Soft delete

### 5. Show Database

"Let's look at the database..."
- Open CampaignsAPI.db with DB Browser for SQLite
- Show tables, indexes, seed data
- Show the soft-deleted record

### 6. Show Docker

```bash
docker-compose up -d
# Access at http://localhost:5000
```

---

## Final Tips

1. **Know Your Code**: Be prepared to explain any line of code
2. **Trade-offs**: Always discuss pros and cons of decisions
3. **Real Experience**: Share challenges you faced and how you solved them
4. **Ask Questions**: Show interest by asking about their tech stack
5. **Be Honest**: If you don't know something, say so and explain how you'd find out

Good luck with your interview! 🎯
