# GenerationResultRepositoryExtensions

This static class provides extension methods for querying a generation result repository. The methods encapsulate common lookup patterns—by entity, generator, status, or arbitrary criteria—returning the first matching result or a collection of results asynchronously.

## API

### GetFirstByEntityAsync
```csharp
public static async Task<GenerationResult?> GetFirstByEntityAsync(
    this IGenerationResultRepository repository,
    Entity entity,
    CancellationToken cancellationToken = default)
```
**Purpose** – Returns the first generation result associated with the supplied entity.  
**Parameters**  
- `repository`: The repository to query. Must not be null.  
- `entity`: The entity used to filter results. Must not be null.  
- `cancellationToken`: Optional token to observe while awaiting the operation.  
**Return Value** – A task that yields the first matching `GenerationResult` or `null` if no result matches the entity.  
**Exceptions**  
- `ArgumentNullException` if `repository` or `entity` is null.  
- `OperationCanceledException` if the cancellation token is triggered.  
- Any exception thrown by the underlying repository implementation.

### GetFirstByGeneratorAsync
```csharp
public static async Task<GenerationResult?> GetFirstByGeneratorAsync(
    this IGenerationResultRepository repository,
    Generator generator,
    CancellationToken cancellationToken = default)
```
**Purpose** – Returns the first generation result produced by the specified generator.  
**Parameters**  
- `repository`: The repository to query. Must not be null.  
- `generator`: The generator used to filter results. Must not be null.  
- `cancellationToken`: Optional token to observe while awaiting the operation.  
**Return Value** – A task that yields the first matching `GenerationResult` or `null` if none exist.  
**Exceptions**  
- `ArgumentNullException` if `repository` or `generator` is null.  
- `OperationCanceledException` if the cancellation token is triggered.  
- Repository‑specific exceptions.

### GetFirstByStatusAsync
```csharp
public static async Task<GenerationResult?> GetFirstByStatusAsync(
    this IGenerationResultRepository repository,
    GenerationStatus status,
    CancellationToken cancellationToken = default)
```
**Purpose** – Returns the first generation result with the given status.  
**Parameters**  
- `repository`: The repository to query. Must not be null.  
- `status`: The status used to filter results.  
- `cancellationToken`: Optional token to observe while awaiting the operation.  
**Return Value** – A task that yields the first matching `GenerationResult` or `null` if no result has the specified status.  
**Exceptions**  
- `ArgumentNullException` if `repository` is null.  
- `OperationCanceledException` if the cancellation token is triggered.  
- Repository‑specific exceptions.

### GetByCriteriaAsync
```csharp
public static async Task<IEnumerable<GenerationResult>> GetByCriteriaAsync(
    this IGenerationResultRepository repository,
    GenerationCriteria criteria,
    CancellationToken cancellationToken = default)
```
**Purpose** – Returns all generation results that satisfy the supplied criteria.  
**Parameters**  
- `repository`: The repository to query. Must not be null.  
- `criteria`: The criteria object defining the filter. Must not be null.  
- `cancellationToken`: Optional token to observe while awaiting the operation.  
**Return Value** – A task that yields an enumerable of `GenerationResult` instances matching the criteria; an empty enumerable if none match.  
**Exceptions**  
- `ArgumentNullException` if `repository` or `criteria` is null.  
- `OperationCanceledException` if the cancellation token is triggered.  
- Repository‑specific exceptions.

## Usage

```csharp
// Example 1: Find the first result for a specific entity.
GenerationResult? result = await _repository
    .GetFirstByEntityAsync(myEntity, cancellationToken: ct);

if (result != null)
{
    Console.WriteLine($"Found result ID: {result.Id}");
}
else
{
    Console.WriteLine("No result found for the entity.");
}
```

```csharp
// Example 2: Retrieve all results that match a complex criteria.
var criteria = new GenerationCriteria
{
    Generator = myGenerator,
    Status    = GenerationStatus.Completed,
    CreatedAfter = DateTime.UtcNow.AddDays(-7)
};

IEnumerable<GenerationResult> results = await _repository
    .GetByCriteriaAsync(criteria, cancellationToken: ct);

foreach (var r in results)
{
    Console.WriteLine($"Result {r.Id} generated at {r.CreatedAt}");
}
```

## Notes
- The extension methods themselves are stateless and thread‑safe; safety during concurrent invocation depends entirely on the underlying `IGenerationResultRepository` implementation.  
- `GetFirstBy*` methods return only the first matching element; the ordering of “first” is undefined unless the repository guarantees a specific sort order.  
- Passing `null` for required arguments will result in an `ArgumentNullException` before any asynchronous work begins.  
- If the repository throws during query execution, the exception is propagated directly from the returned task.  
- Cancellation is cooperative; if the supplied token is already canceled, the method throws `OperationCanceledException` without accessing the repository.  
- The returned `IEnumerable<GenerationResult>` from `GetByCriteriaAsync` is evaluated asynchronously; enumerating after the task completes does not trigger additional repository calls unless the repository’s implementation streams results.
