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
/// Generates mapper implementations for entity transformations.
/// </summary>
public interface IMapperGeneratorService
{
    /// <summary>
    /// Generates all mappers for a set of entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllMappersAsync(List<Entity> entities);

    /// <summary>
    /// Generates a mapper from one entity to another.
    /// </summary>
    Task<GenerationResult> GenerateMapperAsync(Entity sourceEntity, Entity targetEntity);
}
