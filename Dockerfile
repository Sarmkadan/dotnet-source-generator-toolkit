# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# Stage 1 - Restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS restore
WORKDIR /src
COPY ["dotnet-source-generator-toolkit.csproj", "./"]
RUN dotnet restore "dotnet-source-generator-toolkit.csproj"

# Stage 2 - Build and publish
FROM restore AS build
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore \
    /p:UseAppHost=false \
    /p:PublishTrimmed=false

# Stage 3 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

LABEL maintainer="Vladyslav Zaiets <vladyslav@sarmkadan.com>"
LABEL org.opencontainers.image.source="https://github.com/sarmkadan/dotnet-source-generator-toolkit"
LABEL org.opencontainers.image.description="Roslyn source generator toolkit for .NET"

WORKDIR /app

RUN addgroup --system --gid 1001 appgroup \
    && adduser --system --uid 1001 --ingroup appgroup appuser \
    && chown -R appuser:appgroup /app

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD ["dotnet", "--list-runtimes", "||", "exit", "1"]

ENTRYPOINT ["dotnet", "DotNetSourceGeneratorToolkit.dll"]
