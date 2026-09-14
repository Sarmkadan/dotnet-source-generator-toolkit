#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Exceptions;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Manages application configuration with support for default values,
/// environment variables, and configuration file overrides.
/// </summary>
public sealed class ConfigurationManager : IConfigurationManager
{
    private readonly Dictionary<string, string> _config;
    private readonly ILogger<ConfigurationManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging configuration operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <see langword="null"/>.</exception>
    public ConfigurationManager(ILogger<ConfigurationManager> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _config = new Dictionary<string, string>();
        LoadDefaultConfiguration();
    }

    /// <summary>
    /// Gets the configuration value for the specified key, or an empty string if not found.
    /// </summary>
    /// <param name="key">The configuration key to look up.</param>
    /// <returns>The configuration value, or an empty string if the key is not present.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is whitespace.</exception>
    /// <exception cref="ConfigurationException">Thrown when an error occurs while reading the configuration value.</exception>
    public string GetValue(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be whitespace", nameof(key));

        return GetValue(key, string.Empty);
    }

    /// <summary>
    /// Gets the configuration value for the specified key, falling back to a default value when not found.
    /// </summary>
    /// <param name="key">The configuration key to look up.</param>
    /// <param name="defaultValue">The value to return when the key is not present.</param>
    /// <returns>The configuration value, or <paramref name="defaultValue"/> if the key is not present.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is whitespace.</exception>
    /// <exception cref="ConfigurationException">Thrown when an error occurs while reading the configuration value.</exception>
    public string GetValue(string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be whitespace", nameof(key));

        try
        {
            if (_config.TryGetValue(key, out var value))
                return value;

            // Try environment variable
            var envValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envValue))
                return envValue;

            _logger.LogDebug("Configuration key not found, using default: {Key}", key);
            return defaultValue ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading configuration value for key: {Key}", key);
            throw new ConfigurationException($"Error reading configuration value for key '{key}'", ex);
        }
    }

    /// <summary>
    /// Sets the configuration value for the specified key.
    /// </summary>
    /// <param name="key">The configuration key to set.</param>
    /// <param name="value">The value to set for the key.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is whitespace.</exception>
    /// <exception cref="ConfigurationException">Thrown when an error occurs while setting the configuration value.</exception>
    public void SetValue(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be whitespace", nameof(key));

        try
        {
            _config[key] = value ?? string.Empty;
            _logger.LogInformation("Configuration updated: {Key} = {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting configuration value for key: {Key}", key);
            throw new ConfigurationException($"Error setting configuration value for key '{key}'", ex);
        }
    }

    /// <summary>
    /// Determines whether the specified configuration key exists.
    /// </summary>
    /// <param name="key">The configuration key to check.</param>
    /// <returns><see langword="true"/> if the key exists in configuration or environment variables; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ConfigurationException">Thrown when an error occurs while checking the configuration key.</exception>
    public bool HasKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        try
        {
            return _config.ContainsKey(key) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking configuration key existence: {Key}", key);
            throw new ConfigurationException($"Error checking configuration key existence for '{key}'", ex);
        }
    }

    /// <summary>
    /// Gets the output directory for generated files.
    /// </summary>
    /// <returns>The absolute path to the output directory.</returns>
    /// <exception cref="ConfigurationException">Thrown when the output directory is not configured or an error occurs while resolving it.</exception>
    public string GetOutputDirectory()
    {
        try
        {
            var outputDir = GetValue("OutputDirectory", "./Generated");
            if (string.IsNullOrWhiteSpace(outputDir))
                throw new ConfigurationException("OutputDirectory configuration value is null or empty");

            return Path.IsPathRooted(outputDir) ? outputDir : Path.Combine(GetProjectRoot(), outputDir);
        }
        catch (Exception ex) when (ex is not ConfigurationException)
        {
            _logger.LogError(ex, "Error getting output directory from configuration");
            throw new ConfigurationException("Failed to get output directory from configuration", ex);
        }
    }

    /// <summary>
    /// Gets the template directory for source templates.
    /// </summary>
    /// <returns>The absolute path to the template directory.</returns>
    /// <exception cref="ConfigurationException">Thrown when the template directory is not configured or an error occurs while resolving it.</exception>
    public string GetTemplateDirectory()
    {
        try
        {
            var templateDir = GetValue("TemplateDirectory", "./Templates");
            if (string.IsNullOrWhiteSpace(templateDir))
                throw new ConfigurationException("TemplateDirectory configuration value is null or empty");

            return Path.IsPathRooted(templateDir) ? templateDir : Path.Combine(GetProjectRoot(), templateDir);
        }
        catch (Exception ex) when (ex is not ConfigurationException)
        {
            _logger.LogError(ex, "Error getting template directory from configuration");
            throw new ConfigurationException("Failed to get template directory from configuration", ex);
        }
    }

    /// <summary>
    /// Gets the project root directory.
    /// </summary>
    /// <returns>The absolute path to the project root directory.</returns>
    /// <exception cref="ConfigurationException">Thrown when the project root is not configured or an error occurs while resolving it.</exception>
    public string GetProjectRoot()
    {
        try
        {
            var projectRoot = GetValue("ProjectRoot", Directory.GetCurrentDirectory());
            if (string.IsNullOrWhiteSpace(projectRoot))
                throw new ConfigurationException("ProjectRoot configuration value is null or empty");

            return projectRoot;
        }
        catch (Exception ex) when (ex is not ConfigurationException)
        {
            _logger.LogError(ex, "Error getting project root from configuration");
            throw new ConfigurationException("Failed to get project root from configuration", ex);
        }
    }

    /// <summary>
    /// Gets all configuration values as a read-only dictionary.
    /// </summary>
    /// <returns>A read-only dictionary containing all configuration key-value pairs.</returns>
    /// <exception cref="ConfigurationException">Thrown when an error occurs while retrieving the configuration.</exception>
    public IReadOnlyDictionary<string, string> GetAllConfig()
    {
        try
        {
            return new Dictionary<string, string>(_config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all configuration");
            throw new ConfigurationException("Failed to get all configuration", ex);
        }
    }

    private void LoadDefaultConfiguration()
    {
        try
        {
            _config["OutputDirectory"] = "Generated";
            _config["TemplateDirectory"] = "Templates";
            _config["ProjectRoot"] = Directory.GetCurrentDirectory();
            _config["Language"] = "CSharp";
            _config["Version"] = "1.0.0";
            _config["MaxRetries"] = "3";
            _config["TimeoutSeconds"] = "30";
            _config["EnableLogging"] = "true";
            _config["EnableValidation"] = "true";

            _logger.LogInformation("Default configuration loaded with {Count} entries", _config.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading default configuration");
            throw new ConfigurationException("Failed to load default configuration", ex);
        }
    }
}