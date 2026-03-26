// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Loads and merges configuration from JSON files and environment.
/// Provides sensible defaults and command-line override support.
/// </summary>
public class ConfigurationLoader
{
    private readonly ILogger<ConfigurationLoader> _logger;
    private readonly IConfigurationValidator _validator;

    public ConfigurationLoader(ILogger<ConfigurationLoader> logger, IConfigurationValidator validator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Load configuration from file with fallback to defaults.
    /// </summary>
    public async Task<ToolkitOptions> LoadAsync(string? configPath = null)
    {
        var options = _validator.GetDefaults();

        if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
        {
            try
            {
                _logger.LogInformation("Loading configuration from: {ConfigPath}", configPath);
                var json = await File.ReadAllTextAsync(configPath);
                var loadedOptions = JsonSerializer.Deserialize<ToolkitOptions>(json);

                if (loadedOptions != null)
                {
                    options = MergeOptions(options, loadedOptions);
                    _logger.LogInformation("Configuration loaded successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load configuration file, using defaults");
            }
        }

        // Validate loaded configuration
        var validation = _validator.Validate(options);
        if (!validation.IsValid)
        {
            _logger.LogError("Configuration validation failed:");
            foreach (var error in validation.Errors)
            {
                _logger.LogError("  - {Error}", error);
            }

            throw new InvalidOperationException("Configuration is invalid");
        }

        if (validation.Warnings.Count > 0)
        {
            foreach (var warning in validation.Warnings)
            {
                _logger.LogWarning("  - {Warning}", warning);
            }
        }

        return options;
    }

    /// <summary>
    /// Save configuration to JSON file.
    /// </summary>
    public async Task SaveAsync(ToolkitOptions options, string configPath)
    {
        try
        {
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(options, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(configPath, json);

            _logger.LogInformation("Configuration saved to: {ConfigPath}", configPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save configuration to {ConfigPath}", configPath);
            throw;
        }
    }

    /// <summary>
    /// Merge loaded options with defaults, preferring non-default values.
    /// </summary>
    private static ToolkitOptions MergeOptions(ToolkitOptions defaults, ToolkitOptions loaded)
    {
        return new ToolkitOptions
        {
            EnableCaching = loaded.EnableCaching ?? defaults.EnableCaching,
            CacheExpirationMinutes = loaded.CacheExpirationMinutes > 0 ? loaded.CacheExpirationMinutes : defaults.CacheExpirationMinutes,
            EnableCodeFormatting = loaded.EnableCodeFormatting ?? defaults.EnableCodeFormatting,
            CodeFormattingLineLength = loaded.CodeFormattingLineLength > 0 ? loaded.CodeFormattingLineLength : defaults.CodeFormattingLineLength,
            VerboseLogging = loaded.VerboseLogging ?? defaults.VerboseLogging,
            MaxDegreeOfParallelism = loaded.MaxDegreeOfParallelism > 0 ? loaded.MaxDegreeOfParallelism : defaults.MaxDegreeOfParallelism,
            OperationTimeoutSeconds = loaded.OperationTimeoutSeconds > 0 ? loaded.OperationTimeoutSeconds : defaults.OperationTimeoutSeconds,
            GenerateDtos = loaded.GenerateDtos ?? defaults.GenerateDtos,
            DefaultNamespace = loaded.DefaultNamespace ?? defaults.DefaultNamespace,
            OutputDirectory = !string.IsNullOrEmpty(loaded.OutputDirectory) ? loaded.OutputDirectory : defaults.OutputDirectory,
            BackupExistingFiles = loaded.BackupExistingFiles ?? defaults.BackupExistingFiles,
            GenerateInterfaces = loaded.GenerateInterfaces ?? defaults.GenerateInterfaces,
            GenerateXmlComments = loaded.GenerateXmlComments ?? defaults.GenerateXmlComments,
        };
    }
}
