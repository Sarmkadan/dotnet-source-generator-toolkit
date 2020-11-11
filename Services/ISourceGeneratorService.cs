#nullable enable

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
