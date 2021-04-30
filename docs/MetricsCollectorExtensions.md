# MetricsCollectorExtensions

The `MetricsCollectorExtensions` class provides a set of static extension methods designed to simplify the collection of runtime performance metrics within applications built using the `dotnet-source-generator-toolkit`. It offers standardized mechanisms for recording counters, gauges, and histograms, as well as utility methods for measuring the execution time of synchronous and asynchronous operations with minimal boilerplate code.

## API

### IncrementCounter
Increments a specific metric counter by a defined value.
*   **Parameters**: Accepts the metric name and an optional increment value (defaulting to 1).
*   **Return Value**: `void`.
*   **Exceptions**: Throws an exception if the metric name is null, empty, or if the underlying collector is not initialized.

### RecordGauge
Records an instantaneous value for a specific gauge metric.
*   **Parameters**: Accepts the metric name and the numeric value to record.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an exception if the metric name is invalid or if the value type is unsupported by the underlying implementation.

### MeasureTime
Measures the execution time of a synchronous code block.
*   **Parameters**: Accepts an `Action` delegate representing the code to measure.
*   **Return Value**: Returns a `TimeSpan` representing the total elapsed time of the execution.
*   **Exceptions**: Propagates any exceptions thrown by the provided `Action` delegate after stopping the timer.

### MeasureTime<T>
Measures the execution time of a synchronous function and returns both the result and the duration.
*   **Parameters**: Accepts a `Func<T>` delegate representing the function to measure.
*   **Return Value**: Returns a tuple `(T Result, TimeSpan Elapsed)` containing the function's return value and the elapsed time.
*   **Exceptions**: Propagates any exceptions thrown by the provided `Func<T>` delegate.

### MeasureTimeAsync
Measures the execution time of an asynchronous operation.
*   **Parameters**: Accepts a `Func<Task>` delegate representing the asynchronous operation to measure.
*   **Return Value**: Returns a `Task` that completes when the operation finishes.
*   **Exceptions**: Propagates any exceptions thrown by the asynchronous operation.

### MeasureTimeAsync<T>
Measures the execution time of an asynchronous function and returns both the result and the duration.
*   **Parameters**: Accepts a `Func<Task<T>>` delegate representing the asynchronous function to measure.
*   **Return Value**: Returns a `Task<(T Result, TimeSpan Elapsed)>` that yields the result and elapsed time upon completion.
*   **Exceptions**: Propagates any exceptions thrown by the asynchronous operation.

### RecordHistogram
Records a value into a histogram distribution for statistical analysis.
*   **Parameters**: Accepts the metric name and the numeric value to record.
*   **Return Value**: `void`.
*   **Exceptions**: Throws an exception if the metric name is invalid or if the value falls outside the allowed range for the histogram buckets.

### GetSnapshotAndReset
Retrieves the current accumulated metrics data and resets the internal counters to zero.
*   **Parameters**: No parameters.
*   **Return Value**: Returns a `MetricsSnapshot` object containing the collected data up to the point of invocation.
*   **Exceptions**: Throws an exception if the internal state is corrupted or inaccessible.

### RecordOperation
Records a high-level operation event, potentially aggregating status, duration, or custom tags depending on the implementation context.
*   **Parameters**: Accepts the operation name and optional metadata (such as success status or tags).
*   **Return Value**: `void`.
*   **Exceptions**: Throws an exception if the operation name is null or empty.

## Usage

The following example demonstrates measuring the execution time of a synchronous calculation and recording the result as a gauge.

```csharp
using DotNetSourceGeneratorToolkit;

public class DataProcessor
{
    public void ProcessData()
    {
        // Measure the time taken to compute and get the result
        var (result, elapsed) = MetricsCollectorExtensions.MeasureTime(() => 
        {
            return PerformComplexCalculation();
        });

        // Record the result value as a gauge
        MetricsCollectorExtensions.RecordGauge("calculation.output.value", result);
        
        // Record the duration as a histogram for distribution analysis
        MetricsCollectorExtensions.RecordHistogram("calculation.duration.ms", elapsed.TotalMilliseconds);
    }

    private double PerformComplexCalculation()
    {
        // Simulation of work
        System.Threading.Thread.Sleep(50);
        return 42.0;
    }
}
```

The following example illustrates measuring an asynchronous database call and incrementing a counter upon successful completion.

```csharp
using DotNetSourceGeneratorToolkit;
using System.Threading.Tasks;

public class UserRepository
{
    public async Task<User> GetUserAsync(int id)
    {
        try 
        {
            // Measure the async operation
            var (user, elapsed) = await MetricsCollectorExtensions.MeasureTimeAsync(async () => 
            {
                return await FetchUserFromDatabaseAsync(id);
            });

            // Increment success counter
            MetricsCollectorExtensions.IncrementCounter("user.fetch.success");
            
            return user;
        }
        catch
        {
            // Increment failure counter
            MetricsCollectorExtensions.IncrementCounter("user.fetch.failure");
            throw;
        }
    }

    private Task<User> FetchUserFromDatabaseAsync(int id)
    {
        // Mock implementation
        return Task.FromResult(new User { Id = id, Name = "Test" });
    }
}
```

## Notes

*   **Thread Safety**: All methods in `MetricsCollectorExtensions` are designed to be thread-safe. Multiple threads can concurrently invoke `IncrementCounter`, `RecordGauge`, or timing methods without requiring external locking mechanisms.
*   **Exception Propagation**: The `MeasureTime` and `MeasureTimeAsync` overloads ensure that the timer is stopped even if the delegated operation throws an exception. The original exception is re-thrown to the caller immediately after the metric is recorded.
*   **Reset Behavior**: Calling `GetSnapshotAndReset` is not atomic with respect to other write operations across different threads; while the read-and-clear of the internal state is atomic, metrics recorded by other threads during the method execution may appear in the next snapshot rather than the current one.
*   **Overhead**: While optimized, frequent calls to `RecordHistogram` with high-cardinality labels may impact memory usage. It is recommended to use static metric names where possible.
