# BenchmarkEntities

BenchmarkEntities provides a structured data model designed for performance testing, benchmarking, and stress-testing scenarios within the `dotnet-source-generator-toolkit`. The type encapsulates a diverse range of C# data types, including primitive values, nullable types, and complex collections, to serve as a robust workload for evaluating source generator efficiency, serialization throughput, and memory allocation patterns.

## API

*   **`int Id`**: The unique identifier for the entity instance.
*   **`string Name`**: The display name associated with the entity.
*   **`DateTime CreatedAt`**: The timestamp indicating when the entity was initialized.
*   **`string Description`**: A detailed textual description of the entity.
*   **`decimal Price`**: The monetary value assigned to the entity.
*   **`int StockQuantity`**: The current number of units available in stock.
*   **`DateTime? UpdatedAt`**: An optional timestamp reflecting the last modification time. Returns `null` if the entity has not been updated.
*   **`bool IsActive`**: Indicates whether the entity is currently enabled or active.
*   **`string Category`**: The classification category for the entity.
*   **`double Rating`**: The numerical rating score of the entity.
*   **`int Views`**: The total count of views or interactions recorded.
*   **`List<string> Tags`**: A list of associated string tags for categorization or metadata indexing.
*   **`Dictionary<string, object> Metadata`**: A dictionary allowing for flexible, arbitrary key-value metadata storage.
*   **`string[] Aliases`**: An array of alternative names or identifiers.
*   **`HashSet<int> RelatedIds`**: A set of unique identifiers referencing related entities.

## Usage

### Entity Initialization

```csharp
var entity = new BenchmarkEntities
{
    Id = 1,
    Name = "Sample Product",
    CreatedAt = DateTime.UtcNow,
    Description = "A product used for benchmarking generator performance.",
    Price = 99.99m,
    StockQuantity = 500,
    IsActive = true,
    Category = "TestAssets",
    Rating = 4.5,
    Views = 1200,
    Tags = new List<string> { "bench", "test", "generator" },
    Metadata = new Dictionary<string, object> { { "Version", 1.0 }, { "Source", "Toolkit" } },
    Aliases = new[] { "SP-01", "Sample" },
    RelatedIds = new HashSet<int> { 101, 102, 103 }
};
```

### Benchmarking Serialization

```csharp
public void BenchmarkSerialization(BenchmarkEntities entity)
{
    // Example usage scenario within a benchmark loop
    var json = JsonSerializer.Serialize(entity);
    var deserialized = JsonSerializer.Deserialize<BenchmarkEntities>(json);
    
    if (deserialized.Id != entity.Id)
    {
        throw new InvalidOperationException("Serialization fidelity loss detected.");
    }
}
```

## Notes

*   **Thread Safety**: `BenchmarkEntities` is a standard POCO and is not thread-safe. Concurrent access to the collections (`Tags`, `Metadata`, `Aliases`, `RelatedIds`) or modification of properties requires external synchronization if accessed across multiple threads.
*   **Edge Cases**: The `UpdatedAt` property is nullable and should be checked for `null` before access. Collections, including `Tags`, `Metadata`, `Aliases`, and `RelatedIds`, may be initialized as empty rather than null, depending on the instantiation context; callers should ensure proper null checks are performed if these are not guaranteed to be initialized by the generator.
*   **Performance**: Due to the mix of heap-allocated collections (e.g., `Dictionary`, `List`, `HashSet`), usage in high-frequency loops may impact Garbage Collection (GC) pressure.
