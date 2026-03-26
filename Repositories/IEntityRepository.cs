// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Repositories;

/// <summary>
/// Repository interface for persisting and querying Entity objects.
/// </summary>
public interface IEntityRepository
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    Task<Entity?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IEnumerable<Entity>> GetAllAsync();

    /// <summary>
    /// Gets entities by name.
    /// </summary>
    Task<IEnumerable<Entity>> GetByNameAsync(string name);

    /// <summary>
    /// Adds a new entity to storage.
    /// </summary>
    Task<Entity> AddAsync(Entity entity);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task<Entity> UpdateAsync(Entity entity);

    /// <summary>
    /// Deletes an entity by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Gets entities by namespace.
    /// </summary>
    Task<IEnumerable<Entity>> GetByNamespaceAsync(string namespaceName);

    /// <summary>
    /// Gets count of all entities.
    /// </summary>
    Task<int> CountAsync();
}

/// <summary>
/// Repository interface for persisting and querying GenerationResult objects.
/// </summary>
public interface IGenerationResultRepository
{
    /// <summary>
    /// Gets a generation result by ID.
    /// </summary>
    Task<GenerationResult?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all generation results.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GetAllAsync();

    /// <summary>
    /// Gets generation results for a specific entity.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GetByEntityAsync(string entityName);

    /// <summary>
    /// Gets generation results by status.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GetByStatusAsync(GenerationStatus status);

    /// <summary>
    /// Adds a generation result to storage.
    /// </summary>
    Task<GenerationResult> AddAsync(GenerationResult result);

    /// <summary>
    /// Updates an existing generation result.
    /// </summary>
    Task<GenerationResult> UpdateAsync(GenerationResult result);

    /// <summary>
    /// Deletes a generation result by ID.
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Gets count of generation results.
    /// </summary>
    Task<int> CountAsync();

    /// <summary>
    /// Gets count of results by status.
    /// </summary>
    Task<int> CountByStatusAsync(GenerationStatus status);
}
