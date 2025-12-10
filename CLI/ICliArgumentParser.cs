// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.CLI;

/// <summary>
/// Contract for parsing command-line arguments into CliOptions.
/// Provides help text generation and argument validation.
/// </summary>
public interface ICliArgumentParser
{
    /// <summary>
    /// Parse command-line arguments into structured CliOptions.
    /// </summary>
    /// <param name="args">Raw command-line arguments from Main(string[] args)</param>
    /// <returns>Parsed CLI options with defaults applied</returns>
    CliOptions Parse(string[] args);

    /// <summary>
    /// Get formatted help message for command-line usage.
    /// </summary>
    /// <returns>Help text describing all available options</returns>
    string GetHelpMessage();

    /// <summary>
    /// Get version information.
    /// </summary>
    /// <returns>Version string with project details</returns>
    string GetVersionInfo();

    /// <summary>
    /// Validate parsed options for consistency and requirements.
    /// </summary>
    /// <param name="options">Options to validate</param>
    /// <returns>List of validation errors; empty if valid</returns>
    IEnumerable<string> Validate(CliOptions options);
}
