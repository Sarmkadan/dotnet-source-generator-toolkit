// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Manages application configuration with support for default values,
/// environment variables, and configuration file overrides.
/// </summary>
public class ConfigurationManager : IConfigurationManager
{
    private readonly Dictionary<string, string> _config;
    private readonly ILogger<ConfigurationManager> _logger;

    public ConfigurationManager(ILogger<ConfigurationManager> logger)
    {
        _logger = logger;
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

        if (_config.TryGetValue(key, out var value))
            return value;

        // Try environment variable
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(envValue))
            return envValue;

        _logger.LogDebug("Configuration key not found, using default: {Key}", key);
        return defaultValue ?? string.Empty;
    }

    public void SetValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));

        _config[key] = value ?? string.Empty;
        _logger.LogInformation("Configuration updated: {Key} = {Value}", key, value);
    }

    public bool HasKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return _config.ContainsKey(key) || !string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key));
    }

    public string GetOutputDirectory()
    {
        var outputDir = GetValue("OutputDirectory", "./Generated");
        return Path.IsPathRooted(outputDir) ? outputDir : Path.Combine(GetProjectRoot(), outputDir);
    }

    public string GetTemplateDirectory()
    {
        var templateDir = GetValue("TemplateDirectory", "./Templates");
        return Path.IsPathRooted(templateDir) ? templateDir : Path.Combine(GetProjectRoot(), templateDir);
    }

    public string GetProjectRoot()
    {
        return GetValue("ProjectRoot", Directory.GetCurrentDirectory());
    }

    public IReadOnlyDictionary<string, string> GetAllConfig()
    {
        return new Dictionary<string, string>(_config);
    }

    private void LoadDefaultConfiguration()
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
}
