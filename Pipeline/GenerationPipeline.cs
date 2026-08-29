#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DotNetSourceGeneratorToolkit.Pipeline;

/// <summary>
/// Orchestrates the complete code generation pipeline from analysis to output.
/// Coordinates all services and manages the generation workflow.
/// </summary>
public sealed partial class GenerationPipeline
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
    /// Gets a value indicating whether the last execution was successful.
    /// </summary>
    public bool IsSuccessful { get; private set; }

    /// <summary>
    /// Gets the number of entities found during the last execution.
    /// </summary>
    public int EntitiesFound { get; private set; }

    /// <summary>
    /// Gets the number of files generated during the last execution.
    /// </summary>
    public int GeneratedFiles { get; private set; }

    /// <summary>
    /// Gets the number of files successfully written during the last execution.
    /// </summary>
    public int FilesWritten { get; private set; }

    /// <summary>
    /// Gets the error message if the execution failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the timestamp when the pipeline was last executed.
    /// </summary>
    public DateTime ExecutedAt { get; private set; }

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
        var stopwatch = Stopwatch.StartNew();

        try
        {
            LoggerMessages.StartingGenerationPipeline(_logger, projectPath);

            // Phase 1: Analyze project
            var projectInfo = await _generatorService.AnalyzeProjectAsync(projectPath);
            result.EntitiesFound = projectInfo.Entities.Count;

            if (projectInfo.Entities.Count == 0)
            {
                LoggerMessages.NoEntitiesFound(_logger);
                return result;
            }

            LoggerMessages.AnalyzeProjectCompleted(_logger, projectInfo.Entities.Count);

            // Phase 2: Generate code
            var generationResults = await _generatorService.GenerateAllAsync(projectInfo);
            result.GeneratedFiles = generationResults.Count();

            LoggerMessages.CodeGenerationCompleted(_logger, generationResults.Count());

            // Phase 3: Write results (unless dry-run)
            int written = 0;
            if (!dryRun && !string.IsNullOrEmpty(outputPath))
            {
                written = await WriteGeneratedFilesAsync(generationResults, outputPath);
            }

            LoggerMessages.FileWritingCompleted(_logger, written);

            stopwatch.Stop();
            IsSuccessful = true;
            EntitiesFound = projectInfo.Entities.Count;
            GeneratedFiles = generationResults.Count();
            FilesWritten = written;
            ExecutedAt = DateTime.UtcNow;
            LoggerMessages.PipelineCompletedSuccessfully(_logger, stopwatch.ElapsedMilliseconds, EntitiesFound, GeneratedFiles);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            IsSuccessful = false;
            ErrorMessage = ex.Message;
            ExecutedAt = DateTime.UtcNow;
            LoggerMessages.PipelineExecutionFailed(_logger, stopwatch.ElapsedMilliseconds, ex);
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
                LoggerMessages.GeneratedFile(_logger, outputFile);
            }
            catch (Exception ex)
            {
                LoggerMessages.FailedToWriteGeneratedFile(_logger, result.OutputFilePath, ex);
            }
        }

        return filesWritten;
    }

    // Logger messages
    private static partial class LoggerMessages
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Starting generation pipeline for: {ProjectPath}")]
        public static partial void StartingGenerationPipeline(ILogger logger, string projectPath);

        [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Pipeline completed successfully in {ElapsedMs}ms: {Entities} entities, {Files} files generated")]
        public static partial void PipelineCompletedSuccessfully(ILogger logger, long elapsedMs, int entities, int files);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Pipeline execution failed after {ElapsedMs}ms")]
        public static partial void PipelineExecutionFailed(ILogger logger, long elapsedMs, Exception ex);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "No entities found in project")]
        public static partial void NoEntitiesFound(ILogger logger);

        [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Generated file: {OutputFile}")]
        public static partial void GeneratedFile(ILogger logger, string outputFile);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to write generated file: {OutputPath}")]
        public static partial void FailedToWriteGeneratedFile(ILogger logger, string outputPath, Exception ex);

        [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Analyze project completed: {EntityCount} entities found")]
        public static partial void AnalyzeProjectCompleted(ILogger logger, int entityCount);

        [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Code generation completed: {GeneratedCount} files generated")]
        public static partial void CodeGenerationCompleted(ILogger logger, int generatedCount);

        [LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "File writing completed: {WrittenCount} files written")]
        public static partial void FileWritingCompleted(ILogger logger, int writtenCount);
    }
}

/// <summary>
/// Result of pipeline execution.
/// </summary>
public sealed class PipelineResult
{
    public bool IsSuccessful { get; set; }
    public int EntitiesFound { get; set; }
    public int GeneratedFiles { get; set; }
    public int FilesWritten { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
}