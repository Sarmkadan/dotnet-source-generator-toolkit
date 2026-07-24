#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Template-method base class shared by the repository, mapper, serializer, and
/// validator generator services. Centralizes attribute/structural validation,
/// diagnostic reporting through the shared <see cref="GeneratorDiagnostics"/>
/// range table, and hint-name construction through the shared
/// <see cref="GeneratorHintNameFormatter"/>, so the four sibling services can no
/// longer drift on null-handling, diagnostic IDs, or emitted file naming.
/// </summary>
public abstract class GeneratorServiceBase : IGeneratorService
{
    /// <summary>
    /// The generator type this service implements, used to select its
    /// diagnostic-ID range and its conventional hint-name subdirectory/suffix.
    /// </summary>
    protected abstract GeneratorType GeneratorKind { get; }

    /// <summary>The logger used by this generator service.</summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Initializes the base generator service.
    /// </summary>
    /// <param name="logger">The logger to use for generation diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    protected GeneratorServiceBase(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entityModel"/> is null.</exception>
    public GenerationResult Analyze(Entity entityModel)
    {
        ArgumentNullException.ThrowIfNull(entityModel);

        var result = new GenerationResult
        {
            EntityName = entityModel.Name,
            GeneratorType = GeneratorKind,
            Status = GenerationStatus.InProgress,
        };

        var diagnostics = GeneratorDiagnostics.ValidateEntityModel(GeneratorKind, entityModel).ToList();
        if (diagnostics.Count > 0)
        {
            foreach (var diagnostic in diagnostics)
                result.AddError(diagnostic);

            result.MarkAsCompleted(GenerationStatus.Failed, 0);
            return result;
        }

        result.OutputFilePath = BuildHintName(entityModel);
        result.MarkAsCompleted(GenerationStatus.Completed, 0);
        return result;
    }

    /// <summary>
    /// Builds the emitted hint name (relative output path) for the given entity,
    /// using the shared <see cref="GeneratorHintNameFormatter"/> and this
    /// service's conventional naming for <see cref="GeneratorType"/>. Override
    /// to supply a <paramref name="variant"/> qualifier, e.g. a serializer format.
    /// </summary>
    /// <param name="entity">The entity model to build a hint name for.</param>
    /// <param name="variant">An optional variant qualifier inserted before the suffix.</param>
    /// <returns>The formatted relative output path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    protected virtual string BuildHintName(Entity entity, string? variant = null)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return GeneratorHintNameFormatter.Format(GeneratorKind, entity.Name, variant);
    }

    /// <summary>
    /// Runs the shared structural validation rules against the entity and records
    /// every violation on <paramref name="result"/> as an error, using this
    /// service's diagnostic-ID range. Returns whether the entity is valid.
    /// </summary>
    /// <param name="entity">The entity model to validate.</param>
    /// <param name="result">The generation result to record diagnostics on.</param>
    /// <returns><see langword="true"/> when no structural violations were found; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> or <paramref name="result"/> is null.</exception>
    protected bool ValidateOrReport(Entity entity, GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(result);

        var diagnostics = GeneratorDiagnostics.ValidateEntityModel(GeneratorKind, entity).ToList();
        foreach (var diagnostic in diagnostics)
            result.AddError(diagnostic);

        return diagnostics.Count == 0;
    }

    /// <summary>
    /// Records an unhandled generation failure on <paramref name="result"/> using
    /// the shared "generation failed" diagnostic format for this service's
    /// diagnostic-ID range, and marks the result as failed.
    /// </summary>
    /// <param name="result">The generation result to record the failure on.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> or <paramref name="exception"/> is null.</exception>
    protected void ReportGenerationFailure(GenerationResult result, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(exception);

        result.AddError(GeneratorDiagnostics.FormatGenerationFailed(GeneratorKind, exception.Message));
        result.MarkAsCompleted(GenerationStatus.Failed, 0);
    }
}
