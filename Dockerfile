# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Multi-stage build for optimization
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

COPY ["dotnet-source-generator-toolkit.csproj", "./"]
RUN dotnet restore "dotnet-source-generator-toolkit.csproj"

COPY . .
RUN dotnet build -c Release -o /app/build

RUN dotnet publish -c Release -o /app/publish

# Runtime stage - minimal image
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

COPY --from=builder /app/publish .

# Create non-root user for security
RUN useradd -m -u 1001 generator && chown -R generator:generator /app
USER generator

# Set environment for UTF-8 support
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_UseSystemNativeIp=true

ENTRYPOINT ["dotnet", "DotNetSourceGeneratorToolkit.dll"]
