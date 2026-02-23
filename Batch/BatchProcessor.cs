// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Batch;

/// <summary>
/// Processes items in configurable batch sizes with parallel execution.
/// Provides error isolation - failure of one item doesn't stop the batch.
/// </summary>
public class BatchProcessor<T> : IBatchProcessor<T>
{
    private readonly ILogger<BatchProcessor<T>> _logger;

    public BatchProcessor(ILogger<BatchProcessor<T>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<BatchResult<T>>> ProcessAsync(
        IEnumerable<T> items,
        Func<T, Task> processor,
        int batchSize = 10,
        IProgress<BatchProgress>? progress = null)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        if (processor == null)
            throw new ArgumentNullException(nameof(processor));

        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be positive", nameof(batchSize));

        var itemsList = items.ToList();
        var results = new List<BatchResult<T>>();
        var errorCount = 0;

        _logger.LogInformation("Starting batch processing: {ItemCount} items, batch size: {BatchSize}",
            itemsList.Count, batchSize);

        var batches = Partition(itemsList, batchSize);

        foreach (var batch in batches)
        {
            var batchTasks = batch.Select(async item =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    await processor(item);
                    sw.Stop();

                    return new BatchResult<T>
                    {
                        Item = item,
                        IsSuccessful = true,
                        ExecutionTimeMs = sw.ElapsedMilliseconds,
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    errorCount++;

                    _logger.LogWarning(ex, "Batch item processing failed");

                    return new BatchResult<T>
                    {
                        Item = item,
                        IsSuccessful = false,
                        ErrorMessage = ex.Message,
                        ExecutionTimeMs = sw.ElapsedMilliseconds,
                    };
                }
            });

            var batchResults = await Task.WhenAll(batchTasks);
            results.AddRange(batchResults);

            // Report progress
            progress?.Report(new BatchProgress
            {
                ProcessedCount = results.Count,
                TotalCount = itemsList.Count,
                ErrorCount = errorCount,
            });
        }

        _logger.LogInformation(
            "Batch processing completed: {SuccessCount} successful, {ErrorCount} failed",
            results.Count(r => r.IsSuccessful),
            errorCount);

        return results;
    }

    private static IEnumerable<IEnumerable<T>> Partition(List<T> source, int partitionSize)
    {
        for (int i = 0; i < source.Count; i += partitionSize)
        {
            yield return source.Skip(i).Take(partitionSize);
        }
    }
}
