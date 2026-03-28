// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Caching;

/// <summary>
/// In-memory implementation of cache with automatic expiration support.
/// Thread-safe for concurrent access from multiple generation tasks.
/// </summary>
public class MemoryCache : ICache
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly object _lock = new();
    private int _hitCount = 0;
    private int _missCount = 0;

    // Default expiration: 1 hour
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(1);

    public bool TryGet<T>(string key, out T? value)
    {
        lock (_lock)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if entry has expired
                if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt)
                {
                    _cache.Remove(key);
                    value = default;
                    _missCount++;
                    return false;
                }

                _hitCount++;
                value = (T?)entry.Value;
                return true;
            }

            _missCount++;
            value = default;
            return false;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        lock (_lock)
        {
            var expiresAt = expiration.HasValue
                ? DateTime.UtcNow.Add(expiration.Value)
                : DateTime.UtcNow.Add(DefaultExpiration);

            _cache[key] = new CacheEntry
            {
                Value = value,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }

    public bool Contains(string key)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(key, out var entry))
                return false;

            // Check expiration
            if (entry.ExpiresAt.HasValue && DateTime.UtcNow > entry.ExpiresAt)
            {
                _cache.Remove(key);
                return false;
            }

            return true;
        }
    }

    public bool Remove(string key)
    {
        lock (_lock)
        {
            return _cache.Remove(key);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _cache.Clear();
            _hitCount = 0;
            _missCount = 0;
        }
    }

    public CacheStatistics GetStatistics()
    {
        lock (_lock)
        {
            // Remove expired entries before calculating stats
            var expiredKeys = _cache
                .Where(e => e.Value.ExpiresAt.HasValue && DateTime.UtcNow > e.Value.ExpiresAt)
                .Select(e => e.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }

            // Estimate approximate size (naive calculation)
            var sizeBytes = _cache.Sum(e =>
                (e.Key.Length * 2) + // UTF-16
                (e.Value.Value?.ToString()?.Length * 2 ?? 0) + // Value size estimate
                40); // Overhead per entry

            return new CacheStatistics
            {
                EntryCount = _cache.Count,
                HitCount = _hitCount,
                MissCount = _missCount,
                ApproximateSizeBytes = sizeBytes,
            };
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
