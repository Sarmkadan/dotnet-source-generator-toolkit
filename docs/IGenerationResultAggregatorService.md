# IGenerationResultAggregatorService

Provides aggregated metrics and summary statistics across multiple source generator execution results. The service consolidates individual generation outcomes into a single report, exposing counts, durations, success rates, and breakdowns by generator type for monitoring and diagnostics.

## API

### TotalResults
`int TotalResults { get; }`

The total number of generation results processed by the aggregator, including successful, failed, and skipped outcomes.

### SuccessCount
`int SuccessCount { get; }`

The number of generation results that completed without errors or warnings.

### FailureCount
`int FailureCount { get; }`

The number of generation results that terminated with one or more errors.

### SkippedCount
`int SkippedCount { get; }`

The number of generation results that were intentionally bypassed or excluded from execution.

### TotalDurationMs
`long TotalDurationMs { get; }`

The cumulative wall-clock time, in milliseconds, consumed by all generation operations included in the aggregation.

### TotalLinesGenerated
`int TotalLinesGenerated { get; }`

The sum of source code lines produced across all successful generation results.

### TotalWarnings
`int TotalWarnings { get; }`

The aggregate count of warnings emitted during all generation operations.

### TotalErrors
`int TotalErrors { get; }`

The aggregate count of errors emitted during all generation operations.

### ReportGeneratedAt
`DateTime ReportGeneratedAt { get; }`

The UTC timestamp at which the aggregated report was finalized and made available.

### ResultsByType
`Dictionary<GeneratorType, int> ResultsByType { get; }`

A dictionary mapping each generator type to the number of results recorded for that type. Useful for identifying which generators contributed most to the overall workload.

### FailedResults
`List<GenerationResult> FailedResults { get; }`

The collection of individual generation results that ended in failure. Each entry retains its original error details for root-cause analysis.

### SuccessRate
`double SuccessRate { get; }`

The ratio of successful results to total results, expressed as a value between 0.0 and 1.0. Returns 0.0 when `TotalResults` is zero.

### AverageDuration
`TimeSpan AverageDuration { get; }`

The mean duration per generation result, computed as `TotalDurationMs` divided by `TotalResults`. Returns `TimeSpan.Zero` when no results are present.

### TotalCount
`int TotalCount { get; }`

The total number of generation results tracked. Equivalent to `TotalResults`.

### CompletedCount
`int CompletedCount { get; }`

The number of generation results that reached a terminal state, whether success or failure. Excludes skipped results.

### FailedCount
`int FailedCount { get; }`

The number of generation results that failed. Equivalent to `FailureCount`.

### SuccessPercentage
`double SuccessPercentage { get; }`

The success rate expressed as a percentage between 0.0 and 100.0. Returns 0.0 when `TotalResults` is zero.

### MinDurationMs
`long MinDurationMs { get; }`

The shortest duration, in milliseconds, observed among all generation results. Returns `long.MaxValue` when no results are present.

### MaxDurationMs
`long MaxDurationMs { get; }`

The longest duration, in milliseconds, observed among all generation results. Returns 0 when no results are present.

### AverageDurationMs
`double AverageDurationMs { get; }`

The arithmetic mean duration in milliseconds, computed as a floating-point value. Returns 0.0 when no results are present.

## Usage

### Example 1: Aggregating results and logging a summary

```csharp
IGenerationResultAggregatorService aggregator = GetAggregator();
aggregator.AddResult(new GenerationResult { Success = true, DurationMs = 120, LinesGenerated = 340 });
aggregator.AddResult(new GenerationResult { Success = false, DurationMs = 45, Errors = 2 });
aggregator.AddResult(new GenerationResult { Success = true, DurationMs = 200, LinesGenerated = 512, Skipped = true });

Console.WriteLine($"Report generated at: {aggregator.ReportGeneratedAt:O}");
Console.WriteLine($"Total: {aggregator.TotalResults}, Success: {aggregator.SuccessCount}, Failed: {aggregator.FailureCount}, Skipped: {aggregator.SkippedCount}");
Console.WriteLine($"Success rate: {aggregator.SuccessPercentage:F1}%");
Console.WriteLine($"Duration range: {aggregator.MinDurationMs}ms - {aggregator.MaxDurationMs}ms, Average: {aggregator.AverageDurationMs:F1}ms");
Console.WriteLine($"Total lines generated: {aggregator.TotalLinesGenerated}");
Console.WriteLine($"Errors: {aggregator.TotalErrors}, Warnings: {aggregator.TotalWarnings}");

foreach (var failed in aggregator.FailedResults)
{
    Console.WriteLine($"  Failed generator: {failed.GeneratorType}, Errors: {failed.Errors}");
}
```

### Example 2: Monitoring generator health by type

```csharp
IGenerationResultAggregatorService aggregator = GetAggregator();
// ... populate aggregator with results from multiple generator types ...

var report = new StringBuilder();
report.AppendLine($"Aggregation completed at {aggregator.ReportGeneratedAt:R}");
report.AppendLine($"Overall: {aggregator.CompletedCount} completed, {aggregator.FailedCount} failed, {aggregator.SkippedCount} skipped");
report.AppendLine($"Success: {aggregator.SuccessPercentage:F2}%");
report.AppendLine($"Timing: avg {aggregator.AverageDuration.TotalMilliseconds:F1}ms, min {aggregator.MinDurationMs}ms, max {aggregator.MaxDurationMs}ms");
report.AppendLine("Breakdown by generator type:");

foreach (var kvp in aggregator.ResultsByType.OrderByDescending(k => k.Value))
{
    report.AppendLine($"  {kvp.Key}: {kvp.Value} results");
}

if (aggregator.FailedResults.Any())
{
    report.AppendLine("Failures:");
    foreach (var failure in aggregator.FailedResults)
    {
        report.AppendLine($"  [{failure.GeneratorType}] {failure.ErrorMessage}");
    }
}

File.WriteAllText("generator-report.txt", report.ToString());
```

## Notes

- All duration properties (`TotalDurationMs`, `MinDurationMs`, `MaxDurationMs`, `AverageDurationMs`, `AverageDuration`) reflect only results that have been explicitly added to the aggregator. Results with uninitialized or negative duration values may skew statistics; implementers should validate input before aggregation.
- `SuccessRate` and `SuccessPercentage` return 0.0 when `TotalResults` is zero to avoid division-by-zero errors. Callers should check `TotalResults > 0` before interpreting these values as meaningful.
- `MinDurationMs` returns `long.MaxValue` when no results are present, serving as a sentinel value. Callers should guard against this when displaying minimum duration.
- `ResultsByType` keys are determined by the `GeneratorType` enum values present in the aggregated results. Types with zero results are typically absent from the dictionary.
- `FailedResults` contains the original `GenerationResult` instances; modifying them after retrieval may corrupt the aggregated statistics. Treat the list as read-only for diagnostic purposes.
- Thread safety is not guaranteed by the interface. If multiple threads concurrently add results while reading aggregated properties, external synchronization is required to prevent torn reads or inconsistent state.
- `TotalCount`, `CompletedCount`, and `FailedCount` are aliases or near-aliases for `TotalResults`, `SuccessCount + FailureCount`, and `FailureCount` respectively. They exist for caller convenience and should remain consistent with their counterparts.
