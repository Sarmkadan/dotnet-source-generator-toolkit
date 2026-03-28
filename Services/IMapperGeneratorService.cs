// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Contract for generating mapping code between entities and DTOs.
/// Creates bidirectional mappers with null handling.
/// </summary>
public interface IMapperGeneratorService
{
    /// <summary>
    /// Generate mapper for a single entity.
    /// </summary>
    Task<GenerationResult> GenerateMapperAsync(Entity entity);

    /// <summary>
    /// Generate mappers for multiple entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllMappersAsync(IEnumerable<Entity> entities);

    /// <summary>
    /// Generate DTO class for entity.
    /// </summary>
    Task<string> GenerateDtoAsync(Entity entity);

    /// <summary>
    /// Generate mapping methods only.
    /// </summary>
    Task<string> GenerateMappingMethodsAsync(Entity entity);
}
