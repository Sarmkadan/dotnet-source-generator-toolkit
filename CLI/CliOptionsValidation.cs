#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace DotNetSourceGeneratorToolkit.CLI;

/// <summary>
/// Provides validation helpers for <see cref="CliOptions"/> instances.
/// </summary>
public static class CliOptionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="CliOptions"/> instance.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CliOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ProjectPath
        if (string.IsNullOrWhiteSpace(value.ProjectPath))
        {
            problems.Add("ProjectPath cannot be null or whitespace.");
        }
        else if (!Path.IsPathRooted(value.ProjectPath) && !Directory.Exists(value.ProjectPath))
        {
            problems.Add($"ProjectPath '{value.ProjectPath}' does not exist.");
        }

        // Validate OutputPath if specified
        if (!string.IsNullOrWhiteSpace(value.OutputPath))
        {
            if (string.IsNullOrWhiteSpace(value.OutputPath))
            {
                problems.Add("OutputPath cannot be null or whitespace when specified.");
            }
            else if (!Path.IsPathRooted(value.OutputPath) && !Path.GetPathRoot(value.OutputPath)?.StartsWith("\\", StringComparison.Ordinal) != true)
            {
                problems.Add($"OutputPath '{value.OutputPath}' is not a valid absolute path.");
            }
        }

        // Validate GeneratorTypes
        if (value.GeneratorTypes is null)
        {
            problems.Add("GeneratorTypes cannot be null.");
        }
        else
        {
            foreach (var generatorType in value.GeneratorTypes)
            {
                if (string.IsNullOrWhiteSpace(generatorType))
                {
                    problems.Add("GeneratorTypes cannot contain null or whitespace entries.");
                    break;
                }

                var validTypes = new[] { "Repository", "Mapper", "Validator", "Serializer" };
                if (!validTypes.Contains(generatorType, StringComparer.OrdinalIgnoreCase))
                {
                    problems.Add($"GeneratorType '{generatorType}' is not valid. Valid types are: Repository, Mapper, Validator, Serializer.");
                    break;
                }
            }
        }

        // Validate OutputFormat
        if (string.IsNullOrWhiteSpace(value.OutputFormat))
        {
            problems.Add("OutputFormat cannot be null or whitespace.");
        }
        else
        {
            var validFormats = new[] { "Json", "Csv", "Xml", "Text" };
            if (!validFormats.Contains(value.OutputFormat, StringComparer.OrdinalIgnoreCase))
            {
                problems.Add($"OutputFormat '{value.OutputFormat}' is not valid. Valid formats are: Json, Csv, Xml, Text.");
            }
        }

        // Validate DegreeOfParallelism
        if (value.DegreeOfParallelism < 1)
        {
            problems.Add("DegreeOfParallelism must be at least 1.");
        }
        else if (value.DegreeOfParallelism > Environment.ProcessorCount * 4)
        {
            problems.Add($"DegreeOfParallelism {value.DegreeOfParallelism} exceeds reasonable maximum of {Environment.ProcessorCount * 4}.");
        }

        // Validate NamespaceOverride if specified
        if (!string.IsNullOrWhiteSpace(value.NamespaceOverride))
        {
            if (string.IsNullOrWhiteSpace(value.NamespaceOverride))
            {
                problems.Add("NamespaceOverride cannot be null or whitespace when specified.");
            }
            else if (value.NamespaceOverride.Contains(" ", StringComparison.Ordinal) || value.NamespaceOverride.Contains("\t", StringComparison.Ordinal))
            {
                problems.Add("NamespaceOverride cannot contain whitespace characters.");
            }
        }

        // Validate ConfigFile if specified
        if (!string.IsNullOrWhiteSpace(value.ConfigFile))
        {
            if (string.IsNullOrWhiteSpace(value.ConfigFile))
            {
                problems.Add("ConfigFile cannot be null or whitespace when specified.");
            }
            else if (!File.Exists(value.ConfigFile))
            {
                problems.Add($"ConfigFile '{value.ConfigFile}' does not exist.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="CliOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this CliOptions value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="CliOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this CliOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"CliOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}