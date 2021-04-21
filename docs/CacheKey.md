# CacheKey

The `CacheKey` type serves as a centralized registry of constant string identifiers used within the `dotnet-source-generator-toolkit` to manage caching scopes for source generation operations. It provides predefined keys that distinguish between different stages of the generation pipeline, such as entity analysis, result compilation, project metadata retrieval, and configuration loading, ensuring consistent cache lookup and invalidation strategies across the toolkit.

## API

### `ForEntityAnalysis`
```csharp
public static string ForEntityAnalysis
```
A constant string key used to identify cache entries related to the analysis of source entities. This key is typically employed when storing or retrieving intermediate analysis results before code generation occurs. It does not accept parameters and returns a static string value. No exceptions are thrown during access.

### `ForGenerationResult`
```csharp
public static string ForGenerationResult
```
A constant string key designated for cache entries containing the final output of the source generation process. This identifier ensures that generated syntax trees or compilation units are stored under a unique namespace separate from analysis data. It does not accept parameters and returns a static string value. No exceptions are thrown during access.

### `ForProjectMetadata`
```csharp
public static string ForProjectMetadata
```
A constant string key utilized for caching metadata extracted from the project context, such as assembly references, compilation options, or global properties. This key isolates project-level information from entity-specific data. It does not accept parameters and returns a static string value. No exceptions are thrown during access.

### `ForConfiguration`
```csharp
public static string ForConfiguration
```
A constant string key reserved for caching parsed configuration settings, such as analyzer options or generator-specific flags loaded from additional files or attributes. This key facilitates the reuse of configuration objects without reparsing during subsequent generation runs. It does not accept parameters and returns a static string value. No exceptions are thrown during access.

## Usage

The following example demonstrates how to use `CacheKey` constants to store and retrieve analysis data in a hypothetical caching dictionary within a source generator context:

```csharp
using System.Collections.Concurrent;
using DotNetSourceGeneratorToolkit;

public class GeneratorCache
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public void StoreAnalysisResult(string entityId, object analysisData)
    {
        // Use the specific key for entity analysis scope
        var key = $"{CacheKey.ForEntityAnalysis}:{entityId}";
        _cache.TryAdd(key, analysisData);
    }

    public object? GetAnalysisResult(string entityId)
    {
        var key = $"{CacheKey.ForEntityAnalysis}:{entityId}";
        _cache.TryGetValue(key, out var data);
        return data;
    }
}
```

This example illustrates using distinct keys to separate configuration data from generation results, preventing key collisions when managing different types of cached objects:

```csharp
using System.Collections.Generic;
using DotNetSourceGeneratorToolkit;

public class GenerationContext
{
    private readonly Dictionary<string, object> _storage = new();

    public void Initialize(Configuration config, ProjectMetadata metadata)
    {
        // Store configuration using its dedicated key
        _storage[CacheKey.ForConfiguration] = config;
        
        // Store project metadata using its dedicated key
        _storage[CacheKey.ForProjectMetadata] = metadata;
    }

    public Configuration GetConfiguration()
    {
        if (_storage.TryGetValue(CacheKey.ForConfiguration, out var config))
        {
            return (Configuration)config;
        }
        throw new InvalidOperationException("Configuration not found in cache.");
    }
}
```

## Notes

*   **Key Composition**: The members of `CacheKey` are raw string constants intended to be used as prefixes or exact keys. When caching multiple items of the same category (e.g., multiple entities), developers should append unique identifiers to these base keys to avoid collisions, as shown in the usage examples.
*   **Thread Safety**: Accessing these static string properties is inherently thread-safe because they return immutable string literals and involve no state mutation. However, the underlying cache mechanisms (e.g., `Dictionary` or `ConcurrentDictionary`) using these keys must implement their own thread-safety guarantees.
*   **Immutability**: As these are `static readonly` (or `const`) string fields, their values cannot be modified at runtime. This ensures that cache key identities remain consistent throughout the lifetime of the application domain, preventing accidental cache invalidation due to key drift.
*   **Equality Comparisons**: Since these are string constants, standard string equality comparison is sufficient for cache lookups. Ensure that any custom caching logic treats these keys as case-sensitive strings, matching the exact definition provided in the toolkit.
