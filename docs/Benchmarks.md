# Benchmarks

The `Benchmarks` class is designed to facilitate performance evaluation and correctness validation for various source generation tasks within the `dotnet-source-generator-toolkit`. It provides a standardized framework to execute, measure, and analyze the efficiency of entity analysis, repository and component generation, and full-project processing workflows.

## API

### Lifecycle Methods
- `void Setup()`: Initializes the necessary environment, configurations, and dependencies required to execute benchmarking tasks. This method should be called prior to running any benchmark procedures.
- `void Cleanup()`: Releases resources, clears temporary data, and performs necessary teardown operations after benchmark tasks are concluded.

### Nested Data Entity
This `sealed class` defines the structure of the entity used during benchmarking operations.
- `int Id`: A unique identifier for the entity.
- `string Name`: The display name of the entity.
- `string Description`: A detailed description of the entity.
- `decimal Price`: The price associated with the entity.
- `int Quantity`: The stock or item quantity associated with the entity.
- `DateTime CreatedAt`: The timestamp indicating when the entity was created.
- `DateTime? UpdatedAt`: The nullable timestamp indicating when the entity was last modified.
- `bool IsActive`: A flag indicating the active status of the entity.

### Benchmarking Methods
These methods execute performance tests for specific generator components. All methods return an `async Task`.
- `async Task EntityAnalysis_SingleFile()`: Performs a benchmark on the analysis process of a single source file entity.
- `async Task EntityAnalysis_MultipleFiles()`: Performs a benchmark on the analysis process of multiple source file entities.
- `async Task RepositoryGeneration_SingleEntity()`: Executes a benchmark for generating a repository for a single entity.
- `async Task MapperGeneration_SingleEntity()`: Executes a benchmark for generating a mapper for a single entity.
- `async Task ValidatorGeneration_SingleEntity()`: Executes a benchmark for generating a validator for a single entity.
- `async Task SerializerGeneration_SingleEntity()`: Executes a benchmark for generating a serializer for a single entity.
- `async Task ProjectAnalysis_FullProject()`: Executes a benchmark for the comprehensive analysis of a complete project structure.
- `async Task ProjectGeneration_FullProject()`: Executes a benchmark for the comprehensive generation of all components within a complete project.
- `async Task BatchGeneration_ParallelProcessing()`: Executes a benchmark evaluating the performance impact of parallel processing on batch generation tasks.

## Usage

```csharp
// Example 1: Basic Benchmarking Workflow
var benchmark = new Benchmarks();
benchmark.Setup();
try
{
    await benchmark.RepositoryGeneration_SingleEntity();
}
finally
{
    benchmark.Cleanup();
}
```

```csharp
// Example 2: Running a Batch Processing Benchmark
var benchmark = new Benchmarks();
benchmark.Setup();

// Executing parallel processing benchmark
await benchmark.BatchGeneration_ParallelProcessing();

benchmark.Cleanup();
```

## Notes

### Thread Safety
The `Benchmarks` class is not inherently thread-safe. `Setup` and `Cleanup` methods must be invoked sequentially on a single instance. Concurrent execution of benchmark methods (`EntityAnalysis_...`, `Generation_...`) on the same `Benchmarks` instance is not supported and may lead to unpredictable results or resource contention.

### Edge Cases
- **Nullable Types**: The `UpdatedAt` property is nullable (`DateTime?`). Ensure consuming logic correctly handles `null` values to avoid `InvalidOperationException` when accessing the `Value` property.
- **Resource Management**: Failure to call `Cleanup` after `Setup` may lead to resource leaks, particularly when benchmarking large-scale project analysis or batch generation tasks that consume significant memory. Always utilize `try-finally` blocks to ensure `Cleanup` is executed.
