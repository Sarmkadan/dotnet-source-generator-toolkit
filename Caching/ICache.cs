// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Caching;

/// <summary>
/// Contract for caching generation results and analysis data.
/// Implementations can use in-memory, distributed, or persistent storage.
/// </summary>
public interface ICache
{
    /// <summary>
    /// Try to get a value from cache.
    /// </summary>
    /// <typeparam name="T">Type of cached value</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Retrieved value if found</param>
    /// <returns>True if value exists and was retrieved</returns>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Set a value in cache with optional expiration.
    /// </summary>
    /// <typeparam name="T">Type of value to cache</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Optional expiration timespan</param>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Check if a key exists in cache.
    /// </summary>
    /// <param name="key">Cache key to check</param>
    /// <returns>True if key exists</returns>
    bool Contains(string key);

    /// <summary>
    /// Remove a specific entry from cache.
    /// </summary>
    /// <param name="key">Cache key to remove</param>
    /// <returns>True if entry was removed</returns>
    bool Remove(string key);

    /// <summary>
    /// Clear all cache entries.
    /// </summary>
    void Clear();

    /// <summary>
    /// Get cache statistics for monitoring.
    /// </summary>
    /// <returns>Cache statistics</returns>
    CacheStatistics GetStatistics();
}

/// <summary>
/// Statistics about cache performance and usage.
/// </summary>
public class CacheStatistics
{
    public int EntryCount { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public double HitRate => HitCount + MissCount > 0 ? (double)HitCount / (HitCount + MissCount) : 0;
    public long ApproximateSizeBytes { get; set; }
}
