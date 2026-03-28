// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Published when generation process begins for a project.
/// Allows subscribers to initialize resources or log activity.
/// </summary>
public class GenerationStartedEvent : IDomainEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;

    /// <summary>
    /// Path to the project being analyzed.
    /// </summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// Number of entities discovered that will be processed.
    /// </summary>
    public int EntityCount { get; set; }

    /// <summary>
    /// Types of generators that will execute.
    /// </summary>
    public List<string> GeneratorTypes { get; set; } = new();
}
