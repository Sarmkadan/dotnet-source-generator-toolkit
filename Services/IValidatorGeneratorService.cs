// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Contract for generating validation logic for entities.
/// Creates validators with fluent validation rules.
/// </summary>
public interface IValidatorGeneratorService
{
    /// <summary>
    /// Generate validator for a single entity.
    /// </summary>
    Task<GenerationResult> GenerateValidatorAsync(Entity entity);

    /// <summary>
    /// Generate validators for multiple entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync(IEnumerable<Entity> entities);

    /// <summary>
    /// Generate validation rules for entity properties.
    /// </summary>
    Task<string> GenerateValidationRulesAsync(Entity entity);

    /// <summary>
    /// Generate custom validation attributes.
    /// </summary>
    Task<string> GenerateValidationAttributesAsync(Entity entity);
}
