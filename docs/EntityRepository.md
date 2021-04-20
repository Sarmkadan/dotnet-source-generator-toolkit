# EntityRepository

The `EntityRepository` class serves as a dual-purpose data access layer within the `dotnet-source-generator-toolkit`, managing persistence and retrieval operations for both source code entities and their corresponding generation results. It provides an asynchronous API to perform standard CRUD (Create, Read, Update, Delete) operations on `Entity` objects, which represent the input structures for code generation, and `GenerationResult` objects, which track the output and status of the generation process. This repository abstracts the underlying storage mechanism, allowing consumers to query items by ID, name, namespace, or generation status while maintaining consistency between the source entities and their generated artifacts.

## API

### Entity Operations

*   **`public EntityRepository`**
    Initializes a new instance of the `EntityRepository` class. This constructor typically establishes the necessary context or connection to the underlying data store required for subsequent operations.

*   **`public async Task<Entity?> GetByIdAsync`**
    Retrieves a single `Entity` by its unique identifier.
    *   **Parameters:** Accepts the unique ID of the entity to retrieve.
    *   **Return Value:** Returns the found `Entity` object, or `null` if no entity matches the provided ID.
    *   **Exceptions:** May throw if the data store is inaccessible or the ID format is invalid.

*   **`public async Task<IEnumerable<Entity>> GetAllAsync`**
    Retrieves all `Entity` records currently stored in the repository.
    *   **Return Value:** An enumerable collection of all `Entity` objects. Returns an empty collection if no entities exist.
    *   **Exceptions:** May throw if the data store connection fails during enumeration.

*   **`public async Task<IEnumerable<Entity>> GetByNameAsync`**
    Retrieves a collection of `Entity` objects matching a specific name.
    *   **Parameters:** Accepts the name string to search for.
    *   **Return Value:** An enumerable collection of `Entity` objects sharing the specified name.
    *   **Exceptions:** May throw if the search operation encounters a storage error.

*   **`public async Task<IEnumerable<Entity>> GetByNamespaceAsync`**
    Retrieves all `Entity` objects belonging to a specific namespace.
    *   **Parameters:** Accepts the namespace string to filter by.
    *   **Return Value:** An enumerable collection of `Entity` objects within the specified namespace.
    *   **Exceptions:** May throw if the namespace filter causes a query error.

*   **`public async Task<Entity> AddAsync`**
    Persists a new `Entity` to the repository.
    *   **Parameters:** Accepts the `Entity` instance to be added.
    *   **Return Value:** Returns the persisted `Entity`, potentially including system-generated properties like IDs or timestamps.
    *   **Exceptions:** Throws if an entity with a conflicting unique key already exists or if validation fails.

*   **`public async Task<Entity> UpdateAsync`**
    Updates an existing `Entity` in the repository.
    *   **Parameters:** Accepts the modified `Entity` instance.
    *   **Return Value:** Returns the updated `Entity` object.
    *   **Exceptions:** Throws if the entity does not exist or if concurrency checks fail.

*   **`public async Task<bool> DeleteAsync`**
    Removes an `Entity` from the repository.
    *   **Parameters:** Accepts the identifier or the `Entity` instance to delete.
    *   **Return Value:** Returns `true` if the deletion was successful, or `false` if the entity was not found.
    *   **Exceptions:** May throw if foreign key constraints prevent deletion.

*   **`public async Task<int> CountAsync`**
    Returns the total number of `Entity` records in the repository.
    *   **Return Value:** An integer representing the total count.

### GenerationResult Operations

*   **`public async Task<GenerationResult?> GetByIdAsync`**
    Retrieves a single `GenerationResult` by its unique identifier.
    *   **Parameters:** Accepts the unique ID of the result to retrieve.
    *   **Return Value:** Returns the found `GenerationResult` object, or `null` if not found.

*   **`public async Task<IEnumerable<GenerationResult>> GetAllAsync`**
    Retrieves all `GenerationResult` records.
    *   **Return Value:** An enumerable collection of all `GenerationResult` objects.

*   **`public async Task<IEnumerable<GenerationResult>> GetByEntityAsync`**
    Retrieves all generation results associated with a specific source `Entity`.
    *   **Parameters:** Accepts the `Entity` or its ID to filter by.
    *   **Return Value:** An enumerable collection of `GenerationResult` objects linked to the specified entity.

*   **`public async Task<IEnumerable<GenerationResult>> GetByStatusAsync`**
    Retrieves generation results filtered by their current processing status.
    *   **Parameters:** Accepts the `GenerationStatus` enum value to filter by.
    *   **Return Value:** An enumerable collection of `GenerationResult` objects matching the status.

*   **`public async Task<GenerationResult> AddAsync`**
    Persists a new `GenerationResult` to the repository.
    *   **Parameters:** Accepts the `GenerationResult` instance to be added.
    *   **Return Value:** Returns the persisted `GenerationResult`.
    *   **Exceptions:** Throws if the result violates unique constraints or data integrity rules.

*   **`public async Task<GenerationResult> UpdateAsync`**
    Updates an existing `GenerationResult`.
    *   **Parameters:** Accepts the modified `GenerationResult` instance.
    *   **Return Value:** Returns the updated `GenerationResult`.
    *   **Exceptions:** Throws if the record does not exist.

*   **`public async Task<bool> DeleteAsync`**
    Removes a `GenerationResult` from the repository.
    *   **Parameters:** Accepts the identifier or instance to delete.
    *   **Return Value:** Returns `true` if successful, `false` if the record was not found.

*   **`public async Task<int> CountAsync`**
    Returns the total number of `GenerationResult` records.
    *   **Return Value:** An integer representing the total count.

*   **`public async Task<int> CountByStatusAsync`**
    Returns the number of `GenerationResult` records matching a specific status.
    *   **Parameters:** Accepts the `GenerationStatus` to count.
    *   **Return Value:** An integer representing the count of results with the specified status.

## Usage

### Example 1: Managing Source Entities
This example demonstrates adding a new entity, retrieving it by namespace, and updating its metadata.

```csharp
var repository = new EntityRepository();

// Create and persist a new entity
var newEntity = new Entity 
{ 
    Name = "UserService", 
    Namespace = "MyApp.Core.Services",
    Content = "public class UserService { ... }"
};

var persistedEntity = await repository.AddAsync(newEntity);

// Retrieve all entities in the same namespace
var namespaceEntities = await repository.GetByNamespaceAsync("MyApp.Core.Services");

// Update the entity content
persistedEntity.Content = "public class UserService { /* updated */ }";
var updatedEntity = await repository.UpdateAsync(persistedEntity);

// Verify count
int totalEntities = await repository.CountAsync();
```

### Example 2: Tracking Generation Results
This example illustrates querying generation results by status and linking them back to their source entities.

```csharp
var repository = new EntityRepository();

// Fetch all failed generation results
var failedResults = await repository.GetByStatusAsync(GenerationStatus.Failed);

foreach (var result in failedResults)
{
    // Retrieve the source entity associated with the failed result
    var relatedResults = await repository.GetByEntityAsync(result.EntityId);
    
    // Log or retry logic could go here
    Console.WriteLine($"Generation failed for Entity ID: {result.EntityId}");
}

// Get total count of pending generations
int pendingCount = await repository.CountByStatusAsync(GenerationStatus.Pending);

// Clean up old successful results
if (pendingCount == 0)
{
    foreach (var result in await repository.GetByStatusAsync(GenerationStatus.Success))
    {
        await repository.DeleteAsync(result.Id);
    }
}
```

## Notes

*   **Null Handling:** Methods returning a single item (`GetByIdAsync`) return `null` if the item is not found, rather than throwing an exception. Collection-returning methods return an empty `IEnumerable` instead of `null` when no matches are found.
*   **Concurrency:** As an asynchronous repository interacting with a shared data store, concurrent calls to `UpdateAsync` or `DeleteAsync` on the same record may result in race conditions. Implementations should rely on underlying database concurrency tokens or optimistic locking mechanisms where applicable; callers should be prepared to handle potential update conflicts.
*   **Transaction Scope:** Individual methods like `AddAsync` or `UpdateAsync` represent single atomic operations. Operations requiring multiple steps (e.g., adding an Entity and immediately adding a corresponding GenerationResult) should be wrapped in an external transaction scope if data consistency between the two sets is critical during failure scenarios.
*   **Cascading Deletes:** The behavior of deleting an `Entity` regarding its associated `GenerationResult` records is not implicitly defined by the interface signatures. Consumers should verify if `DeleteAsync` on an Entity cascades to results or if results must be deleted explicitly to avoid orphaned records.
