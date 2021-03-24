# IncrementalGenerationContext

Provides incremental source generators with information about the project state and previous generation outcomes, enabling them to decide whether a full regeneration is needed or if only specific entities have changed.

## API

### ContextId
**Type:** `Guid`  
**Purpose:** Unique identifier for this generation context instance. Remains constant for the lifetime of the generator run and can be used to correlate logs or diagnostics.

### ProjectPath
**Type:** `string`  
**Purpose:** Absolute or relative path to the project being compiled. May be empty if the host environment does not expose a project path.

### LastGeneratedAt
**Type:** `DateTimeOffset`  
**Purpose:** Timestamp of the most recent successful source generation. If no prior generation occurred, the value is `DateTimeOffset.MinValue`.

### PreviousFileHashes
**Type:** `Dictionary<string, string>`  
**Purpose:** Mapping of file paths to hash values representing the file contents at the time of the last generation. The key is the file path; the value is a string hash (e.g., SHA256 hex).  
**Throws:** `ArgumentNullException` if accessed via an indexer with a null key; otherwise the dictionary itself is never null.

### CurrentFileHashes
**Type:** `Dictionary<string, string>`  
**Purpose:** Mapping of file paths to hash values representing the file contents at the current compilation. Populated by the host before the generator runs.  
**Throws:** Same as `PreviousFileHashes`.

### ChangedEntityNames
**Type:** `HashSet<string>`  
**Purpose:** Set of entity identifiers (e.g., type names, method signatures) that have changed since the last generation. Entities are added by the generator when it detects modifications.

### UnchangedEntityNames
**Type:** `HashSet<string>`  
**Purpose:** Set of entity identifiers that were examined and found to be unchanged since the last generation. Useful for skipping work on stable code.

### IsFullRebuildRequired
**Type:** `bool`  
**Purpose:** Indicates whether the generator must perform a full rebuild because incremental tracking is not possible (e.g., missing previous hashes). When `true`, the generator should ignore incremental data and regenerate all output.

### RequiresRegeneration
**Type:** `bool`  
**Purpose:** Flag that the generator can set to request another generation pass after the current one completes. Setting it to `true` signals the host to invoke the generator again with updated context.

### MarkChanged()
**Signature:** `public void MarkChanged()`  
**Purpose:** Notifies the context that the current generation has produced changes that may affect downstream consumers. Internally sets `RequiresRegeneration` to `true`.  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** None.

### MarkUnchanged()
**Signature:** `public void MarkUnchanged()`  
**Purpose:** Notifies the context that the current generation produced no changes that require further passes. Internally leaves `RequiresRegeneration` unchanged (typically `false`).  
**Parameters:** None.  
**Returns:** `void`.  
**Throws:** None.

### ComputeChanges()
**Signature:** `public FileChangeSummary ComputeChanges()`  
**Purpose:** Analyzes `PreviousFileHashes`, `CurrentFileHashes`, `ChangedEntityNames`, and `UnchangedEntityNames` to produce a summary of what has changed at the file level.  
**Parameters:** None.  
**Returns:** A `FileChangeSummary` instance containing the comparison results.  
**Throws:** `InvalidOperationException` if either hash dictionary is null or if the context is in a state where incremental computation is not supported (e.g., `IsFullRebuildRequired` is `true`).

### FileChangeSummary
**Type:** `public sealed record FileChangeSummary`  
**Purpose:** Immutable record returned by `ComputeChanges` that encapsulates the outcome of the file‑level change analysis. It contains properties such as whether any files changed, collections of added, removed, and modified file paths, and possibly a flag indicating if the change set is empty. As a sealed record, it supports value‑based equality and deconstruction.

## Usage

### Example 1: Basic incremental check
```csharp
public override void Execute(IncrementalGenerationContext context)
{
    // If a full rebuild is forced, regenerate everything.
    if (context.IsFullRebuildRequired)
    {
        GenerateAllSources(context);
        return;
    }

    // Compute what changed since the last run.
    var changes = context.ComputeChanges();

    if (!changes.AnyChanges)
    {
        // No file or entity changes – we can skip generation.
        context.MarkUnchanged();
        return;
    }

    // Otherwise, process only the changed entities.
    foreach (var entity in changes.ChangedEntities)
    {
        GenerateSourceForEntity(context, entity);
    }

    context.MarkChanged();
}
```

### Example 2: Updating hash dictionaries after generation
```csharp
public override void Execute(IncrementalGenerationContext context)
{
    // Assume we have just written new source files to disk.
    foreach (var file in GeneratedFiles)
    {
        var hash = ComputeFileHash(file.Path);
        context.CurrentFileHashes[file.Path] = hash;
    }

    // Determine if any newFileChangeSummary summary = context.ComputeChanges();
    if (summary.HasChanges)
    {
        // Regenerate only for files that changed.
        foreach (var changed in summary.ModifiedFiles)
        {
            RegenerateFile(changed);
        }
    }

    context.MarkChanged();
}
```

## Notes

- The hash dictionaries (`PreviousFileHashes` and `CurrentFileHashes`) are populated by the host; mutating them directly outside of the generator’s execution window may lead to inconsistent state.
- `IsFullRebuildRequired` takes precedence over incremental logic; when true, the generator should ignore `ChangedEntityNames`, `UnchangedEntityNames`, and the hash dictionaries.
- `MarkChanged` and `MarkUnchanged` only affect the `RequiresRegeneration` flag; they do not alter the hash or entity sets.
- The `FileChangeSummary` record is immutable; any attempt to modify its properties after creation will not compile.
- Thread safety: The context is intended for use on a single thread during a generator execution. Concurrent access to the mutable collections (`Dictionary<string,string>` and `HashSet<string>`) from multiple threads without external synchronization can result in race conditions. The host should ensure that the generator’s `Execute` method is invoked serially for each compilation.
