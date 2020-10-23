// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# API Reference

## Core Services

### ISourceGeneratorService

Main orchestration service for code generation.

```csharp
public interface ISourceGeneratorService
{
    /// Analyzes a project directory and discovers entities with generation attributes
    Task<ProjectInfo> AnalyzeProjectAsync(string projectPath);
    
    /// Generates code for all discovered entities
    Task<List<GenerationResult>> GenerateAllAsync(ProjectInfo projectInfo);
    
    /// Generates code for a specific entity
    Task<GenerationResult> GenerateForEntityAsync(Entity entity);
}
```

#### Example Usage

```csharp
var service = provider.GetRequiredService<ISourceGeneratorService>();

// Analyze project
var projectInfo = await service.AnalyzeProjectAsync(@"C:\MyProject");
Console.WriteLine($"Found {projectInfo.Entities.Count} entities");

// Generate all
var results = await service.GenerateAllAsync(projectInfo);

// Generate specific
var result = await service.GenerateForEntityAsync(projectInfo.Entities[0]);
```

### IRepositoryGeneratorService

Generates repository pattern implementations.

```csharp
public interface IRepositoryGeneratorService
{
    /// Generates repository for a single entity
    Task<RepositoryGeneration> GenerateRepositoryAsync(Entity entity);
    
    /// Generates repositories for multiple entities in parallel
    Task<List<RepositoryGeneration>> GenerateAllAsync(List<Entity> entities);
    
    /// Generates paginated repository
    Task<RepositoryGeneration> GeneratePagedRepositoryAsync(Entity entity, int pageSize = 20);
}
```

#### Generated Repository Methods

```csharp
// CRUD Operations
Task<T?> GetByIdAsync(TKey id);
Task<T> CreateAsync(T entity);
Task UpdateAsync(T entity);
Task DeleteAsync(TKey id);

// Query Operations
Task<List<T>> GetAllAsync();
Task<bool> ExistsAsync(TKey id);
Task<List<T>> GetPagedAsync(int pageNumber, int pageSize);

// Filtered Operations
Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
Task<List<T>> WhereAsync(Expression<Func<T, bool>> predicate);
Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
```

### IMapperGeneratorService

Generates bidirectional mapping implementations.

```csharp
public interface IMapperGeneratorService
{
    /// Generates mapper between entity and DTO
    Task<MapperGeneration> GenerateMappingAsync(Entity source, Entity destination);
    
    /// Generates all mappers for entities
    Task<List<MapperGeneration>> GenerateAllMappersAsync(List<Entity> entities);
    
    /// Generates profile-specific mappers
    Task<MapperGeneration> GenerateProfileMappingAsync(
        Entity source, 
        Entity destination, 
        string profile);
}
```

#### Generated Mapper Methods

```csharp
// Single Entity Mapping
TDestination MapFromSource(TSource source);
TSource MapFromDestination(TDestination destination);

// Async Mapping
Task<TDestination> MapFromSourceAsync(TSource source);
Task<TSource> MapFromDestinationAsync(TDestination destination);

// Collection Mapping
IEnumerable<TDestination> MapCollectionFromSource(IEnumerable<TSource> sources);
IEnumerable<TSource> MapCollectionFromDestination(IEnumerable<TDestination> destinations);

// Async Collection Mapping
Task<List<TDestination>> MapCollectionFromSourceAsync(IEnumerable<TSource> sources);
Task<List<TSource>> MapCollectionFromDestinationAsync(IEnumerable<TDestination> destinations);

// Custom Property Mapping
void ConfigureMapping(Action<MappingConfig<TSource, TDestination>> config);
```

### IValidatorGeneratorService

Generates validation rule implementations.

```csharp
public interface IValidatorGeneratorService
{
    /// Generates validator for a single entity
    Task<ValidatorGeneration> GenerateValidatorAsync(Entity entity);
    
    /// Generates validators for multiple entities
    Task<List<ValidatorGeneration>> GenerateAllValidatorsAsync(List<Entity> entities);
    
    /// Generates async validator with custom rules
    Task<ValidatorGeneration> GenerateAsyncValidatorAsync(
        Entity entity, 
        Dictionary<string, string> customRules);
}
```

#### Generated Validator Methods

```csharp
// Synchronous Validation
ValidationResult Validate(T entity);
ValidationResult ValidateProperty(string propertyName, object value);

// Asynchronous Validation
Task<ValidationResult> ValidateAsync(T entity);
Task<ValidationResult> ValidatePropertyAsync(string propertyName, object value);

// Fluent API for Rule Configuration
IValidatorBuilder<T> RuleFor(Expression<Func<T, object>> property);
IValidatorBuilder<T> RuleFor(Expression<Func<T, object>> property)
    .NotEmpty(string? message = null)
    .Length(int min, int max)
    .Pattern(string regex)
    .GreaterThan(object? value)
    .LessThan(object? value)
    .Email()
    .Custom(Func<T, bool> condition, string? message);
```

### ISerializerGeneratorService

Generates serialization code for multiple formats.

```csharp
public interface ISerializerGeneratorService
{
    /// Generates serializers for specified formats
    Task<SerializerGeneration> GenerateSerializersAsync(
        Entity entity, 
        params string[] formats);
    
    /// Generates all serializers with default formats
    Task<List<SerializerGeneration>> GenerateDefaultSerializersAsync(List<Entity> entities);
    
    /// Generates custom serializer with options
    Task<SerializerGeneration> GenerateCustomSerializerAsync(
        Entity entity, 
        SerializerOptions options);
}
```

#### Generated Serializer Methods

```csharp
// JSON Serialization
string SerializeToJson(T entity);
T DeserializeFromJson(string json);
Task<string> SerializeToJsonAsync(T entity);
Task<T> DeserializeFromJsonAsync(string json);

// XML Serialization
string SerializeToXml(T entity);
T DeserializeFromXml(string xml);
Task<string> SerializeToXmlAsync(T entity);
Task<T> DeserializeFromXmlAsync(string xml);

// CSV Serialization
string SerializeToCsv(T entity);
T DeserializeFromCsv(string csv);
Task<string> SerializeToCsvAsync(T entity);
Task<T> DeserializeFromCsvAsync(string csv);

// Binary Serialization
byte[] SerializeToBinary(T entity);
T DeserializeFromBinary(byte[] binary);
Task<byte[]> SerializeToBinaryAsync(T entity);
Task<T> DeserializeFromBinaryAsync(byte[] binary);
```

## Infrastructure Services

### IConfigurationManager

Loads and manages application configuration.

```csharp
public interface IConfigurationManager
{
    /// Loads configuration from file
    Task<ToolkitOptions> LoadAsync(string filePath = "toolkit-config.json");
    
    /// Saves configuration to file
    Task SaveAsync(ToolkitOptions options, string filePath = "toolkit-config.json");
    
    /// Validates configuration
    ValidationResult Validate(ToolkitOptions options);
    
    /// Gets current configuration
    ToolkitOptions GetCurrent();
}
```

### IFileSystemService

Handles file I/O operations.

```csharp
public interface IFileSystemService
{
    /// Reads file contents
    Task<string> ReadAsync(string filePath);
    
    /// Writes file contents
    Task WriteAsync(string filePath, string content);
    
    /// Checks if file exists
    Task<bool> ExistsAsync(string filePath);
    
    /// Creates directory
    Task CreateDirectoryAsync(string directoryPath);
    
    /// Deletes file
    Task DeleteAsync(string filePath);
    
    /// Lists files with pattern
    Task<List<string>> ListFilesAsync(string directoryPath, string pattern = "*");
    
    /// Backs up existing file
    Task BackupAsync(string filePath, string backupSuffix = ".bak");
}
```

### IAttributeAnalyzer

Uses Roslyn to find and analyze attributes.

```csharp
public interface IAttributeAnalyzer
{
    /// Finds generation attributes in project
    Task<List<AttributeInfo>> FindAttributesAsync(string projectPath);
    
    /// Checks if attribute is present on type
    Task<bool> HasAttributeAsync(string filePath, string typeName, string attributeName);
    
    /// Gets attribute parameters
    Task<Dictionary<string, object>> GetAttributeParametersAsync(
        string filePath, 
        string typeName, 
        string attributeName);
}
```

### IEntityAnalyzer

Analyzes entity structure and metadata.

```csharp
public interface IEntityAnalyzer
{
    /// Analyzes entity and extracts properties
    Task<Entity> AnalyzeEntityAsync(string filePath, string typeName);
    
    /// Gets all properties of entity
    Task<List<EntityProperty>> GetPropertiesAsync(Entity entity);
    
    /// Gets entity base class information
    Task<string?> GetBaseClassAsync(Entity entity);
    
    /// Checks if entity is abstract
    Task<bool> IsAbstractAsync(Entity entity);
}
```

## Data Models

### Entity

Represents a C# entity class.

```csharp
public class Entity
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public List<EntityProperty> Properties { get; set; } = [];
    public List<string> Attributes { get; set; } = [];
    public string? BaseClass { get; set; }
    public bool IsAbstract { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### EntityProperty

Represents a property on an entity.

```csharp
public class EntityProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> Attributes { get; set; } = [];
    public bool IsNavigationProperty { get; set; }
}
```

### GenerationResult

Represents the result of code generation.

```csharp
public class GenerationResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EntityName { get; set; } = string.Empty;
    public string GenerationType { get; set; } = string.Empty; // Repository, Mapper, etc.
    public string FilePath { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan GenerationTime { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}
```

### ProjectInfo

Contains discovered project metadata.

```csharp
public class ProjectInfo
{
    public string ProjectPath { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = string.Empty;
    public List<Entity> Entities { get; set; } = [];
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisTime { get; set; }
}
```

## Event System

### GenerationStartedEvent

Published when generation begins.

```csharp
public class GenerationStartedEvent : IEvent
{
    public string EntityName { get; set; } = string.Empty;
    public string ProjectPath { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
}
```

### GenerationCompletedEvent

Published when generation completes.

```csharp
public class GenerationCompletedEvent : IEvent
{
    public string EntityName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime CompletionTime { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
    public int FileSize { get; set; }
    public bool Success { get; set; } = true;
}
```

## Output Formatting

### IOutputFormatter

Interface for output formatters.

```csharp
public interface IOutputFormatter
{
    /// Formats single generation result
    string Format(GenerationResult result);
    
    /// Formats multiple generation results
    string Format(List<GenerationResult> results);
    
    /// Async formatting
    Task<string> FormatAsync(List<GenerationResult> results);
}
```

### FormatterFactory

Creates output formatters.

```csharp
public interface IFormatterFactory
{
    /// Creates formatter for specified format
    IOutputFormatter CreateFormatter(string format);
}

// Usage
var factory = provider.GetRequiredService<IFormatterFactory>();
var jsonFormatter = factory.CreateFormatter("json");
var formatted = jsonFormatter.Format(results);
```

## Caching

### ICache

Generic caching interface.

```csharp
public interface ICache
{
    /// Gets value from cache
    Task<T?> GetAsync<T>(string key);
    
    /// Sets value in cache with optional expiration
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// Removes value from cache
    Task RemoveAsync(string key);
    
    /// Clears all cache entries
    Task ClearAsync();
}
```

## Metrics

### IMetricsCollector

Collects performance metrics.

```csharp
public interface IMetricsCollector
{
    /// Records metric
    void Record(string metricName, double value);
    
    /// Records timing
    void RecordTiming(string operationName, TimeSpan duration);
    
    /// Gets all collected metrics
    MetricsSnapshot GetMetrics();
    
    /// Resets metrics
    void Reset();
}
```

## Configuration Options

### ToolkitOptions

```csharp
public class ToolkitOptions
{
    // Caching
    public bool EnableCaching { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 60;
    
    // Code Formatting
    public bool EnableCodeFormatting { get; set; } = true;
    public int CodeFormattingLineLength { get; set; } = 100;
    
    // Logging
    public bool VerboseLogging { get; set; } = false;
    
    // Performance
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    public int OperationTimeoutSeconds { get; set; } = 300;
    
    // Generation Features
    public bool GenerateDtos { get; set; } = true;
    public string DefaultNamespace { get; set; } = "GeneratedCode";
    
    // Output
    public string OutputDirectory { get; set; } = "./Generated";
    public bool BackupExistingFiles { get; set; } = true;
    
    // Code Generation Options
    public bool GenerateInterfaces { get; set; } = true;
    public bool GenerateXmlComments { get; set; } = true;
    
    // Integration
    public bool WebhookEnabled { get; set; } = false;
    public string? WebhookUrl { get; set; }
    public int WebhookRetries { get; set; } = 3;
    public int WebhookTimeoutSeconds { get; set; } = 30;
}
```

## Exception Types

### GenerationException

Thrown during generation failures.

```csharp
public class GenerationException : Exception
{
    public string? EntityName { get; set; }
    public string? GenerationType { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

// Usage
try
{
    await service.GenerateForEntityAsync(entity);
}
catch (GenerationException ex)
{
    Console.WriteLine($"Generation failed for {ex.EntityName}: {ex.Message}");
}
```

### ValidationException

Thrown during configuration validation.

```csharp
public class ValidationException : Exception
{
    public List<string> Errors { get; set; } = [];
}

// Usage
try
{
    var options = await configManager.LoadAsync();
}
catch (ValidationException ex)
{
    foreach (var error in ex.Errors)
        Console.WriteLine($"Validation error: {error}");
}
```
