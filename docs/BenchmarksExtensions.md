# BenchmarksExtensions

The `BenchmarksExtensions` class provides a suite of static utility methods designed to process, analyze, and format collections of `BenchmarkReport` data within the `dotnet-source-generator-toolkit`. These helpers simplify the task of aggregating results, identifying performance outliers, and generating human-readable summaries from benchmark execution data.

## API

### FormatBenchmarkResultsAsMarkdown
Converts a collection of benchmark reports into a formatted Markdown string, suitable for logs or documentation.

*   **Parameters:** `IEnumerable<BenchmarkReport> reports`
*   **Returns:** `string` - A string containing the benchmark results in Markdown table format.
*   **Throws:** `ArgumentNullException` if `reports` is null.

### GetSlowestBenchmark
Identifies the benchmark report with the longest execution time from the provided collection.

*   **Parameters:** `IEnumerable<BenchmarkReport> reports`
*   **Returns:** `BenchmarkReport?` - The report instance with the highest duration, or `null` if the collection is empty.
*   **Throws:** `ArgumentNullException` if `reports` is null.

### GetFastestBenchmark
Identifies the benchmark report with the shortest execution time from the provided collection.

*   **Parameters:** `IEnumerable<BenchmarkReport> reports`
*   **Returns:** `BenchmarkReport?` - The report instance with the lowest duration, or `null` if the collection is empty.
*   **Throws:** `ArgumentNullException` if `reports` is null.

### CalculateTotalExecutionTime
Computes the aggregate execution time for all benchmark reports in the collection.

*   **Parameters:** `IEnumerable<BenchmarkReport> reports`
*   **Returns:** `double` - The sum of execution times from all reports. Returns `0` if the collection is empty.
*   **Throws:** `ArgumentNullException` if `reports` is null.

## Usage

### Generating a Summary Report
```csharp
using dotnet_source_generator_toolkit;

// Assume reports is an IEnumerable<BenchmarkReport> populated during test runs
IEnumerable<BenchmarkReport> reports = GetBenchmarkResults();

string markdownReport = BenchmarksExtensions.FormatBenchmarkResultsAsMarkdown(reports);
Console.WriteLine(markdownReport);
```

### Analyzing Performance Statistics
```csharp
using dotnet_source_generator_toolkit;

IEnumerable<BenchmarkReport> reports = GetBenchmarkResults();

double totalTime = BenchmarksExtensions.CalculateTotalExecutionTime(reports);
BenchmarkReport? slowest = BenchmarksExtensions.GetSlowestBenchmark(reports);

Console.WriteLine($"Total execution time: {totalTime}ms");
if (slowest != null)
{
    Console.WriteLine($"Slowest benchmark: {slowest.Name} took {slowest.Duration}ms");
}
```

## Notes

*   **Input Handling:** All methods throw an `ArgumentNullException` if the `reports` parameter is `null`. If the provided `IEnumerable` is empty, `CalculateTotalExecutionTime` returns `0`, while `GetSlowestBenchmark` and `GetFastestBenchmark` return `null`.
*   **Thread Safety:** These methods are designed as pure functional utilities. They do not modify the input `IEnumerable` or the `BenchmarkReport` objects within it. However, the caller is responsible for ensuring that the underlying collection is not modified by another thread during the execution of these methods, as standard `IEnumerable` implementations are not thread-safe for concurrent enumeration.
*   **Data Integrity:** These methods assume that the `Duration` property of each `BenchmarkReport` is correctly populated and represents a comparable unit of time (e.g., milliseconds).
