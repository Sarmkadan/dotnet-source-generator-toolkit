# ICache

The `ICache` interface defines a minimal contract for cache implementations, exposing basic metrics about cache usage and size. It is intended for scenarios where lightweight introspection of cache behavior is required without exposing implementation details.

## API

### `EntryCount`
Gets the current number of entries stored in the cache.

- **Return value**: `int` – the number of entries currently held by the cache.
- **Exceptions**: None. Returns zero if the cache is empty.

### `HitCount`
Gets the total number of cache hits since the cache was created or reset.

- **Return value**: `int` – the cumulative count of successful lookups that returned a cached value.
- **Exceptions**: None. Returns zero if no hits have occurred.

### `MissCount`
Gets the total number of cache misses since the cache was created or reset.

- **Return value**: `int` – the cumulative count of lookups that did not find a cached value.
- **Exceptions**: None. Returns zero if no misses have occurred.

### `ApproximateSizeBytes`
Gets an estimate of the total memory footprint of the cache, in bytes.

- **Return value**: `long` – an approximate measure of the cache’s memory usage.
- **Exceptions**: None. May return zero if the size cannot be estimated.

## Usage
