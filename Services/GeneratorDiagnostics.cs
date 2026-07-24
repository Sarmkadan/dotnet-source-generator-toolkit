#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Central diagnostic-ID range table shared by every <see cref="IGeneratorService"/>
/// implementation. Each <see cref="GeneratorType"/> owns a contiguous, non-overlapping
/// block of one hundred codes so that diagnostics raised by any of the four generator
/// services can never collide, and so that a code's numeric range alone identifies
/// which generator produced it.
/// </summary>
public static class GeneratorDiagnostics
{
    /// <summary>Diagnostic-ID range reserved for the repository generator.</summary>
    public const int RepositoryRangeStart = 1000;

    /// <summary>Diagnostic-ID range reserved for the mapper generator.</summary>
    public const int MapperRangeStart = 1100;

    /// <summary>Diagnostic-ID range reserved for the serializer generator.</summary>
    public const int SerializerRangeStart = 1200;

    /// <summary>Diagnostic-ID range reserved for the validator generator.</summary>
    public const int ValidatorRangeStart = 1300;

    private const int NameRequiredOffset = 1;
    private const int NamespaceRequiredOffset = 2;
    private const int NoPropertiesOffset = 3;
    private const int DuplicatePropertyOffset = 4;
    private const int GenerationFailedOffset = 99;

    /// <summary>
    /// Resolves the diagnostic-ID range start reserved for the given generator type.
    /// </summary>
    /// <param name="generatorType">The generator type to resolve a range for.</param>
    /// <returns>The first numeric code owned by <paramref name="generatorType"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="generatorType"/> has no reserved range.</exception>
    public static int RangeStartFor(GeneratorType generatorType) => generatorType switch
    {
        GeneratorType.Repository => RepositoryRangeStart,
        GeneratorType.Mapper => MapperRangeStart,
        GeneratorType.Serializer => SerializerRangeStart,
        GeneratorType.Validator => ValidatorRangeStart,
        _ => throw new ArgumentOutOfRangeException(nameof(generatorType), generatorType, "No diagnostic-ID range is reserved for this generator type."),
    };

    /// <summary>
    /// Formats a stable diagnostic ID of the form <c>DNSGTnnnn</c> for the given
    /// generator type and offset within its reserved range.
    /// </summary>
    /// <param name="generatorType">The owning generator type.</param>
    /// <param name="offset">The zero-based offset within the generator's reserved range (0-99).</param>
    /// <returns>The formatted diagnostic ID, e.g. <c>DNSGT1001</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="generatorType"/> has no reserved range, or when
    /// <paramref name="offset"/> falls outside 0-99.
    /// </exception>
    public static string FormatId(GeneratorType generatorType, int offset)
    {
        if (offset is < 0 or > 99)
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must fall within a generator's 100-code range (0-99).");

        return $"DNSGT{RangeStartFor(generatorType) + offset:D4}";
    }

    /// <summary>
    /// Runs the shared structural validation rules against an entity model,
    /// yielding diagnostic messages (each prefixed with its diagnostic ID) for
    /// every rule violated. An empty sequence means the entity model is valid.
    /// </summary>
    /// <param name="generatorType">The generator type performing the validation, used to select its diagnostic-ID range.</param>
    /// <param name="entity">The entity model to validate.</param>
    /// <returns>A sequence of diagnostic messages, prefixed with their diagnostic ID.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    public static IEnumerable<string> ValidateEntityModel(GeneratorType generatorType, Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (string.IsNullOrWhiteSpace(entity.Name))
            yield return $"{FormatId(generatorType, NameRequiredOffset)}: Entity name is required.";

        if (string.IsNullOrWhiteSpace(entity.Namespace))
            yield return $"{FormatId(generatorType, NamespaceRequiredOffset)}: Entity namespace is required.";

        if (entity.Properties.Count == 0)
            yield return $"{FormatId(generatorType, NoPropertiesOffset)}: Entity '{entity.Name}' must have at least one property.";

        foreach (var group in entity.Properties.GroupBy(p => p.Name).Where(g => g.Count() > 1))
            yield return $"{FormatId(generatorType, DuplicatePropertyOffset)}: Duplicate property name '{group.Key}' on entity '{entity.Name}'.";
    }

    /// <summary>
    /// Formats the shared "generation failed" diagnostic message for an unhandled
    /// exception raised while generating code for an entity.
    /// </summary>
    /// <param name="generatorType">The generator type reporting the failure.</param>
    /// <param name="exceptionMessage">The exception message to embed in the diagnostic.</param>
    /// <returns>The formatted diagnostic message, prefixed with its diagnostic ID.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="exceptionMessage"/> is null or empty.</exception>
    public static string FormatGenerationFailed(GeneratorType generatorType, string exceptionMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(exceptionMessage);
        return $"{FormatId(generatorType, GenerationFailedOffset)}: {exceptionMessage}";
    }
}
