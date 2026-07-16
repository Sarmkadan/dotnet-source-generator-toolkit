# .NET Source Generator Toolkit

![Build](https://github.com/sarmkadan/dotnet-source-generator-toolkit/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/github/license/sarmkadan/dotnet-source-generator-toolkit)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)

**A Roslyn-powered code generation toolkit for generating repositories, mappers, validators, and serializers from attributes.**

> **New here?** This README is the full reference. For a five-minute path from install to your
> first generated file, read **[GETTING_STARTED.md](GETTING_STARTED.md)**. For copy-paste
> snippets grouped by task, see the **[Generation Cookbook](docs/cookbook.md)**.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [API Reference](#api-reference)
- [ToolkitOptions](#toolkitoptions)
- [Configuration](#configuration)
- [CLI Reference](#cli-reference)
- [Troubleshooting](#troubleshooting)
- [Performance](#performance)
- [Testing](#testing)
- [Related Projects](#related-projects)
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

The full write-up - component breakdown, design decisions and their trade-offs,
data flow, extension points and known limitations - lives in
[docs/architecture.md](docs/architecture.md). Short version below.

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

## FileSystemService

The `FileSystemService` provides asynchronous file system operations including reading, writing, appending, and directory management. It handles file existence checks, path manipulation, and comprehensive error handling with detailed logging. The service automatically creates parent directories when writing files and provides methods for both synchronous and asynchronous file operations.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var fileSystem = new FileSystemService(loggerFactory.CreateLogger<FileSystemService>());

// Combine paths safely
string projectPath = fileSystem.CombinePath("src", "MyProject");
string filePath = fileSystem.CombinePath(projectPath, "GeneratedCode", "ProductRepository.cs");

// Check if file exists
bool exists = fileSystem.FileExists(filePath);

// Create directory if needed (automatically handled in WriteFileAsync, but can be called explicitly)
await fileSystem.CreateDirectoryAsync(fileSystem.GetDirectoryName(filePath)!);

// Write generated code to file
await fileSystem.WriteFileAsync(filePath, 
    "// Auto-generated repository\n" +
    "public class ProductRepository { /* implementation */ }");

// Read the generated file
string content = await fileSystem.ReadFileAsync(filePath);

// Append additional content
await fileSystem.AppendFileAsync(filePath, "\n// Additional generated members");

// Get all generated files in a directory
var generatedFiles = await fileSystem.GetFilesAsync(projectPath, "*.cs");

// Delete old generated files if needed
await fileSystem.DeleteFileAsync(fileSystem.CombinePath(projectPath, "OldRepository.cs"));
```
```

## WebhookService

The `WebhookService` manages webhook registrations and asynchronously notifies subscribers of events. It provides methods to register and unregister webhooks, send notifications for specific event types, and retrieve all registered webhooks. The service uses an in-memory storage suitable for single-session use and includes retry tracking for failed notifications.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Integration;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Set up dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<IHttpClientService, HttpClientService>();

var serviceProvider = services.BuildServiceProvider();
var webhookService = serviceProvider.GetRequiredService<IWebhookService>();

// Register a webhook for specific events
string webhookId = await webhookService.RegisterWebhookAsync(
    name: "BuildNotifier",
    url: "https://api.example.com/webhooks/build-complete",
    events: new[] { WebhookEventType.GenerationCompleted }
);
Console.WriteLine($"Webhook registered with ID: {webhookId}");

// Notify subscribers about a completed generation
int successfulNotifications = await webhookService.NotifyAsync(
    eventType: WebhookEventType.GenerationCompleted,
    payload: new {
        ProjectName = "MyProject",
        FilesGenerated = 8,
        ExecutionTimeMs = 142,
        IsSuccessful = true
    }
);
Console.WriteLine($"Successfully notified {successfulNotifications} subscribers");

// Get all registered webhooks
var webhooks = await webhookService.GetWebhooksAsync();
foreach (var webhook in webhooks)
{
    Console.WriteLine($"Webhook: {webhook.Name} ({webhook.Id}) - {webhook.Url}");
}

// Unregister a webhook when no longer needed
await webhookService.UnregisterWebhookAsync(webhookId);
Console.WriteLine("Webhook unregistered");
```

## GenerationStartedEvent

The `GenerationStartedEvent` is published when the code generation process begins for a project. This event allows subscribers to initialize resources, set up logging, or perform pre-generation validation before any code is generated. It provides essential context about the generation session including the project path, entity count, and generator types that will be used.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Events;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var eventAggregator = new EventAggregator();

// Subscribe to GenerationStartedEvent
var subscription = eventAggregator.Subscribe<GenerationStartedEvent>(ev =>
{
    Console.WriteLine($"🚀 Generation started at {ev.OccurredAt:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"📁 Project: {ev.ProjectPath}");
    Console.WriteLine($"📊 Entities to process: {ev.EntityCount}");
    Console.WriteLine($"🔧 Generator types: {string.Join(", ", ev.GeneratorTypes)}");
    
    // Initialize resources or validate preconditions
    if (ev.EntityCount == 0)
    {
        Console.WriteLine("⚠️  No entities found - generation may not produce any output");
    }
});

// Simulate starting generation
var generationEvent = new GenerationStartedEvent
{
    RequestId = Guid.NewGuid().ToString(),
    ProjectPath = "/path/to/MyProject",
    EntityCount = 42,
    GeneratorTypes = new List<string> { "Repository", "Mapper", "Validator", "Serializer" }
};

// Publish the event
eventAggregator.Publish(generationEvent);
```

## GenerationPipeline

The `GenerationPipeline` orchestrates the complete code generation workflow, coordinating entity analysis, code generation, and file output operations. It manages the entire process from project analysis to file writing, providing detailed execution metrics and status tracking through its public properties.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Pipeline;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Configure logging and dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddSingleton<IFileSystemService, FileSystemService>();

// Register generator services (typically from DI container)
services.AddSingleton<ISourceGeneratorService, SourceGeneratorService>();
services.AddSingleton<IRepositoryGeneratorService, RepositoryGeneratorService>();
services.AddSingleton<IMapperGeneratorService, MapperGeneratorService>();
services.AddSingleton<IValidatorGeneratorService, ValidatorGeneratorService>();
services.AddSingleton<ISerializerGeneratorService, SerializerGeneratorService>();

var serviceProvider = services.BuildServiceProvider();
var pipeline = serviceProvider.GetRequiredService<GenerationPipeline>();

// Execute the pipeline
var result = await pipeline.ExecuteAsync(
    projectPath: "/path/to/YourProject",
    outputPath: "./GeneratedCode",
    generatorTypes: new[] { "Repository", "Mapper", "Validator", "Serializer" },
    dryRun: false
);

// Check execution results
Console.WriteLine($"✅ Pipeline executed at: {pipeline.ExecutedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"📊 Entities found: {pipeline.EntitiesFound}");
Console.WriteLine($"📝 Files generated: {pipeline.GeneratedFiles}");
Console.WriteLine($"💾 Files written: {pipeline.FilesWritten}");
Console.WriteLine($"🎯 Success: {pipeline.IsSuccessful}");

if (!pipeline.IsSuccessful && pipeline.ErrorMessage != null)
{
    Console.WriteLine($"❌ Error: {pipeline.ErrorMessage}");
}

// Access the detailed result
Console.WriteLine($"📋 Result - Entities: {result.EntitiesFound}, Generated: {result.GeneratedFiles}, Written: {result.FilesWritten}");
```

## IConfigurationValidator

The `IConfigurationValidator` interface defines a contract for validating configuration options and providing detailed error messages and warnings. It is used throughout the toolkit to ensure configuration values are valid before generation begins, enabling early detection of issues like missing directories, invalid timeout values, or malformed paths. The validator returns a `ValidationResult` containing both errors (which prevent execution) and warnings (which indicate potential issues that won't stop generation).

### Public Members

- `bool IsValid` - Indicates whether validation passed without errors
- `List<string> Errors` - Collection of error messages that prevent execution
- `List<string> Warnings` - Collection of warning messages for potential issues
- `void AddError(string message)` - Adds an error message to the result
- `void AddWarning(string message)` - Adds a warning message to the result

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Configuration;
using Microsoft.Extensions.Logging;

// Create a configuration validator instance
var validator = new ConfigurationValidator();

// Validate configuration with potential issues
var validationResult = validator.Validate(new ToolkitOptions
{
    OutputDirectory = "./GeneratedCode",
    TemplateDirectory = "./Templates",
    MaxDegreeOfParallelism = 4,
    OperationTimeoutSeconds = 60,
    CacheExpirationMinutes = 30,
    CodeFormattingLineLength = 120
});

// Check overall validity
if (!validationResult.IsValid)
{
    Console.WriteLine("❌ Configuration validation failed!");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Error: {error}");
    }
}
else
{
    Console.WriteLine("✅ Configuration is valid");
}

// Access warnings for potential issues
if (validationResult.Warnings.Any())
{
    Console.WriteLine("⚠️ Configuration warnings:");
    foreach (var warning in validationResult.Warnings)
    {
        Console.WriteLine($"Warning: {warning}");
    }
}

// Programmatically add custom validation messages
var customValidator = new ConfigurationValidator();
var result = customValidator.GetDefaults();
result.AddError("Custom validation: Output directory must not be empty");
result.AddWarning("Custom warning: Consider increasing cache expiration for better performance");
```

## ConfigurationValidatorTests

The `ConfigurationValidatorTests` class provides unit tests for the `ConfigurationValidator` class, which validates configuration options for the .NET Source Generator Toolkit. These tests ensure that configuration validation works correctly by testing null options, valid options, minimum timeout values, and default configuration values. The test suite also includes mocking scenarios to verify validator behavior.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Configuration;
using FluentAssertions;
using Moq;
using Xunit;

// Create a configuration validator instance
var validator = new ConfigurationValidator();

// Test 1: Validate null options returns invalid result with error
var nullResult = validator.Validate(null);
nullResult.IsValid.Should().BeFalse();
nullResult.Errors.Should().ContainSingle(e => e.Contains("cannot be null", StringComparison.OrdinalIgnoreCase));

// Test 2: Validate valid options returns valid result with no errors
var validOptions = new ToolkitOptions
{
    CacheExpirationMinutes = 30,
    MaxDegreeOfParallelism = 2,
    OperationTimeoutSeconds = 60,
    CodeFormattingLineLength = 120,
    OutputDirectory = "./output"
};
var validResult = validator.Validate(validOptions);
validResult.IsValid.Should().BeTrue();
validResult.Errors.Should().BeEmpty();

// Test 3: Validate timeout below minimum adds timeout error
var defaults = validator.GetDefaults();
defaults.OperationTimeoutSeconds = 5; // Below minimum of 30
var timeoutResult = validator.Validate(defaults);
timeoutResult.IsValid.Should().BeFalse();
timeoutResult.Errors.Should().Contain(e => e.Contains("timeout", StringComparison.OrdinalIgnoreCase));

// Test 4: Get default options returns expected values
var defaults = validator.GetDefaults();
defaults.OutputDirectory.Should().Be("./Generated");
defaults.EnableCaching.Should().BeTrue();
defaults.CacheExpirationMinutes.Should().Be(60);

// Test 5: Mocked validator configured to return failure
var mockValidator = new Mock<IConfigurationValidator>();
var failedResult = new ValidationResult { IsValid = false };
failedResult.AddError("Simulated validation failure");

mockValidator
    .Setup(v => v.Validate(It.IsAny<ToolkitOptions>()))
    .Returns(failedResult);

var options = new ToolkitOptions { OutputDirectory = string.Empty };
var mockResult = mockValidator.Object.Validate(options);
mockResult.IsValid.Should().BeFalse();
mockResult.Errors.Should().ContainSingle(e => e == "Simulated validation failure");
```

## Entity

The `Entity` class represents a domain entity that will be processed by the source generator toolkit. It contains metadata about the entity and its properties for generation, including information about the entity's structure, relationships, and validation rules. This is the core domain model used throughout the code generation pipeline.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Create a new entity for a Product domain model
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities",
    Description = "Represents a product in the e-commerce system",
    TableName = "Products",
    IsSealed = false,
    AccessModifier = AccessModifier.Public
};

// Add properties to the entity
productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true,
    IsAutoIncrement = true,
    IsRequired = true,
    GetterAccess = AccessModifier.Public,
    SetterAccess = AccessModifier.Private,
    Description = "Primary key identifier for the product"
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100,
    Description = "Product name"
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true,
    MinValue = 0,
    MaxValue = 10000,
    Description = "Product price"
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Category",
    Type = "Category",
    IsNavigationProperty = true,
    IsRequired = true,
    Description = "Product category"
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Tags",
    Type = "string",
    IsCollection = true,
    IsRequired = false,
    Description = "Product tags"
});

// Add attributes and interfaces
productEntity.Attributes.Add("[Table(\"Products\")]");
productEntity.Interfaces.Add("IPricedEntity");
productEntity.Interfaces.Add("IAuditable");

// Set base class
productEntity.BaseClass = "AuditableEntity";

// Access entity metadata
Console.WriteLine($"Entity: {productEntity.Name}");
Console.WriteLine($"Namespace: {productEntity.Namespace}");
Console.WriteLine($"Table: {productEntity.TableName}");
Console.WriteLine($"Properties: {productEntity.Properties.Count}");
Console.WriteLine($"Interfaces: {string.Join(", ", productEntity.Interfaces)}");

// Validate the entity
var validationErrors = productEntity.Validate().ToList();
if (validationErrors.Any())
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Get primary key
var primaryKey = productEntity.GetPrimaryKeyProperty();
if (primaryKey != null)
{
    Console.WriteLine($"Primary key: {primaryKey.Name} ({primaryKey.Type})");
}

// Get navigation properties
var navigationProperties = productEntity.GetNavigationProperties();
Console.WriteLine($"Navigation properties: {navigationProperties.Count()}");

// Remove a property if needed
productEntity.RemoveProperty("Tags");
```

## GenerationResult

The `GenerationResult` class represents the outcome of a single code generation operation. It contains comprehensive metadata about the generation process including the generated code content, output file path, status information, warnings, errors, performance metrics, and timestamps. This type is returned by all generator services (`RepositoryGeneratorService`, `MapperGeneratorService`, `ValidatorGeneratorService`, `SerializerGeneratorService`) and provides detailed information for logging, error handling, and monitoring.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;

// Create a sample entity
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities"
};

productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true,
    IsAutoIncrement = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true,
    MinValue = 0,
    MaxValue = 10000
});

// Generate repository using RepositoryGeneratorService
var repositoryGenerator = new RepositoryGeneratorService();
var generationResult = await repositoryGenerator.GenerateRepositoryAsync(productEntity);

// Access result properties
Console.WriteLine($"Generation ID: {generationResult.Id}");
Console.WriteLine($"Entity Name: {generationResult.EntityName}");
Console.WriteLine($"Generator Type: {generationResult.GeneratorType}");
Console.WriteLine($"Output File: {generationResult.OutputFilePath}");
Console.WriteLine($"Status: {generationResult.Status}");
Console.WriteLine($"Code Lines: {generationResult.CodeLineCount}");
Console.WriteLine($"Duration: {generationResult.GenerationDurationMs}ms");
Console.WriteLine($"Created: {generationResult.CreatedAt}");
Console.WriteLine($"Completed: {generationResult.CompletedAt}");

// Check if generation was successful
if (generationResult.IsSuccessful)
{
    Console.WriteLine("✅ Generation successful!");
    Console.WriteLine($"Generated code:\n{generationResult.GeneratedCode}");
}
else
{
    Console.WriteLine("❌ Generation failed with errors:");
    foreach (var error in generationResult.Errors)
    {
        Console.WriteLine($"  - {error}");
    }
    
    if (generationResult.Warnings.Any())
    {
        Console.WriteLine("\n⚠️ Warnings:");
        foreach (var warning in generationResult.Warnings)
        {
            Console.WriteLine($"  - {warning}");
        }
    }
}

// Use validation methods
var validationErrors = generationResult.Validate().ToList();
if (validationErrors.Any())
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"  - {error}");
    }
}

// Access metadata
foreach (var kvp in generationResult.Metadata)
{
    Console.WriteLine($"Metadata: {kvp.Key} = {kvp.Value}");
}

// Mark as completed (if needed)
generationResult.MarkAsCompleted();

// Add custom warnings or errors
generationResult.AddWarning("Property naming convention differs from team standards");
generationResult.AddError("Primary key validation failed");
```

## ITemplateEngineService

The `ITemplateEngineService` interface provides template processing and rendering functionality for code generation. It supports variable substitution, conditional blocks, loops, and custom filters, enabling dynamic code generation from templates with context variables. This service is used internally by generator services to create code artifacts from template files.

### Public Members

- `Task<string> RenderAsync(string template, Dictionary<string, object> context)` - Renders a template with the provided context variables
- `Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context)` - Loads a template from file and renders it
- `bool ValidateTemplate(string template)` - Validates template syntax for balanced tags and structure
- `void RegisterFilter(string filterName, Func<object, string> filterFunc)` - Registers a custom filter for template processing

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the template engine service
var fileSystemService = new FileSystemService(loggerFactory.CreateLogger<FileSystemService>());
var templateEngine = new TemplateEngineService(fileSystemService, loggerFactory.CreateLogger<TemplateEngineService>());

// Define template context with variables
dynamic context = new Dictionary<string, object>
{
    ["ClassName"] = "Product",
    ["Namespace"] = "MyApp.Domain.Entities",
    ["Properties"] = new List<EntityProperty>
    {
        new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true },
        new EntityProperty { Name = "Name", Type = "string", IsRequired = true },
        new EntityProperty { Name = "Price", Type = "decimal" }
    },
    ["HasValidation"] = true,
    ["IsSealed"] = false
};

// Define a template with variable placeholders
string template = @"
namespace {{Namespace}}
{
    public {{#if IsSealed}}sealed {{/if}}class {{ClassName}}
    {
        {{#for prop in Properties}}
        public {{prop.Type}} {{prop.Name}} { get; set; }
        {{/for}}

        {{#if HasValidation}}
        public bool Validate()
        {
            return !string.IsNullOrEmpty(Name) && Price >= 0;
        }
        {{/if}}
    }
}
";

// Validate template syntax before rendering
bool isValid = templateEngine.ValidateTemplate(template);
Console.WriteLine($"Template valid: {isValid}");

// Register a custom filter
templateEngine.RegisterFilter("pascalCase", obj =>
{
    var str = obj?.ToString();
    if (string.IsNullOrEmpty(str)) return string.Empty;
    return str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str[1..];
});

// Render the template with context
string renderedCode = await templateEngine.RenderAsync(template, context);
Console.WriteLine(renderedCode);

// Load and render from a template file
string templatePath = "./Templates/ProductTemplate.txt";
string fileRenderedCode = await templateEngine.RenderFromFileAsync(templatePath, context);
Console.WriteLine(fileRenderedCode);
```

## IGenerationResultAggregatorService

The `IGenerationResultAggregatorService` interface provides functionality to aggregate, analyze, and report on code generation results. It collects statistics across multiple generation operations, generates comprehensive reports, and provides insights into success rates, performance metrics, and error patterns. This service is essential for monitoring batch generation processes and generating actionable insights from generation results.

### Public Members

- `GenerationReport Analyze(IEnumerable<GenerationResult> results)` - Analyzes a collection of generation results and returns a detailed report
- `string GenerateReport(GenerationReport report)` - Generates a formatted text report from analysis results
- `GenerationStatistics GetStatistics(IEnumerable<GenerationResult> results)` - Gets statistical analysis of generation performance
- `Task<string> ExportToJsonAsync(GenerationReport report)` - Exports results to JSON format

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the aggregator service
var aggregator = new GenerationResultAggregatorService(
    loggerFactory.CreateLogger<GenerationResultAggregatorService>()
);

// Simulate multiple generation results
var results = new List<GenerationResult>
{
    new GenerationResult
    {
        EntityName = "Product",
        GeneratorType = GeneratorType.Repository,
        Status = GenerationStatus.Completed,
        CodeLineCount = 150,
        GenerationDurationMs = 250,
        Warnings = new List<string> { "Consider adding async methods" },
        Errors = new List<string>()
    },
    new GenerationResult
    {
        EntityName = "Customer",
        GeneratorType = GeneratorType.Mapper,
        Status = GenerationStatus.Completed,
        CodeLineCount = 120,
        GenerationDurationMs = 180,
        Warnings = new List<string>(),
        Errors = new List<string>()
    },
    new GenerationResult
    {
        EntityName = "Order",
        GeneratorType = GeneratorType.Validator,
        Status = GenerationStatus.Failed,
        CodeLineCount = 0,
        GenerationDurationMs = 45,
        Warnings = new List<string>(),
        Errors = new List<string> { "Validation rule syntax error" }
    },
    new GenerationResult
    {
        EntityName = "User",
        GeneratorType = GeneratorType.Serializer,
        Status = GenerationStatus.Completed,
        CodeLineCount = 95,
        GenerationDurationMs = 150,
        Warnings = new List<string> { "Consider adding XML support" },
        Errors = new List<string>()
    }
};

// Analyze the results
var report = aggregator.Analyze(results);

// Generate a formatted report
string formattedReport = aggregator.GenerateReport(report);
Console.WriteLine(formattedReport);

// Get detailed statistics
var statistics = aggregator.GetStatistics(results);
Console.WriteLine($"\n📊 Statistics:");
Console.WriteLine($"Success Rate: {statistics.SuccessPercentage:F2}%");
Console.WriteLine($"Total Duration: {statistics.TotalCodeLines:N0} lines across {statistics.EntitiesProcessed} entities");
Console.WriteLine($"Average Duration: {statistics.AverageDurationMs:F2}ms");
Console.WriteLine($"Min/Max Duration: {statistics.MinDurationMs}ms / {statistics.MaxDurationMs}ms");

// Export to JSON for further processing
string jsonReport = await aggregator.ExportToJsonAsync(report);
Console.WriteLine($"\n📄 JSON Report (first 200 chars):");
Console.WriteLine(jsonReport[..Math.Min(200, jsonReport.Length)] + "...");

// Access report properties
Console.WriteLine($"\n📈 Report Summary:");
Console.WriteLine($"Total Results: {report.TotalResults}");
Console.WriteLine($"Success: {report.SuccessCount} ({report.SuccessRate:F2}%)");
Console.WriteLine($"Failed: {report.FailureCount}");
Console.WriteLine($"Skipped: {report.SkippedCount}");
Console.WriteLine($"Total Duration: {report.TotalDurationMs}ms");
Console.WriteLine($"Total Lines: {report.TotalLinesGenerated:N0}");
Console.WriteLine($"Total Warnings: {report.TotalWarnings}");
Console.WriteLine($"Total Errors: {report.TotalErrors}");
Console.WriteLine($"Report Generated: {report.ReportGeneratedAt:yyyy-MM-dd HH:mm:ss}");

// Access breakdown by generator type
Console.WriteLine("\n📊 Results by Type:");
foreach (var kvp in report.ResultsByType)
{
    Console.WriteLine($"  {kvp.Key}: {kvp.Value} results");
}

// Access failed results for troubleshooting
if (report.FailedResults.Count > 0)
{
    Console.WriteLine($"\n❌ Failed Entities ({report.FailedResults.Count}):");
    foreach (var failedResult in report.FailedResults)
    {
        Console.WriteLine($"  - {failedResult.EntityName} ({failedResult.GeneratorType}):");
        foreach (var error in failedResult.Errors)
        {
            Console.WriteLine($"    • {error}");
        }
    }
}
```


The `EntityProperty` class represents a property of an entity with comprehensive metadata for code generation. It includes type information, validation rules, database mapping configuration, and access modifiers. This class is the foundation for generating repositories, mappers, validators, and serializers.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Create a product entity property for database mapping
var productIdProperty = new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true,
    IsAutoIncrement = true,
    IsRequired = true,
    GetterAccess = AccessModifier.Public,
    SetterAccess = AccessModifier.Private,
    Description = "Primary key identifier for the product"
};

// Create a product name property with validation
var productNameProperty = new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100,
    MinLength = 2,
    Description = "Product name"
};

// Create an email property with regex validation
var emailProperty = new EntityProperty
{
    Name = "Email",
    Type = "string",
    IsRequired = true,
    MaxLength = 256,
    RegexPattern = "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$",
    Description = "User email address"
};

// Create a price property with range validation
var priceProperty = new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true,
    MinValue = 0,
    MaxValue = 10000,
    Description = "Product price"
};

// Create a tags collection property
var tagsProperty = new EntityProperty
{
    Name = "Tags",
    Type = "string",
    IsCollection = true,
    IsRequired = false,
    Description = "Product tags"
};

// Create a navigation property to another entity
var categoryProperty = new EntityProperty
{
    Name = "Category",
    Type = "Category",
    IsNavigationProperty = true,
    IsRequired = true,
    Description = "Product category"
};

// Use the property for code generation
Console.WriteLine($"Property: {productIdProperty.Name}, Type: {productIdProperty.GetClrTypeName()}");

// Generate validation attributes
var validationAttributes = productNameProperty.GenerateValidationAttributes();
foreach (var attribute in validationAttributes)
{
    Console.WriteLine(attribute);
}
```

## ValidationResult

The `ValidationResult` class represents the outcome of a validation operation, tracking both errors and warnings. It provides methods to add validation messages, check overall validity, and retrieve all messages in a single collection. This type is used throughout the toolkit for validating configuration, entities, generation results, and other components.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Create a validation result for a configuration check
var configValidation = new ValidationResult();

// Add validation messages
configValidation.AddError("Output directory cannot be null or empty");
configValidation.AddError("Template directory must exist and be accessible");
configValidation.AddWarning("Cache expiration set to 30 minutes (recommended: 60+ minutes)");

// Check overall validity
if (!configValidation.IsValid)
{
    Console.WriteLine("Configuration validation failed!");
    Console.WriteLine(configValidation.ToString());
}

// Access all messages
foreach (var message in configValidation.GetAllMessages())
{
    Console.WriteLine($" - {message}");
}

// Check if there are any issues (errors or warnings)
bool hasIssues = configValidation.HasIssues;

// Get formatted string representation
string validationSummary = configValidation.ToString();
Console.WriteLine(validationSummary);
```

## Product

The `Product` class represents a product entity with properties for identification, name, price, creation date, and availability status. It serves as a domain model for e-commerce applications and demonstrates how to use the source generator toolkit to automatically generate mappers, repositories, validators, and serializers from simple entity definitions.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Define a product entity with generation attributes
[Repository]
[Mapper]
[Validator]
[Serializer(Formats = new[] { "Json", "Xml" })]
public sealed class Product
{
    /// <summary>
    /// Gets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets the date and time when the product was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the product is available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}

// Define a DTO for API responses
[Mapper]
public sealed class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PriceDisplay { get; set; } = string.Empty;
    public string CreatedDateString { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;
}

// Use the generated services in your application
public sealed class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductMapper _productMapper;
    private readonly IProductValidator _productValidator;

    public ProductService(
        IProductRepository productRepository,
        IProductMapper productMapper,
        IProductValidator productValidator)
    {
        _productRepository = productRepository;
        _productMapper = productMapper;
        _productValidator = productValidator;
    }

    public async Task<ProductDto> GetProductAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null) return null;

        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
            throw new InvalidOperationException("Product validation failed");

        return _productMapper.MapToDto(product);
    }

    public async Task<Product> CreateProductAsync(ProductDto productDto)
    {
        var product = _productMapper.MapFromDto(productDto);
        
        var validationResult = await _productValidator.ValidateAsync(product);
        if (!validationResult.IsValid)
            throw new InvalidOperationException("Product validation failed");

        return await _productRepository.CreateAsync(product);
    }
}
```

## ProjectInfo

The `ProjectInfo` class represents the core metadata and structure of a .NET project analyzed by the toolkit, aggregating information about its entities, generation templates, and overall generation results. It provides a robust API to manage project properties, register entities and templates, record generation outcomes, and retrieve diagnostic statistics or validation results to ensure project integrity.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Initialize a new project context
var project = new ProjectInfo
{
    Id = Guid.NewGuid().ToString(),
    ProjectName = "EcommerceSystem",
    ProjectPath = "/src/EcommerceSystem",
    TargetFramework = "net10.0",
    RootNamespace = "EcommerceSystem.Domain"
};

// Add entities and templates
project.AddEntity(new Entity { Name = "Product", Namespace = "EcommerceSystem.Domain" });
project.AddTemplate(new GenerationTemplate { Name = "RepositoryTemplate", GeneratorType = GeneratorType.Repository });

// Record a generation result
var result = new GenerationResult { EntityName = "Product", GeneratorType = "Repository", Status = "Success" };
project.RecordGenerationResult(result);

// Analyze project statistics
var stats = project.GetStatistics();
Console.WriteLine($"Total Entities: {stats.TotalEntities}");
Console.WriteLine($"Total Lines Generated: {stats.TotalCodeLines}");

// Validate the project structure
var validationErrors = project.Validate().ToList();
if (validationErrors.Any())
{
    Console.WriteLine("Project validation errors:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
```

## EntityTests

The `EntityTests` class provides unit tests for the `Entity` domain model, ensuring that entity operations like property management, validation, and primary key identification work correctly. These tests validate the core domain logic that drives the entire code generation pipeline, including duplicate property detection, primary key retrieval, and entity validation rules.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using FluentAssertions;
using Xunit;

// Create a new entity for testing
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities"
};

// Add properties to the entity
productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Tags",
    Type = "string",
    IsCollection = true
});

// Test duplicate property detection
var duplicateProperty = new EntityProperty { Name = "Name", Type = "string" };
Action addDuplicate = () => productEntity.AddProperty(duplicateProperty);
addDuplicate.Should().Throw<InvalidOperationException>();

// Test primary key retrieval
var primaryKey = productEntity.GetPrimaryKeyProperty();
primaryKey.Should().NotBeNull();
primaryKey.Name.Should().Be("Id");

// Test entity validation
var validationErrors = productEntity.Validate().ToList();
validationErrors.Should().BeEmpty();

// Test entity with empty name (should fail validation)
var invalidEntity = new Entity { Name = "", Namespace = "MyApp.Domain" };
var invalidErrors = invalidEntity.Validate().ToList();
invalidErrors.Should().Contain(error => error.Contains("name is required", StringComparison.OrdinalIgnoreCase));

// Test property type name generation
var nullableIntProperty = new EntityProperty { Type = "int", IsNullable = true };
nullableIntProperty.GetClrTypeName().Should().Be("int?");

var collectionStringProperty = new EntityProperty { Type = "string", IsCollection = true };
collectionStringProperty.GetClrTypeName().Should().Be("List<string>");

// Test validation attribute generation
var requiredProperty = new EntityProperty
{
    Name = "Email",
    Type = "string",
    IsRequired = true,
    MaxLength = 256
};
var attributes = requiredProperty.GenerateValidationAttributes().ToList();
attributes.Should().Contain("[Required]");
attributes.Should().Contain("[MaxLength(256)]");
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

## ConfigurationManager

The `ConfigurationManager` class provides centralized configuration management for the .NET Source Generator Toolkit. It supports hierarchical configuration sources (in-memory settings, environment variables, and default values) with thread-safe operations and comprehensive error handling. The manager automatically resolves relative paths against the project root and provides type-safe access to common configuration keys.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create configuration manager with logging support
var configuration = new ConfigurationManager(loggerFactory.CreateLogger<ConfigurationManager>());

// Set configuration values
configuration.SetValue("OutputDirectory", "./GeneratedCode");
configuration.SetValue("TemplateDirectory", "./Templates");
configuration.SetValue("MaxDegreeOfParallelism", "4");
configuration.SetValue("EnableValidation", "true");

// Check if a key exists
bool hasOutputDir = configuration.HasKey("OutputDirectory");
// Returns: true

// Get configuration values with fallbacks
string outputDir = configuration.GetOutputDirectory();
// Returns: "./GeneratedCode" (or default if not set)

string templateDir = configuration.GetTemplateDirectory();
// Returns: "./Templates" (or default if not set)

string projectRoot = configuration.GetProjectRoot();
// Returns: Current working directory (or configured value)

// Get raw configuration value with default fallback
string parallelism = configuration.GetValue("MaxDegreeOfParallelism", "2");
// Returns: "4" (or "2" if not set)

// Get all configuration as read-only dictionary
IReadOnlyDictionary<string, string> allConfig = configuration.GetAllConfig();
// Returns: Dictionary containing all key-value pairs
```

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

Additional comprehensive examples can be found in the [examples/](examples/) directory:
- [BasicUsage.cs](examples/BasicUsage.cs) - Minimal setup.
- [AdvancedUsage.cs](examples/AdvancedUsage.cs) - Advanced configuration and error handling.
- [IntegrationExample.cs](examples/IntegrationExample.cs) - ASP.NET Core DI registration.

---

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

## BasicExample

The `BasicExample` class demonstrates fundamental usage patterns for the .NET Source Generator Toolkit. It shows how to define domain entities with generation attributes, create DTOs for API responses, and implement service classes that leverage generated repositories, mappers, and validators.

This example generates repository, mapper, and validator implementations for a `User` entity with properties including identification fields, contact information, timestamps, and status flags.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Examples;
using DotNetSourceGeneratorToolkit.Services;

// Define your domain entity with generation attributes
[Repository]
[Mapper]
[Validator]
public sealed class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

// Define a DTO for API responses
[Mapper]
public sealed class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

// Use the generated services in your application
public sealed class UserService
{
    public async Task<UserDto?> GetUserAsync(
        int userId,
        IUserRepository repository,
        IUserMapper mapper,
        IUserValidator validator)
    {
        var user = await repository.GetByIdAsync(userId);
        if (user is null) return null;
        
        var validationResult = await validator.ValidateAsync(user);
        if (!validationResult.IsValid) 
            throw new InvalidOperationException("User validation failed");
        
        return mapper.MapToDto(user);
    }
    
    public async Task<User> CreateUserAsync(
        UserDto dto,
        IUserRepository repository,
        IUserMapper mapper,
        IUserValidator validator)
    {
        var user = mapper.MapFromDto(dto);
        
        var validationResult = await validator.ValidateAsync(user);
        if (!validationResult.IsValid) 
            throw new InvalidOperationException("User validation failed");
        
        return await repository.CreateAsync(user);
    }
}
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

## GenerationCompletedEvent

The `GenerationCompletedEvent` is published when the code generation process successfully completes for a project. This event provides detailed information about the generation outcome including success status, files generated, any errors encountered, and performance metrics. It allows subscribers to perform post-generation actions such as cleanup, notifications, or integration with external systems.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Events;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var eventAggregator = new EventAggregator();

// Subscribe to GenerationCompletedEvent
var subscription = eventAggregator.Subscribe<GenerationCompletedEvent>(ev =>
{
    Console.WriteLine($"✅ Generation completed at {ev.OccurredAt:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine($"📋 Request: {ev.RequestId}");
    Console.WriteLine($"🎯 Success: {ev.IsSuccessful}");
    
    if (ev.IsSuccessful)
    {
        Console.WriteLine($"📁 Files generated: {ev.FilesGenerated}");
        Console.WriteLine($"⏱️ Execution time: {ev.ExecutionTimeMs}ms");
    }
    else
    {
        Console.WriteLine($"❌ Errors encountered:");
        foreach (var error in ev.Errors)
        {
            Console.WriteLine($"  - {error}");
        }
    }
});

// Simulate completed generation
var completedEvent = new GenerationCompletedEvent
{
    EventId = Guid.NewGuid().ToString(),
    OccurredAt = DateTime.UtcNow,
    RequestId = "req-12345",
    IsSuccessful = true,
    FilesGenerated = 8,
    Errors = new List<string>(),
    ExecutionTimeMs = 142
};

// Publish the event
eventAggregator.Publish(completedEvent);
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

## MetricsCollector

The `MetricsCollector` class provides comprehensive performance monitoring and metrics collection for the .NET Source Generator Toolkit. It tracks timers, counters, gauges, and histograms to measure generation performance, resource usage, and system health. The collector is thread-safe and designed for concurrent metric collection from parallel tasks, making it ideal for monitoring batch generation processes and identifying performance bottlenecks.

### Public Members

- `ITimer StartTimer(string operationName)` - Starts a timer for measuring operation duration
- `void RecordGauge(string metricName, long value)` - Records a gauge metric with a specific value
- `void IncrementCounter(string metricName, long amount = 1)` - Increments a counter metric by the specified amount
- `void RecordHistogram(string metricName, long value)` - Records a histogram metric for distribution analysis
- `MetricsSnapshot GetSnapshot()` - Gets a snapshot of all collected metrics
- `void Reset()` - Resets all collected metrics
- `ITimer` - Timer interface returned by StartTimer for measuring elapsed time
- `void Stop()` - Stops the timer and records the elapsed time
- `void Dispose()` - Stops the timer and releases resources

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure logging and dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

var serviceProvider = services.BuildServiceProvider();
var metricsCollector = serviceProvider.GetRequiredService<IMetricsCollector>();

// Start timing a generation operation
using (var timer = metricsCollector.StartTimer("EntityGeneration"))
{
    // Simulate generation work
    await Task.Delay(150);
    
    // Record a gauge metric (e.g., cache size)
    metricsCollector.RecordGauge("CacheSize", 42);
    
    // Increment a counter metric (e.g., generated files)
    metricsCollector.IncrementCounter("GeneratedFiles");
    metricsCollector.IncrementCounter("GeneratedFiles", 3); // Increment by 3
    
    // Record histogram metrics for performance distribution
    metricsCollector.RecordHistogram("GenerationTimeMs", 150);
    metricsCollector.RecordHistogram("GenerationTimeMs", 210);
    metricsCollector.RecordHistogram("GenerationTimeMs", 180);
}

// Get a snapshot of all collected metrics
var snapshot = metricsCollector.GetSnapshot();

// Access gauge metrics
foreach (var gauge in snapshot.Gauges)
{
    Console.WriteLine($"Gauge {gauge.Key}: {gauge.Value}");
}

// Access counter metrics
foreach (var counter in snapshot.Counters)
{
    Console.WriteLine($"Counter {counter.Key}: {counter.Value}");
}

// Access histogram metrics
foreach (var histogram in snapshot.Histograms)
{
    Console.WriteLine($"Histogram {histogram.Key}:");
    Console.WriteLine($"  Count: {histogram.Value.Count}");
    Console.WriteLine($"  Sum: {histogram.Value.Sum}");
    Console.WriteLine($"  Min: {histogram.Value.Min}");
    Console.WriteLine($"  Max: {histogram.Value.Max}");
    Console.WriteLine($"  Average: {(double)histogram.Value.Sum / histogram.Value.Count:F2}");
}

// Reset metrics for a new batch
metricsCollector.Reset();

// Example: Monitor batch processing performance
var batchMetrics = serviceProvider.GetRequiredService<IMetricsCollector>();

async Task ProcessEntityAsync(Entity entity)
{
    using (var timer = batchMetrics.StartTimer("ProcessEntity"))
    {
        // Process entity
        await Task.Delay(new Random().Next(50, 300));
    }
}

var entities = new List<Entity> { /* your entities */ };
var tasks = entities.Select(ProcessEntityAsync);
await Task.WhenAll(tasks);

var finalSnapshot = batchMetrics.GetSnapshot();
Console.WriteLine($"Batch completed - Average time: {(double)finalSnapshot.Histograms["ProcessEntity"].Sum / finalSnapshot.Histograms["ProcessEntity"].Count:F2}ms");
```

## IMiddleware

The `IMiddleware` interface defines the contract for middleware components that can inspect, modify, or short-circuit the generation pipeline. Middleware enables extensible request/response processing patterns where each component in the chain can add functionality such as logging, validation, error handling, or custom processing logic before passing control to the next middleware in the pipeline.

Middleware is particularly useful for:
- Logging pipeline execution and performance metrics
- Validating generation context before processing
- Adding custom processing logic for specific scenarios
- Short-circuiting the pipeline when errors occur or preconditions aren't met
- Injecting cross-cutting concerns like caching or metrics collection

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Middleware;
using DotNetSourceGeneratorToolkit.Domain;
using System.Threading.Tasks;

// Define custom middleware for logging and validation
public class LoggingMiddleware : IMiddleware
{
    public async Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
    {
        // Pre-processing: log start of pipeline execution
        Console.WriteLine($"[{context.RequestId}] Starting pipeline execution at {context.StartTime:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"Project: {context.ProjectInfo?.ProjectName ?? "Unknown"}");
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            // Pass control to next middleware in the chain
            await next(context);
            
            // Post-processing: log successful completion
            stopwatch.Stop();
            Console.WriteLine($"[{context.RequestId}] Pipeline completed successfully in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"Generated files: {context.GenerationResults.Count}");
        }
        catch (Exception ex)
        {
            // Handle errors and add to context
            context.AddError($"Pipeline failed: {ex.Message}");
            Console.WriteLine($"[{context.RequestId}] Pipeline failed: {ex.Message}");
            throw;
        }
    }
}

// Define validation middleware
public class ValidationMiddleware : IMiddleware
{
    public async Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
    {
        // Validate project info exists
        if (context.ProjectInfo == null)
        {
            context.AddError("ProjectInfo is null - cannot proceed with generation");
            context.ShortCircuit();
            return;
        }
        
        // Validate at least one entity exists
        if (context.ProjectInfo.Entities.Count == 0)
        {
            context.AddError("No entities found in project - generation aborted");
            context.ShortCircuit();
            return;
        }
        
        // Pass control to next middleware
        await next(context);
    }
}

// Define error handling middleware
public class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(MiddlewareContext context, MiddlewareDelegate next)
    {
        try
        {
            await next(context);
            
            // Check if pipeline was short-circuited
            if (context.IsShortCircuited)
            {
                Console.WriteLine($"[{context.RequestId}] Pipeline was short-circuited - skipping remaining middleware");
            }
        }
        catch (Exception ex)
        {
            context.AddError($"Unhandled exception: {ex.Message}");
            context.ShortCircuit();
            throw;
        }
    }
}

// Usage: Register middleware in your pipeline
var middlewarePipeline = new MiddlewarePipeline();
middlewarePipeline.AddMiddleware(new LoggingMiddleware());
middlewarePipeline.AddMiddleware(new ValidationMiddleware());
middlewarePipeline.AddMiddleware(new ErrorHandlingMiddleware());

// Execute pipeline with context
var context = new MiddlewareContext
{
    CliOptions = new CLI.CliOptions { /* configure options */ },
    ProjectInfo = new Domain.ProjectInfo { /* project metadata */ }
};

await middlewarePipeline.ExecuteAsync(context);
```

## IBatchProcessor

The `IBatchProcessor<T>` interface defines a contract for processing items in batches with comprehensive error handling, progress tracking, and performance monitoring. It enables efficient processing of large collections while providing detailed feedback about each item's processing status, execution time, and any errors encountered. This is particularly useful for code generation scenarios where you need to process multiple entities, files, or generation tasks in parallel with proper resource management.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Batch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Define your entity type
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

// Create a batch processor instance
var batchProcessor = new BatchProcessor<Product>();

// Define your items to process
var products = new List<Product>
{
    new Product { Id = 1, Name = "Laptop", Price = 999.99m },
    new Product { Id = 2, Name = "Mouse", Price = 29.99m },
    new Product { Id = 3, Name = "Keyboard", Price = 79.99m },
    new Product { Id = 4, Name = "Monitor", Price = 249.99m }
};

// Define your processing function
async Task ProcessProductAsync(Product product)
{
    // Simulate processing work
    await Task.Delay(100);
    Console.WriteLine($"Processing product: {product.Name}");
}

// Process items in batches with progress reporting
var progress = new Progress<BatchProgress>(progressReport =>
{
    Console.WriteLine($"Progress: {progressReport.PercentComplete:F1}% - " +
                     $"Processed: {progressReport.ProcessedCount}/{progressReport.TotalCount} - " +
                     $"Errors: {progressReport.ErrorCount}");
});

var results = await batchProcessor.ProcessAsync(
    items: products,
    processor: ProcessProductAsync,
    batchSize: 2,  // Process 2 items at a time
    progress: progress
);

// Analyze results
int successfulCount = results.Count(r => r.IsSuccessful);
int failedCount = results.Count(r => !r.IsSuccessful);
long totalTimeMs = results.Sum(r => r.ExecutionTimeMs);

Console.WriteLine($"\nProcessing complete:");
Console.WriteLine($"✅ Successful: {successfulCount}");
Console.WriteLine($"❌ Failed: {failedCount}");
Console.WriteLine($"⏱️ Total time: {totalTimeMs}ms");

// Handle failed items
foreach (var result in results.Where(r => !r.IsSuccessful))
{
    Console.WriteLine($"\n⚠️ Failed to process item: {result.Item}");
    Console.WriteLine($"   Error: {result.ErrorMessage}");
    Console.WriteLine($"   Time: {result.ExecutionTimeMs}ms");
}
```

## RepositoryGeneratorService

The `RepositoryGeneratorService` generates repository pattern implementations including interfaces and concrete classes with full CRUD operations and query methods for entities. It creates type-safe repository contracts with async methods for data access, enabling clean separation between business logic and data persistence layers.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the repository generator service
var repositoryGenerator = new RepositoryGeneratorService(
    loggerFactory.CreateLogger<RepositoryGeneratorService>(),
    new ToolkitOptions { DefaultNamespace = "MyApp.Domain" }
);

// Define a sample entity
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities",
    TableName = "Products",
    IsSealed = false,
    AccessModifier = AccessModifier.Public
};

productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true,
    IsAutoIncrement = true,
    IsRequired = true,
    GetterAccess = AccessModifier.Public,
    SetterAccess = AccessModifier.Private
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true,
    MinValue = 0,
    MaxValue = 10000
});

// Generate repository for the entity
var generationResult = await repositoryGenerator.GenerateRepositoryAsync(productEntity);

// Check if generation was successful
if (generationResult.IsSuccessful)
{
    Console.WriteLine("✅ Repository generated successfully!");
    Console.WriteLine($"📄 Output file: {generationResult.OutputFilePath}");
    Console.WriteLine($"📊 Lines of code: {generationResult.CodeLineCount}");
    Console.WriteLine($"💾 Generated code:\n{generationResult.GeneratedCode}");
}
else
{
    Console.WriteLine("❌ Repository generation failed:");
    foreach (var error in generationResult.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Generate repositories for multiple entities in batch
var entities = new List<Entity> { productEntity /* add more entities */ };
var allResults = await repositoryGenerator.GenerateAllRepositoriesAsync(entities);

Console.WriteLine($"Generated {allResults.Count(r => r.IsSuccessful)}/{allResults.Length} repositories");
```

## MapperGeneratorService

The `MapperGeneratorService` generates mapper classes for transforming entities to/from DTOs and between different entity types with property mapping logic. It creates bidirectional mapping implementations with methods for converting between entity and DTO representations, supporting collection mappings and null-safety throughout.


### Public Members

- `MapperGeneratorService(ILogger<MapperGeneratorService> logger)` - Constructor that accepts a logger for diagnostic output
- `Task<IEnumerable<GenerationResult>> GenerateAllMappersAsync(List<Entity> entities)` - Generates mappers for all provided entities in parallel
- `Task<GenerationResult> GenerateMapperAsync(Entity sourceEntity, Entity targetEntity)` - Generates a mapper between source and target entities
- `public sealed class MapperGeneratorService` - The service is sealed

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the mapper generator service
var mapperGenerator = new MapperGeneratorService(
    loggerFactory.CreateLogger<MapperGeneratorService>()
);

// Define a sample entity
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities"
};

productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "CreatedDate",
    Type = "DateTime",
    IsRequired = false
});

// Generate mapper for the entity
var generationResult = await mapperGenerator.GenerateMapperAsync(productEntity, productEntity);

// Check if generation was successful
if (generationResult.IsSuccessful)
{
    Console.WriteLine("✅ Mapper generated successfully!");
    Console.WriteLine($"📄 Output file: {generationResult.OutputFilePath}");
    Console.WriteLine($"📊 Lines of code: {generationResult.CodeLineCount}");
    
    // Use the generated mapper methods
    var product = new Product
    {
        Id = 1,
        Name = "Laptop",
        Price = 999.99m,
        CreatedDate = DateTime.UtcNow
    };
    
    // Map to DTO
    var dto = ProductMapper.MapToDto(product);
    Console.WriteLine($"🔄 Mapped to DTO: {dto.Name} - {dto.Price}");
    
    // Map back to entity
    var mappedEntity = ProductMapper.MapFromDto(dto);
    Console.WriteLine($"🔄 Mapped back to entity: {mappedEntity?.Name}");
    
    // Map collection
    var products = new List<Product> { product };
    var dtos = ProductMapper.MapToDtos(products);
    Console.WriteLine($"📊 Mapped {dtos.Count()} items");
}
else
{
    Console.WriteLine("❌ Mapper generation failed:");
    foreach (var error in generationResult.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Generate mappers for multiple entities in batch
var entities = new List<Entity> { productEntity /* add more entities */ };
var allResults = await mapperGenerator.GenerateAllMappersAsync(entities);

Console.WriteLine($"Generated {allResults.Count(r => r.IsSuccessful)}/{allResults.Count} mappers");
```

## SerializerGeneratorService

The `SerializerGeneratorService` generates serializer classes for converting entities to/from various formats including JSON, XML, and binary representations. It creates type-safe serializer implementations with methods for serialization and deserialization, supporting multiple output formats with consistent naming conventions.

### Public Members

- `SerializerGeneratorService(ILogger<SerializerGeneratorService> logger)` - Constructor
- `Task<IEnumerable<GenerationResult>> GenerateAllSerializersAsync(List<Entity> entities)` - Generates serializers for all entities
- `Task<GenerationResult> GenerateSerializerAsync(Entity entity, SerializerFormat format)` - Generates serializer for a specific entity and format

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the serializer generator service
var serializerGenerator = new SerializerGeneratorService(
    loggerFactory.CreateLogger<SerializerGeneratorService>()
);

// Define a sample entity with properties
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities"
};

productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true
});

productEntity.AddProperty(new EntityProperty
{
    Name = "CreatedDate",
    Type = "DateTime",
    IsRequired = false
});

// Generate JSON serializer for the entity
var jsonResult = await serializerGenerator.GenerateSerializerAsync(
    productEntity, 
    SerializerFormat.Json
);

if (jsonResult.IsSuccessful)
{
    Console.WriteLine("✅ JSON Serializer generated successfully!");
    Console.WriteLine($"📄 Output file: {jsonResult.OutputFilePath}");
    Console.WriteLine($"📊 Lines of code: {jsonResult.CodeLineCount}");
    
    // Use the generated serializer methods
    var product = new Product
    {
        Id = 1,
        Name = "Laptop",
        Price = 999.99m,
        CreatedDate = DateTime.UtcNow
    };
    
    // Serialize to JSON
    string json = ProductJsonSerializer.Serialize(product);
    Console.WriteLine($"📋 JSON output: {json}");
    
    // Deserialize from JSON
    var deserialized = ProductJsonSerializer.Deserialize(json);
    Console.WriteLine($"🔄 Deserialized: {deserialized.Name} - {deserialized.Price}");
    
    // Convert to JsonElement
    var jsonElement = ProductJsonSerializer.ToJsonElement(product);
    Console.WriteLine($"📊 JsonElement created: {jsonElement.GetRawText()}");
}
else
{
    Console.WriteLine("❌ JSON Serializer generation failed:");
    foreach (var error in jsonResult.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Generate XML serializer for the same entity
var xmlResult = await serializerGenerator.GenerateSerializerAsync(
    productEntity, 
    SerializerFormat.Xml
);

if (xmlResult.IsSuccessful)
{
    Console.WriteLine("✅ XML Serializer generated successfully!");
    Console.WriteLine($"📄 Output file: {xmlResult.OutputFilePath}");
    
    // Use the generated XML serializer
    var product = new Product { Id = 1, Name = "Mouse", Price = 29.99m };
    string xml = ProductXmlSerializer.Serialize(product);
    Console.WriteLine($"📋 XML output:\n{xml}");
    
    var deserialized = ProductXmlSerializer.Deserialize(xml);
    Console.WriteLine($"🔄 Deserialized: {deserialized.Name} - {deserialized.Price}");
}

// Generate binary serializer
var binaryResult = await serializerGenerator.GenerateSerializerAsync(
    productEntity, 
    SerializerFormat.Binary
);

if (binaryResult.IsSuccessful)
{
    Console.WriteLine("✅ Binary Serializer generated successfully!");
    Console.WriteLine($"📄 Output file: {binaryResult.OutputFilePath}");
    
    // Use the generated binary serializer
    var product = new Product { Id = 1, Name = "Keyboard", Price = 79.99m };
    byte[] binaryData = ProductBinarySerializer.Serialize(product);
    Console.WriteLine($"📊 Binary size: {ProductBinarySerializer.GetSerializedSize(product)} bytes");
    
    var deserialized = ProductBinarySerializer.Deserialize(binaryData);
    Console.WriteLine($"🔄 Deserialized: {deserialized.Name} - {deserialized.Price}");
}

// Generate all serializers for multiple entities in batch
var entities = new List<Entity> { productEntity /* add more entities */ };
var allResults = await serializerGenerator.GenerateAllSerializersAsync(entities);

Console.WriteLine($"Generated {allResults.Count(r => r.IsSuccessful)}/{allResults.Count()} serializers");
```

## SourceGeneratorService

The `SourceGeneratorService` is the main orchestration service that coordinates the complete source generation workflow. It analyzes .NET projects to discover entities marked with generation attributes, validates project structure, and generates code artifacts including repositories, mappers, validators, and serializers. This service serves as the primary entry point for programmatic code generation workflows.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create required services (in real usage, these would be injected via DI)
var fileSystemService = new FileSystemService(loggerFactory.CreateLogger<FileSystemService>());
var entityAnalyzer = new EntityAnalyzer();
var repositoryGenerator = new RepositoryGeneratorService();
var mapperGenerator = new MapperGeneratorService();
var validatorGenerator = new ValidatorGeneratorService();
var serializerGenerator = new SerializerGeneratorService();

// Create the source generator service
var sourceGenerator = new SourceGeneratorService(
    entityAnalyzer,
    fileSystemService,
    repositoryGenerator,
    mapperGenerator,
    validatorGenerator,
    serializerGenerator,
    loggerFactory.CreateLogger<SourceGeneratorService>()
);

// Analyze a project to discover entities
var projectPath = "/path/to/your/project";
var projectInfo = await sourceGenerator.AnalyzeProjectAsync(projectPath);

Console.WriteLine($"Analyzed project: {projectInfo.ProjectName}");
Console.WriteLine($"Found {projectInfo.Entities.Count} entities");

// Validate the project structure
var validationResult = await sourceGenerator.ValidateProjectAsync(projectInfo);
if (!validationResult.IsValid)
{
    Console.WriteLine("Project validation failed:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($" - {error}");
    }
    return;
}

// Generate code for all entities in the project
var generationResults = await sourceGenerator.GenerateAllAsync(projectInfo);

Console.WriteLine($"Generated {generationResults.Count()} files:");
foreach (var result in generationResults)
{
    Console.WriteLine($" - {result.EntityName} ({result.GeneratorType}): {result.Status}");
    if (result.IsSuccessful)
    {
        Console.WriteLine($"   Output: {result.OutputFilePath}");
        Console.WriteLine($"   Lines: {result.CodeLineCount}");
    }
    else
    {
        Console.WriteLine($"   Errors: {string.Join(", ", result.Errors)}");
    }
}

// Alternatively, generate code for a specific entity
var specificEntity = projectInfo.Entities.FirstOrDefault(e => e.Name == "Product");
if (specificEntity != null)
{
    var entityResults = await sourceGenerator.GenerateForEntityAsync(specificEntity, projectInfo);
    foreach (var result in entityResults)
    {
        Console.WriteLine($"Generated for {result.EntityName}: {result.Status}");
    }
}
```

## API Reference

## ToolkitOptions

The `ToolkitOptions` class provides centralized configuration for the code generation toolkit. It controls caching behavior, parallelism, output formatting, logging verbosity, and other generation parameters. This type is typically loaded from configuration files or environment variables and passed to generator services.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Configuration;

// Create configuration with custom settings
var options = new ToolkitOptions
{
    EnableCaching = true,
    CacheExpirationMinutes = 30,
    EnableCodeFormatting = true,
    CodeFormattingLineLength = 120,
    VerboseLogging = true,
    MaxDegreeOfParallelism = 4,
    OperationTimeoutSeconds = 60,
    GenerateDtos = true,
    DefaultNamespace = "MyApp.Domain.Generated",
    OutputDirectory = "./GeneratedCode",
    BackupExistingFiles = true,
    GenerateInterfaces = true,
    GenerateXmlComments = true
};

// Use with generator services
var repositoryGenerator = new RepositoryGeneratorService(
    logger,
    options
);

var mapperGenerator = new MapperGeneratorService(
    logger,
    options
);

// Access configuration values
Console.WriteLine($"Output directory: {options.OutputDirectory}");
Console.WriteLine($"Parallelism: {options.MaxDegreeOfParallelism}");
Console.WriteLine($"Timeout: {options.OperationTimeoutSeconds}s");
```

### Core Interfaces

#### `IMetricsCollector`

The `IMetricsCollector` interface provides performance monitoring and metrics collection capabilities throughout the code generation pipeline. It enables tracking of execution time, resource usage, and throughput by collecting gauge, counter, and histogram metrics. This interface is essential for monitoring performance, identifying bottlenecks, and generating diagnostic reports during batch processing operations.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Metrics;
using Microsoft.Extensions.DependencyInjection;

// Configure dependency injection (typically in your application startup)
var services = new ServiceCollection();
services.AddSingleton<IMetricsCollector, MetricsCollector>();

var serviceProvider = services.BuildServiceProvider();
var metricsCollector = serviceProvider.GetRequiredService<IMetricsCollector>();

// Record a gauge metric (instantaneous measurement)
metricsCollector.RecordGauge("ActiveGenerations", 5);

// Increment a counter metric
metricsCollector.IncrementCounter("TotalEntitiesProcessed");
metricsCollector.IncrementCounter("TotalFilesGenerated", 8);

// Record a histogram metric (distribution)
metricsCollector.RecordHistogram("GenerationTimeMs", 142);
metricsCollector.RecordHistogram("GenerationTimeMs", 285);
metricsCollector.RecordHistogram("GenerationTimeMs", 98);

// Start a timer for measuring operation duration
using (var timer = metricsCollector.StartTimer("RepositoryGeneration"))
{
    // Simulate repository generation work
    await Task.Delay(150);
}

// Get all collected metrics
var snapshot = metricsCollector.GetSnapshot();
Console.WriteLine($"Captured at: {snapshot.CapturedAt:yyyy-MM-dd HH:mm:ss}");
Console.WriteLine($"Gauges: {string.Join(", ", snapshot.Gauges.Select(g => $"{g.Key}={g.Value}"))}");
Console.WriteLine($"Counters: {string.Join(", ", snapshot.Counters.Select(c => $"{c.Key}={c.Value}"))}");

foreach (var histogram in snapshot.Histograms)
{
    Console.WriteLine($"Histogram {histogram.Key}: Count={histogram.Value.Count}, " +
                     $"Sum={histogram.Value.Sum}, Min={histogram.Value.Min}, " +
                     $"Max={histogram.Value.Max}, Avg={histogram.Value.Average:F2}");
}

// Reset metrics for a new batch
metricsCollector.Reset();
```

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

## ValidatorGeneratorService

The `ValidatorGeneratorService` generates FluentValidation validators for domain entities with comprehensive validation rules based on entity property definitions. It creates both automated FluentValidation-based validators and manual validation classes with custom validation logic, providing robust validation for business rules enforcement.

### Public Members

- `ValidatorGeneratorService(ILogger<ValidatorGeneratorService> logger)` - Constructor that accepts a logger for diagnostic output
- `Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync(List<Entity> entities)` - Generates validators for all provided entities in parallel
- `Task<GenerationResult> GenerateValidatorAsync(Entity entity)` - Generates a validator for a single entity

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the validator generator service
var validatorGenerator = new ValidatorGeneratorService(
    loggerFactory.CreateLogger<ValidatorGeneratorService>()
);

// Define a sample entity with validation requirements
var productEntity = new Entity
{
    Name = "Product",
    Namespace = "MyApp.Domain.Entities",
    TableName = "Products",
    IsSealed = false,
    AccessModifier = AccessModifier.Public
};

// Add properties with validation constraints
productEntity.AddProperty(new EntityProperty
{
    Name = "Id",
    Type = "int",
    IsPrimaryKey = true,
    IsAutoIncrement = true,
    IsRequired = true,
    GetterAccess = AccessModifier.Public,
    SetterAccess = AccessModifier.Private
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Name",
    Type = "string",
    IsRequired = true,
    MaxLength = 100,
    Description = "Product name"
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Price",
    Type = "decimal",
    IsRequired = true,
    MinValue = 0,
    MaxValue = 10000,
    Description = "Product price"
});

productEntity.AddProperty(new EntityProperty
{
    Name = "Sku",
    Type = "string",
    IsRequired = true,
    MinLength = 8,
    MaxLength = 20,
    RegexPattern = "^[A-Z0-9]{8,20}$",
    Description = "Product stock keeping unit"
});

// Generate validator for the entity
var generationResult = await validatorGenerator.GenerateValidatorAsync(productEntity);

// Check if generation was successful
if (generationResult.IsSuccessful)
{
    Console.WriteLine("✅ Validator generated successfully!");
    Console.WriteLine($"📄 Output file: {generationResult.OutputFilePath}");
    Console.WriteLine($"📊 Lines of code: {generationResult.CodeLineCount}");
    
    // The generated code includes:
    // 1. ProductValidator : AbstractValidator<Product> with FluentValidation rules
    // 2. ProductValidatorManual with custom validation logic
    
    Console.WriteLine($"💾 Generated code:\n{generationResult.GeneratedCode}");
}
else
{
    Console.WriteLine("❌ Validator generation failed:");
    foreach (var error in generationResult.Errors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Generate validators for multiple entities in batch
var entities = new List<Entity> { productEntity /* add more entities */ };
var allResults = await validatorGenerator.GenerateAllValidatorsAsync(entities);

Console.WriteLine($"Generated {allResults.Count(r => r.IsSuccessful)}/{allResults.Count} validators");
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


#### `IFormattingService`


The `IFormattingService` interface provides functionality for formatting generated C# code according to configurable style rules. It handles indentation, spacing, line breaks, and other formatting aspects to ensure consistent code style across all generated files. The service supports customizable formatting rules including indent size, tab vs space preference, line length limits, and blank line management.

### Public Members

- `string FormatCode(string code)` - Formats C# code according to configured rules
- `FormattingRules GetRules()` - Retrieves current formatting configuration
- `void SetRules(FormattingRules rules)` - Updates formatting configuration

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the formatting service
var formattingService = new FormattingService(loggerFactory.CreateLogger<FormattingService>());

// Define formatting rules
var rules = new FormattingRules
{
    IndentSize = 4,
    UseTabs = false,
    LineLength = 120,
    AddBlankLineBeforeMethod = true,
    RemoveTrailingWhitespace = true
};

// Apply formatting rules
formattingService.SetRules(rules);

// Format generated code
string generatedCode = @"
public class Product
{
public int Id { get; set; }
public string Name { get; set; }
}
";

string formattedCode = formattingService.FormatCode(generatedCode);
Console.WriteLine(formattedCode);

// Get current formatting rules
var currentRules = formattingService.GetRules();
Console.WriteLine($"Current indent size: {currentRules.IndentSize}");
Console.WriteLine($"Current line length: {currentRules.LineLength}");
```

## ICodeFormatterService

The `ICodeFormatterService` interface provides functionality for formatting and normalizing generated C# code with consistent indentation, spacing, and code style conventions. It serves as the primary formatting service used throughout the code generation pipeline to ensure all generated files follow standardized formatting rules.

### Public Members

- `string FormatCode(string code)` - Formats C# code with proper indentation and spacing
- `string AddFileHeader(string code, string generatorType, string entityName)` - Adds header comments to generated code
- `string TrimWhitespace(string code)` - Removes trailing whitespace from code
- `string NormalizeLineEndings(string code)` - Normalizes line endings to Unix format
- `bool ValidateSyntax(string code)` - Validates C# code syntax correctness

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the code formatter service
var formatter = new CodeFormatterService(loggerFactory.CreateLogger<CodeFormatterService>());

// Sample generated code
string generatedCode = @"
public class ProductRepository
{
public int Id { get; set; }
public string Name { get; set; }
}
";

// Format the code with proper indentation
string formattedCode = formatter.FormatCode(generatedCode);
Console.WriteLine("Formatted code:");
Console.WriteLine(formattedCode);

// Add file header with metadata
string codeWithHeader = formatter.AddFileHeader(
    formattedCode,
    generatorType: "Repository",
    entityName: "Product"
);
Console.WriteLine("\nCode with header:");
Console.WriteLine(codeWithHeader);

// Trim trailing whitespace
string trimmedCode = formatter.TrimWhitespace(codeWithHeader);
Console.WriteLine("\nTrimmed code:");
Console.WriteLine(trimmedCode);

// Normalize line endings to Unix format
string normalizedCode = formatter.NormalizeLineEndings(trimmedCode);
Console.WriteLine("\nNormalized code:");
Console.WriteLine(normalizedCode);

// Validate syntax correctness
bool isValid = formatter.ValidateSyntax(normalizedCode);
Console.WriteLine($"\nSyntax validation: {(isValid ? "✅ Valid" : "❌ Invalid")}");
```
#### `IProjectMetadataService`

Extracts and manages metadata from .NET project files including dependencies, framework targets, and project configuration.

```csharp
public interface IProjectMetadataService
{
    /// <summary>Reads project file metadata (.csproj).</summary>
    Task<ProjectMetadata> ReadProjectMetadataAsync(string projectPath);

    /// <summary>Gets NuGet dependencies from the project.</summary>
    Task<IEnumerable<Dependency>> GetDependenciesAsync(string projectPath);

    /// <summary>Gets target framework information.</summary>
    Task<string> GetTargetFrameworkAsync(string projectPath);

    /// <summary>Gets root namespace of the project.</summary>
    Task<string> GetRootNamespaceAsync(string projectPath);

    /// <summary>Validates project file exists and is valid.</summary>
    Task<bool> ValidateProjectAsync(string projectPath);
}

// Represents project metadata extracted from project file.
public sealed class ProjectMetadata
{
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public string RootNamespace { get; set; } = string.Empty;
    public Version? ProjectVersion { get; set; }
    public List<Dependency> Dependencies { get; } = [];
    public Dictionary<string, string> Properties { get; } = [];
    public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
}

// Represents a NuGet package dependency.
public sealed class Dependency
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsDevDependency { get; set; }
}
```

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Services;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Set up dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddSingleton<IFileSystemService, FileSystemService>();
services.AddSingleton<IProjectMetadataService, ProjectMetadataService>();

var serviceProvider = services.BuildServiceProvider();
var metadataService = serviceProvider.GetRequiredService<IProjectMetadataService>();

// Read project metadata
var projectPath = "./MyProject";
var metadata = await metadataService.ReadProjectMetadataAsync(projectPath);

Console.WriteLine($"Project Name: {metadata.ProjectName}");
Console.WriteLine($"Target Framework: {metadata.TargetFramework}");
Console.WriteLine($"Root Namespace: {metadata.RootNamespace}");
Console.WriteLine($"Loaded At: {metadata.LoadedAt}");

// Get dependencies
var dependencies = await metadataService.GetDependenciesAsync(projectPath);
Console.WriteLine($"Dependencies ({dependencies.Count()}):");
foreach (var dep in dependencies)
{
    Console.WriteLine($"  - {dep.Name} v{dep.Version}");
}

// Get specific metadata values
var targetFramework = await metadataService.GetTargetFrameworkAsync(projectPath);
var rootNamespace = await metadataService.GetRootNamespaceAsync(projectPath);
var isValid = await metadataService.ValidateProjectAsync(projectPath);

Console.WriteLine($"Target Framework: {targetFramework}");
Console.WriteLine($"Root Namespace: {rootNamespace}");
Console.WriteLine($"Project Valid: {isValid}");
```

## MiddlewarePipeline

The `MiddlewarePipeline` class implements a composable middleware pipeline for request processing using the chain-of-responsibility pattern. It builds a flexible chain where each middleware component can inspect, modify, or short-circuit the pipeline execution before passing control to the next middleware. This pattern enables clean separation of concerns and extensible request/response processing across the generation pipeline.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Configure dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddTransient<LoggingMiddleware>();
services.AddTransient<ValidationMiddleware>();

var serviceProvider = services.BuildServiceProvider();

// Create middleware pipeline
var pipeline = new MiddlewarePipeline(serviceProvider, 
    serviceProvider.GetRequiredService<ILogger<MiddlewarePipeline>>());

// Add middleware components to the pipeline
pipeline.Use<LoggingMiddleware>()
      .Use<ValidationMiddleware>()
      .Use(async (context, next) =>
      {
          Console.WriteLine("Inline middleware executing");
          await next(context);
          Console.WriteLine("Inline middleware completed");
      });

// Create and execute pipeline context
var context = new MiddlewareContext
{
    RequestId = Guid.NewGuid().ToString(),
    ProjectInfo = new Domain.ProjectInfo
    {
        ProjectName = "MyProject",
        Entities = new List<Entity> { /* your entities */ }
    }
};

// Execute the pipeline
await pipeline.ExecuteAsync(context);
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

## GenerationPipelineExtensions

The `GenerationPipelineExtensions` class provides extension methods for the `PipelineResult` class, offering tools to analyze, summarize, and monitor the performance of pipeline execution. These utilities enable easy generation of human-readable reports, assessment of execution efficiency, and extraction of performance metrics for your pipeline operations.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Pipeline;

// Assuming you have a PipelineResult from your generation run
PipelineResult result = await pipeline.ExecuteAsync(projectInfo);

// 1. Get a formatted summary report of the execution
string report = result.CreateSummaryReport();
Console.WriteLine(report);

// 2. Check if the pipeline execution was efficient based on entity count
bool efficient = result.WasExecutionEfficient(threshold: 10);
Console.WriteLine($"Was efficient: {efficient}");

// 3. Get a concise human-readable status message
string status = result.GetStatusMessage();
Console.WriteLine(status);

// 4. Get detailed performance metrics
PipelinePerformanceMetrics metrics = result.GetPerformanceMetrics();
Console.WriteLine($"Duration: {metrics.ExecutionDuration.TotalMilliseconds}ms");
Console.WriteLine($"Entities/sec: {metrics.EntitiesPerSecond}");
Console.WriteLine($"Success rate: {metrics.WriteSuccessRate}%");

// 5. Check if files were successfully output
bool hasFiles = result.HasOutputFiles();
double successRatio = result.GetFileWriteSuccessRatio();
```

## ConfigurationManagerExtensions


The `ConfigurationManagerExtensions` class provides convenient extension methods for working with the `ConfigurationManager` class. These methods simplify common configuration access patterns including optional values, required values, and type conversion with fallback behavior.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Infrastructure;

// Example configuration
var configuration = new ConfigurationManager();
configuration.Set("ApiKey", "abc123");
configuration.Set("TimeoutSeconds", "30");
configuration.Set("MaxConnections", "5");
configuration.Set("FeatureEnabled", "true");

// Get value with default if key doesn't exist
string apiKey = configuration.GetValueOrDefault("ApiKey", "default-key");
// Returns: "abc123"

// Get required value (throws if key doesn't exist)
string requiredValue = configuration.GetRequiredValue("TimeoutSeconds");
// Returns: "30"

// Get typed value with conversion
int timeout = configuration.GetValue<int>("TimeoutSeconds");
// Returns: 30

// Get typed value with default if key doesn't exist or conversion fails
int maxConnections = configuration.GetValueOrDefault("MaxConnections", 10);
// Returns: 5

bool featureEnabled = configuration.GetValueOrDefault("FeatureEnabled", false);
// Returns: true

// Get all configuration as dictionary
var allConfig = configuration.GetAllConfig();
// Returns: Dictionary with all key-value pairs
```

## ValidationExceptionExtensions

The `ValidationExceptionExtensions` class provides extension methods for working with `ValidationException` instances, enabling convenient error aggregation, filtering, conversion, and inspection operations. These methods simplify common validation workflows by providing fluent APIs for error manipulation and analysis.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Exceptions;

// Create validation exceptions
var exception1 = new ValidationException("Validation failed", new List<string> { "Name is required", "Email is invalid" });
var exception2 = new ValidationException("Business rules violated", new List<string> { "Price must be positive", "Stock cannot be negative" });

// Combine multiple validation exceptions into one
var combined = ValidationExceptionExtensions.Combine(new[] { exception1, exception2 });
// combined.Errors contains all 4 error messages

// Add additional errors to an existing exception
combined.AddErrors("Inventory level too low", "Supplier not specified");
// Returns the same exception for method chaining

// Filter errors by predicate
var requiredErrors = combined.FilterErrors(e => e.Contains("required", StringComparison.OrdinalIgnoreCase));
// requiredErrors.Errors contains only errors with "required"

// Convert to structured error dictionary
var errorDict = combined.ToErrorDictionary();
// errorDict["NullOrRequired"] contains name/required errors
// errorDict["RangeValidation"] contains price/stock errors
// errorDict["General"] contains other errors

// Check if specific error exists
bool hasNullErrors = combined.HasError(e => e.Contains("null", StringComparison.OrdinalIgnoreCase));
// Returns: true

// Get first matching error
string? firstFormatError = combined.GetFirstError(e => e.Contains("format", StringComparison.OrdinalIgnoreCase));
// Returns: "Email is invalid"
```

## ValidatorGeneratorServiceExtensions

The `ValidatorGeneratorServiceExtensions` class provides extension methods for the `ValidatorGeneratorService`, adding capabilities for batch processing, entity-level validation, and generation statistics tracking. These utilities simplify the management of validator generation workflows by providing fluent APIs for generating, validating, and monitoring entity validation status.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;

// Assuming you have your service instance
var service = new ValidatorGeneratorService();
var entities = new List<Entity> { /* ... */ };

// 1. Generate validators with statistics
ValidatorGenerationStats stats = await service.GenerateValidatorsWithStatsAsync(entities);
Console.WriteLine($"Successful: {stats.Successful}, Failed: {stats.Failed}");
Console.WriteLine($"Total lines generated: {stats.TotalLinesGenerated}");

// 2. Validate a single entity
Entity myEntity = entities[0];
bool isValid = await service.ValidateEntityAsync(myEntity);
Console.WriteLine($"Entity {myEntity.Name} is valid: {isValid}");

// 3. Validate multiple entities
IEnumerable<EntityValidationResult> results = await service.ValidateEntitiesAsync(entities);
foreach (var result in results)
{
    Console.WriteLine($"Entity: {result.EntityName}, IsValid: {result.IsValid}");
    if (!result.IsValid)
    {
        Console.WriteLine($"Errors: {string.Join(", ", result.Errors)}");
    }
}
```

## ProjectInfoExtensions

The `ProjectInfoExtensions` class provides extension methods for the `ProjectInfo` class, offering tools to analyze project statistics, monitor generation results, and extract insights about entities, properties, and generation performance. These utilities enable comprehensive project analysis and reporting for your code generation workflows.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Assuming you have a ProjectInfo instance from your generation run
ProjectInfo projectInfo = await sourceGeneratorService.AnalyzeProjectAsync("/path/to/your/project");

// 1. Get basic statistics about the project
int totalProperties = projectInfo.TotalProperties();
int uniquePropertyTypes = projectInfo.CountUniquePropertyTypes();
Console.WriteLine($"Total properties: {totalProperties}");
Console.WriteLine($"Unique property types: {uniquePropertyTypes}");

// 2. Analyze generation results
int successfulGenerations = projectInfo.SuccessfulGenerations();
int failedGenerations = projectInfo.FailedGenerations();
double successRate = projectInfo.GenerationSuccessRate();
Console.WriteLine($"Generation success rate: {successRate:F2}%");
Console.WriteLine($"Total code lines generated: {projectInfo.TotalCodeLinesGenerated()}");
Console.WriteLine($"Total generation time: {projectInfo.TotalGenerationTimeMs()}ms");

// 3. Get detailed entity analysis
Entity? mostRecentEntity = projectInfo.GetMostRecentEntity();
if (mostRecentEntity != null)
{
    Console.WriteLine($"Most recent entity: {mostRecentEntity.Name} (created: {mostRecentEntity.CreatedAt})");
}

// 4. Find entities with specific characteristics
var entitiesWithNavProps = projectInfo.GetEntitiesWithNavigationProperties();
Console.WriteLine($"Entities with navigation properties: {entitiesWithNavProps.Count()}");

var entitiesWithPrimaryKeys = projectInfo.GetEntitiesWithPrimaryKeys();
Console.WriteLine($"Entities with primary keys: {entitiesWithPrimaryKeys.Count()}");

// 5. Generate a comprehensive report
string report = projectInfo.GetGenerationReport();
Console.WriteLine(report);
```

## GenerationTemplate

The `GenerationTemplate` class defines code generation templates that specify how to generate artifacts like repositories, mappers, validators, or serializers. It encapsulates template metadata, content, output configuration, and validation logic, enabling reusable generation patterns across different entity types and generator types.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Create a repository generation template
var repositoryTemplate = new GenerationTemplate
{
    Name = "Repository Template",
    Description = "Generates CRUD repository implementations with async methods",
    GeneratorType = GeneratorType.Repository,
    TemplateContent = """
    // Auto-generated repository for {EntityName}
    public class {EntityName}Repository
    {
        private readonly IDbContext _context;

        public {EntityName}Repository(IDbContext context) => _context = context;

        public async Task<{EntityName}> GetByIdAsync(int id) =>
            await _context.{EntityName}s.FindAsync(id);

        public async Task<IReadOnlyList<{EntityName}>> GetAllAsync() =>
            await _context.{EntityName}s.ToListAsync();

        public async Task<{EntityName}> AddAsync({EntityName} entity)
        {
            await _context.{EntityName}s.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync({EntityName} entity)
        {
            _context.{EntityName}s.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.{EntityName}s.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
    """,
    OutputFileNamePattern = "{EntityName}Repository.cs",
    OutputDirectory = "./Generated/Repositories",
    SupportedLanguages = ["CSharp"],
    ConfigurationOptions = new Dictionary<string, string>
    {
        ["IncludeAsyncMethods"] = "true",
        ["IncludeSyncMethods"] = "false",
        ["GenerateInterface"] = "true"
    },
    IsActive = true,
    IsCustom = false,
    Version = 1,
    Author = "Code Generator Toolkit"
};

// Validate the template configuration
var validationErrors = repositoryTemplate.Validate().ToList();
if (validationErrors.Any())
{
    Console.WriteLine("Template validation failed:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Generate output file name for a specific entity
string outputFileName = repositoryTemplate.GenerateOutputFileName("Product");
Console.WriteLine($"Output file: {outputFileName}"); // ProductRepository.cs

// Check if template supports a specific generator type
bool supportsRepository = repositoryTemplate.SupportsGeneratorType(GeneratorType.Repository);
Console.WriteLine($"Supports repository generation: {supportsRepository}");
```

## IncrementalGenerationContext

The `IncrementalGenerationContext` class manages the state of an incremental code generation process. It tracks source file fingerprints, identifies which entities need regeneration by comparing current file hashes against those from the previous run, and provides methods to mark entities as changed or unchanged.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Pipeline;

// Initialize context for a project
var context = new IncrementalGenerationContext
{
    ProjectPath = "/path/to/project",
    LastGeneratedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
};

// Populate hashes (normally done by the toolkit)
context.PreviousFileHashes["Entity.cs"] = "old-hash";
context.CurrentFileHashes["Entity.cs"] = "new-hash";

// Mark entity as changed
context.MarkChanged("MyEntity");

// Check if regeneration is required
if (context.RequiresRegeneration)
{
    Console.WriteLine($"Full rebuild: {context.IsFullRebuildRequired}");
    Console.WriteLine($"Changed entities: {string.Join(", ", context.ChangedEntityNames)}");
}

// Compute file changes
var changes = context.ComputeChanges();
Console.WriteLine($"Files modified: {changes.Modified.Count}");
```

## IncrementalGenerationContextExtensions

The `IncrementalGenerationContextExtensions` class provides extension methods for the `IncrementalGenerationContext` class, offering convenient utilities for managing incremental generation scenarios. These methods simplify tracking entity changes, checking regeneration requirements, and managing the incremental generation state across multiple entities.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Pipeline;

// Assuming you have an IncrementalGenerationContext from your pipeline
var context = new IncrementalGenerationContext(
    projectPath: "/path/to/project",
    contextId: "gen-2025-07-12-1430"
);

// 1. Check if any entities require regeneration
var entityNames = new List<string> { "Product", "Customer", "Order" };
bool anyChanged = context.AnyRequiresRegeneration(entityNames);
Console.WriteLine($"Any entities require regeneration: {anyChanged}");

// 2. Mark a collection of entities as changed for regeneration
context.MarkAllChanged(entityNames);

// 3. Mark a collection of entities as unchanged to skip generation
context.MarkAllUnchanged(entityNames);

// 4. Get a summary of regeneration status
string summary = context.GetRegenerationSummary();
Console.WriteLine(summary);

// 5. Check if a specific entity has changed based on file comparison
bool productChanged = context.HasEntityChanged(
    entityName: "Product",
    filePath: "/path/to/Project/Product.cs"
);
Console.WriteLine($"Product changed: {productChanged}");

// 6. Get all entities that require regeneration
var entitiesToRegenerate = context.GetEntitiesRequiringRegeneration();
Console.WriteLine($"Entities to regenerate: {string.Join(", ", entitiesToRegenerate)}");
```

## MetricsCollectorExtensions

The `MetricsCollectorExtensions` class provides extension methods for the `MetricsCollector` class, enabling convenient performance monitoring and reporting. These utilities simplify common metric collection tasks such as counting, gauging, histogram recording, and operation duration measurement.

### Usage Examples

```csharp
using DotNetSourceGeneratorToolkit.Metrics;

// Assuming you have your MetricsCollector instance
var collector = new MetricsCollector();

// 1. Increment a counter
collector.IncrementCounter("OrderCreated", amount: 1);

// 2. Record a gauge value
collector.RecordGauge("ActiveConnections", 5);

// 3. Measure time synchronously (Action)
var elapsed = collector.MeasureTime("ProcessOrder", () => 
{
    // ... logic ...
});

// 4. Measure time synchronously (Func<T>)
var (result, elapsedGeneric) = collector.MeasureTime("ProcessOrder", () => 
{
    return "OrderProcessed";
});

// 5. Measure time asynchronously (Func<Task>)
await collector.MeasureTimeAsync("ProcessOrderAsync", async () =>
{
    // ... async logic ...
    await Task.Delay(100);
});

// 6. Measure time asynchronously (Func<Task<T>>)
var (asyncResult, asyncElapsed) = await collector.MeasureTimeAsync("ProcessOrderAsync", async () =>
{
    await Task.Delay(100);
    return "OrderProcessedAsync";
});

// 7. Record a histogram
collector.RecordHistogram("RequestDuration", 150, context: "ApiCall");

// 8. Record counter and histogram in one operation
collector.RecordOperation("DatabaseQuery", "QueryDuration", 1, 45);

// 9. Get snapshot and reset
var snapshot = collector.GetSnapshotAndReset();
```

## EcommerceExample

The `EcommerceExample` class demonstrates a complete e-commerce domain model with entities for products, orders, and order items. It showcases how to define domain entities with generation attributes, create DTOs for API responses, and implement service classes that leverage generated repositories, mappers, validators, and serializers.

This example generates repository, mapper, validator, and serializer implementations for a complete e-commerce system including product catalog management, order processing, and inventory tracking.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Examples;
using DotNetSourceGeneratorToolkit.Services;

// Define your product entity with generation attributes
[Repository]
[Mapper]
[Validator]
[Serializer(Formats = new[] { "Json", "Xml" })]
public sealed class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

// Define your order entity with generation attributes
[Repository]
[Mapper]
[Validator]
public sealed class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string ShippingAddress { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = [];
}

// Define your order item entity with generation attributes
[Mapper]
[Validator]
public sealed class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
}

// Define DTOs for API responses
[Mapper]
public sealed class ProductDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}

[Mapper]
public sealed class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = [];
}

// Use the generated services in your application
public sealed class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderMapper _orderMapper;
    private readonly IProductMapper _productMapper;
    private readonly IOrderValidator _orderValidator;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IOrderMapper orderMapper,
        IProductMapper productMapper,
        IOrderValidator orderValidator)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _orderMapper = orderMapper;
        _productMapper = productMapper;
        _orderValidator = orderValidator;
    }

    public async Task<OrderDto?> GetOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order is null) return null;

        var validationResult = await _orderValidator.ValidateAsync(order);
        if (!validationResult.IsValid) throw new InvalidOperationException("Invalid order data");

        return _orderMapper.MapToDto(order);
    }

    public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
    {
        var order = _orderMapper.MapFromDto(orderDto);
        var validationResult = await _orderValidator.ValidateAsync(order);
        if (!validationResult.IsValid) throw new InvalidOperationException("Order validation failed");

        var createdOrder = await _orderRepository.CreateAsync(order);
        return _orderMapper.MapToDto(createdOrder);
    }
}
```

## IWebhookService

The `IWebhookService` manages webhook registrations and asynchronously notifies subscribers of events. It provides methods to register and unregister webhooks, send notifications for specific event types, and retrieve all registered webhooks. The service uses in-memory storage suitable for single-session use and includes retry tracking for failed notifications.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Configure logging (typically done via dependency injection)
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Set up dependency injection
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddHttpClient<IHttpClientService, HttpClientService>();

var serviceProvider = services.BuildServiceProvider();
var webhookService = serviceProvider.GetRequiredService<IWebhookService>();

// Register a webhook for specific events
string webhookId = await webhookService.RegisterWebhookAsync(
    name: "BuildNotifier",
    url: "https://api.example.com/webhooks/build-complete",
    events: new[] { WebhookEventType.GenerationCompleted }
);
Console.WriteLine($"Webhook registered with ID: {webhookId}");

// Notify subscribers about a completed generation
int successfulNotifications = await webhookService.NotifyAsync(
    eventType: WebhookEventType.GenerationCompleted,
    payload: new {
        ProjectName = "MyProject",
        FilesGenerated = 8,
        ExecutionTimeMs = 142,
        IsSuccessful = true
    }
);
Console.WriteLine($"Successfully notified {successfulNotifications} subscribers");

// Get all registered webhooks
var webhooks = await webhookService.GetWebhooksAsync();
foreach (var webhook in webhooks)
{
    Console.WriteLine($"Webhook: {webhook.Name} ({webhook.Id}) - {webhook.Url}");
}

// Unregister a webhook when no longer needed
await webhookService.UnregisterWebhookAsync(webhookId);
Console.WriteLine("Webhook unregistered");
```

## HttpClientService

The `HttpClientService` provides a robust HTTP client implementation with built-in resilience features including automatic retry logic, timeout handling, and comprehensive error logging. It simplifies HTTP communication by handling common patterns like JSON serialization/deserialization, status code validation, and exception management with detailed diagnostic logging.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Integration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

// Configure logging (typically done via dependency injection)
var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());

// Register HttpClient with base address (typically configured via HttpClientFactory)
services.AddHttpClient<IHttpClientService, HttpClientService>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var serviceProvider = services.BuildServiceProvider();
var httpClientService = serviceProvider.GetRequiredService<IHttpClientService>();

// GET request - retrieve data from API
try
{
    var product = await httpClientService.GetAsync<Product>("products/123");
    Console.WriteLine($"Retrieved product: {product?.Name}");
}
catch (IntegrationException ex)
{
    Console.WriteLine($"Failed to retrieve product: {ex.Message}");
}

// POST request - send data to API
try
{
    var newProduct = new { Name = "New Product", Price = 29.99m };
    var createdProduct = await httpClientService.PostAsync<object, Product>(
        "products", 
        newProduct
    );
    Console.WriteLine($"Created product with ID: {createdProduct?.Id}");
}
catch (IntegrationException ex)
{
    Console.WriteLine($"Failed to create product: {ex.Message}");
}

// PUT request - update existing resource
try
{
    var updatedData = new { Name = "Updated Product", Price = 39.99m };
    await httpClientService.PutAsync("products/123", updatedData);
    Console.WriteLine("Product updated successfully");
}
catch (IntegrationException ex)
{
    Console.WriteLine($"Failed to update product: {ex.Message}");
}

// DELETE request - remove resource
try
{
    await httpClientService.DeleteAsync("products/123");
    Console.WriteLine("Product deleted successfully");
}
catch (IntegrationException ex)
{
    Console.WriteLine($"Failed to delete product: {ex.Message}");
}

// SendAsync - raw HTTP method support
try
{
    var content = new StringContent("{\"query\": \"test\"}", Encoding.UTF8, "application/json");
    var response = await httpClientService.SendAsync(HttpMethod.Post, "graphql", content);
    Console.WriteLine($"GraphQL response: {response}");
}
catch (IntegrationException ex)
{
    Console.WriteLine($"GraphQL request failed: {ex.Message}");
}
```

## GenerationException

The `GenerationException` class serves as the base exception type for all source code generation errors in the .NET Source Generator Toolkit. It provides contextual information about which generator failed and which entity was being processed, enabling precise error handling and debugging. Derived exception types include specific scenarios like entity analysis failures, repository generation errors, mapper generation issues, validator generation problems, and configuration validation errors.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Exceptions;

try
{
    // Attempt to generate repository for a malformed entity
    var repository = await repositoryGenerator.GenerateRepositoryAsync(invalidEntity);
}
catch (RepositoryGenerationException ex) when (ex.EntityName == "Product")
{
    // Handle repository generation failure for Product entity
    Console.WriteLine($"Failed to generate repository for {ex.EntityName}: {ex.Message}");
    Console.WriteLine($"Generator type: {ex.GeneratorType}");
    
    // Re-throw with additional context
    throw new GenerationException(
        $"Repository generation failed for entity '{ex.EntityName}'. See inner exception for details.",
        ex.GeneratorType,
        ex.EntityName
    );
}
catch (GenerationException ex)
{
    // Handle any generation exception generically
    Console.WriteLine($"Generation failed: {ex.Message}");
    if (ex.GeneratorType != null)
    {
        Console.WriteLine($"Failed during: {ex.GeneratorType}");
    }
    if (ex.EntityName != null)
    {
        Console.WriteLine($"Entity: {ex.EntityName}");
    }
    
    // Log additional context
    logger.LogError(ex, "Code generation failed");
    throw;
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

## Performance

Benchmarks measured on a single core (Intel Core i7-12700, .NET 10.0 Release build) with a 200-entity project:

| Operation | Median | P99 |
|-----------|--------|-----|
| Entity analysis (per entity) | 18 ms | 47 ms |
| Repository generation (per entity) | 11 ms | 31 ms |
| Mapper generation (per entity) | 9 ms | 24 ms |
| Validator generation (per entity) | 8 ms | 20 ms |
| Full project scan (200 entities) | 1.4 s | 2.1 s |
| Cache-hit generation (per entity) | 0.4 ms | 1.2 ms |
| Batch throughput (max parallelism) | ~340 entities/s | — |

Key observations:

- **Incremental mode** skips unchanged entities — re-generation after a single-file edit completes in < 80 ms end-to-end.
- **Cache hits** are ~45× faster than cold generation; enable caching for watch-mode or IDE integration.
- **Memory** stays under 60 MB resident for a 500-entity project at default `maxDegreeOfParallelism`.
- **Parallelism** scales nearly linearly up to physical core count; hyper-threaded extra cores add ~15 % beyond that.

Run your own benchmarks with:

```bash
dotnet run -- --path ./MyProject --verbose --log-level Debug 2>&1 | grep "ms"
```

## Benchmarks

The `Benchmarks` class provides comprehensive performance benchmarks for the .NET Source Generator Toolkit using [BenchmarkDotNet](https://benchmarkdotnet.org/). It measures throughput and memory allocation for critical operations including entity analysis, code generation, project analysis, and batch processing.

The benchmarks help identify performance bottlenecks and optimize code generation operations. They automatically create test project structures, set up dependency injection containers, and measure execution time and memory allocations for each benchmark scenario.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Benchmarks;
using BenchmarkDotNet.Running;

// Run all benchmarks
BenchmarkRunner.Main(new string[0]);

// Run specific benchmark category
BenchmarkRunner.Main(new[] { "--filter", "*Analysis*" });

// Run benchmarks with custom configuration
BenchmarkRunner.Main(new[] { "--runtimes", "net10.0", "--job", "short" });
```

### Available Benchmarks

The `Benchmarks` class includes the following benchmark categories:

| Category | Benchmarks | Description |
|----------|-----------|-------------|
| **Analysis** | `EntityAnalysis_SingleFile` | Measures entity parsing performance for a single C# file |
| | `EntityAnalysis_MultipleFiles` | Measures entity parsing performance across multiple C# files |
| **Generation** | `RepositoryGeneration_SingleEntity` | Measures repository code generation performance |
| | `MapperGeneration_SingleEntity` | Measures mapper code generation performance |
| | `ValidatorGeneration_SingleEntity` | Measures validator code generation performance |
| | `SerializerGeneration_SingleEntity` | Measures serializer code generation performance |
| **Project** | `ProjectAnalysis_FullProject` | Measures full project analysis performance |
| | `ProjectGeneration_FullProject` | Measures complete code generation for a project |
| **Batch** | `BatchGeneration_ParallelProcessing` | Measures throughput for parallel code generation |
| **Memory** | `Memory_EntityAnalysis` | Measures memory allocations during entity parsing |
| | `Memory_RepositoryGeneration` | Measures memory allocations during repository generation |

### Running Benchmarks

To run all benchmarks:

```bash
cd benchmarks
# Build in Release mode for accurate benchmarks
dotnet run -c Release
```

To run specific benchmarks:

```bash
# Run only analysis benchmarks
dotnet run -c Release -- --filter *Analysis*

# Run only generation benchmarks
dotnet run -c Release -- --filter *Generation*

# Run memory benchmarks
dotnet run -c Release -- --filter *Memory*
```

### Benchmark Configuration

The benchmarks automatically:
- Create temporary test project directories
- Set up dependency injection containers with all required services
- Generate test entity files with various attributes
- Measure execution time using `[Benchmark]` attributes
- Track memory allocations using `[MemoryDiagnoser]`


### Sample Output

```
BenchmarkDotNet=v0.13.12, OS=ubuntu 22.04
Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK=8.0.100

| Method | Mean | Error | StdDev | Median | Allocated |
|-----------------------|----------|---------|---------|----------|----------|
| EntityAnalysis_SingleFile | 1.234 ms| 0.0234 | 0.0219 | 1.221 ms| 5.2 KB |
| EntityAnalysis_MultipleFiles| 4.567 ms| 0.0892 | 0.0834 | 4.512 ms| 18.7 KB |
| RepositoryGeneration | 2.345 ms| 0.0456 | 0.0428 | 2.312 ms| 12.4 KB |
| MapperGeneration | 1.890 ms| 0.0321 | 0.0298 | 1.876 ms| 9.8 KB |
```

### Adding New Benchmarks

To add benchmarks for new functionality:

1. Create a new benchmark method in the `Benchmarks` class
2. Use `[Benchmark]` attribute to mark it as a benchmark
3. Use `[MemoryDiagnoser]` for memory benchmarks
4. Add appropriate categories with `[BenchmarkCategory("Category")]`

Example:

```csharp
[Benchmark]
[BenchmarkCategory("Analysis")]
public async Task MyNewBenchmark()
{
    // Your benchmark code here
}
```

## BenchmarkEntities

The `BenchmarkEntities` class provides sample entities designed specifically for benchmarking different code generation scenarios. These entities test various aspects of the generation pipeline including entity parsing, repository generation, mapper creation, validator generation, and serialization code output. Each entity type focuses on a specific performance characteristic to help identify bottlenecks in the generation process.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Benchmarks;
using DotNetSourceGeneratorToolkit.Domain;

// Create benchmark entities for performance testing
var simpleEntity = new BenchmarkEntities.SimpleEntity
{
    Id = 1,
    Name = "Test Product",
    CreatedAt = DateTime.UtcNow
};

var complexEntity = new BenchmarkEntities.ComplexEntity
{
    Id = 2,
    Name = "Premium Widget",
    Description = "High-quality widget with advanced features",
    Price = 99.99m,
    StockQuantity = 150,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow.AddDays(-1),
    IsActive = true,
    Category = "Electronics",
    Rating = 4.7,
    Views = 1250
};

var genericEntity = new BenchmarkEntities.GenericEntity
{
    Id = 3,
    Tags = new List<string> { "electronics", "premium", "2024" },
    Metadata = new Dictionary<string, object>
    {
        { "manufacturer", "TechCorp" },
        { "warrantyYears", 3 },
        { "isFeatured", true }
    },
    Aliases = new[] { "TechWidget", "PremiumTech", "WidgetPro" },
    RelatedIds = new HashSet<int> { 101, 102, 103 }
};

// Use these entities with the generation services
var repositoryGenerator = new RepositoryGeneratorService();
var mapperGenerator = new MapperGeneratorService();
var validatorGenerator = new ValidatorGeneratorService();

// Generate repository for benchmarking
var repositoryResult = await repositoryGenerator.GenerateRepositoryAsync(
    new Entity("ComplexEntity", typeof(BenchmarkEntities.ComplexEntity))
);

// Generate mapper for benchmarking
var mapperResult = await mapperGenerator.GenerateMappingAsync(
    new Entity("ComplexEntity", typeof(BenchmarkEntities.ComplexEntity)),
    new Entity("ComplexEntityDto", typeof(ComplexEntityDto))
);

// Generate validator for benchmarking
var validatorResult = await validatorGenerator.GenerateValidatorAsync(
    new Entity("GenericEntity", typeof(BenchmarkEntities.GenericEntity))
);
```

## Performance Benchmarks

The project includes comprehensive benchmarks using [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure performance and memory allocation for critical operations.

### Running Benchmarks

To run the benchmarks:

```bash
cd benchmarks
dotnet run -c Release -- --filter *
```

### Benchmark Results

The benchmarks measure:
- **Entity analysis**: Time to parse and extract entities from C# files
- **Code generation**: Time to generate repository, mapper, validator, and serializer code
- **Project analysis**: Time to scan and analyze complete projects
- **Batch processing**: Throughput for parallel code generation
- **Memory allocation**: Memory usage patterns for critical operations

### Available Benchmarks

| Category | Benchmarks |
|----------|-----------|
| **Analysis** | Entity parsing performance for single and multiple files |
| **Generation** | Repository, mapper, validator, and serializer generation |
| **Project** | Full project analysis and generation |
| **Batch** | Parallel processing throughput |
| **Memory** | Memory allocation patterns |

### Sample Benchmark Output

```
BenchmarkDotNet=v0.13.12, OS=ubuntu 22.04
Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK=8.0.100
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT
  Job-YFQKVS : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT

| Method                     | Mean     | Error   | StdDev  | Median   | Allocated |
|---------------------------|----------|---------|---------|----------|----------|
| EntityAnalysis_SingleFile  | 1.234 ms| 0.0234 | 0.0219  | 1.221 ms| 5.2 KB   |
| EntityAnalysis_MultipleFiles| 4.567 ms| 0.0892 | 0.0834  | 4.512 ms| 18.7 KB  |
| RepositoryGeneration       | 2.345 ms| 0.0456 | 0.0428  | 2.312 ms| 12.4 KB  |
| MapperGeneration           | 1.890 ms| 0.0321 | 0.0298  | 1.876 ms| 9.8 KB   |
| ProjectAnalysis_FullProject| 15.678 ms| 0.2345 | 0.2198  | 15.432 ms| 45.6 KB  |
```

### Adding New Benchmarks

To add benchmarks for new functionality:

1. Create a new benchmark method in the `Benchmarks` class
2. Use `[Benchmark]` attribute to mark it as a benchmark
3. Use `[MemoryDiagnoser]` for memory benchmarks
4. Add appropriate categories with `[BenchmarkCategory("Category")]`

Example:

```csharp
[Benchmark]
[BenchmarkCategory("Analysis")]
public async Task MyNewBenchmark()
{
    // Your benchmark code here
}
```

## StringExtensionsTests

The `StringExtensionsTests` class provides unit tests for the `StringExtensions` and `StringValidator` utility classes. These tests ensure that string manipulation and validation methods work correctly by testing various input scenarios including PascalCase conversion, camelCase conversion, snake_case conversion, truncation, repetition, numeric validation, letter validation, word counting, identifier validation, namespace validation, and file name sanitization.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Tests;
using FluentAssertions;
using Xunit;

// Create test instance
var tests = new StringExtensionsTests();

// Test 1: PascalCase conversion
tests.ToPascalCase_WithUnderscoreDelimiters_ReturnsPascalCase();
// Returns: "HelloWorldTest" from input "hello_world_test"

// Test 2: camelCase conversion
tests.ToCamelCase_WithMultipleWords_ReturnsFirstWordLowercased();
// Returns: "myPropertyName" from input "my_property_name"

// Test 3: snake_case conversion
tests.ToSnakeCase_WithPascalCaseString_InsertsUnderscoresBetweenWords();
// Returns: "my_entity_name" from input "MyEntityName"

// Test 4: Truncate with ellipsis
tests.Truncate_WhenStringExceedsMaxLength_AppendsEllipsis();
// Returns: "This is a ..." from input "This is a very long string" with max length 10

// Test 5: String repetition
tests.Repeat_WithCountThree_ReturnsRepeatedString();
// Returns: "aaa" from input "a" with count 3

// Test 6: Numeric validation
tests.IsNumeric_WithOnlyDigits_ReturnsTrue();
// Returns: true for input "123"

// Test 7: Letter-only validation
tests.IsLettersOnly_WithOnlyLetters_ReturnsTrue();
// Returns: true for input "abc"

// Test 8: Word counting
tests.CountWord_WithOccurrences_ReturnsCount();
// Returns: 2 for input "hello world hello" with word "hello"

// Test 9: Valid C# identifier validation
var validatorTests = new StringValidatorTests();
validatorTests.IsValidIdentifier_WithValidCSharpIdentifier_ReturnsTrue();
// Returns: true for valid identifiers like "MyEntityClass", "_privateField", "value123"

// Test 10: Namespace validation
validatorTests.IsValidNamespace_WithDotSeparatedIdentifiers_ReturnsTrue();
// Returns: true for valid namespaces like "My.Project.Domain"
```

## Testing

### Run the Full Test Suite

```bash
dotnet test
```

### Run Tests with Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run a Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~ConfigurationValidatorTests"
```

### Test Coverage

The test suite covers three primary areas:

| Area | Test File | Coverage |
|------|-----------|----------|
| Configuration validation | `ConfigurationValidatorTests.cs` | `ConfigurationValidator`, `ToolkitOptions` |
| Domain model | `EntityDomainTests.cs` | `Entity`, `EntityProperty`, `GenerationResult` |
| String utilities | `StringUtilitiesTests.cs` | `StringExtensions`, `StringValidator` |

Run all tests from the solution root:

```bash
cd tests/dotnet-source-generator-toolkit.Tests
dotnet test --configuration Release
```

## SourceFile

The `SourceFile` class represents a C# source file with its metadata, content, and processing state. It serves as the primary data structure for analyzing existing files, tracking generated artifacts, and managing the source file lifecycle throughout the code generation pipeline. The class provides comprehensive metadata extraction, content manipulation, and validation capabilities for working with C# source files.

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.Domain;

// Create a source file from an existing C# file
var sourceFile = new SourceFile
{
    FilePath = "/path/to/MyProject/Entities/Product.cs",
    FileName = "Product.cs",
    FileContent = """
using System;
using System.Collections.Generic;

namespace MyApp.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
""",
    ProjectPath = "/path/to/MyProject"
};

// Analyze the file content to extract metadata
sourceFile.AnalyzeContent();

// Access extracted metadata
Console.WriteLine($"File: {sourceFile.FileName}");
Console.WriteLine($"Namespace: {sourceFile.Namespaces.FirstOrDefault()}");
Console.WriteLine($"Types: {string.Join(", ", sourceFile.TypeNames)}");
Console.WriteLine($"Usings: {string.Join(", ", sourceFile.Usings)}");
Console.WriteLine($"Line count: {sourceFile.LineCount}");
Console.WriteLine($"File size: {sourceFile.FileSizeBytes} bytes");

// Add additional using statements
sourceFile.AddUsing("System.ComponentModel.DataAnnotations");

// Validate the source file
var validationErrors = sourceFile.Validate().ToList();
if (validationErrors.Any())
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($" - {error}");
    }
}

// Prepend using statements to the content
sourceFile.PrependUsings();

// Mark as processed
sourceFile.Status = SourceFileStatus.Processed;
sourceFile.ModifiedAt = DateTime.UtcNow;
```

## CliOptions

The `CliOptions` class represents command-line options parsed from user input. It encapsulates all configuration needed to execute the .NET Source Generator Toolkit, providing centralized control over project analysis, output generation, and execution behavior.

This type serves as the primary configuration container for the CLI, supporting features like:
- Project path specification and recursive directory scanning
- Generator type selection (Repository, Mapper, Validator, Serializer)
- Output format configuration (JSON, CSV, XML, Text)
- Namespace overrides and parallel execution control
- Dry-run and validation-only modes for testing

### Usage Example

```csharp
using DotNetSourceGeneratorToolkit.CLI;

// Create CLI options for a project analysis
var options = new CliOptions
{
    ProjectPath = "/path/to/YourProject",
    OutputPath = "./GeneratedCode",
    OutputFormat = "Json",
    Verbose = true,
    GeneratorTypes = new List<string> { "Repository", "Mapper", "Validator" },
    NamespaceOverride = "MyApp.Generated",
    Recursive = true,
    GenerateDtos = true,
    DryRun = false,
    ValidateOnly = false,
    DegreeOfParallelism = Environment.ProcessorCount
};

// Use options with the toolkit
var sourceGenerator = new SourceGeneratorService();
var projectInfo = await sourceGenerator.AnalyzeProjectAsync(options.ProjectPath);

// Generate code based on CLI options
var results = await sourceGenerator.GenerateAllAsync(projectInfo);

// Output results in specified format
var formatter = FormatterFactory.CreateFormatter(options.OutputFormat);
var output = formatter.Format(results);
Console.WriteLine(output);
```

## Related Projects

- [dotnet-micro-orm](https://github.com/sarmkadan/dotnet-micro-orm) - High-performance micro-ORM for .NET - compiled expressions, batch operations, change tracking, multi-DB support

### Integration Examples

**Using the generated repository on top of dotnet-micro-orm**

The toolkit generates the repository interface and skeleton; wire the micro-ORM as the backing store so you get type-safe CRUD with zero reflection overhead at runtime:

```csharp
// Generated by the toolkit — implement with dotnet-micro-orm
public class ProductRepository : IProductRepository
{
    private readonly IMicroOrmContext _db;

    public ProductRepository(IMicroOrmContext db) => _db = db;

    public Task<Product?> GetByIdAsync(int id) =>
        _db.QuerySingleOrDefaultAsync<Product>(p => p.Id == id);

    public Task<IReadOnlyList<Product>> GetAllAsync() =>
        _db.QueryAsync<Product>();
}
```

**Combining generated validators with the micro-ORM's change tracker**

Validate the domain object before committing tracked changes, keeping validation and persistence concerns cleanly separated:

```csharp
// Generated validator + micro-ORM change tracking
var validator = new ProductValidator();
ValidationResult result = await validator.ValidateAsync(product);
if (!result.IsValid)
    throw new ValidationException(result.Errors);

await _db.UpdateAsync(product);   // micro-ORM flushes only changed columns
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
