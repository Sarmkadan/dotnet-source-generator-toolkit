// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Contract for formatting generation results in various output formats.
/// Each formatter handles serialization to a specific format (JSON, CSV, XML, etc).
/// </summary>
public interface IOutputFormatter
{
    /// <summary>
    /// Format generation results into a string representation.
    /// </summary>
    /// <param name="results">Generation results to format</param>
    /// <returns>Formatted output as string</returns>
    string Format(IEnumerable<Domain.GenerationResult> results);

    /// <summary>
    /// Format generation results and write to a file.
    /// </summary>
    /// <param name="results">Generation results to format</param>
    /// <param name="filePath">Destination file path</param>
    /// <returns>Awaitable task</returns>
    Task FormatToFileAsync(IEnumerable<Domain.GenerationResult> results, string filePath);

    /// <summary>
    /// Get the file extension used for this format.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Get human-readable name of the format.
    /// </summary>
    string FormatName { get; }
}
