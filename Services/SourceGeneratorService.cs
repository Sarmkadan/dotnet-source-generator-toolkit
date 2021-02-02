// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Orchestrates the complete source generation workflow, coordinating
/// project analysis, entity parsing, and code generation for all artifact types.
/// </summary>
public class SourceGeneratorService : ISourceGeneratorService
{
    private readonly IEntityAnalyzer _entityAnalyzer;
    private readonly IAttributeAnalyzer _attributeAnalyzer;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<SourceGeneratorService> _logger;

    public SourceGeneratorService(
        IEntityAnalyzer entityAnalyzer,
        IAttributeAnalyzer attributeAnalyzer,
        IFileSystemService fileSystemService,
        ILogger<SourceGeneratorService> logger)
    {
        _entityAnalyzer = entityAnalyzer;
        _attributeAnalyzer = attributeAnalyzer;
        _fileSystemService = fileSystemService;
        _logger = logger;
    }

    public async Task<ProjectInfo> AnalyzeProjectAsync(string projectPath)
    {
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
                var result = new GenerationResult
                {
                    EntityName = entity.Name,
                    GeneratorType = GeneratorType.Repository,
                    Status = GenerationStatus.InProgress,
                };

                result.GeneratedCode = GenerateRepositoryCode(entity);
                result.OutputFilePath = Path.Combine(projectInfo.ProjectPath, $"{entity.Name}Repository.cs");
                result.MarkAsCompleted(GenerationStatus.Completed, 100);

                results.Add(result);
            }

            if (attr.Contains("Mapper", StringComparison.OrdinalIgnoreCase))
            {
                var result = new GenerationResult
                {
                    EntityName = entity.Name,
                    GeneratorType = GeneratorType.Mapper,
                    Status = GenerationStatus.InProgress,
                };

                result.GeneratedCode = GenerateMapperCode(entity);
                result.OutputFilePath = Path.Combine(projectInfo.ProjectPath, $"{entity.Name}Mapper.cs");
                result.MarkAsCompleted(GenerationStatus.Completed, 100);

                results.Add(result);
            }

            if (attr.Contains("Validator", StringComparison.OrdinalIgnoreCase))
            {
                var result = new GenerationResult
                {
                    EntityName = entity.Name,
                    GeneratorType = GeneratorType.Validator,
                    Status = GenerationStatus.InProgress,
                };

                result.GeneratedCode = GenerateValidatorCode(entity);
                result.OutputFilePath = Path.Combine(projectInfo.ProjectPath, $"{entity.Name}Validator.cs");
                result.MarkAsCompleted(GenerationStatus.Completed, 100);

                results.Add(result);
            }
        }

        return await Task.FromResult(results);
    }

    public async Task<ValidationResult> ValidateProjectAsync(ProjectInfo projectInfo)
    {
        var result = new ValidationResult();

        var projectErrors = projectInfo.Validate();
        foreach (var error in projectErrors)
            result.AddError(error);

        if (projectInfo.Entities.Count == 0)
            result.AddWarning("No entities found in project");

        return await Task.FromResult(result);
    }

    private string GenerateRepositoryCode(Entity entity)
    {
        var code = $@"// Generated repository for {entity.Name}
namespace {entity.Namespace}
{{
    public interface I{entity.Name}Repository
    {{
        Task<{entity.Name}> GetByIdAsync(object id);
        Task<IEnumerable<{entity.Name}>> GetAllAsync();
        Task<{entity.Name}> CreateAsync({entity.Name} entity);
        Task<{entity.Name}> UpdateAsync({entity.Name} entity);
        Task<bool> DeleteAsync(object id);
    }}

    public class {entity.Name}Repository : I{entity.Name}Repository
    {{
        public async Task<{entity.Name}> GetByIdAsync(object id)
        {{
            // Implementation
            return await Task.FromResult(new {entity.Name}());
        }}

        public async Task<IEnumerable<{entity.Name}>> GetAllAsync()
        {{
            // Implementation
            return await Task.FromResult(new List<{entity.Name}>());
        }}

        public async Task<{entity.Name}> CreateAsync({entity.Name} entity)
        {{
            // Implementation
            return await Task.FromResult(entity);
        }}

        public async Task<{entity.Name}> UpdateAsync({entity.Name} entity)
        {{
            // Implementation
            return await Task.FromResult(entity);
        }}

        public async Task<bool> DeleteAsync(object id)
        {{
            // Implementation
            return await Task.FromResult(true);
        }}
    }}
}}";
        return code;
    }

    private string GenerateMapperCode(Entity entity)
    {
        return $@"// Generated mapper for {entity.Name}
namespace {entity.Namespace}
{{
    public class {entity.Name}Mapper
    {{
        public static {entity.Name}Dto MapToDto({entity.Name} entity)
        {{
            if (entity == null) return null;
            return new {entity.Name}Dto();
        }}

        public static {entity.Name} MapFromDto({entity.Name}Dto dto)
        {{
            if (dto == null) return null;
            return new {entity.Name}();
        }}
    }}

    public class {entity.Name}Dto
    {{
    }}
}}";
    }

    private string GenerateValidatorCode(Entity entity)
    {
        return $@"// Generated validator for {entity.Name}
namespace {entity.Namespace}
{{
    public class {entity.Name}Validator
    {{
        public bool Validate({entity.Name} entity, out List<string> errors)
        {{
            errors = new List<string>();

            if (entity == null)
            {{
                errors.Add(""Entity cannot be null"");
                return false;
            }}

            return errors.Count == 0;
        }}
    }}
}}";
    }
}
