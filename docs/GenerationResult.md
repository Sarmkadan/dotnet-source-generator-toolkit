# GenerationResult

Represents the outcome of a single source generator execution within the `dotnet-source-generator-toolkit`. It captures the generated code, metadata, timing, and any warnings or errors produced during generation. This type is used to track and report the results of source generation steps, enabling downstream consumers to inspect, validate, and summarize the generation process.

## API

### Properties

- **`public string Id`**  
  A unique identifier for this generation result. Typically set at creation and not modified afterward.

- **`public string EntityName`**  
  The name of the entity (e.g., class, struct, record) that was the target of generation. May be empty if generation was not entity-specific.

- **`public GeneratorType GeneratorType`**  
  An enum value indicating the type of generator that produced this result (e.g., `ClassGenerator`, `InterfaceGenerator`). The exact values depend on the `GeneratorType` enum definition.

- **`public string GeneratedCode`**  
  The full text of the generated source code. May be empty if generation failed or produced no output.

- **`public string OutputFilePath`**  
  The file system path where the generated code was written, or an empty string if the output was not persisted.

- **`public GenerationStatus Status`**  
  The current status of the generation (e.g., `Pending`, `Running`, `Completed`, `Failed`). Updated by `MarkAsCompleted` and possibly other internal logic.

- **`public List<string> Warnings`**  
  A mutable list of warning messages collected during generation. Add warnings via `AddWarning` or directly manipulate the list.

- **`public List<string> Errors`**  
  A mutable list of error messages collected during generation. Add errors via `AddError` or directly manipulate the list.

- **`public int CodeLineCount`**  
  The number of lines in `GeneratedCode`. Set after code is generated; may be zero if no code was produced.

- **`public long GenerationDurationMs`**  
  The elapsed time in milliseconds for the generation process. Typically set when `MarkAsCompleted` is called.

- **`public DateTime CreatedAt`**  
  The UTC timestamp when this result instance was created.

- **`public DateTime CompletedAt`**  
  The UTC timestamp when generation completed. Set by `MarkAsCompleted`. Defaults to `DateTime.MinValue` if not yet completed.

- **`public string? CreatedBy`**  
  An optional identifier (e.g., tool name, user) that created this result. May be `null`.

- **`public Dictionary<string, string> Metadata`**  
  A mutable dictionary for arbitrary key-value metadata associated with the generation. Can be used to store custom context.

- **`public bool IsSuccessful`**  
  Returns `true` if `Status` is `Completed` and `Errors` is empty; otherwise `false`. This is a computed property based on current state.

### Methods

- **`public void MarkAsCompleted()`**  
  Sets `Status` to `Completed`, records the current UTC time in `CompletedAt`, and calculates `GenerationDurationMs` as the difference between `CompletedAt` and `CreatedAt`.  
  **Throws:** `InvalidOperationException` if `Status` is already `Completed` or if `CreatedAt` is `DateTime.MinValue`.

- **`public void AddWarning(string warning)`**  
  Appends the specified `warning` string to the `Warnings` list. Does nothing if `warning` is `null` or empty.  
  **Throws:** None.

- **`public void AddError(string error)`**  
  Appends the specified `error` string to the `Errors` list. Does nothing if `error` is `null` or empty.  
  **Throws:** None.

- **`public IEnumerable<string> Validate()`**  
  Returns a sequence of validation messages. Checks that `Id` is not null or empty, `EntityName` is not null, `GeneratedCode` is not null, and that `Errors` does not contain null entries. Additional validation may be performed based on `GeneratorType`.  
  **Returns:** An `IEnumerable<string>` of validation issues (empty if valid).  
  **Throws:** None.

- **`public string GetSummary()`**  
  Returns a human-readable summary string containing `Id`, `EntityName`, `Status`, `CodeLineCount`, `GenerationDurationMs`, and counts of warnings and errors. Format is implementation-defined but stable within a version.  
  **Returns:** A formatted string.  
  **Throws:** None.

## Usage

### Example 1: Creating and populating a generation result

```csharp
using System;
using dotnet_source_generator_toolkit;

var result = new GenerationResult
{
    Id = Guid.NewGuid().ToString(),
    EntityName = "Customer",
    GeneratorType = GeneratorType.ClassGenerator,
    GeneratedCode = "public class Customer { }",
    OutputFilePath = "./Generated/Customer.g.cs",
    CreatedBy = "MyGenerator",
    Metadata =
    {
        ["template"] = "entity.tpl"
    }
};

// Simulate generation work
result.AddWarning("Property 'Name' has no validation attributes.");
result.AddWarning("Consider adding XML documentation.");

// Complete the generation
result.MarkAsCompleted();

Console.WriteLine(result.GetSummary());
// Output example:
// GenerationResult [Id=..., EntityName=Customer, Status=Completed, Lines=1, Duration=12ms, Warnings=2, Errors=0]
```

### Example 2: Validating and handling errors

```csharp
using System;
using System.Linq;
using dotnet_source_generator_toolkit;

var result = new GenerationResult
{
    Id = "gen-001",
    EntityName = "Order",
    GeneratorType = GeneratorType.InterfaceGenerator,
    GeneratedCode = null // intentionally missing
};

result.AddError("Generated code is null.");

// Validate before using
var issues = result.Validate().ToList();
if (issues.Any())
{
    foreach (var issue in issues)
    {
        Console.Error.WriteLine($"Validation: {issue}");
    }
}

if (!result.IsSuccessful)
{
    Console.Error.WriteLine("Generation failed.");
}
```

## Notes

- **Thread safety:** `GenerationResult` is not thread-safe. Its mutable collections (`Warnings`, `Errors`, `Metadata`) and state-modifying methods (`MarkAsCompleted`, `AddWarning`, `AddError`) should not be accessed concurrently from multiple threads without external synchronization.
- **Edge cases:**  
  - Calling `MarkAsCompleted` more than once throws `InvalidOperationException`.  
  - `Validate()` may return messages even if `IsSuccessful` is `true` (e.g., if `Id` is empty but no errors exist).  
  - `AddWarning` and `AddError` silently ignore `null` or empty strings; they do not throw.  
  - `GenerationDurationMs` is computed from `CreatedAt` and `CompletedAt`; if the system clock changes between creation and completion, the duration may be negative or unexpectedly large.  
  - `GetSummary` always returns a string; it does not throw even if the result is in an incomplete state.
