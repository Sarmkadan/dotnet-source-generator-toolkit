## .NET Source Generator Toolkit

**A Roslyn-powered code generation toolkit for generating repositories, mappers, validators, and serializers from attributes.**

### Overview

The dotnet-source-generator-toolkit is a comprehensive .NET 10.0 solution that uses Roslyn to analyze C# projects and automatically generate boilerplate code. It features a modern architecture with middleware pipeline, event system, output formatting, caching, metrics collection, and HTTP integration.

### Features

**Core Generation**
- Automatic repository pattern implementation
- Mapper/DTO code generation with bidirectional mapping
- Validator creation with fluent validation rules
- Serializer generation (JSON, XML, CSV, Binary)
- Entity analysis and property introspection

**Infrastructure**
- Command-line interface with full argument parsing
- Middleware pipeline with logging and error handling
- Multiple output formatters (JSON, CSV, XML, Text)
- In-memory caching with automatic expiration
- Event-driven pub-sub system
- HTTP client with retry logic and webhook support
- Metrics collection and performance monitoring
- Batch processing with parallel execution
- Configuration management with validation

**Developer Experience**
- Comprehensive utilities and extension methods
- Type-safe interfaces for all components
- Full async/await support
- Detailed logging throughout
- Thread-safe concurrent operations

### Project Structure

```
dotnet-source-generator-toolkit/
├── CLI/                          # Command-line interface
├── Middleware/                   # Pipeline middleware components
├── Formatters/                   # Output formatters (JSON, CSV, XML, Text)
├── Caching/                      # In-memory caching layer
├── Events/                       # Event pub-sub system
├── Integration/                  # HTTP and webhook services
├── Utilities/                    # Extension methods and helpers
├── Configuration/                # Config management and validation
├── Metrics/                      # Performance metrics collection
├── Batch/                        # Batch processing
├── Pipeline/                     # Generation orchestration
├── Domain/                       # Domain models
├── Infrastructure/               # Core infrastructure services
├── Services/                     # Code generation services
├── Repositories/                 # Data repositories
├── Exceptions/                   # Custom exceptions
├── Constants/                    # Application constants
├── Program.cs                    # Entry point
├── dotnet-source-generator-toolkit.csproj  # Project file
├── LICENSE                       # MIT License
├── .gitignore                    # Git ignore rules
├── PHASE_2_SUMMARY.md            # Phase 2 build summary
└── README.md                     # This file
```

### Getting Started

#### Prerequisites
- .NET 10.0 SDK or later
- C# 12.0 or later

#### Building

```bash
cd dotnet-source-generator-toolkit
dotnet build
```

#### Running

```bash
# Basic usage (analyze current directory)
dotnet run

# With options
dotnet run -- --path /path/to/project --format Json --verbose

# Get help
dotnet run -- --help

# Dry-run mode (analyze without writing)
dotnet run -- --path /path/to/project --dry-run
```

### Configuration

Create a `toolkit-config.json`:

```json
{
  "enableCaching": true,
  "cacheExpirationMinutes": 60,
  "enableCodeFormatting": true,
  "codeFormattingLineLength": 100,
  "verboseLogging": false,
  "maxDegreeOfParallelism": 4,
  "operationTimeoutSeconds": 300,
  "generateDtos": false,
  "defaultNamespace": "MyApp",
  "outputDirectory": "./Generated",
  "backupExistingFiles": true,
  "generateInterfaces": true,
  "generateXmlComments": true
}
```

### Architecture

#### Middleware Pipeline
The toolkit uses a chain-of-responsibility pattern:
1. **LoggingMiddleware** - Logs all operations
2. **ErrorHandlingMiddleware** - Handles errors with retry
3. **ValidationMiddleware** - Validates configuration
4. Generation services execute
5. Results formatted and returned

#### Event System
- Type-safe pub-sub messaging
- Decouples components
- Events: `GenerationStartedEvent`, `GenerationCompletedEvent`

#### Extensibility
- Custom formatters via `IFormatterFactory`
- Custom middleware via `IMiddleware`
- Custom caching via `ICache`
- Custom webhooks via `IWebhookService`

### Output Formats

**JSON** - Structured with metadata  
**CSV** - Spreadsheet-compatible  
**XML** - Document-oriented  
**Text** - Human-readable summaries  

### Technical Stack

- **.NET**: 10.0
- **Language**: C# 12.0+
- **Architecture**: Clean Architecture
- **Patterns**: Middleware, Factory, Repository, Observer
- **Async**: Full async/await support

### License

MIT License - Copyright 2026 Vladyslav Zaiets

---

**Built by Vladyslav Zaiets**  
https://sarmkadan.com
