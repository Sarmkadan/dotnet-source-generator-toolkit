#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for collection operations.
/// Provides batch processing, partitioning, and bulk operations.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Partition a collection into chunks of specified size.
    /// Useful for batch processing large datasets.
    /// </summary>
    public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int partitionSize)
    {
        if (partitionSize <= 0)
            throw new ArgumentException("Partition size must be positive", nameof(partitionSize));

        var list = source.ToList();

        for (int i = 0; i < list.Count; i += partitionSize)
        {
            yield return list.Skip(i).Take(partitionSize);
        }
    }

    /// <summary>
    /// Process collection items in parallel with specified degree of parallelism.
    /// </summary>
    public static async Task ParallelForEachAsync<T>(
        this IEnumerable<T> source,
        Func<T, Task> action,
        int degreeOfParallelism = -1)
    {
        if (degreeOfParallelism <= 0)
            degreeOfParallelism = Environment.ProcessorCount;

        var semaphore = new System.Threading.SemaphoreSlim(degreeOfParallelism);

        var tasks = source.Select(async item =>
        {
            await semaphore.WaitAsync();
            try
            {
                await action(item);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Add multiple items to a collection in one call.
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Check if collection is null or empty.
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
    {
        return source is null || !source.Any();
    }

    /// <summary>
    /// Get distinct items by a key selector.
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in source)
        {
            if (seen.Add(keySelector(item)))
                yield return item;
        }
    }

    /// <summary>
    /// Flatten a collection of collections.
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany(x => x);
    }

    /// <summary>Batches collection into groups of specified size.</summary>
    public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be greater than 0");

        var batch = new System.Collections.Generic.List<T>();
        foreach (var item in collection)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = [];
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>Safely accesses collection element or returns default.</summary>
    public static T SafeElementAt<T>(this IEnumerable<T> collection, int index, T defaultValue = default)
    {
        try
        {
            return collection.ElementAt(index);
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>Chunks collection into groups of specified size (for .NET versions without built-in Chunk).</summary>
    public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> collection, int size)
    {
        if (size <= 0)
            throw new ArgumentException("Chunk size must be greater than 0");

        var chunk = new System.Collections.Generic.List<T>(size);
        foreach (var item in collection)
        {
            chunk.Add(item);
            if (chunk.Count == size)
            {
                yield return chunk.ToArray();
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
            yield return chunk.ToArray();
    }
}
