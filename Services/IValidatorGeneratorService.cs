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
/// Generates validation logic for entities.
/// </summary>
public interface IValidatorGeneratorService
{
    /// <summary>
    /// Generates validators for all entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync(List<Entity> entities);

    /// <summary>
    /// Generates a FluentValidation validator for an entity.
    /// </summary>
    Task<GenerationResult> GenerateValidatorAsync(Entity entity);
}
