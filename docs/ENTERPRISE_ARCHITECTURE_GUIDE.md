# 🏗️ Enterprise Architecture Guide

## Complete Guide to Redis, RabbitMQ, Docker, and Enterprise Patterns

This document explains your current implementation and provides step-by-step guides for adding Redis caching and RabbitMQ messaging.

---

## Table of Contents

1. [Current Docker Configuration](#current-docker-configuration)
2. [Current Async/Await Implementation](#current-asyncawait-implementation)
3. [Adding Redis Cache](#adding-redis-cache)
4. [Adding RabbitMQ](#adding-rabbitmq)
5. [Updated Docker Compose](#updated-docker-compose)
6. [Enterprise Patterns Explained](#enterprise-patterns-explained)

---

## Current Docker Configuration

### What You Have Now

Your `docker-compose.yml` currently has **1 container**:

```yaml
services:
  api:                          # Campaigns API container
    build: .                    # Builds from Dockerfile
    container_name: campaigns-api
    ports:
      - "5000:80"               # Maps localhost:5000 to container port 80
    volumes:
      - campaigns-data:/app/data  # Persists SQLite database
```

### Your Dockerfile Explained

```dockerfile
# Stage 1: Build - Uses full SDK to compile code
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Stage 2: Publish - Creates optimized release build
FROM build AS publish

# Stage 3: Runtime - Uses smaller runtime image (much smaller!)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
```

**Interview Point**: Multi-stage builds reduce image size from ~700MB (SDK) to ~200MB (runtime only).

### Running Docker

```bash
# Make sure Docker Desktop is running first!

# Navigate to CampaignsAPI folder
cd "c:\Users\AGOMOH\Desktop\CODE\ASP.NET PROJECT\CampaignsAPI"

# Build and run
docker-compose up --build -d

# Check running containers
docker ps

# View logs
docker-compose logs -f api

# Stop containers
docker-compose down
```

---

## Current Async/Await Implementation

### Where Async is Used in Your Code

#### 1. Controller Layer (CampaignsController.cs)

```csharp
// All controller actions are async
[HttpGet]
public async Task<IActionResult> GetCampaigns(...)
{
    // Non-blocking database call
    var totalRecords = await query.CountAsync();
    
    // Non-blocking list retrieval
    var campaigns = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return Ok(response);
}
```

#### 2. Service Layer (AuthService.cs)

```csharp
public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
{
    // Non-blocking database query
    var user = await _context.Users
        .AsNoTracking()
        .FirstOrDefaultAsync(u => u.Email == loginDto.Email);
    
    // Non-blocking save
    await _context.SaveChangesAsync();
    
    return authResponse;
}
```

#### 3. DbContext (ApplicationDbContext.cs)

```csharp
// Override SaveChangesAsync for audit trail
public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    UpdateAuditFields();
    return base.SaveChangesAsync(cancellationToken);
}
```

### Why Async Matters

```
WITHOUT ASYNC (Blocking):
Request 1 → [Thread 1 BLOCKED waiting for DB] → Response 1
Request 2 → [Thread 2 BLOCKED waiting for DB] → Response 2
Request 3 → [NO THREADS AVAILABLE - WAIT!] → Response 3

WITH ASYNC (Non-Blocking):
Request 1 → Thread 1 starts DB call → Thread 1 FREE → DB returns → Thread 3 completes
Request 2 → Thread 1 starts DB call → Thread 1 FREE → DB returns → Thread 2 completes
Request 3 → Thread 2 starts DB call → Thread 2 FREE → DB returns → Thread 1 completes

Result: Same 3 threads can handle MANY more concurrent requests!
```

**Interview Point**: "Async doesn't make individual requests faster, it allows the server to handle more concurrent requests with the same resources."

---

## Adding Redis Cache

### Step 1: Install NuGet Package

```bash
cd "c:\Users\AGOMOH\Desktop\CODE\ASP.NET PROJECT\CampaignsAPI"
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

### Step 2: Create Cache Service

Create `Services/CacheService.cs`:

```csharp
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace CampaignsAPI.Services
{
    /// <summary>
    /// Redis Cache Service
    /// Purpose: Provides distributed caching for frequently accessed data
    /// Interview Notes:
    /// - Redis is in-memory, extremely fast (sub-millisecond)
    /// - Distributed: shared across multiple API instances
    /// - Reduces database load significantly
    /// - Supports expiration policies
    /// </summary>
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPrefixAsync(string prefix);
    }

    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheService> _logger;

        // Default cache duration: 5 minutes
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

        public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Get item from cache
        /// Returns null if not found or expired
        /// </summary>
        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cached = await _cache.GetStringAsync(key);
                if (cached == null)
                {
                    _logger.LogDebug("Cache MISS for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache HIT for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache key: {Key}", key);
                return default; // Fail gracefully - don't break the app if Redis is down
            }
        }

        /// <summary>
        /// Set item in cache with expiration
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
                };

                var serialized = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, serialized, options);
                
                _logger.LogDebug("Cache SET for key: {Key}, expires in: {Expiration}", 
                    key, expiration ?? DefaultExpiration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache key: {Key}", key);
                // Don't throw - caching failure shouldn't break the app
            }
        }

        /// <summary>
        /// Remove specific key from cache
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogDebug("Cache REMOVE for key: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        /// <summary>
        /// Remove all keys with prefix (cache invalidation pattern)
        /// Note: This requires Redis SCAN - simplified version here
        /// </summary>
        public async Task RemoveByPrefixAsync(string prefix)
        {
            // For production, use Redis SCAN command
            // This is a simplified version
            _logger.LogDebug("Cache invalidation requested for prefix: {Prefix}", prefix);
            await Task.CompletedTask;
        }
    }
}
```

### Step 3: Update Program.cs

Add to your service configuration:

```csharp
// ============================================
// REDIS CACHE CONFIGURATION
// ============================================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") 
        ?? "localhost:6379";
    options.InstanceName = "CampaignsAPI_";
});

builder.Services.AddScoped<ICacheService, CacheService>();
```

### Step 4: Use Cache in Controller

Update `CampaignsController.cs`:

```csharp
private readonly ICacheService _cache;

public CampaignsController(
    ApplicationDbContext context,
    ICacheService cache, // Inject cache service
    ...)
{
    _cache = cache;
}

[HttpGet("{id}")]
public async Task<IActionResult> GetCampaign(int id)
{
    // Try cache first
    var cacheKey = $"campaign:{id}";
    var cached = await _cache.GetAsync<CampaignResponseDto>(cacheKey);
    
    if (cached != null)
    {
        return Ok(new ApiResponse<CampaignResponseDto>
        {
            Success = true,
            Message = "Campaign retrieved from cache",
            Data = cached
        });
    }

    // Cache miss - get from database
    var campaign = await _context.Campaigns
        .AsNoTracking()
        .Where(c => c.Id == id && !c.IsDeleted)
        .Select(c => new CampaignResponseDto { /* ... */ })
        .FirstOrDefaultAsync();

    if (campaign == null)
        return NotFound(...);

    // Store in cache for next time
    await _cache.SetAsync(cacheKey, campaign, TimeSpan.FromMinutes(10));

    return Ok(...);
}

[HttpPut("{id}")]
public async Task<IActionResult> UpdateCampaign(int id, ...)
{
    // ... update logic ...

    // Invalidate cache after update
    await _cache.RemoveAsync($"campaign:{id}");

    return Ok(...);
}
```

---

## Adding RabbitMQ

### Step 1: Install NuGet Package

```bash
dotnet add package RabbitMQ.Client
```

### Step 2: Create Message Service

Create `Services/MessageService.cs`:

```csharp
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace CampaignsAPI.Services
{
    /// <summary>
    /// RabbitMQ Message Service
    /// Purpose: Asynchronous message queue for background processing
    /// Interview Notes:
    /// - Decouples components (producer doesn't wait for consumer)
    /// - Handles spikes in traffic (queue buffers messages)
    /// - Enables microservices communication
    /// - Supports pub/sub patterns
    /// - Messages persist even if consumer is down
    /// </summary>
    public interface IMessageService
    {
        void PublishMessage<T>(string queueName, T message);
        void Subscribe<T>(string queueName, Action<T> handler);
    }

    public class RabbitMQService : IMessageService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IConfiguration configuration, ILogger<RabbitMQService> logger)
        {
            _logger = logger;

            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
                    Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                    UserName = configuration["RabbitMQ:UserName"] ?? "guest",
                    Password = configuration["RabbitMQ:Password"] ?? "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _logger.LogInformation("RabbitMQ connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        }

        /// <summary>
        /// Publish message to queue
        /// Use cases: Campaign created notifications, email triggers, audit logs
        /// </summary>
        public void PublishMessage<T>(string queueName, T message)
        {
            try
            {
                // Declare queue (creates if doesn't exist)
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,       // Survives broker restart
                    exclusive: false,     // Can be accessed by other connections
                    autoDelete: false,    // Won't delete when last consumer disconnects
                    arguments: null);

                var json = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(json);

                // Set message properties
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true; // Message survives broker restart

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation("Message published to queue {Queue}: {Message}", 
                    queueName, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing message to {Queue}", queueName);
                throw;
            }
        }

        /// <summary>
        /// Subscribe to queue and process messages
        /// </summary>
        public void Subscribe<T>(string queueName, Action<T> handler)
        {
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Fair dispatch - don't give more messages until previous is acknowledged
            _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json);

                    if (message != null)
                    {
                        handler(message);
                    }

                    // Acknowledge message (remove from queue)
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    
                    _logger.LogInformation("Message processed from queue {Queue}", queueName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from {Queue}", queueName);
                    // Negative acknowledge - requeue message
                    _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            _logger.LogInformation("Subscribed to queue {Queue}", queueName);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
```

### Step 3: Create Message Models

Create `Models/Messages.cs`:

```csharp
namespace CampaignsAPI.Models
{
    /// <summary>
    /// Message sent when a campaign is created
    /// Could trigger: email notifications, analytics, audit logging
    /// </summary>
    public class CampaignCreatedMessage
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Message sent when a campaign status changes
    /// Could trigger: notifications, workflow automation
    /// </summary>
    public class CampaignStatusChangedMessage
    {
        public int CampaignId { get; set; }
        public string OldStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
```

### Step 4: Use in Controller

```csharp
private readonly IMessageService _messageService;

[HttpPost]
public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignDto createDto)
{
    // ... create campaign ...

    // Publish event to message queue (async, non-blocking)
    _messageService.PublishMessage("campaign.created", new CampaignCreatedMessage
    {
        CampaignId = campaign.Id,
        CampaignName = campaign.Name,
        CreatedBy = userId,
        CreatedAt = DateTime.UtcNow
    });

    return CreatedAtAction(...);
}
```

---

## Updated Docker Compose

Replace your `docker-compose.yml` with this complete version:

```yaml
# ============================================
# Docker Compose - Full Stack with Redis & RabbitMQ
# ============================================

version: '3.8'

services:
  # ============================================
  # Campaigns API
  # ============================================
  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: campaigns-api
    restart: unless-stopped
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=/app/data/CampaignsAPI.db
      - ConnectionStrings__Redis=redis:6379
      - RabbitMQ__HostName=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__UserName=guest
      - RabbitMQ__Password=guest
      - JwtSettings__SecretKey=YourProductionSecretKeyThatShouldBeAtLeast32CharactersLong!
    volumes:
      - campaigns-data:/app/data
    networks:
      - campaigns-network
    depends_on:
      - redis
      - rabbitmq
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  # ============================================
  # Redis - Distributed Cache
  # ============================================
  redis:
    image: redis:7-alpine
    container_name: campaigns-redis
    restart: unless-stopped
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - campaigns-network
    command: redis-server --appendonly yes
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 3

  # ============================================
  # RabbitMQ - Message Broker
  # ============================================
  rabbitmq:
    image: rabbitmq:3-management-alpine
    container_name: campaigns-rabbitmq
    restart: unless-stopped
    ports:
      - "5672:5672"   # AMQP protocol
      - "15672:15672" # Management UI
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - campaigns-network
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "check_running"]
      interval: 30s
      timeout: 10s
      retries: 3

# ============================================
# Networks
# ============================================
networks:
  campaigns-network:
    driver: bridge

# ============================================
# Volumes
# ============================================
volumes:
  campaigns-data:
  redis-data:
  rabbitmq-data:
```

### Running Everything

```bash
# Start all containers
cd "c:\Users\AGOMOH\Desktop\CODE\ASP.NET PROJECT\CampaignsAPI"
docker-compose up --build -d

# Check all containers are running
docker ps

# Expected output:
# CONTAINER ID   IMAGE                     STATUS    PORTS                                        NAMES
# abc123         campaigns-api             Up        0.0.0.0:5000->80/tcp                        campaigns-api
# def456         redis:7-alpine            Up        0.0.0.0:6379->6379/tcp                      campaigns-redis
# ghi789         rabbitmq:3-management     Up        0.0.0.0:5672->5672/tcp, 15672->15672/tcp    campaigns-rabbitmq

# Access points:
# API:              http://localhost:5000
# RabbitMQ UI:      http://localhost:15672 (guest/guest)
# Redis:            localhost:6379
```

---

## Enterprise Patterns Explained

### 1. Dependency Injection (Your Code)

```csharp
// Registration (Program.cs)
builder.Services.AddScoped<IAuthService, AuthService>();

// Usage (Controller) - No "new" keyword!
public AuthController(IAuthService authService)
{
    _authService = authService; // Automatically injected
}
```

**Why it matters**: 
- Testability (inject mock in tests)
- Loose coupling (can swap implementations)
- Lifecycle management (Scoped = per request)

### 2. Repository Pattern via DbContext

```csharp
// DbContext IS your repository
public class ApplicationDbContext : DbContext
{
    public DbSet<Campaign> Campaigns { get; set; } // Repository
}

// Usage
var campaigns = await _context.Campaigns.ToListAsync();
```

### 3. Unit of Work Pattern

```csharp
// DbContext tracks changes across multiple entities
campaign.Status = CampaignStatus.Active;
user.LastModified = DateTime.UtcNow;

// Single transaction commits all changes
await _context.SaveChangesAsync(); // Atomic operation
```

### 4. DTO Pattern (Data Transfer Objects)

```csharp
// Domain Model (internal)
public class Campaign
{
    public int Id { get; set; }
    public string PasswordHash { get; set; } // Sensitive!
    public bool IsDeleted { get; set; }      // Internal
}

// DTO (external API contract)
public class CampaignResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    // No sensitive fields exposed!
}
```

### 5. Middleware Pipeline

```csharp
// Order matters!
app.UseExceptionHandling(); // 1. Catch errors first
app.UseHttpsRedirection();  // 2. Redirect HTTP
app.UseCors();              // 3. CORS headers
app.UseAuthentication();    // 4. Who are you?
app.UseAuthorization();     // 5. What can you do?
app.MapControllers();       // 6. Route to action
```

### 6. Options Pattern

```csharp
// Configuration in appsettings.json
"JwtSettings": {
    "SecretKey": "...",
    "ExpiryMinutes": 60
}

// Strongly-typed access
public class JwtSettings
{
    public string SecretKey { get; set; }
    public int ExpiryMinutes { get; set; }
}

// Registration
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// Usage
public TokenService(IOptions<JwtSettings> options)
{
    _settings = options.Value;
}
```

### 7. CQRS-Lite (Query vs Command separation)

```csharp
// Query (Read) - Uses AsNoTracking, optimized for reads
[HttpGet]
public async Task<IActionResult> GetCampaigns()
{
    var campaigns = await _context.Campaigns
        .AsNoTracking() // No change tracking = faster
        .ToListAsync();
}

// Command (Write) - Tracks changes
[HttpPost]
public async Task<IActionResult> CreateCampaign()
{
    _context.Campaigns.Add(campaign); // Tracked
    await _context.SaveChangesAsync();
}
```

---

## Quick Reference Card

| Component | Port | URL | Purpose |
|-----------|------|-----|---------|
| API | 5000 | http://localhost:5000 | Your REST API |
| Swagger | 5000 | http://localhost:5000/swagger | API Docs |
| Redis | 6379 | localhost:6379 | Cache |
| RabbitMQ | 5672 | localhost:5672 | Message Queue |
| RabbitMQ UI | 15672 | http://localhost:15672 | Queue Management |

---

## Interview Sound Bites

> "I use **async/await** throughout to maximize thread utilization - the same server can handle 10x more concurrent requests."

> "**Redis** provides sub-millisecond cache lookups, reducing database load by 80% for frequently accessed data."

> "**RabbitMQ** decouples the API from slow operations like sending emails - the user gets an immediate response while background workers process the queue."

> "**Docker Compose** orchestrates all services with a single command, ensuring consistent environments from dev to production."

> "I implemented **soft deletes** for audit compliance - data is never truly lost, and we maintain referential integrity."

Good luck! 🚀
