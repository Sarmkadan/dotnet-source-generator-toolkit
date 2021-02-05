# Migration Guide: v1.x to v2.0

This guide covers breaking changes and required steps to upgrade from v1.x to v2.0.0.

## Prerequisites

- .NET 10 SDK (10.0.100 or later)
- Docker 24+ (if using containerized builds)

## Breaking Changes

### 1. Docker Image Base Change

The runtime base image changed from `mcr.microsoft.com/dotnet/runtime:10.0` to `mcr.microsoft.com/dotnet/aspnet:10.0` to support HTTP health endpoints and ASP.NET hosting.

**Action required:** Rebuild all Docker images. Cached layers from v1.x are incompatible.

```bash
docker compose build --no-cache
```

### 2. Default Port Changed to 8080

The container now exposes port **8080** instead of relying on the default .NET port. If you have reverse proxy or firewall rules targeting the old port, update them.

**Before (v1.x):**
```yaml
# No explicit port mapping
```

**After (v2.0):**
```yaml
ports:
  - "8080:8080"
```

### 3. docker-compose.yml Format Updated

- Removed deprecated `version: '3.9'` key (Compose V2 specification)
- Volumes now use named volumes instead of bind mounts for logs
- Removed inline `entrypoint` override - use environment variables or config file instead

**Action required:** Replace your `docker-compose.yml` with the new version or merge changes manually.

### 4. Non-root User Setup Changed

The Dockerfile now creates a system user (`appuser:appgroup` with UID/GID 1001) instead of using `useradd`. If your volume permissions depend on the old UID, update accordingly.

### 5. Health Check Updated

The health check command changed from `dotnet --version` to `dotnet --list-runtimes` for a more meaningful runtime verification.

## Step-by-Step Migration

### 1. Update Package Reference

```xml
<PackageReference Include="Zaiets.dotnet.source.generator.toolkit" Version="2.0.0" />
```

Or update via CLI:

```bash
dotnet add package Zaiets.dotnet.source.generator.toolkit --version 2.0.0
```

### 2. Rebuild Docker Images

```bash
# Stop running containers
docker compose down

# Rebuild with new Dockerfile
docker compose build --no-cache

# Start with new configuration
docker compose up -d
```

### 3. Verify Health Check

```bash
docker inspect --format='{{.State.Health.Status}}' source-generator
```

Expected output: `healthy`

### 4. Update CI/CD Pipelines

If your CI/CD references the Docker image tag, update from `latest` to `2.0.0`:

```yaml
image: dotnet-source-generator-toolkit:2.0.0
```

## Configuration Changes

No changes to `toolkit-config.json` format. All existing configuration files are compatible with v2.0.

## Rollback

If you need to revert to v1.x:

```bash
git checkout v1.0.0
docker compose build --no-cache
docker compose up -d
```

## Support

- Issues: [GitHub Issues](https://github.com/Sarmkadan/dotnet-source-generator-toolkit/issues)
- Security: vladyslav@sarmkadan.com
