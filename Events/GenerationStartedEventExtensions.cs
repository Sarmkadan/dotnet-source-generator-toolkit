#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Provides extension methods for <see cref="GenerationStartedEvent"/> to enhance functionality
/// and improve developer experience when working with generation events.
/// </summary>
public static class GenerationStartedEventExtensions
{
    /// <summary>
    /// Creates a deep copy of the event to allow safe modifications without affecting the original.
    /// </summary>
    /// <param name="event">The source event to copy.</param>
    /// <returns>A new instance with identical property values.</returns>
    public static GenerationStartedEvent DeepCopy(this GenerationStartedEvent @event)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var copy = new GenerationStartedEvent();
        copy.RequestId = @event.RequestId;
        copy.ProjectPath = @event.ProjectPath;
        copy.EntityCount = @event.EntityCount;
        copy.GeneratorTypes.AddRange(@event.GeneratorTypes);
        return copy;
    }

    /// <summary>
    /// Determines whether the generation involves any of the specified generator types.
    /// </summary>
    /// <param name="event">The event to check.</param>
    /// <param name="generatorTypes">The generator types to search for.</param>
    /// <returns>True if any of the specified generator types are present; otherwise, false.</returns>
    public static bool HasGeneratorType(this GenerationStartedEvent @event, params string[] generatorTypes)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (generatorTypes == null || generatorTypes.Length == 0)
        {
            return false;
        }

        return @event.GeneratorTypes.Any(genType => generatorTypes.Contains(genType, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Formats a summary string containing key information about the generation event.
    /// </summary>
    /// <param name="event">The event to format.</param>
    /// <param name="includeTimestamp">Whether to include the timestamp in the output.</param>
    /// <returns>A formatted summary string.</returns>
    public static string ToSummaryString(this GenerationStartedEvent @event, bool includeTimestamp = true)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var summary = new System.Text.StringBuilder();
        summary.Append("Generation Started [");
        summary.Append(@event.EventId.AsSpan(0, Math.Min(8, @event.EventId.Length)));
        summary.Append("]");

        if (includeTimestamp && @event.OccurredAt != default)
        {
            summary.Append(" at ");
            summary.Append(@event.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        summary.Append(" for project: ");
        summary.Append(Path.GetFileName(@event.ProjectPath) ?? @event.ProjectPath);

        if (@event.EntityCount > 0)
        {
            summary.Append(" with ");
            summary.Append(@event.EntityCount);
            summary.Append(" entities");
        }

        if (@event.GeneratorTypes.Count > 0)
        {
            summary.Append(" using generators: ");
            summary.Append(string.Join(", ", @event.GeneratorTypes));
        }

        if (!string.IsNullOrEmpty(@event.RequestId))
        {
            summary.Append(" (Request: ");
            summary.Append(@event.RequestId);
            summary.Append(")");
        }

        return summary.ToString();
    }

    /// <summary>
    /// Adds a new generator type to the event's generator types collection.
    /// </summary>
    /// <param name="event">The event to modify.</param>
    /// <param name="generatorType">The generator type to add.</param>
    /// <returns>The modified event (for method chaining).</returns>
    public static GenerationStartedEvent AddGeneratorType(this GenerationStartedEvent @event, string generatorType)
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        if (!string.IsNullOrWhiteSpace(generatorType))
        {
            if (@event.GeneratorTypes.Contains(generatorType, StringComparer.OrdinalIgnoreCase))
            {
                return @event;
            }

            @event.GeneratorTypes.Add(generatorType.Trim());
        }

        return @event;
    }
}