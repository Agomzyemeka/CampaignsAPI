using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using CampaignsAPI.Data;
using CampaignsAPI.Services;
using CampaignsAPI.Middleware;
using CampaignsAPI.Validators;
using FluentValidation;

/// <summary>
/// Program.cs - Application Entry Point
/// Interview Notes:
/// - Dependency Injection configuration
/// - Middleware pipeline setup
/// - JWT authentication configuration
/// - Swagger/OpenAPI documentation
/// - Database context configuration
/// - Service registration with proper lifetimes
/// - CORS policy for frontend integration
/// - Health checks for monitoring
/// </summary>

var builder = WebApplication.CreateBuilder(args);

// ============================================
// LOGGING CONFIGURATION
// ============================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ============================================
// DATABASE CONFIGURATION
// ============================================
// Interview Note: SQLite for development, easily swappable to SQL Server for production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("CampaignsAPI"));
    
    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ============================================
// AUTHENTICATION & AUTHORIZATION
// ============================================
// Interview Note: JWT Bearer authentication for stateless REST API
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set to true in production
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };

    // Logging for authentication failures
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication failed: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var userId = context.Principal?.FindFirst("userId")?.Value;
            logger.LogInformation("Token validated for user: {UserId}", userId);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ============================================
// CORS CONFIGURATION
// ============================================
// Interview Note: CORS for frontend applications
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    // More restrictive policy for production
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================
// DEPENDENCY INJECTION - SERVICES
// ============================================
// Interview Note: Scoped services for per-request instances
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ============================================
// FLUENT VALIDATION
// ============================================
// Interview Note: Validators for request DTOs
builder.Services.AddValidatorsFromAssemblyContaining<CreateCampaignValidator>();

// ============================================
// CONTROLLERS & API CONFIGURATION
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // JSON serialization settings
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ============================================
// SWAGGER/OPENAPI CONFIGURATION
// ============================================
// Interview Note: API documentation with Swagger UI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Campaigns API",
        Version = "v1",
        Description = "A professional REST API for campaign management with JWT authentication",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com",
            Url = new Uri("https://github.com/yourusername")
        }
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// ============================================
// HEALTH CHECKS
// ============================================
// Interview Note: Health checks for monitoring and load balancers
builder.Services.AddHealthChecks();

// ============================================
// BUILD APPLICATION
// ============================================
var app = builder.Build();

// ============================================
// DATABASE MIGRATION & SEEDING
// ============================================
// Interview Note: Automatic database creation and migration on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Apply pending migrations
        var migrationLogger = services.GetRequiredService<ILogger<Program>>();
        migrationLogger.LogInformation("Database migrated successfully");
    }
    catch (Exception ex)
    {
        var errorLogger = services.GetRequiredService<ILogger<Program>>();
        errorLogger.LogError(ex, "An error occurred while migrating the database");
    }
}

// ============================================
// MIDDLEWARE PIPELINE CONFIGURATION
// ============================================
// Interview Note: Order matters! Each request flows through middleware in this sequence

// Global exception handling (must be first)
app.UseExceptionHandling();

// Swagger UI (development only)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Campaigns API V1");
        c.RoutePrefix = "swagger"; // Swagger UI at /swagger
    });
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS (must be before Authentication & Authorization)
app.UseCors("AllowAll");

// Static files from ClientApp folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "ClientApp")),
    RequestPath = ""
});

// Serve index.html as default
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "ClientApp")),
    DefaultFileNames = new List<string> { "index.html" }
});

// Routing
app.UseRouting();

// Authentication (must be before Authorization)
app.UseAuthentication();

// Authorization
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

// Map controllers
app.MapControllers();

// ============================================
// APPLICATION INFO
// ============================================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("===========================================");
logger.LogInformation("Campaigns API Starting...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("===========================================");

// ============================================
// RUN APPLICATION
// ============================================
app.Run();
