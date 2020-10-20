// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Pipeline;

/// <summary>
/// Orchestrates the complete code generation pipeline from analysis to output.
/// Coordinates all services and manages the generation workflow.
/// </summary>
public class GenerationPipeline
{
    private readonly ISourceGeneratorService _generatorService;
    private readonly IRepositoryGeneratorService _repositoryGenerator;
    private readonly IMapperGeneratorService _mapperGenerator;
    private readonly IValidatorGeneratorService _validatorGenerator;
    private readonly ISerializerGeneratorService _serializerGenerator;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<GenerationPipeline> _logger;

    public GenerationPipeline(
        ISourceGeneratorService generatorService,
        IRepositoryGeneratorService repositoryGenerator,
        IMapperGeneratorService mapperGenerator,
        IValidatorGeneratorService validatorGenerator,
        ISerializerGeneratorService serializerGenerator,
        IFileSystemService fileSystemService,
        ILogger<GenerationPipeline> logger)
    {
        _generatorService = generatorService ?? throw new ArgumentNullException(nameof(generatorService));
        _repositoryGenerator = repositoryGenerator ?? throw new ArgumentNullException(nameof(repositoryGenerator));
        _mapperGenerator = mapperGenerator ?? throw new ArgumentNullException(nameof(mapperGenerator));
        _validatorGenerator = validatorGenerator ?? throw new ArgumentNullException(nameof(validatorGenerator));
        _serializerGenerator = serializerGenerator ?? throw new ArgumentNullException(nameof(serializerGenerator));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Execute the complete generation pipeline for a project.
    /// </summary>
    public async Task<PipelineResult> ExecuteAsync(
        string projectPath,
        string? outputPath = null,
        IEnumerable<string>? generatorTypes = null,
        bool dryRun = false)
    {
        var result = new PipelineResult();

        try
        {
            _logger.LogInformation("Starting generation pipeline for: {ProjectPath}", projectPath);

            // Phase 1: Analyze project
            var projectInfo = await _generatorService.AnalyzeProjectAsync(projectPath);
            result.EntitiesFound = projectInfo.Entities.Count;

            if (projectInfo.Entities.Count == 0)
            {
                _logger.LogWarning("No entities found in project");
                return result;
            }

            // Phase 2: Generate code
            var generationResults = await _generatorService.GenerateAllAsync(projectInfo);
            result.GeneratedFiles = generationResults.Count();

            // Phase 3: Write results (unless dry-run)
            if (!dryRun && !string.IsNullOrEmpty(outputPath))
            {
                var written = await WriteGeneratedFilesAsync(generationResults, outputPath);
                result.FilesWritten = written;
            }

            result.IsSuccessful = true;
            _logger.LogInformation(
                "Pipeline completed successfully: {Entities} entities, {Files} files generated",
                result.EntitiesFound,
                result.GeneratedFiles);
        }
        catch (Exception ex)
        {
            result.IsSuccessful = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Pipeline execution failed");
        }

        return result;
    }

    private async Task<int> WriteGeneratedFilesAsync(IEnumerable<GenerationResult> results, string outputPath)
    {
        int filesWritten = 0;

        foreach (var result in results)
        {
            if (string.IsNullOrEmpty(result.OutputFilePath) || string.IsNullOrEmpty(result.GeneratedCode))
                continue;

            try
            {
                var outputFile = Path.Combine(outputPath, Path.GetFileName(result.OutputFilePath));
                await _fileSystemService.WriteFileAsync(outputFile, result.GeneratedCode);
                filesWritten++;
                _logger.LogInformation("Generated file: {OutputFile}", outputFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write generated file: {OutputPath}", result.OutputFilePath);
            }
        }

        return filesWritten;
    }
}

/// <summary>
/// Result of pipeline execution.
/// </summary>
public class PipelineResult
{
    public bool IsSuccessful { get; set; }
    public int EntitiesFound { get; set; }
    public int GeneratedFiles { get; set; }
    public int FilesWritten { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}
