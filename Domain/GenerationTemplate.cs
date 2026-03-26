// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents a code generation template that defines how to generate specific code artifacts
/// like repositories, mappers, validators, or serializers from entity definitions.
/// </summary>
public class GenerationTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public GeneratorType GeneratorType { get; set; }

    public string TemplateContent { get; set; } = string.Empty;

    public string OutputFileNamePattern { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = string.Empty;

    public List<string> SupportedLanguages { get; } = ["CSharp"];

    public Dictionary<string, string> ConfigurationOptions { get; } = [];

    public bool IsActive { get; set; } = true;

    public bool IsCustom { get; set; }

    public int Version { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? Author { get; set; }

    /// <summary>
    /// Generates the output file name for a specific entity.
    /// </summary>
    public string GenerateOutputFileName(string entityName)
    {
        if (string.IsNullOrEmpty(OutputFileNamePattern))
            throw new InvalidOperationException("Output file name pattern is not configured.");

        return OutputFileNamePattern
            .Replace("{EntityName}", entityName)
            .Replace("{EntityNamePlural}", Pluralize(entityName))
            .Replace("{EntityNameLower}", entityName.ToLower())
            .Replace("{Timestamp}", DateTime.UtcNow.Ticks.ToString());
    }

    /// <summary>
    /// Validates the template configuration.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Template name is required.");

        if (string.IsNullOrWhiteSpace(TemplateContent))
            errors.Add("Template content is required.");

        if (string.IsNullOrWhiteSpace(OutputFileNamePattern))
            errors.Add("Output file name pattern is required.");

        if (string.IsNullOrWhiteSpace(OutputDirectory))
            errors.Add("Output directory is required.");

        if (SupportedLanguages.Count == 0)
            errors.Add("At least one supported language must be specified.");

        return errors;
    }

    /// <summary>
    /// Checks if the template can be used for a specific generator type.
    /// </summary>
    public bool SupportsGeneratorType(GeneratorType type) => GeneratorType == type;

    /// <summary>
    /// Simple pluralization helper for common English words.
    /// </summary>
    private static string Pluralize(string word)
    {
        if (word.EndsWith("y"))
            return word[..^1] + "ies";
        if (word.EndsWith("s") || word.EndsWith("ss") || word.EndsWith("x") || word.EndsWith("z"))
            return word + "es";
        return word + "s";
    }
}

public enum GeneratorType
{
    Repository,
    Mapper,
    Validator,
    Serializer,
    DataTransferObject,
    Service,
    Controller,
}
