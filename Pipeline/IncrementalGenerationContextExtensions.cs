#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace DotNetSourceGeneratorToolkit.Pipeline;

/// <summary>
/// Provides extension methods for <see cref="IncrementalGenerationContext"/> to simplify common
/// incremental generation scenarios and improve developer productivity.
/// </summary>
public static class IncrementalGenerationContextExtensions
{
    /// <summary>
    /// Determines whether any entities in the specified collection require regeneration.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <param name="entityNames">The collection of entity names to check.</param>
    /// <returns><c>true</c> if any entity requires regeneration; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="entityNames"/> is null.</exception>
    public static bool AnyRequiresRegeneration(
        this IncrementalGenerationContext context,
        IEnumerable<string> entityNames)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityNames);

        return entityNames.Any(entityName => context.RequiresRegeneration(entityName));
    }

    /// <summary>
    /// Marks all entities in the specified collection as changed, scheduling them for regeneration.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <param name="entityNames">The collection of entity names to mark as changed.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="entityNames"/> is null.</exception>
    public static void MarkAllChanged(
        this IncrementalGenerationContext context,
        IEnumerable<string> entityNames)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityNames);

        foreach (var entityName in entityNames)
        {
            context.MarkChanged(entityName);
        }
    }

    /// <summary>
    /// Marks all entities in the specified collection as unchanged so they can be skipped during generation.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <param name="entityNames">The collection of entity names to mark as unchanged.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="entityNames"/> is null.</exception>
    public static void MarkAllUnchanged(
        this IncrementalGenerationContext context,
        IEnumerable<string> entityNames)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityNames);

        foreach (var entityName in entityNames)
        {
            context.MarkUnchanged(entityName);
        }
    }

    /// <summary>
    /// Gets a summary of the regeneration status for display purposes.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <param name="includeContextId">Whether to include the context ID in the summary.</param>
    /// <returns>A formatted string containing regeneration statistics.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static string GetRegenerationSummary(
        this IncrementalGenerationContext context,
        bool includeContextId = true)
    {
        ArgumentNullException.ThrowIfNull(context);

        var summary = new System.Text.StringBuilder();

        if (includeContextId)
        {
            summary.AppendLine($"Context: {context.ContextId}");
        }

        summary.AppendLine($"Project: {context.ProjectPath}");
        summary.AppendLine($"Last Generated: {context.LastGeneratedAt:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine($"Changed Entities: {context.ChangedCount}");
        summary.AppendLine($"Skipped Entities: {context.SkippedCount}");
        summary.AppendLine($"Full Rebuild Required: {context.IsFullRebuildRequired}");

        return summary.ToString();
    }

    /// <summary>
    /// Determines whether the specified entity has changed based on file hash comparison.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <param name="entityName">The entity name to check.</param>
    /// <param name="filePath">The file path associated with the entity.</param>
    /// <returns><c>true</c> if the entity has changed; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="entityName"/> is null.</exception>
    public static bool HasEntityChanged(
        this IncrementalGenerationContext context,
        string entityName,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(entityName);
        ArgumentNullException.ThrowIfNull(filePath);

        // If full rebuild is required, the entity has effectively changed
        if (context.IsFullRebuildRequired)
        {
            return true;
        }

        // Check if entity is explicitly marked as changed
        if (context.ChangedEntityNames.Contains(entityName))
        {
            return true;
        }

        // Check if the file hash has changed
        if (context.PreviousFileHashes.TryGetValue(filePath, out var previousHash) &&
            context.CurrentFileHashes.TryGetValue(filePath, out var currentHash))
        {
            return previousHash != currentHash;
        }

        // Entity is new (file didn't exist previously)
        return !context.PreviousFileHashes.ContainsKey(filePath);
    }

    /// <summary>
    /// Gets the set of all entity names that require regeneration.
    /// </summary>
    /// <param name="context">The generation context.</param>
    /// <returns>A new hash set containing all entity names that require regeneration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
    public static HashSet<string> GetEntitiesRequiringRegeneration(this IncrementalGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Add all explicitly changed entities
        foreach (var entityName in context.ChangedEntityNames)
        {
            result.Add(entityName);
        }

        // Add entities that require regeneration due to full rebuild
        if (context.IsFullRebuildRequired)
        {
            // In full rebuild mode, all unchanged entities become changed
            foreach (var entityName in context.UnchangedEntityNames)
            {
                result.Add(entityName);
            }
        }

        return result;
    }
}