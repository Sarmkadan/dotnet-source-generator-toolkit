#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Orchestrates the entire source generation workflow for a .NET project. Coordinates
/// Roslyn-based analysis, template-driven code generation, and output file management.
/// </summary>
/// <remarks>
/// <para>
/// The typical generation workflow is:
/// <list type="number">
///   <item>Analyze the project via <see cref="AnalyzeProjectAsync"/> to extract entity metadata</item>
///   <item>Validate the project structure via <see cref="ValidateProjectAsync"/></item>
///   <item>Generate artifacts via <see cref="GenerateAllAsync"/> or <see cref="GenerateForEntityAsync"/></item>
/// </list>
/// </para>
/// <para>
/// The toolkit can generate repositories, mappers, validators, and serializers. Each generator
/// is available as a separate service (<see cref="IRepositoryGeneratorService"/>,
/// <see cref="IMapperGeneratorService"/>, <see cref="IValidatorGeneratorService"/>,
/// <see cref="ISerializerGeneratorService"/>) and can be used independently.
/// </para>
/// </remarks>
public interface ISourceGeneratorService
{
    /// <summary>
    /// Analyzes a .NET project at the given path, extracting entity definitions,
    /// property metadata, and attribute annotations using Roslyn.
    /// </summary>
    /// <param name="projectPath">Absolute path to the .csproj file or project directory.</param>
    /// <returns>A <see cref="ProjectInfo"/> containing all discovered entities and project metadata.</returns>
    Task<ProjectInfo> AnalyzeProjectAsync(string projectPath);

    /// <summary>
    /// Generates all code artifacts (repositories, mappers, validators, serializers) for every
    /// entity in the analyzed project.
    /// </summary>
    /// <param name="projectInfo">The analyzed project metadata from <see cref="AnalyzeProjectAsync"/>.</param>
    /// <returns>A collection of <see cref="GenerationResult"/> objects, one per generated file.</returns>
    Task<IEnumerable<GenerationResult>> GenerateAllAsync(ProjectInfo projectInfo);

    /// <summary>
    /// Generates code artifacts for a single entity within the project context.
    /// </summary>
    /// <param name="entity">The entity to generate artifacts for.</param>
    /// <param name="projectInfo">Project metadata providing namespace and output path context.</param>
    Task<IEnumerable<GenerationResult>> GenerateForEntityAsync(Entity entity, ProjectInfo projectInfo);

    /// <summary>
    /// Validates that the project structure and entity definitions are suitable for code generation.
    /// Returns warnings for non-critical issues and errors for blocking problems.
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

public sealed class ValidationResult
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
