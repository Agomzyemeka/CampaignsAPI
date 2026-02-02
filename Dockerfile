# ============================================
# Dockerfile for Campaigns API
# ============================================
# Multi-stage build for optimized production image
# Interview Notes:
# - Multi-stage builds reduce final image size
# - Base image separated from build image
# - Runtime image uses smaller ASP.NET runtime base
# - Non-root user for security
# ============================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (layer caching)
# Interview Note: This is done first to leverage Docker layer caching
COPY ["CampaignsAPI.csproj", "./"]
RUN dotnet restore "CampaignsAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "CampaignsAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "CampaignsAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
WORKDIR /app

# Interview Note: Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser

# Copy published application
COPY --from=publish /app/publish .

# Create directory for SQLite database with proper permissions
RUN mkdir -p /app/data && chown -R appuser:appuser /app/data

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/CampaignsAPI.db"

# Expose port
EXPOSE 80

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "CampaignsAPI.dll"]
