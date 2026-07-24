#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Formats the emitted hint name (relative output path) for generated code, shared
/// by every <see cref="IGeneratorService"/> implementation so all four generators
/// name their outputs the same way: <c>{Subdirectory}/{EntityName}{Variant}{Suffix}.cs</c>.
/// </summary>
public static class GeneratorHintNameFormatter
{
    /// <summary>
    /// Formats a hint name for a piece of generated code.
    /// </summary>
    /// <param name="subdirectory">The output subdirectory the file belongs to, e.g. <c>"Repositories"</c>.</param>
    /// <param name="entityName">The name of the entity the code was generated for.</param>
    /// <param name="suffix">The generator-specific suffix, e.g. <c>"Repository"</c>, <c>"Mapper"</c>, <c>"Validator"</c>.</param>
    /// <param name="variant">
    /// An optional variant qualifier inserted between the entity name and the suffix,
    /// e.g. a serializer format such as <c>"Json"</c>. Pass null or empty to omit it.
    /// </param>
    /// <returns>The formatted relative output path, using <see cref="Path.Combine(string, string)"/> semantics.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="subdirectory"/>, <paramref name="entityName"/>, or <paramref name="suffix"/> is null or empty.</exception>
    public static string Format(string subdirectory, string entityName, string suffix, string? variant = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(subdirectory);
        ArgumentException.ThrowIfNullOrEmpty(entityName);
        ArgumentException.ThrowIfNullOrEmpty(suffix);

        var fileName = string.IsNullOrEmpty(variant)
            ? $"{entityName}{suffix}.cs"
            : $"{entityName}{variant}{suffix}.cs";

        return Path.Combine(subdirectory, fileName);
    }

    /// <summary>
    /// Formats a hint name for a piece of generated code, deriving the subdirectory
    /// and suffix from the given generator type's conventional naming.
    /// </summary>
    /// <param name="generatorType">The generator type producing the code.</param>
    /// <param name="entityName">The name of the entity the code was generated for.</param>
    /// <param name="variant">An optional variant qualifier, e.g. a serializer format.</param>
    /// <returns>The formatted relative output path.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entityName"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="generatorType"/> has no known naming convention.</exception>
    public static string Format(GeneratorType generatorType, string entityName, string? variant = null)
    {
        var (subdirectory, suffix) = generatorType switch
        {
            GeneratorType.Repository => ("Repositories", "Repository"),
            GeneratorType.Mapper => ("Mappers", "Mapper"),
            GeneratorType.Serializer => ("Serializers", "Serializer"),
            GeneratorType.Validator => ("Validators", "Validator"),
            _ => throw new ArgumentOutOfRangeException(nameof(generatorType), generatorType, "No hint-name convention is defined for this generator type."),
        };

        return Format(subdirectory, entityName, suffix, variant);
    }
}
