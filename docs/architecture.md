// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Architecture Documentation

## System Overview

The .NET Source Generator Toolkit is built on a layered, modular architecture designed for extensibility, maintainability, and performance.

```
┌─────────────────────────────────────────────────────┐
│           CLI & User Interface                       │
│  (CliArgumentParser, CliOptions, Program.cs)        │
└──────────────────────┬────────────────────────────────┘
                       │
┌──────────────────────▼────────────────────────────────┐
│         Middleware Pipeline Layer                    │
│  ┌────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Logging  │→ │ Error Handler│→ │  Validation  │  │
│  └────────────┘  └──────────────┘  └──────────────┘  │
└──────────────────────┬────────────────────────────────┘
                       │
┌──────────────────────▼────────────────────────────────┐
│     Core Generation Services Layer                   │
│  ┌──────────────────────────────────────────────────┐│
│  │  - SourceGeneratorService (Orchestrator)        ││
│  │  - RepositoryGeneratorService                   ││
│  │  - MapperGeneratorService                       ││
│  │  - ValidatorGeneratorService                    ││
│  │  - SerializerGeneratorService                   ││
│  └──────────────────────────────────────────────────┘│
└──────────────────────┬────────────────────────────────┘
                       │
┌──────────────────────▼────────────────────────────────┐
│      Infrastructure & Analysis Layer                 │
│  ┌──────────────────────────────────────────────────┐│
│  │  - AttributeAnalyzer (Roslyn-based)             ││
│  │  - EntityAnalyzer (Property introspection)      ││
│  │  - ConfigurationManager (File-based config)     ││
│  │  - FileSystemService (I/O operations)           ││
│  └──────────────────────────────────────────────────┘│
└──────────────────────┬────────────────────────────────┘
                       │
┌──────────────────────▼────────────────────────────────┐
│        Cross-Cutting Concerns                         │
│  ┌──────────────────────────────────────────────────┐│
│  │  - Event System (EventAggregator, pub-sub)       ││
│  │  - Caching (MemoryCache with TTL)               ││
│  │  - Metrics (Performance monitoring)             ││
│  │  - Batch Processing (Parallel execution)        ││
│  │  - HTTP Integration (Webhook delivery)          ││
│  └──────────────────────────────────────────────────┘│
└──────────────────────┬────────────────────────────────┘
                       │
┌──────────────────────▼────────────────────────────────┐
│        Output & Formatting Layer                     │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐        │
│  │  JSON  │ │  CSV   │ │  XML   │ │  Text  │        │
│  └────────┘ └────────┘ └────────┘ └────────┘        │
└─────────────────────────────────────────────────────┘
```

## Design Patterns

### 1. Middleware Pipeline Pattern

**Location**: `Middleware/`

The toolkit implements a chain-of-responsibility middleware pipeline:

```csharp
public interface IMiddleware
{
    Task ExecuteAsync(
        GenerationContext context,
        Func<GenerationContext, Task> next);
}
```

**Implementations**:
- `LoggingMiddleware`: Logs generation events
- `ValidationMiddleware`: Validates configuration and input
- `ErrorHandlingMiddleware`: Handles exceptions with retry logic

**Execution Flow**:
```
Request
  ↓
LoggingMiddleware
  ↓
ErrorHandlingMiddleware
  ↓
ValidationMiddleware
  ↓
Generation Services
  ↓
Response
```

**Benefits**:
- Clean separation of concerns
- Easy to add/remove middleware
- Testable in isolation
- Extensible for custom middleware

### 2. Factory Pattern

**Location**: `Formatters/FormatterFactory.cs`

Encapsulates object creation for output formatters:

```csharp
public interface IFormatterFactory
{
    IOutputFormatter CreateFormatter(string format);
}

public class FormatterFactory : IFormatterFactory
{
    public IOutputFormatter CreateFormatter(string format) => format.ToLower() switch
    {
        "json" => new JsonOutputFormatter(),
        "csv" => new CsvOutputFormatter(),
        "xml" => new XmlOutputFormatter(),
        "text" => new TextOutputFormatter(),
        _ => throw new InvalidOperationException($"Unknown format: {format}")
    };
}
```

**Implementations**:
- `JsonOutputFormatter`: Structured JSON with metadata
- `CsvOutputFormatter`: Spreadsheet-compatible CSV
- `XmlOutputFormatter`: Document-oriented XML
- `TextOutputFormatter`: Human-readable text

### 3. Repository Pattern

**Location**: `Repositories/`

Abstracts data access and persistence:

```csharp
public interface IEntityRepository
{
    Task<Entity?> GetByIdAsync(string id);
    Task<List<Entity>> GetAllAsync();
    Task AddAsync(Entity entity);
    Task UpdateAsync(Entity entity);
    Task DeleteAsync(string id);
}

public interface IGenerationResultRepository
{
    Task SaveResultAsync(GenerationResult result);
    Task<GenerationResult?> GetResultAsync(string resultId);
}
```

**Benefits**:
- Decouples business logic from data access
- Easy to mock for testing
- Supports multiple storage backends
- Consistent interface for persistence

### 4. Observer Pattern (Pub-Sub)

**Location**: `Events/`

Decouples components through event-driven architecture:

```csharp
public interface IEventPublisher
{
    void Publish<T>(T @event) where T : IEvent;
}

public interface IEventSubscriber
{
    void Subscribe<T>(Action<T> handler) where T : IEvent;
}

public class EventAggregator : IEventPublisher, IEventSubscriber
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = [];
    
    public void Publish<T>(T @event) where T : IEvent
    {
        if (_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            foreach (var handler in handlers)
                ((Action<T>)handler)(@event);
        }
    }
    
    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        var key = typeof(T);
        if (!_subscribers.ContainsKey(key))
            _subscribers[key] = [];
        _subscribers[key].Add(handler);
    }
}
```

**Events**:
- `GenerationStartedEvent`: Fired when generation begins
- `GenerationCompletedEvent`: Fired when generation completes
- `ValidationFailedEvent`: Fired on validation errors

### 5. Strategy Pattern

**Location**: `Services/` and `Formatters/`

Different implementations for different output strategies:

```csharp
public interface IOutputFormatter
{
    string Format(GenerationResult result);
    Task<string> FormatAsync(GenerationResult result);
}
```

Multiple concrete implementations for different formats.

## Layer Responsibilities

### CLI Layer
- **Responsibility**: Parse command-line arguments and options
- **Files**: `CLI/CliArgumentParser.cs`, `CLI/CliOptions.cs`
- **Interfaces**: `ICliArgumentParser`

### Middleware Layer
- **Responsibility**: Pre/post processing of requests
- **Files**: `Middleware/MiddlewarePipeline.cs`, `Middleware/*Middleware.cs`
- **Interfaces**: `IMiddleware`, `IMiddlewarePipeline`
- **Execution Order**: LoggingMiddleware → ErrorHandlingMiddleware → ValidationMiddleware

### Service Layer
- **Responsibility**: Core business logic and code generation
- **Files**: `Services/*.cs`
- **Key Services**:
  - `ISourceGeneratorService`: Orchestrates entire generation
  - `IRepositoryGeneratorService`: Repository code generation
  - `IMapperGeneratorService`: Mapping code generation
  - `IValidatorGeneratorService`: Validation code generation
  - `ISerializerGeneratorService`: Serialization code generation

### Infrastructure Layer
- **Responsibility**: Low-level operations and Roslyn analysis
- **Files**: `Infrastructure/*.cs`
- **Key Components**:
  - `AttributeAnalyzer`: Finds generation attributes using Roslyn
  - `EntityAnalyzer`: Extracts entity metadata
  - `ConfigurationManager`: Loads/validates configuration
  - `FileSystemService`: File I/O operations

### Data Layer
- **Responsibility**: Entity and result persistence
- **Files**: `Repositories/*.cs`, `Domain/*.cs`
- **Interfaces**: `IEntityRepository`, `IGenerationResultRepository`

### Cross-Cutting Concerns
- **Events**: `Events/EventAggregator.cs`
- **Caching**: `Caching/MemoryCache.cs`
- **Metrics**: `Metrics/MetricsCollector.cs`
- **Batch Processing**: `Batch/BatchProcessor.cs`
- **Integration**: `Integration/WebhookService.cs`, `Integration/HttpClientService.cs`

## Data Flow

### Generation Pipeline

```
1. CLI Input Parsing
   └─→ args → CliArgumentParser.Parse()

2. Configuration Loading
   └─→ toolkit-config.json → ConfigurationManager.LoadAsync()

3. Middleware Pipeline Initialization
   └─→ LoggingMiddleware → ErrorHandlingMiddleware → ValidationMiddleware

4. Project Analysis
   └─→ AttributeAnalyzer.FindAttributesAsync()
   └─→ EntityAnalyzer.AnalyzeEntitiesAsync()

5. Service Execution (in SourceGeneratorService)
   ├─→ RepositoryGeneratorService.GenerateAllAsync()
   ├─→ MapperGeneratorService.GenerateAllMappersAsync()
   ├─→ ValidatorGeneratorService.GenerateAllValidatorsAsync()
   └─→ SerializerGeneratorService.GenerateSerializersAsync()

6. Result Formatting
   └─→ FormatterFactory.CreateFormatter()
   └─→ IOutputFormatter.FormatAsync()

7. Output & Persistence
   ├─→ FileSystemService.WriteAsync()
   ├─→ GenerationResultRepository.SaveResultAsync()
   ├─→ EventAggregator.Publish(GenerationCompletedEvent)
   └─→ WebhookService.SendAsync() (if configured)
```

### Cache Interaction

```
Request
  ↓
ConfigurationManager checks cache
  ├─ Hit: Return cached result
  └─ Miss: Generate and store
  ↓
MemoryCache stores with TTL
```

### Event Flow

```
Generation Starts
  ↓
EventAggregator.Publish(GenerationStartedEvent)
  ├─→ MetricsCollector records start time
  └─→ LoggingMiddleware logs event

Generation Processing
  └─→ Entity analysis and code generation

Generation Completes
  ↓
EventAggregator.Publish(GenerationCompletedEvent)
  ├─→ MetricsCollector records completion
  ├─→ FileSystemService persists results
  ├─→ WebhookService sends webhook (if configured)
  └─→ LoggingMiddleware logs event
```

## Dependency Injection

The toolkit uses .NET's built-in DI container:

```csharp
static IServiceCollection ConfigureServices()
{
    var services = new ServiceCollection();
    
    // Infrastructure
    services.AddLogging();
    services.AddSingleton<IConfigurationManager, ConfigurationManager>();
    services.AddSingleton<IFileSystemService, FileSystemService>();
    services.AddSingleton<ICache, MemoryCache>();
    services.AddSingleton<IEventAggregator, EventAggregator>();
    services.AddSingleton<IMetricsCollector, MetricsCollector>();
    
    // Core Services
    services.AddScoped<ISourceGeneratorService, SourceGeneratorService>();
    services.AddScoped<IRepositoryGeneratorService, RepositoryGeneratorService>();
    services.AddScoped<IMapperGeneratorService, MapperGeneratorService>();
    services.AddScoped<IValidatorGeneratorService, ValidatorGeneratorService>();
    services.AddScoped<ISerializerGeneratorService, SerializerGeneratorService>();
    
    // Analyzers
    services.AddScoped<IAttributeAnalyzer, AttributeAnalyzer>();
    services.AddScoped<IEntityAnalyzer, EntityAnalyzer>();
    
    // Repositories
    services.AddScoped<IEntityRepository, EntityRepository>();
    services.AddScoped<IGenerationResultRepository, GenerationResultRepository>();
    
    return services;
}
```

**Lifetimes**:
- **Singleton**: Expensive to create, thread-safe (ConfigurationManager, FileSystemService)
- **Scoped**: New instance per generation request (Services, Analyzers)
- **Transient**: Lightweight, stateless (would use if any)

## Extensibility Points

### 1. Custom Middleware

```csharp
public class CustomAnalyticsMiddleware : IMiddleware
{
    public async Task ExecuteAsync(
        GenerationContext context,
        Func<GenerationContext, Task> next)
    {
        var sw = Stopwatch.StartNew();
        await next(context);
        sw.Stop();
        
        // Send analytics
        await _analyticsService.RecordAsync(new
        {
            EntityName = context.EntityName,
            Duration = sw.ElapsedMilliseconds
        });
    }
}
```

Register in DI:
```csharp
services.AddScoped<IMiddleware, CustomAnalyticsMiddleware>();
```

### 2. Custom Output Formatter

```csharp
public class MarkdownOutputFormatter : IOutputFormatter
{
    public string Format(GenerationResult result)
    {
        // Format as Markdown
    }
    
    public Task<string> FormatAsync(GenerationResult result)
    {
        return Task.FromResult(Format(result));
    }
}
```

Register in factory:
```csharp
public IOutputFormatter CreateFormatter(string format) => format.ToLower() switch
{
    "markdown" => new MarkdownOutputFormatter(),
    // ... other formats
};
```

### 3. Custom Cache Implementation

```csharp
public class RedisCache : ICache
{
    public async Task<T?> GetAsync<T>(string key)
    {
        // Retrieve from Redis
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        // Store in Redis
    }
    
    public async Task RemoveAsync(string key)
    {
        // Remove from Redis
    }
}
```

### 4. Custom Event Handlers

```csharp
public class CustomEventHandler
{
    private readonly IEventAggregator _eventAggregator;
    
    public void Subscribe()
    {
        _eventAggregator.Subscribe<GenerationCompletedEvent>(ev =>
        {
            // Custom handling
            SendNotification(ev.EntityName);
        });
    }
}
```

## Performance Considerations

### Caching Strategy
- Configuration files cached for 60 minutes (configurable)
- Results cached with TTL
- Cache clearing available on demand

### Parallelization
- Batch processing leverages `Parallel.ForEach`
- Configurable degree of parallelism (defaults to CPU count)
- Thread-safe operations throughout

### Memory Management
- Streaming for large file I/O
- EventAggregator uses weak references
- Proper disposal patterns for resources

## Testing Strategy

### Unit Tests
- Test each service independently
- Mock dependencies via DI
- Test edge cases and error conditions

### Integration Tests
- Test middleware pipeline
- Test full generation flow
- Test output formatters

### Example Test Structure
```csharp
[TestClass]
public class RepositoryGeneratorServiceTests
{
    private IRepositoryGeneratorService _service;
    private Mock<IFileSystemService> _fileSystemMock;
    
    [TestInitialize]
    public void Setup()
    {
        _fileSystemMock = new Mock<IFileSystemService>();
        _service = new RepositoryGeneratorService(_fileSystemMock.Object);
    }
    
    [TestMethod]
    public async Task GenerateRepository_WithValidEntity_CreatesFile()
    {
        // Arrange
        var entity = new Entity { Name = "User", Namespace = "MyApp" };
        
        // Act
        var result = await _service.GenerateRepositoryAsync(entity);
        
        // Assert
        Assert.IsNotNull(result);
        _fileSystemMock.Verify(x => x.WriteAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }
}
```

## Security Considerations

- Input validation at system boundaries
- Path traversal prevention in FileSystemService
- Webhook URL validation
- Safe reflection usage with Roslyn
- No arbitrary code execution
- Configuration file permissions (recommend 0600)
