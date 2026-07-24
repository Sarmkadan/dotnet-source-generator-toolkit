#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Common pipeline contract shared by all entity-based code generator services
/// (repository, mapper, serializer, validator). Defines a single entry point
/// that performs attribute/structural validation and, when successful,
/// produces a <see cref="GenerationResult"/> with a hint name assigned through
/// the shared <see cref="GeneratorHintNameFormatter"/>.
/// </summary>
public interface IGeneratorService
{
    /// <summary>
    /// Analyzes the supplied entity model, validating it against the shared
    /// structural rules and producing a <see cref="GenerationResult"/> whose
    /// <see cref="GenerationResult.OutputFilePath"/> was assigned through the
    /// shared hint-name formatter.
    /// </summary>
    /// <param name="entityModel">The entity model to analyze.</param>
    /// <returns>
    /// A <see cref="GenerationResult"/> describing the outcome of the analysis.
    /// The status is <see cref="GenerationStatus.Failed"/> when validation
    /// fails, with diagnostics recorded in <see cref="GenerationResult.Errors"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityModel"/> is null.</exception>
    GenerationResult Analyze(Entity entityModel);
}
