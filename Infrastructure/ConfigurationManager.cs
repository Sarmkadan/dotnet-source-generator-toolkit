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

    public ConfigurationManager(ILogger<ConfigurationManager> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _config = new Dictionary<string, string>();
        LoadDefaultConfiguration();
    }

    public string GetValue(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        return GetValue(key, string.Empty);
    }

    public string GetValue(string key, string defaultValue)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

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

    public void SetValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

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