#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Orchestrates the complete source generation workflow, coordinating
/// project analysis, entity parsing, and code generation for all artifact types.
/// </summary>
public sealed class SourceGeneratorService : ISourceGeneratorService
{
    private readonly IEntityAnalyzer _entityAnalyzer;

    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<SourceGeneratorService> _logger;

    public SourceGeneratorService(
        IEntityAnalyzer entityAnalyzer,
        IFileSystemService fileSystemService,
        IRepositoryGeneratorService repositoryGeneratorService,
        IMapperGeneratorService mapperGeneratorService,
        IValidatorGeneratorService validatorGeneratorService,
        ISerializerGeneratorService serializerGeneratorService,
        ILogger<SourceGeneratorService> logger)
    {
        _entityAnalyzer = entityAnalyzer;
        _fileSystemService = fileSystemService;
        _repositoryGeneratorService = repositoryGeneratorService;
        _mapperGeneratorService = mapperGeneratorService;
        _validatorGeneratorService = validatorGeneratorService;
        _serializerGeneratorService = serializerGeneratorService;
        _logger = logger;
    }

    private readonly IRepositoryGeneratorService _repositoryGeneratorService;
    private readonly IMapperGeneratorService _mapperGeneratorService;
    private readonly IValidatorGeneratorService _validatorGeneratorService;
    private readonly ISerializerGeneratorService _serializerGeneratorService;

    public async Task<ProjectInfo> AnalyzeProjectAsync(string projectPath)
    {
        if (string.IsNullOrWhiteSpace(projectPath))
            throw new ArgumentNullException(nameof(projectPath));
        if (!Directory.Exists(projectPath))
            throw new GenerationException($"Project path does not exist: {projectPath}");

        _logger.LogInformation("Starting project analysis for: {ProjectPath}", projectPath);

        var projectName = new DirectoryInfo(projectPath).Name;
        var projectInfo = new ProjectInfo
        {
            ProjectName = projectName,
            ProjectPath = projectPath,
        };

        try
        {
            // Find all C# files in the project
            var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
            _logger.LogInformation("Found {Count} C# files to analyze", csFiles.Length);

            foreach (var filePath in csFiles)
            {
                try
                {
                    var content = await _fileSystemService.ReadFileAsync(filePath);
                    var entities = await _entityAnalyzer.AnalyzeFileAsync(filePath, content);

                    foreach (var entity in entities)
                    {
                        projectInfo.AddEntity(entity);
                        _logger.LogInformation("Added entity: {EntityName}", entity.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error analyzing file: {FilePath}", filePath);
                    projectInfo.AnalysisErrors.Add($"Error in {filePath}: {ex.Message}");
                }
            }

            if (projectInfo.Entities.Count == 0)
                _logger.LogWarning("No entities found during project analysis");
            else
                _logger.LogInformation("Successfully analyzed {Count} entities", projectInfo.Entities.Count);

            return projectInfo;
        }
        catch (Exception ex)
        {
            projectInfo.HasAnalysisErrors = true;
            projectInfo.AnalysisErrors.Add(ex.Message);
            _logger.LogError(ex, "Project analysis failed");
            throw;
        }
    }

    public async Task<IEnumerable<GenerationResult>> GenerateAllAsync(ProjectInfo projectInfo)
    {
        if (projectInfo is null)
            throw new ArgumentNullException(nameof(projectInfo));
        var validationResult = await ValidateProjectAsync(projectInfo);
        if (!validationResult.IsValid)
            throw new ValidationException("Project validation failed", validationResult.Errors);

        _logger.LogInformation("Starting generation for project: {ProjectName}", projectInfo.ProjectName);

        var results = new List<GenerationResult>();

        foreach (var entity in projectInfo.Entities)
        {
            var entityResults = await GenerateForEntityAsync(entity, projectInfo);
            results.AddRange(entityResults);
        }

        _logger.LogInformation("Generation completed: {Total} results", results.Count);
        return results;
    }

    public async Task<IEnumerable<GenerationResult>> GenerateForEntityAsync(Entity entity, ProjectInfo projectInfo)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));
        if (projectInfo is null)
            throw new ArgumentNullException(nameof(projectInfo));
        var results = new List<GenerationResult>();

        // Validate entity
        var errors = entity.Validate().ToList();
        if (errors.Count > 0)
            throw new ValidationException($"Entity validation failed: {entity.Name}", errors);

        _logger.LogInformation("Generating code for entity: {EntityName}", entity.Name);

        // Generate for different types based on attributes
        foreach (var attr in entity.Attributes)
        {
            if (attr.Contains("Repository", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _repositoryGeneratorService.GenerateRepositoryAsync(entity);
                results.Add(result);
            }

            if (attr.Contains("Mapper", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _mapperGeneratorService.GenerateMapperAsync(entity, entity); // Assuming source and target entity are the same for simplicity
                results.Add(result);
            }

            if (attr.Contains("Validator", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _validatorGeneratorService.GenerateValidatorAsync(entity);
                results.Add(result);
            }

            if (attr.Contains("Serializer", StringComparison.OrdinalIgnoreCase))
            {
                var result = await _serializerGeneratorService.GenerateSerializerAsync(entity, SerializerFormat.Json); // Default to JSON for now
                results.Add(result);
            }
        }

        return await Task.FromResult(results);
    }

    public async Task<ValidationResult> ValidateProjectAsync(ProjectInfo projectInfo)
    {
        if (projectInfo is null)
            throw new ArgumentNullException(nameof(projectInfo));
        var result = new ValidationResult();

        var projectErrors = projectInfo.Validate();
        foreach (var error in projectErrors)
            result.AddError(error);

        if (projectInfo.Entities.Count == 0)
            result.AddWarning("No entities found in project");

        return await Task.FromResult(result);
    }
}

