# IBatchProcessor

The `IBatchProcessor` interface defines the contract for tracking the state and results of a batch processing operation within the source generator toolkit. It provides read-only access to the current item being processed, the overall success status of the batch, detailed error information, execution metrics, and progress counters, enabling generators to monitor and report on bulk operations efficiently.

## API

### `Item`
```csharp
public T Item { get; }
```
Retrieves the specific item currently being processed or the last item handled in the batch. The generic type `T` corresponds to the input data type defined by the concrete implementation. This property allows inspection of the data context during iteration or error reporting.

### `IsSuccessful`
```csharp
public bool IsSuccessful { get; }
```
Indicates whether the entire batch operation completed without encountering any critical failures. Returns `true` if all items were processed successfully according to the implementation's logic; otherwise, returns `false`. This flag provides a quick boolean check for overall job health.

### `ErrorMessage`
```csharp
public string? ErrorMessage { get; }
```
Contains the descriptive error message associated with the first encountered failure or the aggregate failure reason if the batch was unsuccessful. Returns `null` if `IsSuccessful` is `true`. This property is essential for diagnostics and logging when `IsSuccessful` is `false`.

### `ExecutionTimeMs`
```csharp
public long ExecutionTimeMs { get; }
```
Reports the total elapsed time required to process the batch, measured in milliseconds. This metric is useful for performance profiling and detecting regressions in source generation speed during large-scale builds.

### `ProcessedCount`
```csharp
public int ProcessedCount { get; }
```
Returns the number of items that have been successfully processed so far in the current batch. This counter increments as items pass validation and transformation logic, providing real-time progress tracking.

### `TotalCount`
```csharp
public int TotalCount { get; }
```
Specifies the total number of items included in the batch operation. This value remains constant throughout the execution of a specific batch and serves as the denominator for calculating completion percentages.

### `ErrorCount`
```csharp
public int ErrorCount { get; }
```
Indicates the total number of items that failed processing within the batch. A value greater than zero implies that `IsSuccessful` will be `false`, unless the implementation defines a tolerance threshold for non-critical errors.

## Usage

### Monitoring Batch Progress
The following example demonstrates how to inspect batch statistics after execution to determine if a source generation pass met performance and accuracy requirements.

```csharp
public void AnalyzeBatchResult(IBatchProcessor<SyntaxNode> processor)
{
    if (!processor.IsSuccessful)
    {
        Console.WriteLine($"Batch failed: {processor.ErrorMessage}");
        Console.WriteLine($"Errors encountered: {processor.ErrorCount} out of {processor.TotalCount}");
        return;
    }

    Console.WriteLine($"Batch completed successfully.");
    Console.WriteLine($"Processed: {processor.ProcessedCount}/{processor.TotalCount}");
    Console.WriteLine($"Execution time: {processor.ExecutionTimeMs}ms");
    
    // Inspect the last processed item if needed
    var lastItem = processor.Item;
    Console.WriteLine($"Last processed node kind: {lastItem.Kind()}");
}
```

### Conditional Logic Based on Error Rates
This example illustrates using the error count and success status to decide whether to emit additional diagnostic warnings or abort subsequent generation phases.

```csharp
public void DecideNextSteps(IBatchProcessor<TypeDeclaration> processor)
{
    // If more than 10% of the batch failed, log a severe warning
    if (processor.ErrorCount > 0)
    {
        double errorRate = (double)processor.ErrorCount / processor.TotalCount;
        if (errorRate > 0.10)
        {
            throw new InvalidOperationException(
                $"High failure rate detected ({errorRate:P2}): {processor.ErrorMessage}"
            );
        }
        
        // Log individual error context using the item property if available
        Console.WriteLine($"Partial failure on item: {processor.Item.Identifier.Text}");
    }
    
    // Proceed only if the batch is marked as successful
    if (processor.IsSuccessful)
    {
        EmitFinalArtifacts(processor.ExecutionTimeMs);
    }
}
```

## Notes

*   **Read-Only State**: All members exposed by `IBatchProcessor` are read-only properties. The state is managed entirely by the concrete implementation, ensuring that consumers cannot inadvertently modify batch metrics or error states during iteration.
*   **Null Safety**: The `ErrorMessage` property is nullable (`string?`). Consumers must check `IsSuccessful` or perform a null check before accessing this property to avoid dereferencing null in strict null-checking contexts.
*   **Timing Granularity**: `ExecutionTimeMs` is provided as a `long` to prevent overflow during extremely long-running batch operations, though typical source generator batches should complete within integer ranges.
*   **Thread Safety**: The interface definition does not enforce thread safety. Implementations may capture state from a single execution thread. If `IBatchProcessor` instances are shared across multiple threads (e.g., in parallel processing scenarios), external synchronization is required when reading these properties concurrently with the writing implementation.
*   **Item Context**: The `Item` property reflects the state at the time of the last update. In a completed batch, this typically represents the final item processed. If the batch failed early, it may represent the item that caused the failure.
