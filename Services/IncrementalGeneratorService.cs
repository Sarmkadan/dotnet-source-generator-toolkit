// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Cryptography;
using System.Text;
using DotNetSourceGeneratorToolkit.Caching;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Pipeline;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Defines the contract for incremental generation support, enabling selective regeneration
/// of only the entities whose source files have changed since the last build.
/// </summary>
public interface IIncrementalGeneratorService
{
    /// <summary>
    /// Builds an <see cref="IncrementalGenerationContext"/> by hashing the project's source files
    /// and comparing them against the previously persisted state.
    /// </summary>
    /// <param name="projectInfo">Project metadata containing the path and entity list.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// A context describing which entities require regeneration and which can be skipped.
    /// </returns>
    Task<IncrementalGenerationContext> BuildContextAsync(
        ProjectInfo projectInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the current file hashes from the given context so subsequent runs can detect
    /// incremental changes against this baseline.
    /// </summary>
    /// <param name="context">The context whose current hashes should be saved.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task CommitContextAsync(
        IncrementalGenerationContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Filters the supplied entity list to only those that require regeneration according to
    /// the incremental context, skipping unchanged entities entirely.
    /// </summary>
    /// <param name="entities">All entities discovered in the project.</param>
    /// <param name="context">The incremental context produced by <see cref="BuildContextAsync"/>.</param>
    /// <returns>The subset of entities that must be regenerated in this run.</returns>
    IReadOnlyList<Entity> FilterChangedEntities(
        IReadOnlyList<Entity> entities,
        IncrementalGenerationContext context);
}

/// <inheritdoc cref="IIncrementalGeneratorService"/>
public sealed class IncrementalGeneratorService : IIncrementalGeneratorService
{
    private const string CacheKeyPrefix = "incremental:hashes:";
    private const string SourceFilePattern = "*.cs";

    private readonly ILogger<IncrementalGeneratorService> _logger;
    private readonly IFileSystemService _fileSystemService;
    private readonly ICache _cache;

    /// <summary>
    /// Initialises a new instance of <see cref="IncrementalGeneratorService"/>.
    /// </summary>
    /// <param name="logger">Logger for diagnostic and informational output.</param>
    /// <param name="fileSystemService">Service used to enumerate and read source files.</param>
    /// <param name="cache">Cache store used to persist file fingerprints between runs.</param>
    public IncrementalGeneratorService(
        ILogger<IncrementalGeneratorService> logger,
        IFileSystemService fileSystemService,
        ICache cache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public async Task<IncrementalGenerationContext> BuildContextAsync(
        ProjectInfo projectInfo,
        CancellationToken cancellationToken = default)
    {
        if (projectInfo == null)
            throw new ArgumentNullException(nameof(projectInfo));

        _logger.LogDebug("Building incremental context for project: {ProjectPath}", projectInfo.ProjectPath);

        var context = new IncrementalGenerationContext { ProjectPath = projectInfo.ProjectPath };

        // Restore previous fingerprints from cache so we have a baseline to diff against
        var cacheKey = CacheKeyPrefix + projectInfo.ProjectPath;
        if (_cache.TryGet<Dictionary<string, string>>(cacheKey, out var previousHashes) && previousHashes != null)
        {
            foreach (var kvp in previousHashes)
                context.PreviousFileHashes[kvp.Key] = kvp.Value;
        }

        // Hash every .cs file currently on disk
        var sourceFiles = await _fileSystemService.GetFilesAsync(projectInfo.ProjectPath, SourceFilePattern);
        foreach (var filePath in sourceFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_fileSystemService.FileExists(filePath))
                continue;

            try
            {
                var content = await _fileSystemService.ReadFileAsync(filePath);
                context.CurrentFileHashes[filePath] = ComputeContentHash(content);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Treat unreadable files as always-changed to avoid silently skipping regeneration
                _logger.LogWarning(ex, "Could not hash file {FilePath}; treating it as changed", filePath);
                context.CurrentFileHashes[filePath] = Guid.NewGuid().ToString("N");
            }
        }

        var changes = context.ComputeChanges();
        _logger.LogInformation(
            "Incremental analysis — added: {Added}, modified: {Modified}, removed: {Removed}",
            changes.Added.Count, changes.Modified.Count, changes.Removed.Count);

        var isFirstRun = context.PreviousFileHashes.Count == 0;

        if (isFirstRun)
        {
            _logger.LogInformation("No previous baseline found; scheduling full generation");
            context.IsFullRebuildRequired = true;
        }
        else if (!changes.HasChanges)
        {
            _logger.LogInformation(
                "No source changes detected; all {Count} entities can be skipped",
                projectInfo.Entities.Count);

            foreach (var entity in projectInfo.Entities)
                context.MarkUnchanged(entity.Name);
        }
        else
        {
            // Map changed file paths back to entity names by the EntityName.cs convention
            var changedFilePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in changes.Added) changedFilePaths.Add(f);
            foreach (var f in changes.Modified) changedFilePaths.Add(f);
            foreach (var f in changes.Removed) changedFilePaths.Add(f);

            foreach (var entity in projectInfo.Entities)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var entityChanged = changedFilePaths.Any(f =>
                    string.Equals(
                        Path.GetFileNameWithoutExtension(f),
                        entity.Name,
                        StringComparison.OrdinalIgnoreCase));

                if (entityChanged)
                    context.MarkChanged(entity.Name);
                else
                    context.MarkUnchanged(entity.Name);
            }
        }

        _logger.LogInformation(
            "Incremental context built — {Changed} entities to regenerate, {Skipped} entities skipped",
            context.ChangedCount, context.SkippedCount);

        return context;
    }

    /// <inheritdoc/>
    public Task CommitContextAsync(
        IncrementalGenerationContext context,
        CancellationToken cancellationToken = default)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        cancellationToken.ThrowIfCancellationRequested();

        var cacheKey = CacheKeyPrefix + context.ProjectPath;
        var snapshot = new Dictionary<string, string>(
            context.CurrentFileHashes,
            StringComparer.OrdinalIgnoreCase);

        _cache.Set(cacheKey, snapshot, TimeSpan.FromHours(24));

        _logger.LogDebug(
            "Committed {Count} file hashes for project: {ProjectPath}",
            snapshot.Count, context.ProjectPath);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public IReadOnlyList<Entity> FilterChangedEntities(
        IReadOnlyList<Entity> entities,
        IncrementalGenerationContext context)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        if (context.IsFullRebuildRequired)
            return entities;

        var result = new List<Entity>(capacity: context.ChangedCount);
        foreach (var entity in entities)
        {
            if (context.RequiresRegeneration(entity.Name))
                result.Add(entity);
        }

        _logger.LogDebug(
            "Filtered to {Count}/{Total} entities for incremental regeneration",
            result.Count, entities.Count);

        return result;
    }

    private static string ComputeContentHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes);
    }
}
