// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Analyzes C# source files to extract entity definitions and metadata.
/// </summary>
public interface IEntityAnalyzer
{
    /// <summary>
    /// Analyzes a source file and extracts entity definitions.
    /// </summary>
    Task<IEnumerable<Entity>> AnalyzeFileAsync(string filePath, string content);

    /// <summary>
    /// Parses a C# class definition into an Entity object.
    /// </summary>
    Task<Entity> ParseClassAsync(string className, string classContent, string fileNamespace);
}

/// <summary>
/// Analyzes attributes decorating entities and properties.
/// </summary>
public interface IAttributeAnalyzer
{
    /// <summary>
    /// Extracts all attributes from entity definition.
    /// </summary>
    IEnumerable<string> ExtractEntityAttributes(string classDefinition);

    /// <summary>
    /// Extracts attributes from a property definition.
    /// </summary>
    IEnumerable<string> ExtractPropertyAttributes(string propertyDefinition);

    /// <summary>
    /// Checks if an attribute specifies repository generation.
    /// </summary>
    bool HasRepositoryAttribute(IEnumerable<string> attributes);

    /// <summary>
    /// Checks if an attribute specifies mapper generation.
    /// </summary>
    bool HasMapperAttribute(IEnumerable<string> attributes);

    /// <summary>
    /// Checks if an attribute specifies validator generation.
    /// </summary>
    bool HasValidatorAttribute(IEnumerable<string> attributes);
}
