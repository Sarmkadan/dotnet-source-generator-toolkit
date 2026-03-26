// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Validates ToolkitOptions and provides sensible defaults.
/// Ensures configuration is safe and valid before toolkit execution.
/// </summary>
public class ConfigurationValidator : IConfigurationValidator
{
    public ValidationResult Validate(ToolkitOptions options)
    {
        var result = new ValidationResult();

        if (options == null)
        {
            result.IsValid = false;
            result.AddError("Options cannot be null");
            return result;
        }

        // Validate caching settings
        if (options.CacheExpirationMinutes < 1)
        {
            result.AddError("Cache expiration must be at least 1 minute");
        }

        // Validate parallelism settings
        if (options.MaxDegreeOfParallelism < 1)
        {
            result.AddError("Max degree of parallelism must be at least 1");
        }

        if (options.MaxDegreeOfParallelism > Environment.ProcessorCount * 2)
        {
            result.AddWarning(
                $"Max degree of parallelism ({options.MaxDegreeOfParallelism}) exceeds recommended value ({Environment.ProcessorCount * 2})");
        }

        // Validate timeout settings
        if (options.OperationTimeoutSeconds < 10)
        {
            result.AddError("Operation timeout must be at least 10 seconds");
        }

        if (options.OperationTimeoutSeconds > 3600)
        {
            result.AddWarning("Operation timeout is very high (> 1 hour)");
        }

        // Validate code formatting settings
        if (options.CodeFormattingLineLength < 40)
        {
            result.AddError("Code formatting line length must be at least 40 characters");
        }

        if (options.CodeFormattingLineLength > 500)
        {
            result.AddWarning("Code formatting line length is very high (> 500 characters)");
        }

        // Validate output directory
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            result.AddError("Output directory cannot be empty");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public ToolkitOptions GetDefaults()
    {
        return new ToolkitOptions
        {
            EnableCaching = true,
            CacheExpirationMinutes = 60,
            EnableCodeFormatting = true,
            CodeFormattingLineLength = 100,
            VerboseLogging = false,
            MaxDegreeOfParallelism = Environment.ProcessorCount,
            OperationTimeoutSeconds = 300,
            GenerateDtos = false,
            OutputDirectory = "./Generated",
            BackupExistingFiles = true,
            GenerateInterfaces = true,
            GenerateXmlComments = true,
        };
    }
}
