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
	/// <param name="source">The source collection to partition.</param>
	/// <param name="partitionSize">The size of each partition.</param>
	/// <returns>An enumerable of partitions.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="partitionSize"/> is not positive.</exception>
	public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int partitionSize)
	{
		ArgumentNullException.ThrowIfNull(source);
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
	/// <param name="source">The source collection.</param>
	/// <param name="action">The async action to perform on each item.</param>
	/// <param name="degreeOfParallelism">Maximum concurrent operations. Defaults to processor count.</param>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
	public static async Task ParallelForEachAsync<T>(
		this IEnumerable<T> source,
		Func<T, Task> action,
		int degreeOfParallelism = -1)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(action);

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
	/// Adds multiple items to a collection in one call.
	/// </summary>
	/// <param name="collection">The target collection.</param>
	/// <param name="items">The items to add.</param>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="items"/> is null.</exception>
	public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
	{
		ArgumentNullException.ThrowIfNull(collection);
		ArgumentNullException.ThrowIfNull(items);

		foreach (var item in items)
		{
			collection.Add(item);
		}
	}

	/// <summary>
	/// Check if collection is null or empty.
	/// </summary>
	/// <param name="source">The collection to check.</param>
	/// <returns>True if null or empty; otherwise false.</returns>
	public static bool IsNullOrEmpty<T>(this IEnumerable<T>? source)
	{
		return source is null || !source.Any();
	}

	/// <summary>
	/// Returns distinct elements based on a key selector function.
	/// </summary>
	/// <param name="source">The source collection.</param>
	/// <param name="keySelector">A function to extract the key for each element.</param>
	/// <returns>An enumerable containing only distinct elements.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="keySelector"/> is null.</exception>
	public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(keySelector);

		var seen = new HashSet<TKey>();
		foreach (var item in source)
		{
			if (seen.Add(keySelector(item)))
				yield return item;
		}
	}

	/// <summary>
	/// Flattens a collection of collections into a single collection.
	/// </summary>
	/// <param name="source">The collection of collections to flatten.</param>
	/// <returns>A single flattened collection.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
	{
		ArgumentNullException.ThrowIfNull(source);
		return source.SelectMany(x => x);
	}

	/// <summary>
	/// Batches collection into groups of specified size.
	/// </summary>
	/// <param name="collection">The collection to batch.</param>
	/// <param name="batchSize">The maximum size of each batch.</param>
	/// <returns>An enumerable of batches.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="batchSize"/> is not positive.</exception>
	public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
	{
		ArgumentNullException.ThrowIfNull(collection);
		if (batchSize <= 0)
			throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

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

	/// <summary>
	/// Safely accesses collection element or returns default value if out of range.
	/// </summary>
	/// <param name="collection">The source collection.</param>
	/// <param name="index">The zero-based index.</param>
	/// <param name="defaultValue">The default value to return if index is out of range.</param>
	/// <returns>The element at the specified index or default value.</returns>
	public static T SafeElementAt<T>(this IEnumerable<T> collection, int index, T defaultValue = default)
	{
		ArgumentNullException.ThrowIfNull(collection);

		try
		{
			return collection.ElementAt(index);
		}
		catch (ArgumentOutOfRangeException)
		{
			return defaultValue;
		}
	}

	/// <summary>
	/// Chunks collection into groups of specified size (for .NET versions without built-in Chunk).
	/// </summary>
	/// <param name="collection">The collection to chunk.</param>
	/// <param name="size">The maximum size of each chunk.</param>
	/// <returns>An enumerable of chunks as arrays.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="collection"/> is null.</exception>
	/// <exception cref="ArgumentException"><paramref name="size"/> is not positive.</exception>
	public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> collection, int size)
	{
		ArgumentNullException.ThrowIfNull(collection);
		if (size <= 0)
			throw new ArgumentException("Chunk size must be greater than 0", nameof(size));

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