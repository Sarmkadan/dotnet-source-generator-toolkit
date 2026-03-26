// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Base exception for source code generation errors.
/// </summary>
public class GenerationException : Exception
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
public class EntityAnalysisException : GenerationException
{
    public EntityAnalysisException(string message) : base(message) { }

    public EntityAnalysisException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Thrown when repository generation fails.
/// </summary>
public class RepositoryGenerationException : GenerationException
{
    public RepositoryGenerationException(string message) : base(message) { }

    public RepositoryGenerationException(string message, string entityName)
        : base(message, "Repository", entityName) { }
}

/// <summary>
/// Thrown when mapper generation fails.
/// </summary>
public class MapperGenerationException : GenerationException
{
    public MapperGenerationException(string message) : base(message) { }

    public MapperGenerationException(string message, string entityName)
        : base(message, "Mapper", entityName) { }
}

/// <summary>
/// Thrown when validator generation fails.
/// </summary>
public class ValidatorGenerationException : GenerationException
{
    public ValidatorGenerationException(string message) : base(message) { }

    public ValidatorGenerationException(string message, string entityName)
        : base(message, "Validator", entityName) { }
}

/// <summary>
/// Thrown when validation of entities or templates fails.
/// </summary>
public class ValidationException : GenerationException
{
    public List<string> ValidationErrors { get; }

    public ValidationException(string message, List<string> errors)
        : base(message)
    {
        ValidationErrors = errors;
    }
}

/// <summary>
/// Thrown when configuration is invalid.
/// </summary>
public class ConfigurationException : GenerationException
{
    public ConfigurationException(string message) : base(message) { }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }
}
