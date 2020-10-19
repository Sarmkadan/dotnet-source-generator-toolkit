// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Contract for generating repository pattern implementations.
/// Creates interfaces and implementations for data access.
/// </summary>
public interface IRepositoryGeneratorService
{
    /// <summary>
    /// Generate repository implementation for a single entity.
    /// </summary>
    Task<GenerationResult> GenerateRepositoryAsync(Entity entity);

    /// <summary>
    /// Generate repositories for multiple entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllRepositoriesAsync(IEnumerable<Entity> entities);

    /// <summary>
    /// Generate repository interface only.
    /// </summary>
    Task<string> GenerateRepositoryInterfaceAsync(Entity entity);

    /// <summary>
    /// Generate repository implementation only.
    /// </summary>
    Task<string> GenerateRepositoryImplementationAsync(Entity entity);
}
