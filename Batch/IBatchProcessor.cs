// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Batch;

/// <summary>
/// Contract for processing items in batches with error handling and progress tracking.
/// Enables efficient processing of large collections with resource controls.
/// </summary>
public interface IBatchProcessor<T>
{
    /// <summary>
    /// Process items in batches with progress reporting.
    /// </summary>
    /// <param name="items">Items to process</param>
    /// <param name="processor">Async function to process each item</param>
    /// <param name="batchSize">Number of items to process in parallel</param>
    /// <param name="progress">Optional progress reporting callback</param>
    /// <returns>List of processing results</returns>
    Task<IEnumerable<BatchResult<T>>> ProcessAsync(
        IEnumerable<T> items,
        Func<T, Task> processor,
        int batchSize = 10,
        IProgress<BatchProgress>? progress = null);
}

/// <summary>
/// Result of processing a single batch item.
/// </summary>
public class BatchResult<T>
{
    public T Item { get; set; } = default!;
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// Progress report for batch processing.
/// </summary>
public class BatchProgress
{
    public int ProcessedCount { get; set; }
    public int TotalCount { get; set; }
    public int ErrorCount { get; set; }
    public double PercentComplete => TotalCount > 0 ? (double)ProcessedCount / TotalCount * 100 : 0;
}
