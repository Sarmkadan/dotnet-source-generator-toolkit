// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Contract for validating configuration and providing helpful error messages.
/// </summary>
public interface IConfigurationValidator
{
    /// <summary>
    /// Validate toolkit options for correctness.
    /// </summary>
    /// <param name="options">Options to validate</param>
    /// <returns>Validation result with any errors found</returns>
    ValidationResult Validate(ToolkitOptions options);

    /// <summary>
    /// Get default configuration with sensible defaults.
    /// </summary>
    /// <returns>Default toolkit options</returns>
    ToolkitOptions GetDefaults();
}

/// <summary>
/// Result of validation operation.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public void AddError(string message) => Errors.Add(message);
    public void AddWarning(string message) => Warnings.Add(message);
}
