#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetSourceGeneratorToolkit.Pipeline;

/// <summary>
/// Provides validation helpers for <see cref="GenerationPipeline"/> instances.
/// Validates pipeline state including execution results, error messages, and timestamps.
/// </summary>
public static class GenerationPipelineValidation
{
    /// <summary>
    /// Validates the generation pipeline instance for common issues and edge cases.
    /// </summary>
    /// <param name="value">The pipeline instance to validate</param>
    /// <returns>A list of human-readable validation problems; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this GenerationPipeline value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate execution state consistency
        if (!value.IsSuccessful && string.IsNullOrEmpty(value.ErrorMessage))
        {
            errors.Add("Pipeline execution failed but ErrorMessage is empty.");
        }

        if (value.IsSuccessful && !string.IsNullOrEmpty(value.ErrorMessage))
        {
            errors.Add("Pipeline execution succeeded but ErrorMessage is not empty.");
        }

        // Validate entity and file counts
        if (value.EntitiesFound < 0)
        {
            errors.Add("EntitiesFound cannot be negative.");
        }

        if (value.GeneratedFiles < 0)
        {
            errors.Add("GeneratedFiles cannot be negative.");
        }

        if (value.FilesWritten < 0)
        {
            errors.Add("FilesWritten cannot be negative.");
        }

        // Validate file count relationships
        if (value.FilesWritten > value.GeneratedFiles)
        {
            errors.Add("FilesWritten cannot exceed GeneratedFiles.");
        }

        if (value.EntitiesFound > 0 && value.GeneratedFiles > value.EntitiesFound * 1000)
        {
            errors.Add("GeneratedFiles exceeds reasonable threshold compared to EntitiesFound.");
        }

        // Validate timestamp
        if (value.ExecutedAt == default)
        {
            errors.Add("ExecutedAt must be set to a valid DateTime.");
        }
        else if (value.ExecutedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("ExecutedAt is in the future.");
        }
        else if (value.ExecutedAt < DateTime.UtcNow.AddYears(-1))
        {
            errors.Add("ExecutedAt is more than one year in the past.");
        }

        // Validate error message format if present
        if (!string.IsNullOrEmpty(value.ErrorMessage))
        {
            if (value.ErrorMessage.Length > 1000)
            {
                errors.Add("ErrorMessage exceeds maximum length of 1000 characters.");
            }

            if (value.ErrorMessage.Contains("\r\n") && value.ErrorMessage.Contains('\n') && !value.ErrorMessage.Contains("\r\n"))
            {
                errors.Add("ErrorMessage contains inconsistent line endings.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the generation pipeline instance is in a valid state.
    /// </summary>
    /// <param name="value">The pipeline instance to check</param>
    /// <returns>True if the pipeline is valid; otherwise false</returns>
    public static bool IsValid(this GenerationPipeline value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the generation pipeline instance is in a valid state.
    /// Throws an <see cref="ArgumentException"/> if validation fails, listing all problems.
    /// </summary>
    /// <param name="value">The pipeline instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if validation fails, containing all error messages</exception>
    public static void EnsureValid(this GenerationPipeline value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"GenerationPipeline validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}