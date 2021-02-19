#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Repositories;

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
