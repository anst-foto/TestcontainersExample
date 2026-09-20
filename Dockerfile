# Multi-stage build: SDK for compilation
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy solution and project files
COPY ["TestcontainersExample.slnx", "."]
COPY ["TestcontainersExample.WebApi/", "TestcontainersExample.WebApi/"]
COPY ["TestcontainersExample.DataBaseLib/", "TestcontainersExample.DataBaseLib/"]
COPY ["TestcontainersExample.DataBaseLib.Tests/", "TestcontainersExample.DataBaseLib.Tests/"]

# Restore NuGet packages (cached layer)
RUN dotnet restore "TestcontainersExample.slnx"

# Build the solution
RUN dotnet build "TestcontainersExample.slnx" -c Release --no-restore

# Publish the WebApi project
RUN dotnet publish "TestcontainersExample.WebApi/TestcontainersExample.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage: lightweight runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Install curl for container health checks
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy published app from builder
COPY --from=builder /app/publish .

# Health check for container orchestration
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Expose port (8080 is default for .NET apps)
EXPOSE 8080

# Set environment for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "TestcontainersExample.WebApi.dll"]