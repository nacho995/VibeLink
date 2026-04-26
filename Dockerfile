# VibeLink - Production Dockerfile
# Multi-stage build: Frontend (Node) + Backend (.NET)

# Stage 1: Build frontend
FROM node:22-slim AS frontend-build
WORKDIR /web
COPY web/package.json web/package-lock.json ./
RUN npm ci
COPY web/ .
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /src
COPY backend/backend.csproj .
RUN dotnet restore
COPY backend/ .
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN useradd --no-create-home --shell /bin/false appuser

# Copy published backend
COPY --from=backend-build /app/publish .

# Copy frontend build to wwwroot
COPY --from=frontend-build /web/dist ./wwwroot

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Switch to non-root user
USER appuser

EXPOSE 8080

ENTRYPOINT ["dotnet", "backend.dll"]
