#nullable enable

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

