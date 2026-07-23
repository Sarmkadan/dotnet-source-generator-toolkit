#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Contains cacheability diagnostics for incremental source generation.
/// Tracks forced regenerations, non-equatable model violations, and generator stage timings.
/// </summary>
public sealed class CacheDiagnostics
{
/// <summary>
/// Total number of forced regenerations due to cache invalidation.
/// </summary>
public int ForcedRegenerations { get; set; }

/// <summary>
/// Per-generator counts of forced regenerations.
/// </summary>
public Dictionary<string, int> GeneratorRegenerationCounts { get; } = new(StringComparer.OrdinalIgnoreCase);

/// <summary>
/// Per-generator counts of models without proper value equality implementation.
/// </summary>
public Dictionary<string, int> NonEquatableModelViolations { get; } = new(StringComparer.OrdinalIgnoreCase);

/// <summary>
/// Per-generator stage durations in milliseconds.
/// </summary>
public Dictionary<string, long> GeneratorStageDurations { get; } = new(StringComparer.OrdinalIgnoreCase);

/// <summary>
/// Adds diagnostics from another instance.
/// </summary>
public void Merge(CacheDiagnostics other)
{
if (other == null)
throw new ArgumentNullException(nameof(other));

ForcedRegenerations += other.ForcedRegenerations;

foreach (var kvp in other.GeneratorRegenerationCounts)
{
GeneratorRegenerationCounts[kvp.Key] = GeneratorRegenerationCounts.GetValueOrDefault(kvp.Key) + kvp.Value;
}

foreach (var kvp in other.NonEquatableModelViolations)
{
NonEquatableModelViolations[kvp.Key] = NonEquatableModelViolations.GetValueOrDefault(kvp.Key) + kvp.Value;
}

foreach (var kvp in other.GeneratorStageDurations)
{
GeneratorStageDurations[kvp.Key] = GeneratorStageDurations.GetValueOrDefault(kvp.Key) + kvp.Value;
}
}

/// <summary>
/// Creates a new empty instance.
/// </summary>
public static CacheDiagnostics Create() => new();
}
