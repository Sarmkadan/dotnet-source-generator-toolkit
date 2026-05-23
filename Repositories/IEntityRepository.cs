#nullable enable

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

