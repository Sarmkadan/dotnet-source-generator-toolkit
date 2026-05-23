#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Generates repository implementations from entity definitions.
/// </summary>
public interface IRepositoryGeneratorService
{
    /// <summary>
    /// Generates a repository for a single entity.
    /// </summary>
    Task<GenerationResult> GenerateRepositoryAsync(Entity entity);

    /// <summary>
    /// Generates repositories for multiple entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllRepositoriesAsync(List<Entity> entities);
}
