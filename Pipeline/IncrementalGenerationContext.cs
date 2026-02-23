// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Pipeline;

/// <summary>
/// Holds the state for an incremental generation run, tracking source-file fingerprints
/// and determining which entities require regeneration based on detected changes.
/// </summary>
public sealed class IncrementalGenerationContext
{
    /// <summary>Gets the unique identifier for this generation context.</summary>
    public Guid ContextId { get; } = Guid.NewGuid();

    /// <summary>Gets or sets the project path this context was built for.</summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>Gets or sets the timestamp of the previous successful generation run.</summary>
    public DateTimeOffset LastGeneratedAt { get; set; } = DateTimeOffset.MinValue;

    /// <summary>
    /// Gets the file-path-to-hash map captured during the previous generation run.
    /// </summary>
    public Dictionary<string, string> PreviousFileHashes { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the file-path-to-hash map computed from the current source files.
    /// </summary>
    public Dictionary<string, string> CurrentFileHashes { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the names of entities whose source files have changed and need regeneration.</summary>
    public HashSet<string> ChangedEntityNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets the names of entities whose source files are unchanged and can be skipped.</summary>
    public HashSet<string> UnchangedEntityNames { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Gets or sets whether all entities must be regenerated regardless of change detection.</summary>
    public bool IsFullRebuildRequired { get; set; }

    /// <summary>Gets the number of entities scheduled for regeneration in this run.</summary>
    public int ChangedCount => ChangedEntityNames.Count;

    /// <summary>Gets the number of entities that will be skipped in this run.</summary>
    public int SkippedCount => UnchangedEntityNames.Count;

    /// <summary>
    /// Determines whether the specified entity needs to be regenerated in this run.
    /// </summary>
    /// <param name="entityName">The entity name to evaluate.</param>
    /// <returns><c>true</c> if regeneration is required; otherwise <c>false</c>.</returns>
    public bool RequiresRegeneration(string entityName) =>
        IsFullRebuildRequired || ChangedEntityNames.Contains(entityName);

    /// <summary>
    /// Marks an entity as changed, scheduling it for regeneration.
    /// Has no effect if the entity was already marked changed.
    /// </summary>
    /// <param name="entityName">The name of the entity to mark as changed.</param>
    public void MarkChanged(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        ChangedEntityNames.Add(entityName);
        UnchangedEntityNames.Remove(entityName);
    }

    /// <summary>
    /// Marks an entity as unchanged so it can be skipped during generation.
    /// Has no effect if the entity has already been marked as changed.
    /// </summary>
    /// <param name="entityName">The name of the entity to mark as unchanged.</param>
    public void MarkUnchanged(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        if (!ChangedEntityNames.Contains(entityName))
            UnchangedEntityNames.Add(entityName);
    }

    /// <summary>
    /// Computes the differences between the previous and current file hashes.
    /// </summary>
    /// <returns>A <see cref="FileChangeSummary"/> describing added, modified, and removed files.</returns>
    public FileChangeSummary ComputeChanges()
    {
        var added = CurrentFileHashes.Keys
            .Except(PreviousFileHashes.Keys, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var removed = PreviousFileHashes.Keys
            .Except(CurrentFileHashes.Keys, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var modified = CurrentFileHashes
            .Where(kv => PreviousFileHashes.TryGetValue(kv.Key, out var prev) && prev != kv.Value)
            .Select(kv => kv.Key)
            .ToList();

        return new FileChangeSummary(added, modified, removed);
    }
}

/// <summary>
/// Describes the file-level changes detected between two consecutive incremental generation runs.
/// </summary>
/// <param name="Added">Paths of files that did not exist in the previous run.</param>
/// <param name="Modified">Paths of files whose content has changed since the previous run.</param>
/// <param name="Removed">Paths of files that no longer exist since the previous run.</param>
public sealed record FileChangeSummary(
    IReadOnlyList<string> Added,
    IReadOnlyList<string> Modified,
    IReadOnlyList<string> Removed)
{
    /// <summary>Gets the total number of files that have changed (added + modified + removed).</summary>
    public int TotalChanges => Added.Count + Modified.Count + Removed.Count;

    /// <summary>Gets whether any files have changed since the previous run.</summary>
    public bool HasChanges => TotalChanges > 0;
}
