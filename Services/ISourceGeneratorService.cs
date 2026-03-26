// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Orchestrates the entire source generation workflow for a project.
/// Coordinates analysis, code generation, and output management.
/// </summary>
public interface ISourceGeneratorService
{
    /// <summary>
    /// Analyzes a .NET project and extracts entity definitions.
    /// </summary>
    Task<ProjectInfo> AnalyzeProjectAsync(string projectPath);

    /// <summary>
    /// Generates all code artifacts for a project's entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllAsync(ProjectInfo projectInfo);

    /// <summary>
    /// Generates code artifacts for a specific entity.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateForEntityAsync(Entity entity, ProjectInfo projectInfo);

    /// <summary>
    /// Validates the project structure before generation.
    /// </summary>
    Task<ValidationResult> ValidateProjectAsync(ProjectInfo projectInfo);
}

/// <summary>
/// Generates repository implementations from entity definitions.
/// </summary>
public interface IRepositoryGeneratorService
{
    /// <summary>
    /// Generates a repository for a single entity.
    /// </summary>
    Task<GenerationResult> GenerateRepositoryAsync(Entity entity);

    /// <summary>
    /// Generates repositories for multiple entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllRepositoriesAsync(List<Entity> entities);
}

/// <summary>
/// Generates mapper implementations for entity transformations.
/// </summary>
public interface IMapperGeneratorService
{
    /// <summary>
    /// Generates all mappers for a set of entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllMappersAsync(List<Entity> entities);

    /// <summary>
    /// Generates a mapper from one entity to another.
    /// </summary>
    Task<GenerationResult> GenerateMapperAsync(Entity sourceEntity, Entity targetEntity);
}

/// <summary>
/// Generates validation logic for entities.
/// </summary>
public interface IValidatorGeneratorService
{
    /// <summary>
    /// Generates validators for all entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync(List<Entity> entities);

    /// <summary>
    /// Generates a FluentValidation validator for an entity.
    /// </summary>
    Task<GenerationResult> GenerateValidatorAsync(Entity entity);
}

/// <summary>
/// Generates serialization/deserialization code.
/// </summary>
public interface ISerializerGeneratorService
{
    /// <summary>
    /// Generates serializers for all entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllSerializersAsync(List<Entity> entities);

    /// <summary>
    /// Generates a serializer for a specific entity.
    /// </summary>
    Task<GenerationResult> GenerateSerializerAsync(Entity entity, SerializerFormat format);
}

public enum SerializerFormat
{
    Json,
    Xml,
    Binary,
}

public class ValidationResult
{
    public bool IsValid { get; set; } = true;

    public List<string> Errors { get; } = [];

    public List<string> Warnings { get; } = [];

    public void AddError(string error)
    {
        Errors.Add(error);
        IsValid = false;
    }

    public void AddWarning(string warning) => Warnings.Add(warning);
}
