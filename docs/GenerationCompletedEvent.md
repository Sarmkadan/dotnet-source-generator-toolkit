# GenerationCompletedEvent

The `GenerationCompletedEvent` class encapsulates the terminal state and diagnostic metadata of a source generation process within the toolkit. It serves as a data transfer object, providing necessary details—including timing, success status, and error reports—to facilitate the monitoring, auditing, and debugging of automated code generation pipelines.

## API

### EventId
*   **Type:** `string`
*   **Purpose:** A unique identifier for the specific event instance, used for tracking and correlation within log management or event aggregation systems.

### OccurredAt
*   **Type:** `DateTime`
*   **Purpose:** A timestamp indicating exactly when the generation process concluded.

### RequestId
*   **Type:** `string`
*   **Purpose:** The unique identifier for the specific generation request that initiated this process, allowing linkage between the request and its final outcome.

### IsSuccessful
*   **Type:** `bool`
*   **Purpose:** Indicates whether the generation process completed without critical failures.

### FilesGenerated
*   **Type:** `int`
*   **Purpose:** The total count of source files successfully produced during the execution.

### Errors
*   **Type:** `List<string>`
*   **Purpose:** A collection of error messages accumulated during the generation process. This list remains empty if the process completes successfully.

### ExecutionTimeMs
*   **Type:** `long`
*   **Purpose:** The total duration of the generation process, measured in milliseconds.

## Usage

### Example 1: Logging a completed generation event
```csharp
public void LogGenerationResult(GenerationCompletedEvent e)
{
    if (e.IsSuccessful)
    {
        Console.WriteLine($"Request {e.RequestId} succeeded in {e.ExecutionTimeMs}ms, generated {e.FilesGenerated} files.");
    }
    else
    {
        Console.Error.WriteLine($"Request {e.RequestId} failed. Errors: {string.Join(", ", e.Errors)}");
    }
}
```

### Example 2: Aggregating metrics from multiple events
```csharp
public void ProcessEventMetrics(IEnumerable<GenerationCompletedEvent> events)
{
    var totalTime = events.Sum(e => e.ExecutionTimeMs);
    var failedCount = events.Count(e => !e.IsSuccessful);

    Console.WriteLine($"Processed {events.Count()} events. Total execution time: {totalTime}ms. Failures: {failedCount}.");
}
```

## Notes

*   **Thread Safety:** This class is designed as a data transfer object and is not inherently thread-safe. It is expected to be populated once at the completion of a generation task and then treated as immutable by consumers.
*   **Error Handling:** The `Errors` property may be null depending on the implementation used to instantiate the event; consumers should perform null checks before iterating over this list to avoid `NullReferenceException`.
*   **Timing Accuracy:** `ExecutionTimeMs` represents the wall-clock time elapsed for the generator execution. It may be affected by system load and is intended for performance monitoring rather than precise micro-benchmarking.
