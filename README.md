# .NET Source Generator Toolkit

**A Roslyn-powered code generation toolkit for generating repositories, mappers, validators, and serializers from attributes.**

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [Configuration](#configuration)
- [CLI Reference](#cli-reference)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## Overview

The .NET Source Generator Toolkit is a comprehensive solution for automating boilerplate code generation in .NET 10.0 projects. Built on Microsoft Roslyn, it intelligently analyzes your C# codebase and generates production-ready code for common patterns including repositories, mappers, validators, and serializers.

### Motivation

Enterprise .NET applications often require massive amounts of repetitive code:
- **Repository implementations** for data access patterns
- **Mapper/DTO conversions** for cross-layer communication
- **Validation logic** for business rules enforcement
- **Serialization code** for multiple formats (JSON, XML, CSV)

This toolkit eliminates that drudgery by using Roslyn to analyze your entity definitions and automatically generate complete, type-safe implementations. Define your domain model once, mark it with attributes, and let the toolkit handle the rest.

### Why Not Code Templates?

Traditional code generation tools rely on templates, which are:
- **Fragile** - easily broken by whitespace or syntax errors
- **Hard to maintain** - changes require template updates across the codebase
- **Poor at static analysis** - limited context about your actual types

This toolkit uses Roslyn's full semantic analyzer to understand your code structure, ensuring generated code is always type-safe, properly namespaced, and automatically updated when your models change.

## Key Features

### Core Generation Capabilities

**Repository Pattern Generation**
- Automatic repository implementations from entity attributes
- Support for common query patterns (FindBy*, GetAll, GetPaged)
- Built-in filtering, sorting, and pagination
- Async/await throughout

**Mapper/DTO Generation**
- Bidirectional mapping between entities and DTOs
- Nested object mapping
- Collection mapping with preservation
- Custom property mapping rules
- Profile-based generation (separate mappers for different scenarios)

**Validator Generation**
- Fluent validation rule creation
- Built-in rule library (NotEmpty, Length, Pattern, etc.)
- Localized error messages
- Async validation support
- Custom validation rules with messages

**Serializer Generation**
- JSON serialization with custom naming strategies
- XML serialization with schema validation
- CSV serialization with header mapping
- Binary serialization for performance-critical paths
- Format-specific options and customization

### Infrastructure Features

- **Middleware Pipeline**: Chain-of-responsibility pattern for extensibility
- **Event System**: Decoupled pub-sub messaging for cross-component communication
- **Output Formatting**: Multiple output formats (JSON, CSV, XML, Text) with extensible factory
- **Caching Layer**: In-memory caching with configurable expiration policies
- **Metrics Collection**: Performance monitoring and reporting
- **Batch Processing**: Parallel execution with degree-of-parallelism control
- **HTTP Integration**: Built-in webhook support with retry policies
- **Configuration Management**: File-based configuration with validation
- **Logging**: Structured logging throughout the pipeline

### Developer Experience

- Full **async/await** support for non-blocking operations
- Type-safe interfaces for all components
- Comprehensive extension methods and utilities
- Thread-safe concurrent operations
- Rich command-line interface with helpful error messages
- Detailed diagnostic logging for troubleshooting

## Architecture

### High-Level Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    CLI Layer                                │
│         (CliArgumentParser, CliOptions)                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                Middleware Pipeline                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Logging    │→ │ Error Handler│→ │ Validation   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│             Generation Pipeline                             │
│  ┌─────────────────────────────────────────────────────┐   │
│  │ 1. Analyze Project (AttributeAnalyzer)              │   │
│  │ 2. Extract Entities (EntityAnalyzer)                │   │
│  │ 3. Generate Code:                                   │   │
│  │    - RepositoryGeneratorService                     │   │
│  │    - MapperGeneratorService                         │   │
│  │    - ValidatorGeneratorService                      │   │
│  │    - SerializerGeneratorService                     │   │
│  │ 4. Format Output (FormatterFactory)                 │   │
│  └─────────────────────────────────────────────────────┘   │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│        Output Formatters & Integration                      │
│  ┌──────────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────────┐      │
│  │   JSON   │ │ CSV  │ │ XML  │ │ Text │ │ Webhooks │      │
│  └──────────┘ └──────┘ └──────┘ └──────┘ └──────────┘      │
└─────────────────────────────────────────────────────────────┘

Supporting Infrastructure:
├── Event System (EventAggregator, GenerationStartedEvent, etc.)
├── Caching (MemoryCache, CacheKey)
├── Metrics (MetricsCollector)
├── Configuration (ConfigurationManager, ToolkitOptions)
└── File I/O (FileSystemService)
```

### Design Patterns Used

| Pattern | Components | Purpose |
|---------|-----------|---------|
| **Middleware** | Pipeline, LoggingMiddleware, ValidationMiddleware, ErrorHandlingMiddleware | Extensible request/response chain |
| **Factory** | FormatterFactory, MiddlewarePipeline | Creation of formatters and middleware |
| **Repository** | EntityRepository, GenerationResultRepository | Data access abstraction |
| **Observer** | EventAggregator, IEventPublisher, IEventSubscriber | Decoupled event handling |
| **Strategy** | IOutputFormatter implementations | Multiple output format strategies |
| **Async/Await** | All service methods | Non-blocking operations |

### Component Responsibilities

**Analyzers** (`Infrastructure/`)
- `AttributeAnalyzer`: Finds and parses generation attributes in source
- `EntityAnalyzer`: Extracts entity properties and metadata

**Generators** (`Services/`)
- `RepositoryGeneratorService`: Creates CRUD implementations
- `MapperGeneratorService`: Generates bidirectional mappers
- `ValidatorGeneratorService`: Creates validation rules
- `SerializerGeneratorService`: Builds serialization code

**Infrastructure** (`Infrastructure/`)
- `ConfigurationManager`: Loads and manages configuration
- `FileSystemService`: Handles file I/O operations
- `GenerationResultRepository`: Persists generation results

**Formatting** (`Formatters/`)
- `JsonOutputFormatter`: JSON output with metadata
- `CsvOutputFormatter`: Spreadsheet-compatible format
- `XmlOutputFormatter`: Document-oriented XML
- `TextOutputFormatter`: Human-readable summaries
- `FormatterFactory`: Runtime formatter selection

## Installation

### Prerequisites

- **.NET**: 10.0 SDK or later ([Download](https://dotnet.microsoft.com/en-us/download))
- **Operating System**: Windows, macOS, or Linux
- **Terminal**: PowerShell, Bash, or command prompt

### Method 1: Clone and Build

```bash
# Clone the repository
git clone https://github.com/sarmkadan/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit

# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run tests (if available)
dotnet test
```

### Method 2: Run Directly (Requires .NET 10 SDK)

```bash
# Clone the repository
git clone https://github.com/sarmkadan/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit

# Run directly without building
dotnet run -- [options]
```

### Method 3: Global Tool Installation (Recommended for Frequent Use)

```bash
# Pack as NuGet package
dotnet pack -c Release -o ./nupkg

# Install globally (requires private NuGet feed setup)
dotnet tool install --global DotNetSourceGeneratorToolkit --add-source ./nupkg
```

### Method 4: Docker

```bash
# Build Docker image
docker build -t dotnet-source-generator-toolkit .

# Run in container
docker run --rm -v $(pwd):/workspace \
  dotnet-source-generator-toolkit \
  dotnet run -- /workspace
```

## Quick Start

### 1. Mark Your Entities with Attributes

```csharp
using DotNetSourceGeneratorToolkit.Domain;

namespace MyApp.Domain
{
    [Repository]
    [Mapper]
    [Validator]
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
```

### 2. Run the Toolkit

```bash
# From your project directory
dotnet run -- --path . --format Json --output ./Generated
```

### 3. Generated Code Ready to Use

```csharp
// Generated repository
var repository = new ProductRepository(dbContext);
var products = await repository.GetAllAsync();
var product = await repository.FindByIdAsync(1);

// Generated mapper
var mapper = new ProductMapper();
var dto = mapper.MapToDto(product);
var entity = mapper.MapFromDto(dto);

// Generated validator
var validator = new ProductValidator();
var validationResult = await validator.ValidateAsync(product);
```

## Usage Examples

### Example 1: Basic Entity with Repository Generation

```csharp
// User.cs
[Repository]
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// Generated code creates UserRepository with:
// - GetAllAsync()
// - GetByIdAsync(int id)
// - FindByEmailAsync(string email)
// - CreateAsync(User user)
// - UpdateAsync(User user)
// - DeleteAsync(int id)
// - GetPagedAsync(int pageNumber, int pageSize)
```

### Example 2: DTO Mapping with Profile

```csharp
// CustomerDto.cs
[Mapper(Profile = "ApiResponse")]
public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Customer.cs
[Mapper]
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string GetFullName() => $"{FirstName} {LastName}";
}

// Generated CustomerMapper handles:
// - Nested mapping (FirstName + LastName → FullName)
// - Bidirectional conversion
// - Null-safety checks
// - Collection mappings
```

### Example 3: Complex Validator with Custom Rules

```csharp
[Validator(MessageLanguage = "en")]
public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

// Generated OrderItemValidator includes:
// - NotEmpty validation for ProductId
// - GreaterThan(0) for Quantity
// - PrecisionScale checks for UnitPrice
// - Custom async validation for stock availability
// - Localized error messages
```

### Example 4: Multi-Format Serialization

```csharp
[Serializer(Formats = new[] { "Json", "Xml", "Csv" })]
public class Report
{
    public string Title { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<DataPoint> DataPoints { get; set; } = [];
}

// Generated serializers create:
// - JsonReportSerializer: JSON with custom naming
// - XmlReportSerializer: XML with schema
// - CsvReportSerializer: Spreadsheet format
// - BinaryReportSerializer: Compact binary format
```

### Example 5: Batch Processing Multiple Entities

```bash
# Process entire project directory
dotnet run -- --path ./MyProject \
  --format Json \
  --batch-size 10 \
  --max-parallelism 4 \
  --output ./Generated \
  --backup-existing

# Monitor progress in real-time
dotnet run -- --path ./MyProject --verbose --log-level Trace
```

### Example 6: Webhook Integration

```csharp
// Configure in toolkit-config.json
{
  "webhookEnabled": true,
  "webhookUrl": "https://api.example.com/generation-complete",
  "webhookRetries": 3,
  "webhookTimeoutSeconds": 30
}

// Toolkit automatically POSTs generation results after completion
// Includes metadata: entity count, generation time, format, file paths
```

### Example 7: Custom Middleware Integration

```csharp
public class CustomMiddleware : IMiddleware
{
    public async Task ExecuteAsync(
        GenerationContext context, 
        Func<GenerationContext, Task> next)
    {
        // Pre-processing
        Console.WriteLine($"Starting generation for {context.EntityName}");
        
        var startTime = DateTime.UtcNow;
        await next(context);
        var duration = DateTime.UtcNow - startTime;
        
        // Post-processing
        Console.WriteLine($"Completed in {duration.TotalSeconds}s");
    }
}
```

### Example 8: Caching Generated Code

```csharp
// Automatic caching with configuration
{
  "enableCaching": true,
  "cacheExpirationMinutes": 60
}

// First run: generates and caches
dotnet run -- --path ./Project1

// Second run (within 60 min): serves from cache
dotnet run -- --path ./Project1

// Force cache refresh
dotnet run -- --path ./Project1 --clear-cache
```

### Example 9: Event-Driven Architecture

```csharp
// Subscribe to generation events
var eventAggregator = provider.GetRequiredService<IEventAggregator>();

eventAggregator.Subscribe<GenerationStartedEvent>(ev =>
{
    Console.WriteLine($"Generation started: {ev.EntityName}");
});

eventAggregator.Subscribe<GenerationCompletedEvent>(ev =>
{
    Console.WriteLine($"Generated {ev.FilePath}");
});

// Toolkit publishes these events automatically during execution
```

### Example 10: Metrics Collection and Reporting

```csharp
var metricsCollector = provider.GetRequiredService<IMetricsCollector>();

// Toolkit automatically collects:
// - Generation time per entity
// - Cache hit rates
// - Output file sizes
// - Batch processing throughput
// - Error counts and types

var metrics = metricsCollector.GetMetrics();
Console.WriteLine($"Average generation time: {metrics.AverageGenerationTimeMs}ms");
Console.WriteLine($"Cache hit rate: {metrics.CacheHitRate:P}");
```

## API Reference

### Core Interfaces

#### `ISourceGeneratorService`

Main orchestration service for the entire generation pipeline.

```csharp
public interface ISourceGeneratorService
{
    // Analyzes a project directory and returns discovered entities
    Task<ProjectInfo> AnalyzeProjectAsync(string projectPath);
    
    // Generates code for all discovered entities
    Task<List<GenerationResult>> GenerateAllAsync(ProjectInfo projectInfo);
    
    // Generates code for a specific entity
    Task<GenerationResult> GenerateForEntityAsync(Entity entity);
}
```

#### `IRepositoryGeneratorService`

Generates repository pattern implementations.

```csharp
public interface IRepositoryGeneratorService
{
    // Generates repository code for an entity
    Task<RepositoryGeneration> GenerateRepositoryAsync(Entity entity);
    
    // Generates multiple repositories in parallel
    Task<List<RepositoryGeneration>> GenerateAllAsync(List<Entity> entities);
}
```

#### `IMapperGeneratorService`

Generates DTO mapper implementations.

```csharp
public interface IMapperGeneratorService
{
    // Generates mapper between entity and DTO
    Task<MapperGeneration> GenerateMappingAsync(Entity entity, Entity dto);
    
    // Generates all mappers for entities
    Task<List<MapperGeneration>> GenerateAllMappersAsync(List<Entity> entities);
}
```

#### `IValidatorGeneratorService`

Generates validation rule implementations.

```csharp
public interface IValidatorGeneratorService
{
    // Generates validator for an entity
    Task<ValidatorGeneration> GenerateValidatorAsync(Entity entity);
    
    // Generates all validators
    Task<List<ValidatorGeneration>> GenerateAllValidatorsAsync(List<Entity> entities);
}
```

#### `ISerializerGeneratorService`

Generates serialization code for multiple formats.

```csharp
public interface ISerializerGeneratorService
{
    // Generates serializers for specified formats
    Task<SerializerGeneration> GenerateSerializersAsync(
        Entity entity, 
        string[] formats);
}
```

### Configuration

#### `ToolkitOptions`

```csharp
public class ToolkitOptions
{
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 60;
    public bool EnableCodeFormatting { get; set; } = true;
    public int CodeFormattingLineLength { get; set; } = 100;
    public bool VerboseLogging { get; set; } = false;
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    public int OperationTimeoutSeconds { get; set; } = 300;
    public bool GenerateDtos { get; set; } = true;
    public string DefaultNamespace { get; set; } = "GeneratedCode";
    public string OutputDirectory { get; set; } = "./Generated";
    public bool BackupExistingFiles { get; set; } = true;
    public bool GenerateInterfaces { get; set; } = true;
    public bool GenerateXmlComments { get; set; } = true;
    public bool WebhookEnabled { get; set; } = false;
    public string? WebhookUrl { get; set; }
    public int WebhookRetries { get; set; } = 3;
}
```

## Configuration

### Configuration File Format

Create `toolkit-config.json` in your project root:

```json
{
  "$schema": "https://your-domain.com/toolkit-config.schema.json",
  
  "generation": {
    "enableCaching": true,
    "cacheExpirationMinutes": 60,
    "enableCodeFormatting": true,
    "codeFormattingLineLength": 100,
    "maxDegreeOfParallelism": 4
  },
  
  "output": {
    "outputDirectory": "./Generated",
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
    "operationTimeoutSeconds": 300,
    "batchProcessingEnabled": true,
    "batchSize": 10
  },
  
  "integration": {
    "webhookEnabled": false,
    "webhookUrl": null,
    "webhookRetries": 3,
    "webhookTimeoutSeconds": 30
  }
}
```

### Configuration Validation

The toolkit validates configuration on startup:
- All file paths must be accessible
- Timeout values must be positive
- Degree of parallelism must not exceed CPU count
- Required fields must be present

## CLI Reference

### Usage

```bash
dotnet run -- [options]
```

### Options

| Option | Short | Type | Default | Description |
|--------|-------|------|---------|-------------|
| `--path` | `-p` | string | `.` | Project path to analyze |
| `--format` | `-f` | string | `Json` | Output format (Json, Csv, Xml, Text) |
| `--output` | `-o` | string | `./Generated` | Output directory path |
| `--config` | `-c` | string | `toolkit-config.json` | Configuration file path |
| `--verbose` | `-v` | flag | false | Enable verbose logging |
| `--log-level` | `-l` | string | `Information` | Logging level (Trace, Debug, Information, Warning, Error, Critical) |
| `--dry-run` | | flag | false | Analyze without writing files |
| `--clear-cache` | | flag | false | Clear caches before generation |
| `--backup-existing` | | flag | true | Backup existing files before overwriting |
| `--max-parallelism` | | int | CPU count | Maximum parallel tasks |
| `--batch-size` | | int | 10 | Batch processing size |
| `--help` | `-h` | flag | | Display help information |
| `--version` | | flag | | Display version information |

### Examples

```bash
# Analyze current directory with JSON output
dotnet run -- --path . --format Json

# Verbose mode with custom output directory
dotnet run -- -p ./MyProject -o ./GeneratedCode -v

# Dry-run to preview without writing
dotnet run -- --path ./MyProject --dry-run

# Custom configuration file
dotnet run -- --path ./MyProject --config custom-config.json

# With specific parallelism settings
dotnet run -- --max-parallelism 2 --batch-size 5

# Get help
dotnet run -- --help
```

## Troubleshooting

### Problem: "No entities found for generation"

**Cause**: Your classes don't have generation attributes.

**Solution**:
```csharp
// Add [Repository], [Mapper], [Validator], or [Serializer] attributes
[Repository]
[Mapper]
public class MyEntity
{
    public int Id { get; set; }
}
```

### Problem: "Configuration validation failed"

**Cause**: Invalid `toolkit-config.json`.

**Solution**:
```bash
# Validate configuration
dotnet run -- --path . --dry-run --verbose

# Use default config (remove custom file temporarily)
rm toolkit-config.json
dotnet run -- --path .
```

### Problem: "Access denied writing to output directory"

**Cause**: Insufficient permissions.

**Solution**:
```bash
# Check permissions
ls -la ./Generated

# On Windows
icacls ./Generated /grant Everyone:F

# On Unix/Linux
chmod 755 ./Generated
```

### Problem: "Out of memory during batch processing"

**Cause**: Too many entities processed in parallel.

**Solution**:
```bash
# Reduce parallelism
dotnet run -- --max-parallelism 2 --batch-size 5
```

### Problem: "Roslyn analyzer not found"

**Cause**: Missing Microsoft.CodeAnalysis package.

**Solution**:
```bash
# Restore NuGet packages
dotnet restore

# Verify packages
dotnet package list
```

### Problem: "Webhook delivery failed"

**Cause**: Network connectivity or webhook endpoint unreachable.

**Solution**:
```json
{
  "integration": {
    "webhookEnabled": true,
    "webhookUrl": "https://verified-endpoint.com/webhook",
    "webhookRetries": 5,
    "webhookTimeoutSeconds": 30
  }
}
```

## Contributing

### How to Contribute

We welcome contributions! Here's how:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/your-feature`)
3. **Commit** your changes (`git commit -am 'Add feature'`)
4. **Push** to the branch (`git push origin feature/your-feature`)
5. **Create** a Pull Request

### Development Setup

```bash
# Clone your fork
git clone https://github.com/your-username/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit

# Create feature branch
git checkout -b feature/amazing-feature

# Build and test
dotnet build
dotnet test

# Make your changes and commit
git commit -am "Add amazing feature"

# Push and create PR
git push origin feature/amazing-feature
```

### Code Standards

- **Language**: C# 12.0+
- **Framework**: .NET 10.0
- **Style**: Follow Microsoft C# coding conventions
- **Testing**: All public methods must have tests
- **Documentation**: Update README and inline comments
- **Comments**: Every class and public method needs XML documentation

### File Headers

Every `.cs` file must start with:

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================
```

## License

MIT License - Copyright © 2026 Vladyslav Zaiets

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com) - CTO & Software Architect**

[Portfolio](https://sarmkadan.com) | [GitHub](https://github.com/Sarmkadan) | [Telegram](https://t.me/sarmkadan)
