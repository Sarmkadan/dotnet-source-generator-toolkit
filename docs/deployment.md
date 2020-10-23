// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Deployment Guide

## Overview

The .NET Source Generator Toolkit can be deployed as:
1. A standalone CLI tool
2. A Docker container
3. A CI/CD integrated component
4. A global .NET tool

## Prerequisites

- .NET 10.0 SDK or runtime
- Docker (for containerized deployment)
- Git (for source installation)

## Deployment Methods

### Method 1: Standalone Executable

#### Build Release Binary

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit

# Build release configuration
dotnet build -c Release

# Binary location: ./bin/Release/net10.0/
```

#### Run Release Binary

```bash
# Windows
.\bin\Release\net10.0\DotNetSourceGeneratorToolkit.exe --path . --format Json

# Linux/macOS
./bin/Release/net10.0/DotNetSourceGeneratorToolkit --path . --format Json
```

#### Create Platform-Specific Executable

```bash
# Self-contained executable (includes runtime)
dotnet publish -c Release -r win-x64 --self-contained
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r osx-x64 --self-contained

# Output: ./bin/Release/net10.0/[RID]/publish/
```

### Method 2: Docker Deployment

#### Build Docker Image

```bash
# Using provided Dockerfile
docker build -t dotnet-source-generator-toolkit:latest .

# Tag for registry
docker tag dotnet-source-generator-toolkit:latest myregistry.azurecr.io/dotnet-source-generator-toolkit:latest
```

#### Run Docker Container

```bash
# Basic usage
docker run --rm -v $(pwd):/workspace \
  dotnet-source-generator-toolkit:latest \
  dotnet run -- /workspace

# With options
docker run --rm \
  -v $(pwd):/workspace \
  -v $(pwd)/config:/config \
  dotnet-source-generator-toolkit:latest \
  dotnet run -- \
    --path /workspace \
    --config /config/toolkit-config.json \
    --output /workspace/Generated

# Mount configuration
docker run --rm \
  -v $(pwd):/workspace \
  -e TOOLKIT_CONFIG=/workspace/toolkit-config.json \
  dotnet-source-generator-toolkit:latest \
  dotnet run -- --path /workspace
```

#### Docker Compose Deployment

```yaml
version: '3.9'

services:
  generator:
    build:
      context: .
      dockerfile: Dockerfile
    image: dotnet-source-generator-toolkit:latest
    container_name: code-generator
    volumes:
      - ./src:/workspace/src
      - ./Generated:/workspace/Generated
      - ./config:/config
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - TOOLKIT_CONFIG=/config/toolkit-config.json
    command: >
      dotnet run --
        --path /workspace/src
        --config /config/toolkit-config.json
        --output /workspace/Generated
        --format Json
```

Run with:
```bash
docker-compose up --build
docker-compose down
```

### Method 3: CI/CD Integration

#### GitHub Actions

Create `.github/workflows/generate-code.yml`:

```yaml
name: Generate Code

on:
  push:
    branches: [main]
    paths:
      - 'src/Domain/**'
  pull_request:
    branches: [main]

jobs:
  generate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore
        run: dotnet restore
      
      - name: Clone Toolkit
        run: git clone https://github.com/Sarmkadan/dotnet-source-generator-toolkit.git toolkit
      
      - name: Build Toolkit
        run: dotnet build -c Release
        working-directory: toolkit
      
      - name: Generate Code
        run: |
          dotnet run -c Release -- \
            --path ./src \
            --output ./Generated \
            --format Json \
            --verbose
        working-directory: toolkit
      
      - name: Upload Generated Code
        uses: actions/upload-artifact@v3
        with:
          name: generated-code
          path: Generated/
      
      - name: Create Pull Request
        if: github.event_name == 'pull_request'
        uses: peter-evans/create-pull-request@v4
        with:
          commit-message: 'chore: regenerate code'
          title: 'Auto: Generated Code Update'
          branch: generated-code-update
```

#### GitLab CI

Create `.gitlab-ci.yml`:

```yaml
stages:
  - build
  - generate
  - test

generate_code:
  stage: generate
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - git clone https://github.com/Sarmkadan/dotnet-source-generator-toolkit.git toolkit
    - cd toolkit
    - dotnet build -c Release
    - dotnet run -c Release -- --path ../src --output ../Generated --format Json
  artifacts:
    paths:
      - Generated/
    expire_in: 1 day
  only:
    - merge_requests
    - main
```

#### Azure Pipelines

Create `azure-pipelines.yml`:

```yaml
trigger:
  - main
  - develop

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: UseDotNet@2
    inputs:
      version: '10.0.x'
  
  - task: DotNetCoreCLI@2
    inputs:
      command: 'build'
      arguments: '-c Release'
  
  - script: |
      git clone https://github.com/Sarmkadan/dotnet-source-generator-toolkit.git toolkit
      cd toolkit
      dotnet build -c Release
      dotnet run -c Release -- --path $(Build.SourcesDirectory)/src --output $(Build.SourcesDirectory)/Generated
    displayName: 'Generate Code'
  
  - task: PublishBuildArtifacts@1
    inputs:
      pathToPublish: '$(Build.SourcesDirectory)/Generated'
      artifactName: 'generated-code'
```

### Method 4: Global .NET Tool

#### Installation

```bash
# Pack as NuGet package
dotnet pack -c Release -o ./nupkg

# Install globally (requires NuGet feed)
dotnet tool install --global DotNetSourceGeneratorToolkit \
  --version 1.0.0 \
  --add-source ./nupkg

# Or from NuGet.org (if published)
dotnet tool install --global DotNetSourceGeneratorToolkit
```

#### Usage as Global Tool

```bash
# After installation
dotnet-source-generator-toolkit --path . --format Json

# List installed tools
dotnet tool list --global

# Update tool
dotnet tool update --global DotNetSourceGeneratorToolkit

# Uninstall
dotnet tool uninstall --global DotNetSourceGeneratorToolkit
```

## Production Deployment Checklist

- [ ] Use Release build configuration
- [ ] Verify .NET 10.0 runtime is installed on target
- [ ] Configure toolkit-config.json with production settings
- [ ] Enable backups for existing generated files
- [ ] Set up logging to file (configure via appsettings.json)
- [ ] Configure webhook endpoints for monitoring
- [ ] Set up error alerting/monitoring
- [ ] Test generation with sample entities
- [ ] Validate output format requirements
- [ ] Plan cache expiration strategy
- [ ] Document custom middleware/formatters
- [ ] Set up CI/CD pipeline for automated generation
- [ ] Create runbooks for troubleshooting
- [ ] Schedule regular generation runs
- [ ] Monitor performance metrics

## Configuration for Deployment

### Production Configuration

```json
{
  "generation": {
    "enableCaching": true,
    "cacheExpirationMinutes": 120,
    "enableCodeFormatting": true,
    "codeFormattingLineLength": 120,
    "maxDegreeOfParallelism": 2
  },
  
  "output": {
    "outputDirectory": "/opt/generated",
    "backupExistingFiles": true,
    "defaultNamespace": "MyApp.Generated"
  },
  
  "features": {
    "generateDtos": true,
    "generateInterfaces": true,
    "generateXmlComments": true,
    "verboseLogging": false
  },
  
  "performance": {
    "operationTimeoutSeconds": 600,
    "batchProcessingEnabled": true,
    "batchSize": 5
  },
  
  "integration": {
    "webhookEnabled": true,
    "webhookUrl": "https://monitoring.example.com/webhook",
    "webhookRetries": 5,
    "webhookTimeoutSeconds": 30
  }
}
```

## Monitoring & Logging

### Enable File Logging

Create `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    },
    "File": {
      "Path": "/var/log/toolkit/generation.log",
      "IncludeScopes": true
    }
  }
}
```

### Monitor Webhook Events

```bash
# Using curl to test webhook
curl -X POST https://your-webhook.example.com/webhook \
  -H "Content-Type: application/json" \
  -d '{
    "entityName": "User",
    "filePath": "/path/to/User.cs",
    "generationTime": 250,
    "success": true
  }'
```

### Performance Metrics

```bash
# Monitor generation time
dotnet run -- --path . --verbose 2>&1 | grep "Duration"

# Check output file sizes
ls -lah Generated/

# Count generated files
find Generated/ -type f | wc -l
```

## Scaling Considerations

### High-Volume Scenarios

For large projects with many entities:

1. **Increase parallelism cautiously**:
   ```json
   {
     "maxDegreeOfParallelism": 4
   }
   ```

2. **Batch processing**:
   ```json
   {
     "batchProcessingEnabled": true,
     "batchSize": 20
   }
   ```

3. **Cache management**:
   ```json
   {
     "enableCaching": true,
     "cacheExpirationMinutes": 240
   }
   ```

4. **Increase timeout**:
   ```json
   {
     "operationTimeoutSeconds": 900
   }
   ```

### Distributed Scenarios

For CI/CD with multiple agents:

1. Use shared output directory
2. Configure webhook for centralized monitoring
3. Implement file locking for concurrent writes
4. Monitor for cache coherency issues

## Troubleshooting Deployment

### Issue: "Unable to find .NET SDK"

**Solution**:
```bash
dotnet --version
dotnet --list-sdks

# Install missing version
# Visit https://dotnet.microsoft.com/download
```

### Issue: "Docker image too large"

**Solution**: Create multi-stage Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# ... build stage
FROM mcr.microsoft.com/dotnet/runtime:10.0
# ... runtime stage only
```

### Issue: "Permission denied writing to output"

**Solution**:
```bash
# Check directory permissions
ls -la Generated/

# Fix permissions
chmod 755 Generated/
sudo chown username:username Generated/
```

### Issue: "Out of disk space"

**Solution**:
```bash
# Check disk usage
df -h

# Clear old generated files
find Generated/ -mtime +30 -delete

# Enable backup rotation
```

## Backup & Recovery

### Backup Strategy

```bash
#!/bin/bash
# backup.sh - Daily backup of generated code

BACKUP_DIR="/backups/generated-code"
SOURCE_DIR="./Generated"
DATE=$(date +%Y%m%d_%H%M%S)

mkdir -p $BACKUP_DIR
tar -czf $BACKUP_DIR/generated-$DATE.tar.gz $SOURCE_DIR
find $BACKUP_DIR -name "generated-*.tar.gz" -mtime +30 -delete
```

### Recovery Procedure

```bash
# List available backups
ls -la /backups/generated-code/

# Restore from backup
tar -xzf /backups/generated-code/generated-20260101_120000.tar.gz

# Verify restored files
ls -la Generated/
```

## Security Best Practices

1. **Restrict file permissions**:
   ```bash
   chmod 700 Generated/
   ```

2. **Use HTTPS for webhooks**:
   ```json
   {
     "webhookUrl": "https://secure-endpoint.example.com/webhook"
   }
   ```

3. **Validate webhook signatures** (if implementing receiver):
   ```csharp
   var signature = request.Headers["X-Webhook-Signature"];
   var isValid = VerifySignature(signature, requestBody, secretKey);
   ```

4. **Store secrets securely**:
   - Use environment variables for sensitive config
   - Use secret management services (Azure Keyvault, AWS Secrets Manager)
   - Never commit secrets to version control

5. **Audit file changes**:
   ```bash
   # Track all generated files
   git add Generated/
   git commit -m "chore: regenerate code"
   ```

## Maintenance

### Regular Tasks

- Monitor log files for errors
- Check disk space usage
- Validate backup integrity
- Review metrics for performance degradation
- Update dependencies monthly
- Test disaster recovery procedures

### Update Procedure

```bash
# Check for updates
cd dotnet-source-generator-toolkit
git fetch origin
git log main..origin/main

# Update to latest
git pull origin main
dotnet build -c Release

# Test with dry-run
dotnet run -c Release -- --path . --dry-run
```
