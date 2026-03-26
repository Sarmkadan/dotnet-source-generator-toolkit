// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Repository interface for storing entity analysis and generation metadata.
/// </summary>
public interface IEntityRepository
{
    /// <summary>
    /// Add or update an entity in the repository.
    /// </summary>
    Task<Entity> SaveAsync(Entity entity);

    /// <summary>
    /// Retrieve an entity by ID.
    /// </summary>
    Task<Entity?> GetByIdAsync(string id);

    /// <summary>
    /// Retrieve an entity by name.
    /// </summary>
    Task<Entity?> GetByNameAsync(string name);

    /// <summary>
    /// Get all entities in the repository.
    /// </summary>
    Task<IEnumerable<Entity>> GetAllAsync();

    /// <summary>
    /// Delete an entity from the repository.
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Get entities by namespace.
    /// </summary>
    Task<IEnumerable<Entity>> GetByNamespaceAsync(string @namespace);

    /// <summary>
    /// Clear all entities from the repository.
    /// </summary>
    Task ClearAsync();
}

/// <summary>
/// In-memory implementation of entity repository.
/// </summary>
public class EntityRepository : IEntityRepository
{
    private readonly Dictionary<string, Entity> _entities = new();

    public async Task<Entity> SaveAsync(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = Guid.NewGuid().ToString();

        entity.UpdatedAt = DateTime.UtcNow;
        _entities[entity.Id] = entity;

        return await Task.FromResult(entity);
    }

    public async Task<Entity?> GetByIdAsync(string id)
    {
        _entities.TryGetValue(id, out var entity);
        return await Task.FromResult(entity);
    }

    public async Task<Entity?> GetByNameAsync(string name)
    {
        var entity = _entities.Values.FirstOrDefault(e => e.Name == name);
        return await Task.FromResult(entity);
    }

    public async Task<IEnumerable<Entity>> GetAllAsync()
    {
        return await Task.FromResult(_entities.Values.ToList());
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var removed = _entities.Remove(id);
        return await Task.FromResult(removed);
    }

    public async Task<IEnumerable<Entity>> GetByNamespaceAsync(string @namespace)
    {
        var entities = _entities.Values.Where(e => e.Namespace == @namespace);
        return await Task.FromResult(entities);
    }

    public async Task ClearAsync()
    {
        _entities.Clear();
        await Task.CompletedTask;
    }
}
