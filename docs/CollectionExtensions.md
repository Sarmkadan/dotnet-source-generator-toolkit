# CollectionExtensions

`CollectionExtensions` provides a set of pure‑functional extension methods for working with `IEnumerable<T>`, `ICollection<T>` and `IList<T>` instances. The helpers are designed to be composable, side‑effect free (except where mutation is explicit, e.g. `AddRange`), and safe to use with LINQ queries.

## API

### Partition<T>
```csharp
public static IEnumerable<IEnumerable<T>> Partition<T>(
    this IEnumerable<T> source,
    int partitionCount)
```
**Purpose** – Splits the source sequence into approximately equal‑sized partitions, distributing elements in a round‑robin fashion.  
**Parameters**  
- `source`: The sequence to partition.  
- `partitionCount`: The number of partitions to create; must be greater than zero.  
**Return value** – An `IEnumerable<IEnumerable<T>>` where each inner sequence yields the elements belonging to one partition. The partitions are lazily enumerated; enumerating a partition forces enumeration of the source up to the elements needed for that partition.  
**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `partitionCount` ≤ 0.

### ParallelForEachAsync<T>
```csharp
public static Task ParallelForEachAsync<T>(
    this IEnumerable<T> source,
    Func<T, Task> body,
    int maxDegreeOfParallelism = Environment.ProcessorCount)
```
**Purpose** – Asynchronously invokes `body` for each element in `source`, allowing a configurable degree of parallelism. The method returns a `Task` that completes when all invocations have finished.  
**Parameters**  
- `source`: The sequence of items to process.  
- `body`: An asynchronous delegate that receives an element and returns a `Task`.  
- `maxDegreeOfParallelism`: Optional; the maximum number of concurrent `body` executions. Defaults to the number of processors.  
**Return value** – A `Task` representing the asynchronous operation.  
**Exceptions**  
- `ArgumentNullException` if `source` or `body` is `null`.  
- `ArgumentOutOfRangeException` if `maxDegreeOfParallelism` ≤ 0.  
- Any exception thrown by `body` is propagated through the returned `Task` (aggregated into an `AggregateException` if multiple occur).

### AddRange<T>
```csharp
public static void AddRange<T>(
    this ICollection<T> collection,
    IEnumerable<T> items)
```
**Purpose** – Adds all elements from `items` to the end of `collection`.  
**Parameters**  
- `collection`: The target collection to receive the items.  
- `items`: The sequence of items to add.  
**Return value** – None (the collection is modified in place).  
**Exceptions**  
- `ArgumentNullException` if `collection` or `items` is `null`.  
- `NotSupportedException` if `collection` is read‑only or does not support adding items.  
- Any exception thrown by the underlying collection’s `Add` method is propagated.

### IsNullOrEmpty<T>
```csharp
public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
```
**Purpose** – Determines whether `source` is `null` or contains no elements.  
**Parameters**  
- `source`: The sequence to test.  
**Return value** – `true` if `source` is `null` or empty; otherwise `false`.  
**Exceptions** – None.

### DistinctBy<T, TKey>
```csharp
public static IEnumerable<T> DistinctBy<T, TKey>(
    this IEnumerable<T> source,
    Func<T, TKey> keySelector)
```
**Purpose** – Returns distinct elements from `source` based on the key produced by `keySelector`, preserving the original order of first occurrence.  
**Parameters**  
- `source`: The sequence to filter.  
- `keySelector`: A function that extracts the key used for comparison.  
**Return value** – An `IEnumerable<T>` that yields each element whose key has not been seen before.  
**Exceptions**  
- `ArgumentNullException` if `source` or `keySelector` is `null`.  
- Any exception thrown by `keySelector` during enumeration is propagated.

### Flatten<T>
```csharp
public static IEnumerable<T> Flatten<T>(
    this IEnumerable<IEnumerable<T>> source)
```
**Purpose** – Concatenates the inner sequences of `source` into a single sequence.  
**Parameters**  
- `source`: A sequence of sequences to flatten.  
**Return value** – An `IEnumerable<T>` that yields all elements from all inner sequences in the order they appear.  
**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- Any exception thrown while enumerating an inner sequence is propagated.

### Batch<T>
```csharp
public static IEnumerable<IEnumerable<T>> Batch<T>(
    this IEnumerable<T> source,
    int size)
```
**Purpose** – Splits `source` into consecutive batches, each containing up to `size` elements.  
**Parameters**  
- `source`: The sequence to batch.  
- `size`: The maximum number of elements per batch; must be greater than zero.  
**Return value** – An `IEnumerable<IEnumerable<T>>` where each inner sequence yields a batch. The final batch may contain fewer than `size` elements if the source is not evenly divisible.  
**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `size` ≤ 0.

### SafeElementAt<T>
```csharp
public static T SafeElementAt<T>(
    this IList<T> source,
    int index,
    T fallback = default)
```
**Purpose** – Retrieves the element at `index` if it exists; otherwise returns `fallback` without throwing.  
**Parameters**  
- `source`: The list to index into.  
- `index`: The zero‑based position to retrieve.  
- `fallback`: The value to return when `index` is out of range; defaults to `default(T)`.  
**Return value** – The element at `index` if `0 ≤ index < source.Count`; otherwise `fallback`.  
**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- No exception is thrown for an out‑of‑range index; the method is “safe”.

### Chunk<T>
```csharp
public static IEnumerable<T[]> Chunk<T>(
    this IEnumerable<T> source,
    int size)
```
**Purpose** – Divides `source` into chunks of exactly `size` elements, returning each chunk as an array; the final chunk may be smaller.  
**Parameters**  
- `source`: The sequence to chunk.  
- `size`: The number of elements per chunk; must be greater than zero.  
**Return value** – An `IEnumerable<T[]>` where each array contains a chunk of elements.  
**Exceptions**  
- `ArgumentNullException` if `source` is `null`.  
- `ArgumentOutOfRangeException` if `size` ≤ 0.

## Usage

```csharp
// Example 1: Batch processing of file lines
var lines = File.ReadAllLines("log.txt");
foreach (var batch in lines.Batch(100))
{
    // Process up to 100 lines at a time
    var processed = batch.Select(ParseLogEntry).ToList();
    SaveToDatabase(processed);
}

// Example 2: Safe retrieval with fallback and parallel transformation
var numbers = Enumerable.Range(1, 10);
int fifthOrDefault = numbers.SafeElementAt(4, -1); // returns 5
int fifteenthOrDefault = numbers.SafeElementAt(14, -1); // returns -1 (fallback)

var transformed = await numbers
    .Select(n => Task.FromResult(n * n))
    .ParallelForEachAsync(async task => await task, maxDegreeOfParallelism: 4);
```

## Notes

- All extension methods are **pure** with respect to their input sequences; they do not modify the source unless the method’s purpose is mutation (`AddRange`).  
- Methods that return lazy sequences (`Partition`, `Batch`, `Flatten`, `DistinctBy`, `Chunk`) defer work until the returned sequence is enumerated. Enumerating multiple times may cause multiple enumerations of the source.  
- `ParallelForEachAsync` schedules work on the thread pool; the delegate `body` must be thread‑safe if it accesses shared state. The method does not provide a cancellation token; callers wishing to cancel should use a `CancellationToken` inside `body` and handle `OperationCanceledException` themselves.  
- `SafeElementAt` is safe only for `IList<T>` implementations that provide O(1) indexed access; passing other collection types will still compile but may incur O(n) cost due to the implicit conversion to `IList<T>` via the extension method’s constraint.  
- When `size` arguments are supplied to `Batch` or `Chunk`, values less than or equal to 1 will produce sequences where each element forms its own batch/chunk; a size of zero is invalid and throws.  
- `DistinctBy` uses the default equality comparer for `TKey`; if a custom comparer is required, callers should materialize keys or use `GroupBy` followed by `Select`.  
- The methods do not allocate unnecessary intermediate collections beyond what is required for their specific operation (e.g., `Flush` does not create arrays unless the underlying implementation does so for efficiency).  
- Thread safety of the returned sequences follows the thread safety of the source enumeration; if the source is safe for concurrent read‑only enumeration, the returned sequences inherit that property. Mutating the source while enumerating any of these methods’ results leads to undefined behavior.
