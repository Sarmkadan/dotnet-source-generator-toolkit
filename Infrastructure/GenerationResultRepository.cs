// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Repository for storing and retrieving generation results.
/// Provides querying and filtering capabilities for analysis.
/// </summary>
public interface IGenerationResultRepository
{
    Task AddAsync(GenerationResult result);
    Task<GenerationResult?> GetByIdAsync(string id);
    Task<IEnumerable<GenerationResult>> GetByEntityAsync(string entityName);
    Task<IEnumerable<GenerationResult>> GetByGeneratorAsync(GeneratorType generatorType);
    Task<IEnumerable<GenerationResult>> GetByStatusAsync(GenerationStatus status);
    Task<IEnumerable<GenerationResult>> GetAllAsync();
    Task DeleteAsync(string id);
    Task ClearAsync();
}

/// <summary>
/// In-memory implementation of generation result repository.
/// Suitable for single-session use; extend with persistence for production.
/// </summary>
public class GenerationResultRepository : IGenerationResultRepository
{
    private readonly Dictionary<string, GenerationResult> _results = new();
    private readonly ILogger<GenerationResultRepository> _logger;

    public GenerationResultRepository(ILogger<GenerationResultRepository> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task AddAsync(GenerationResult result)
    {
        if (result == null)
            throw new ArgumentNullException(nameof(result));

        _results[result.Id] = result;
        _logger.LogDebug("Generation result added: {Id}", result.Id);
        await Task.CompletedTask;
    }

    public async Task<GenerationResult?> GetByIdAsync(string id)
    {
        _results.TryGetValue(id, out var result);
        return await Task.FromResult(result);
    }

    public async Task<IEnumerable<GenerationResult>> GetByEntityAsync(string entityName)
    {
        var results = _results.Values.Where(r => r.EntityName == entityName);
        return await Task.FromResult(results);
    }

    public async Task<IEnumerable<GenerationResult>> GetByGeneratorAsync(GeneratorType generatorType)
    {
        var results = _results.Values.Where(r => r.GeneratorType == generatorType);
        return await Task.FromResult(results);
    }

    public async Task<IEnumerable<GenerationResult>> GetByStatusAsync(GenerationStatus status)
    {
        var results = _results.Values.Where(r => r.Status == status);
        return await Task.FromResult(results);
    }

    public async Task<IEnumerable<GenerationResult>> GetAllAsync()
    {
        return await Task.FromResult(_results.Values.ToList());
    }

    public async Task DeleteAsync(string id)
    {
        if (_results.Remove(id))
        {
            _logger.LogDebug("Generation result deleted: {Id}", id);
        }

        await Task.CompletedTask;
    }

    public async Task ClearAsync()
    {
        _results.Clear();
        _logger.LogInformation("All generation results cleared");
        await Task.CompletedTask;
    }
}
