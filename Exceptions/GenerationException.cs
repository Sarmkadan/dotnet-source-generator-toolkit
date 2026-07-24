#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Base exception for source code generation errors.
/// </summary>
public class GenerationException : DotNetSourceGeneratorToolkitException
{
    public string? GeneratorType { get; set; }

    public string? EntityName { get; set; }

    public GenerationException(string message) : base(message) { }

    public GenerationException(string message, Exception innerException)
        : base(message, innerException) { }

    public GenerationException(string message, string? generatorType, string? entityName)
        : base(message)
    {
        GeneratorType = generatorType;
        EntityName = entityName;
    }
}

/// <summary>
/// Thrown when entity analysis fails.
/// </summary>
public sealed class EntityAnalysisException : GenerationException
{
    public EntityAnalysisException(string message) : base(message) { }

    public EntityAnalysisException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when repository generation fails.
/// </summary>
public sealed class RepositoryGenerationException : GenerationException
{
    public RepositoryGenerationException(string message) : base(message) { }

    public RepositoryGenerationException(string message, string entityName)
        : base(message, "Repository", entityName) { }
}

/// <summary>
/// Thrown when mapper generation fails.
/// </summary>
public sealed class MapperGenerationException : GenerationException
{
    public MapperGenerationException(string message) : base(message) { }

    public MapperGenerationException(string message, string entityName)
        : base(message, "Mapper", entityName) { }
}

/// <summary>
/// Thrown when validator generation fails.
/// </summary>
public sealed class ValidatorGenerationException : GenerationException
{
    public ValidatorGenerationException(string message) : base(message) { }

    public ValidatorGenerationException(string message, string entityName)
        : base(message, "Validator", entityName) { }
}


/// <summary>
/// Thrown when generation-time configuration is invalid.
/// </summary>
public class GenerationConfigurationException : GenerationException
{
    public GenerationConfigurationException(string message) : base(message) { }

    public GenerationConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when a template variable is missing from the context.
/// </summary>
public sealed class MissingVariableException : GenerationException
{
    /// <summary>Gets the name of the missing variable.</summary>
    public string VariableName { get; }

    /// <summary>Gets the template that contained the missing variable.</summary>
    public string Template { get; }

    /// <summary>Initializes a new instance of the <see cref="MissingVariableException"/> class.</summary>
    /// <param name="variableName">Name of the missing variable.</param>
    /// <param name="template">Template that contained the missing variable.</param>
    public MissingVariableException(string variableName, string template)
        : base($"Missing variable '{variableName}' in template: {template}")
    {
        VariableName = variableName;
        Template = template;
    }

    /// <summary>Initializes a new instance of the <see cref="MissingVariableException"/> class.</summary>
    /// <param name="variableName">Name of the missing variable.</param>
    /// <param name="template">Template that contained the missing variable.</param>
    /// <param name="innerException">The inner exception.</param>
    public MissingVariableException(string variableName, string template, Exception innerException)
        : base($"Missing variable '{variableName}' in template: {template}", innerException)
    {
        VariableName = variableName;
        Template = template;
    }
}