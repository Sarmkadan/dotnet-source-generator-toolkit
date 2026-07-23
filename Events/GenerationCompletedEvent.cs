#nullable enable

// =====================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Published when generation process completes (success or failure).
/// Allows subscribers to clean up resources or persist results.
/// </summary>
public sealed class GenerationCompletedEvent : IDomainEvent
{
public string EventId { get; } = Guid.NewGuid().ToString();
public DateTime OccurredAt { get; } = DateTime.UtcNow;
public string RequestId { get; set; } = string.Empty;

/// <summary>
/// Whether generation completed successfully.
/// </summary>
public bool IsSuccessful { get; set; }

/// <summary>
/// Total number of files generated.
/// </summary>
public int FilesGenerated { get; set; }

/// <summary>
/// Any errors encountered during generation.
/// </summary>
public List<string> Errors { get; set; } = new();

/// <summary>
/// Total execution time in milliseconds.
/// </summary>
public long ExecutionTimeMs { get; set; }

/// <summary>
/// Cache diagnostics for tracking incremental generation cacheability.
/// </summary>
public CacheDiagnostics CacheDiagnostics { get; set; } = new();
}
