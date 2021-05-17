#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Repositories;

/// <summary>
/// Extension methods for <see cref="GenerationResultRepository"/> providing
/// convenient querying and aggregation operations on generation results.
/// </summary>
public static class GenerationResultRepositoryExtensions
{
    /// <summary>
    /// Gets the first generation result by entity name, or null if not found.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="entityName">Name of the entity to find.</param>
    /// <returns>The first matching generation result or null if no result is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="entityName"/> is null or whitespace.</exception>
    public static async Task<GenerationResult?> GetFirstByEntityAsync(this GenerationResultRepository repository, string entityName)
    {
        ArgumentNullException.ThrowIfNull(repository);

        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);

        var results = await repository.GetByEntityAsync(entityName);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Gets the first generation result by generator type, or null if not found.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="generatorType">Type of generator to find.</param>
    /// <returns>The first matching generation result or null if no result is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<GenerationResult?> GetFirstByGeneratorAsync(this GenerationResultRepository repository, GeneratorType generatorType)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var results = await repository.GetByGeneratorAsync(generatorType);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Gets the first generation result by status, or null if not found.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="status">Status to filter by.</param>
    /// <returns>The first matching generation result or null if no result is found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<GenerationResult?> GetFirstByStatusAsync(this GenerationResultRepository repository, GenerationStatus status)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var results = await repository.GetByStatusAsync(status);
        return results.FirstOrDefault();
    }

    /// <summary>
    /// Gets all generation results filtered by multiple criteria.
    /// </summary>
    /// <param name="repository">The repository instance.</param>
    /// <param name="entityName">Optional entity name to filter by.</param>
    /// <param name="generatorType">Optional generator type to filter by.</param>
    /// <param name="status">Optional status to filter by.</param>
    /// <returns>Filtered collection of generation results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="repository"/> is null.</exception>
    public static async Task<IEnumerable<GenerationResult>> GetByCriteriaAsync(
        this GenerationResultRepository repository,
        string? entityName = null,
        GeneratorType? generatorType = null,
        GenerationStatus? status = null)
    {
        ArgumentNullException.ThrowIfNull(repository);

        var results = await repository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            results = results.Where(r => r.EntityName == entityName);
        }

        if (generatorType.HasValue)
        {
            results = results.Where(r => r.GeneratorType == generatorType.Value);
        }

        if (status.HasValue)
        {
            results = results.Where(r => r.Status == status.Value);
        }

        return results;
    }
}