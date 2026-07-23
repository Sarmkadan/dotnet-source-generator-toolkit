#nullable enable

// =====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Provides cache health diagnostics for incremental source generation.
/// Reports cacheability issues that prevent optimal incremental generator performance.
/// </summary>
public sealed class CacheHealthDiagnostics
{
/// <summary>
/// Gets the total number of forced regenerations due to cache invalidation.
/// Higher values indicate cache is being invalidated frequently, reducing performance benefits.
/// </summary>
public int ForcedRegenerations { get; init; }

/// <summary>
/// Gets the percentage of regenerations that were forced (0-100).
/// Values above 50% indicate significant cache invalidation issues.
/// </summary>
public double ForcedRegenerationRate => TotalGenerations > 0
? (double)ForcedRegenerations / TotalGenerations * 100
: 0;

/// <summary>
/// Gets the total number of generations tracked.
/// </summary>
public int TotalGenerations { get; init; }

/// <summary>
/// Gets per-generator counts of forced regenerations.
/// Higher values indicate specific generators are causing cache invalidation.
/// </summary>
public IReadOnlyDictionary<string, int> GeneratorRegenerationCounts { get; init; } = new Dictionary<string, int>();

/// <summary>
/// Gets per-generator counts of models without proper value equality implementation.
/// These models cannot participate in incremental caching effectively.
/// </summary>
public IReadOnlyDictionary<string, int> NonEquatableModelViolations { get; init; } = new Dictionary<string, int>();

/// <summary>
/// Gets per-generator stage durations in milliseconds.
/// </summary>
public IReadOnlyDictionary<string, long> GeneratorStageDurations { get; init; } = new Dictionary<string, long>();

/// <summary>
/// Gets whether cache health is optimal (no forced regenerations and no non-equatable models).
/// </summary>
public bool IsCacheHealthy => ForcedRegenerations == 0 && NonEquatableModelViolations.Count == 0;

/// <summary>
/// Gets a summary message about cache health.
/// </summary>
public string HealthSummary
{
get
{
if (IsCacheHealthy)
{
return "✅ Cache health is optimal - incremental generation can cache effectively";
}

var issues = new List<string>();
if (ForcedRegenerations > 0)
{
issues.Add($"{ForcedRegenerations} forced regenerations ({ForcedRegenerationRate:F1}% of total)");
}
if (NonEquatableModelViolations.Count > 0)
{
issues.Add($"{NonEquatableModelViolations.Count} generators have models without value equality");
}

return $"⚠️ Cache health issues detected: {string.Join(", ", issues)}";
}
}

/// <summary>
/// Creates cache health diagnostics from metrics snapshot.
/// </summary>
public static CacheHealthDiagnostics FromMetrics(GenerationMetricsCollector.MetricsSnapshot metrics)
{
if (metrics == null)
throw new ArgumentNullException(nameof(metrics));

return new CacheHealthDiagnostics
{
ForcedRegenerations = metrics.ForcedRegenerations,
TotalGenerations = metrics.TotalGenerations,
GeneratorRegenerationCounts = metrics.GeneratorRegenerationCounts,
NonEquatableModelViolations = metrics.NonEquatableModelViolations,
GeneratorStageDurations = metrics.GeneratorStageDurations
};
}
}
