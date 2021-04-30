# GenerationCompletedEventExtensions

Provides utility extension methods for `GenerationCompletedEventArgs` that simplify inspection of source generator execution results, including error reporting, duration measurement, and summary generation.

## API

### `ToSummary(GenerationCompletedEventArgs eventArgs)`

Generates a human-readable summary of the source generator execution.

- **Parameters**
  - `eventArgs`: The event arguments containing generator execution details.
- **Return Value**
  - A string containing a formatted summary of the execution, including success status, duration, and error details if present.
- **Throws**
  - `ArgumentNullException`: If `eventArgs` is `null`.

### `HasErrors(GenerationCompletedEventArgs eventArgs)`

Determines whether the source generator execution resulted in any errors.

- **Parameters**
  - `eventArgs`: The event arguments containing generator execution details.
- **Return Value**
  - `true` if the generator reported any errors; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `eventArgs` is `null`.

### `GetErrorReport(GenerationCompletedEventArgs eventArgs)`

Retrieves a detailed error report if the generator execution encountered errors.

- **Parameters**
  - `eventArgs`: The event arguments containing generator execution details.
- **Return Value**
  - A string containing the error report if errors occurred; otherwise, `null`.
- **Throws**
  - `ArgumentNullException`: If `eventArgs` is `null`.

### `GetExecutionDuration(GenerationCompletedEventArgs eventArgs)`

Calculates the total execution time of the source generator.

- **Parameters**
  - `eventArgs`: The event arguments containing generator execution details.
- **Return Value**
  - A `TimeSpan` representing the duration of the generator execution.
- **Throws**
  - `ArgumentNullException`: If `eventArgs` is `null`.

## Usage

```csharp
// Example 1: Logging generator execution results
var eventArgs = new GenerationCompletedEventArgs(...);
if (GenerationCompletedEventExtensions.HasErrors(eventArgs))
{
    var errorReport = GenerationCompletedEventExtensions.GetErrorReport(eventArgs);
    Console.Error.WriteLine($"Generator failed: {errorReport}");
}
else
{
    var duration = GenerationCompletedEventExtensions.GetExecutionDuration(eventArgs);
    Console.WriteLine($"Generator succeeded in {duration.TotalMilliseconds}ms");
}

// Example 2: Generating a summary for CI/CD reporting
var summary = GenerationCompletedEventExtensions.ToSummary(eventArgs);
File.WriteAllText("generator-summary.txt", summary);
```

## Notes

- All methods are thread-safe and may be called concurrently from multiple threads without additional synchronization.
- `GetErrorReport` returns `null` when no errors occurred; always check the result before use.
- `GetExecutionDuration` measures wall-clock time and may be affected by system load or process scheduling.
- Methods throw `ArgumentNullException` for `null` inputs; ensure event arguments are validated before calling.
