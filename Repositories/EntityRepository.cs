// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Repositories;

/// <summary>
/// In-memory repository implementation for Entity persistence and querying.
/// Provides complete CRUD operations and advanced query methods.
/// </summary>
public class EntityRepository : IEntityRepository
{
    private readonly List<Entity> _entities = [];
    private readonly ILogger<EntityRepository> _logger;

    public EntityRepository(ILogger<EntityRepository> logger)
    {
        _logger = logger;
    }

    public async Task<Entity?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var entity = _entities.FirstOrDefault(e => e.Id == id);
        _logger.LogInformation("Retrieved entity by ID: {Id} - Found: {Found}", id, entity != null);
        return await Task.FromResult(entity);
    }

    public async Task<IEnumerable<Entity>> GetAllAsync()
    {
        _logger.LogInformation("Retrieved all entities: {Count} total", _entities.Count);
        return await Task.FromResult(_entities.AsEnumerable());
    }

    public async Task<IEnumerable<Entity>> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        var entities = _entities.Where(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
        _logger.LogInformation("Retrieved entities by name '{Name}': {Count} found", name, entities.Count);
        return await Task.FromResult(entities);
    }

    public async Task<Entity> AddAsync(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var validationErrors = entity.Validate();
        if (validationErrors.Any())
            throw new InvalidOperationException($"Entity validation failed: {string.Join(", ", validationErrors)}");

        // Ensure unique ID
        if (string.IsNullOrWhiteSpace(entity.Id))
            entity.Id = Guid.NewGuid().ToString();

        _entities.Add(entity);
        _logger.LogInformation("Added entity: {EntityId} - {EntityName}", entity.Id, entity.Name);

        return await Task.FromResult(entity);
    }

    public async Task<Entity> UpdateAsync(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var existing = _entities.FirstOrDefault(e => e.Id == entity.Id);
        if (existing == null)
            throw new InvalidOperationException($"Entity with ID {entity.Id} not found");

        entity.UpdatedAt = DateTime.UtcNow;
        _entities.Remove(existing);
        _entities.Add(entity);

        _logger.LogInformation("Updated entity: {EntityId} - {EntityName}", entity.Id, entity.Name);
        return await Task.FromResult(entity);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var entity = _entities.FirstOrDefault(e => e.Id == id);
        if (entity == null)
        {
            _logger.LogWarning("Delete attempted for non-existent entity: {EntityId}", id);
            return false;
        }

        var removed = _entities.Remove(entity);
        if (removed)
            _logger.LogInformation("Deleted entity: {EntityId} - {EntityName}", id, entity.Name);

        return await Task.FromResult(removed);
    }

    public async Task<IEnumerable<Entity>> GetByNamespaceAsync(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            return [];

        var entities = _entities.Where(e => e.Namespace.Equals(namespaceName, StringComparison.OrdinalIgnoreCase)).ToList();
        _logger.LogInformation("Retrieved entities by namespace '{Namespace}': {Count} found", namespaceName, entities.Count);
        return await Task.FromResult(entities);
    }

    public async Task<int> CountAsync()
    {
        var count = _entities.Count;
        _logger.LogInformation("Entity count: {Count}", count);
        return await Task.FromResult(count);
    }
}

/// <summary>
/// In-memory repository implementation for GenerationResult persistence and querying.
/// Tracks and provides access to code generation operation results.
/// </summary>
public class GenerationResultRepository : IGenerationResultRepository
{
    private readonly List<GenerationResult> _results = [];
    private readonly ILogger<GenerationResultRepository> _logger;

    public GenerationResultRepository(ILogger<GenerationResultRepository> logger)
    {
        _logger = logger;
    }

    public async Task<GenerationResult?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var result = _results.FirstOrDefault(r => r.Id == id);
        _logger.LogInformation("Retrieved generation result by ID: {Id} - Found: {Found}", id, result != null);
        return await Task.FromResult(result);
    }

    public async Task<IEnumerable<GenerationResult>> GetAllAsync()
    {
        _logger.LogInformation("Retrieved all generation results: {Count} total", _results.Count);
        return await Task.FromResult(_results.AsEnumerable());
    }

    public async Task<IEnumerable<GenerationResult>> GetByEntityAsync(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            return [];

        var results = _results.Where(r => r.EntityName.Equals(entityName, StringComparison.OrdinalIgnoreCase)).ToList();
        _logger.LogInformation("Retrieved results for entity '{EntityName}': {Count} found", entityName, results.Count);
        return await Task.FromResult(results);
    }

    public async Task<IEnumerable<GenerationResult>> GetByStatusAsync(GenerationStatus status)
    {
        var results = _results.Where(r => r.Status == status).ToList();
        _logger.LogInformation("Retrieved results by status '{Status}': {Count} found", status, results.Count);
        return await Task.FromResult(results);
    }

    public async Task<GenerationResult> AddAsync(GenerationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        if (string.IsNullOrWhiteSpace(result.Id))
            result.Id = Guid.NewGuid().ToString();

        _results.Add(result);
        _logger.LogInformation("Added generation result: {ResultId} - {EntityName} ({Status})", result.Id, result.EntityName, result.Status);
        return await Task.FromResult(result);
    }

    public async Task<GenerationResult> UpdateAsync(GenerationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        var existing = _results.FirstOrDefault(r => r.Id == result.Id);
        if (existing == null)
            throw new InvalidOperationException($"Generation result with ID {result.Id} not found");

        _results.Remove(existing);
        _results.Add(result);

        _logger.LogInformation("Updated generation result: {ResultId} - Status: {Status}", result.Id, result.Status);
        return await Task.FromResult(result);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var result = _results.FirstOrDefault(r => r.Id == id);
        if (result == null)
        {
            _logger.LogWarning("Delete attempted for non-existent result: {ResultId}", id);
            return false;
        }

        var removed = _results.Remove(result);
        if (removed)
            _logger.LogInformation("Deleted generation result: {ResultId}", id);

        return await Task.FromResult(removed);
    }

    public async Task<int> CountAsync()
    {
        var count = _results.Count;
        _logger.LogInformation("Generation result count: {Count}", count);
        return await Task.FromResult(count);
    }

    public async Task<int> CountByStatusAsync(GenerationStatus status)
    {
        var count = _results.Count(r => r.Status == status);
        _logger.LogInformation("Generation result count by status '{Status}': {Count}", status, count);
        return await Task.FromResult(count);
    }
}
