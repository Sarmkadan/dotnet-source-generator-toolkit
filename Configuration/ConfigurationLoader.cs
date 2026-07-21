#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Loads and merges configuration from JSON files and environment.
/// Provides sensible defaults and command-line override support.
/// </summary>
public sealed class ConfigurationLoader
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

                if (loadedOptions is not null)
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
                _logger.LogError(" - {Error}", error);
            }

            throw new InvalidOperationException("Configuration is invalid");
        }

        if (validation.Warnings.Count > 0)
        {
            foreach (var warning in validation.Warnings)
            {
                _logger.LogWarning(" - {Warning}", warning);
            }
        }

        return options;
    }

    /// <summary>
    /// Load configuration from .env-style file with fallback to defaults.
    /// Supports comments (#), blank lines, and key=value pairs.
    /// </summary>
    public async Task<ToolkitOptions> LoadEnvAsync(string? envPath = null)
    {
        var options = _validator.GetDefaults();

        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
        {
            try
            {
                _logger.LogInformation("Loading .env configuration from: {EnvPath}", envPath);
                var envContent = await File.ReadAllTextAsync(envPath);
                var envOptions = ParseEnvFile(envContent);

                if (envOptions is not null)
                {
                    options = MergeEnvOptions(options, envOptions);
                    _logger.LogInformation(".env configuration loaded successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load .env configuration file, using defaults");
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
            EnableCaching = loaded.EnableCaching,
            CacheExpirationMinutes = loaded.CacheExpirationMinutes > 0 ? loaded.CacheExpirationMinutes : defaults.CacheExpirationMinutes,
            EnableCodeFormatting = loaded.EnableCodeFormatting,
            CodeFormattingLineLength = loaded.CodeFormattingLineLength > 0 ? loaded.CodeFormattingLineLength : defaults.CodeFormattingLineLength,
            VerboseLogging = loaded.VerboseLogging,
            MaxDegreeOfParallelism = loaded.MaxDegreeOfParallelism > 0 ? loaded.MaxDegreeOfParallelism : defaults.MaxDegreeOfParallelism,
            OperationTimeoutSeconds = loaded.OperationTimeoutSeconds > 0 ? loaded.OperationTimeoutSeconds : defaults.OperationTimeoutSeconds,
            GenerateDtos = loaded.GenerateDtos,
            DefaultNamespace = loaded.DefaultNamespace ?? defaults.DefaultNamespace,
            OutputDirectory = !string.IsNullOrEmpty(loaded.OutputDirectory) ? loaded.OutputDirectory : defaults.OutputDirectory,
            BackupExistingFiles = loaded.BackupExistingFiles,
            GenerateInterfaces = loaded.GenerateInterfaces,
            GenerateXmlComments = loaded.GenerateXmlComments,
        };
    }

    /// <summary>
    /// Parse .env-style file content into ToolkitOptions.
    /// Supports:
    /// - Comments (lines starting with #)
    /// - Blank lines
    /// - Key=value pairs
    /// - Whitespace trimming
    /// </summary>
    private static ToolkitOptions? ParseEnvFile(string envContent)
    {
        if (string.IsNullOrWhiteSpace(envContent))
        {
            return null;
        }

        var options = new ToolkitOptions();
        var lines = envContent.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        var hasValidSettings = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip blank lines and comments
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue; // Invalid line format
            }

            var key = trimmedLine.Substring(0, separatorIndex).Trim();
            var value = trimmedLine.Substring(separatorIndex + 1).Trim();

            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            // Parse key-value pairs
            SetOptionFromEnv(options, key, value);
            hasValidSettings = true;
        }

        return hasValidSettings ? options : null;
    }

    /// <summary>
    /// Set ToolkitOptions property from .env key-value pair.
    /// Supports both PascalCase and UPPER_CASE_WITH_UNDERSCORES naming conventions.
    /// </summary>
    private static void SetOptionFromEnv(ToolkitOptions options, string key, string value)
    {
        try
        {
            var normalizedKey = key
                .Replace("_", "")
                .Replace("-", "")
                .ToLowerInvariant();

            switch (normalizedKey)
            {
                case "enablecaching":
                    if (bool.TryParse(value, out var enableCaching))
                    {
                        options.EnableCaching = enableCaching;
                    }
                    break;

                case "cacheexpirationminutes":
                case "cacheexpiration":
                    if (int.TryParse(value, out var cacheExpiration))
                    {
                        options.CacheExpirationMinutes = cacheExpiration;
                    }
                    break;

                case "enablecodeformatting":
                case "codeformatting":
                    if (bool.TryParse(value, out var enableCodeFormatting))
                    {
                        options.EnableCodeFormatting = enableCodeFormatting;
                    }
                    break;

                case "codeformattinglinelength":
                case "linelength":
                    if (int.TryParse(value, out var lineLength))
                    {
                        options.CodeFormattingLineLength = lineLength;
                    }
                    break;

                case "verboselogging":
                case "verbose":
                    if (bool.TryParse(value, out var verboseLogging))
                    {
                        options.VerboseLogging = verboseLogging;
                    }
                    break;

                case "maxdegreeofparallelism":
                case "parallelism":
                    if (int.TryParse(value, out var maxDegree))
                    {
                        options.MaxDegreeOfParallelism = maxDegree;
                    }
                    break;

                case "operationtimeoutseconds":
                case "timeout":
                    if (int.TryParse(value, out var timeout))
                    {
                        options.OperationTimeoutSeconds = timeout;
                    }
                    break;

                case "generatedtos":
                case "dtos":
                    if (bool.TryParse(value, out var generateDtos))
                    {
                        options.GenerateDtos = generateDtos;
                    }
                    break;

                case "defaultnamespace":
                case "namespace":
                    options.DefaultNamespace = value.Trim('"').Trim();
                    break;

                case "outputdirectory":
                case "outputdir":
                    options.OutputDirectory = value.Trim('"').Trim();
                    break;

                case "backupexistingfiles":
                case "backup":
                    if (bool.TryParse(value, out var backupExisting))
                    {
                        options.BackupExistingFiles = backupExisting;
                    }
                    break;

                case "generateinterfaces":
                case "interfaces":
                    if (bool.TryParse(value, out var generateInterfaces))
                    {
                        options.GenerateInterfaces = generateInterfaces;
                    }
                    break;

                case "generatexmlcomments":
                case "xmlcomments":
                    if (bool.TryParse(value, out var generateXmlComments))
                    {
                        options.GenerateXmlComments = generateXmlComments;
                    }
                    break;
            }
        }
        catch
        {
            // Silently ignore parsing errors to allow graceful fallback
        }
    }

    /// <summary>
    /// Merge .env options with existing options, preferring .env values.
    /// </summary>
    private static ToolkitOptions MergeEnvOptions(ToolkitOptions defaults, ToolkitOptions envOptions)
    {
        return new ToolkitOptions
        {
            EnableCaching = envOptions.EnableCaching,
            CacheExpirationMinutes = envOptions.CacheExpirationMinutes > 0 ? envOptions.CacheExpirationMinutes : defaults.CacheExpirationMinutes,
            EnableCodeFormatting = envOptions.EnableCodeFormatting,
            CodeFormattingLineLength = envOptions.CodeFormattingLineLength > 0 ? envOptions.CodeFormattingLineLength : defaults.CodeFormattingLineLength,
            VerboseLogging = envOptions.VerboseLogging,
            MaxDegreeOfParallelism = envOptions.MaxDegreeOfParallelism > 0 ? envOptions.MaxDegreeOfParallelism : defaults.MaxDegreeOfParallelism,
            OperationTimeoutSeconds = envOptions.OperationTimeoutSeconds > 0 ? envOptions.OperationTimeoutSeconds : defaults.OperationTimeoutSeconds,
            GenerateDtos = envOptions.GenerateDtos,
            DefaultNamespace = envOptions.DefaultNamespace ?? defaults.DefaultNamespace,
            OutputDirectory = !string.IsNullOrEmpty(envOptions.OutputDirectory) ? envOptions.OutputDirectory : defaults.OutputDirectory,
            BackupExistingFiles = envOptions.BackupExistingFiles,
            GenerateInterfaces = envOptions.GenerateInterfaces,
            GenerateXmlComments = envOptions.GenerateXmlComments,
        };
    }
}