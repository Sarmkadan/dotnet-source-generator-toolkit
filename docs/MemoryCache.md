# MemoryCache

A lightweight in-memory cache implementation designed for short-lived data storage within a single process. It provides basic key-value storage with optional expiration semantics and statistics tracking, suitable for scenarios where low-latency access to frequently used data is required without the overhead of distributed caching.

## API

### `bool TryGet<T>(string key, out T? value)`

Attempts to retrieve a value from the cache by its key. The method returns `true` if the key exists and the value is of the requested type `T`; otherwise, it returns `false` and sets `value` to `null`.

- **Parameters**
  - `key`: The unique identifier for the cached value.
  - `value`: Output parameter that receives the cached value if found and of type `T`.
- **Return Value**: `true` if the key exists and the value is of type `T`; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

---

### `void Set<T>(string key, T value, DateTime? expiresAt = null)`

Inserts or updates a value in the cache with an optional expiration timestamp. If `expiresAt` is provided, the entry will be automatically removed after the specified time.

- **Parameters**
  - `key`: The unique identifier for the cached value.
  - `value`: The value to cache.
  - `expiresAt`: Optional expiration timestamp. If `null`, the entry does not expire.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null` or `value` is `null`.

---

### `bool Contains(string key)`

Checks whether a key exists in the cache, regardless of its expiration status or type.

- **Parameters**
  - `key`: The key to check.
- **Return Value**: `true` if the key exists; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

---
### `bool Remove(string key)`

Removes a key-value pair from the cache if it exists.

- **Parameters**
  - `key`: The key to remove.
- **Return Value**: `true` if the key existed and was removed; otherwise, `false`.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

---
### `void Clear()`

Removes all key-value pairs from the cache.

---
### `CacheStatistics GetStatistics()`

Retrieves statistics about the cache, including the number of entries, hits, misses, and evictions.

- **Return Value**: A `CacheStatistics` object containing cache metrics.
- **Exceptions**: None.

---
### `object? Value { get; }`

Gets the cached value associated with the current key, if it exists and has not expired. Returns `null` if the key does not exist or the entry has expired.

- **Exceptions**: None.

---
### `DateTime? ExpiresAt { get; }`

Gets the expiration timestamp of the current cache entry, if set. Returns `null` if the entry does not expire.

- **Exceptions**: None.

---
### `DateTime CreatedAt { get; }`

Gets the timestamp when the current cache entry was created.

- **Exceptions**: None.

## Usage

### Basic Usage with Expiration
