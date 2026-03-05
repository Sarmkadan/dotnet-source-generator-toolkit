#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Manages application configuration and settings.
/// </summary>
public interface IConfigurationManager
{
    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    string GetValue(string key);

    /// <summary>
    /// Gets a configuration value with a default if not found.
    /// </summary>
    string GetValue(string key, string defaultValue);

    /// <summary>
    /// Sets a configuration value.
    /// </summary>
    void SetValue(string key, string value);

    /// <summary>
    /// Checks if a configuration key exists.
    /// </summary>
    bool HasKey(string key);

    /// <summary>
    /// Gets the output directory for generated files.
    /// </summary>
    string GetOutputDirectory();

    /// <summary>
    /// Gets the template directory path.
    /// </summary>
    string GetTemplateDirectory();

    /// <summary>
    /// Gets the project root directory.
    /// </summary>
    string GetProjectRoot();

    /// <summary>
    /// Gets all configuration as a dictionary.
    /// </summary>
    IReadOnlyDictionary<string, string> GetAllConfig();
}
