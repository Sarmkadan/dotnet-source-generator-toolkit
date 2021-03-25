# IncrementalGenerationContextExtensions

Provides extension methods for interacting with the incremental generator context to track which entities require regeneration during source generation.

## API

### `AnyRequiresRegeneration(this IncrementalGeneratorContext context)`
- **Purpose**: Determines whether any tracked entity has been marked as requiring regeneration.
- **Parameters**: 
  - `context` – The incremental generator context instance.
- **Return value**: `true` if at least one entity is flagged for regeneration; otherwise `false`.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `context` is `null`.

### `MarkAllChanged(this IncrementalGeneratorContext context)`
- **Purpose**: Marks all known entities as changed, forcing their regeneration in the current generation pass.
- **Parameters**: 
  - `context` – The incremental generator context instance.
- **Return value**: None.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `context` is `null`.

### `MarkAllUnchanged(this IncrementalGeneratorContext context)`
- **Purpose**: Resets the changed state of all known entities, indicating that none require regeneration unless subsequently marked.
- **Parameters**: 
  - `context` – The incremental generator context instance.
- **Return value**: None.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `context` is `null`.

### `GetRegenerationSummary(this IncrementalGeneratorContext context)`
- **Purpose**: Retrieves a human‑readable summary of the regeneration state (e.g., counts of changed/unchanged entities).
- **Parameters**: 
  - `context` – The incremental generator context instance.
- **Return value**: A string describing the current regeneration state; returns an empty string if no entities are tracked.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `context` is `null`.

### `HasEntityChanged(this IncrementalGeneratorContext context, string entityName)`
- **Purpose**: Checks whether a specific entity has been marked as changed.
- **Parameters**: 
  - `context` – The incremental generator context instance.
  - `entityName` – The name of the entity to inspect.
- **Return value**: `true` if the entity is flagged as changed; otherwise `false`.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `context` is `null`.
  - Throws `ArgumentException` if `entityName` is `null` or whitespace.

### `GetEntitiesRequiringRegeneration(this IncrementalGeneratorContext context)`
- **Purpose**: Returns the set of entity names that have been marked as requiring regeneration.
- **Parameters**: 
  - `context` – The incremental generator context instance.
- **Return value**: A `HashSet<string>` containing the names of entities flagged for regeneration; returns an empty set if none are flagged.
- **Exceptions**: 
  - Throws `ArgumentNullException` if `context` is `null`.

## Usage

### Example 1: Detecting changes and triggering regeneration
```csharp
using Microsoft.CodeAnalysis;
using DotNet.SourceGeneratorToolkit;

[Generator]
public class SampleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Assume some logic marks entities as changed elsewhere.
        if (context.AnyRequiresRegeneration())
        {
            // Regenerate sources for all changed entities.
            var changed = context.GetEntitiesRequiringRegeneration();
            foreach (var entity in changed)
            {
                // Generate source for entity...
            }
        }
    }
}
```

### Example 2: Resetting state after a successful generation pass
```csharp
using Microsoft.CodeAnalysis;
using DotNet.SourceGeneratorToolkit;

[Generator]
public class ResetGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // After generating sources, mark everything as unchanged
        // to prepare for the next incremental pass.
        context.MarkAllUnchanged();

        // Optionally log a summary.
        var summary = context.GetRegenerationSummary();
        // logger.Info(summary);
    }
}
```

## Notes
- All extension methods operate on the mutable state held by the `IncrementalGeneratorContext` instance for the current generator execution. Changes made by these methods do not persist beyond the generation session.
- The methods are **not thread‑safe**; invoking them concurrently from multiple threads on the same context instance may lead to race conditions or inconsistent state. Use them only from the single‑threaded initialization pipeline provided by the incremental generator framework.
- Passing a `null` context will always result in an `ArgumentNullException`. Similarly, supplying a null or whitespace `entityName` to `HasEntityChanged` throws an `ArgumentException`.
- The hash set returned by `GetEntitiesRequiringRegeneration` reflects the internal state at the moment of the call; subsequent modifications to the context will not update the returned set. If a live view is required, call the method again after any state changes.
