# ProjectInfoExtensions

Provides aggregated metrics and query helpers for source‑generation runs within the `dotnet-source-generator-toolkit`. The type exposes static properties that track generation statistics and static methods that retrieve filtered sets of `Entity` objects representing the processed model.

## API

### TotalProperties
- **Purpose**: Gets the total number of properties discovered across all processed entities.
- **Parameters**: None.
- **Return value**: An `Int32` representing the cumulative property count.
- **Throws**: Does not throw under normal operation.

### SuccessfulGenerations
- **Purpose**: Gets the count of entity generations that completed without error.
- **Parameters**: None.
- **Return value**: An `Int32` indicating successful generation attempts.
- **Throws**: Does not throw under normal operation.

### FailedGenerations
- **Purpose**: Gets the count of entity generations that encountered an error.
- **Parameters**: None.
- **Return value**: An `Int32` indicating failed generation attempts.
- **Throws**: Does not throw under normal operation.

### TotalCodeLinesGenerated
- **Purpose**: Gets the total number of lines of source code produced by the generator.
- **Parameters**: None.
- **Return value**: An `Int32` representing generated lines of code.
- **Throws**: Does not throw under normal operation.

### TotalGenerationTimeMs
- **Purpose**: Gets the cumulative time spent in generation, measured in milliseconds.
- **Parameters**: None.
- **Return value**: An `Int64` representing total elapsed time.
- **Throws**: Does not throw under normal operation.

### GenerationSuccessRate
- **Purpose**: Gets the ratio of successful generations to total generation attempts as a value between 0 and 1.
- **Parameters**: None.
- **Return value**: A `Double` where `0.0` indicates no successes and `1.0` indicates all attempts succeeded; returns `0.0` when no attempts have been recorded.
- **Throws**: Does not throw under normal operation.

### GetGenerationReport
- **Purpose**: Produces a formatted multi‑line string summarizing the current generation metrics.
- **Parameters**: None.
- **Return value**: A `String` containing a human‑readable report (e.g., totals, success rate, timing).
- **Throws**: Does not throw under normal operation.

### GetMostRecentEntity
- **Purpose**: Retrieves the last entity that was processed during the generation run.
- **Parameters**: None.
- **Return value**: An `Entity?` representing the most recent entity, or `null` if no entities have been processed.
- **Throws**: Does not throw under normal operation.

### GetEntitiesWithNavigationProperties
- **Purpose**: Returns all entities that declare at least one navigation property.
- **Parameters**: None.
- **Return value**: An `IEnumerable<Entity>` containing entities with navigation properties; the enumeration reflects the state at the time of the call.
- **Throws**: Does not throw under normal operation; however, enumerating the returned sequence while the underlying collection is being modified may result in undefined behavior.

### GetEntitiesWithPrimaryKeys
- **Purpose**: Returns all entities that have at least one primary key defined.
- **Parameters**: None.
- **Return value**: An `IEnumerable<Entity>` containing entities with primary keys; the enumeration reflects the state at the time of the call.
- **Throws**: Does not throw under normal operation; however, enumerating the returned sequence while the underlying collection is being modified may result in undefined behavior.

### CountUniquePropertyTypes
- **Purpose**: Gets the number of distinct CLR types used for properties across all entities.
- **Parameters**: None.
- **Return value**: An `Int32` count of unique property types.
- **Throws**: Does not throw under normal operation.

## Usage

```csharp
using DotnetSourceGeneratorToolkit;

// After generation has run, inspect the metrics.
int totalProps = ProjectInfoExtensions.TotalProperties;
double successRate = ProjectInfoExtensions.GenerationSuccessRate;
string report = ProjectInfoExtensions.GetGenerationReport();

Console.WriteLine($"Properties: {totalProps}");
Console.WriteLine($"Success rate: {successRate:P2}");
Console.WriteLine(report);
```

```csharp
using DotnetSourceGeneratorToolkit;
using System.Linq;

// Find entities that need special handling because they have navigation properties.
var navEntities = ProjectInfoExtensions.GetEntitiesWithNavigationProperties()
                                       .Where(e => e.Name.StartsWith("Order"))
                                       .ToList();

foreach (var entity in navEntities)
{
    Console.WriteLine($"Processing navigation entity: {entity.Name}");
}
```

## Notes

- All metric properties are updated incrementally during generation; reading them while generation is in progress yields a snapshot that may change on subsequent reads.
- `GenerationSuccessRate` is calculated as `SuccessfulGenerations / (SuccessfulGenerations + FailedGenerations)`. When both counters are zero the property returns `0.0` to avoid division by zero.
- The methods returning `IEnumerable<Entity>` do not create copies of the underlying data; they expose the current view of the internal collection. If the collection is mutated while enumerating, the enumerator may throw or skip elements. For a stable snapshot, materialize the result (e.g., `.ToList()`) before further processing.
- The type is thread‑safe for concurrent reads; internal updates use atomic operations, so reading metrics from multiple threads will not cause race conditions. However, callers should not rely on the exact ordering of entities returned by the query methods if concurrent modifications are possible.
