#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===========================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Aggregates, analyzes, and reports on code generation results including
/// success rates, performance metrics, and detailed summaries.
/// </summary>
public interface IGenerationResultAggregatorService
{
    /// <summary>Analyzes a collection of generation results.</summary>
    GenerationReport Analyze(IEnumerable<GenerationResult> results);

    /// <summary>Analyzes a collection of generation results and includes file diff summary.</summary>
    Task<GenerationReport> AnalyzeWithFileDiffAsync(IEnumerable<GenerationResult> results);

    /// <summary>Generates a detailed report as formatted text.</summary>
    string GenerateReport(GenerationReport report);

    /// <summary>Gets statistics about generation performance.</summary>
    GenerationStatistics GetStatistics(IEnumerable<GenerationResult> results);

    /// <summary>Exports results to JSON format.</summary>
    Task<string> ExportToJsonAsync(GenerationReport report);

    /// <summary>Compares generated SourceFile contents against existing files on disk and returns diff summary.</summary>
    Task<FileDiffSummary> GenerateFileDiffSummaryAsync(IEnumerable<GenerationResult> generatedResults);
}

/// <summary>
/// Contains aggregated analysis of generation results.
/// </summary>
public sealed class GenerationReport
{
    public int TotalResults { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public int SkippedCount { get; set; }
    public long TotalDurationMs { get; set; }
    public int TotalLinesGenerated { get; set; }
    public int TotalWarnings { get; set; }
    public int TotalErrors { get; set; }
    public DateTime ReportGeneratedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<GeneratorType, int> ResultsByType { get; } = [];
    public List<GenerationResult> FailedResults { get; } = [];
    public double SuccessRate { get; set; }
    public TimeSpan AverageDuration { get; set; }

    /// <summary>
    /// Count of files that were added (didn't exist before)
    /// </summary>
    public int FilesAdded { get; set; }

    /// <summary>
    /// Count of files that were changed (content was different)
    /// </summary>
    public int FilesChanged { get; set; }

    /// <summary>
    /// Count of files that were unchanged (content was identical)
    /// </summary>
    public int FilesUnchanged { get; set; }

    /// <summary>
    /// Total count of files compared against disk
    /// </summary>
    public int FilesCompared { get; set; }
}

/// <summary>
/// Contains statistical analysis of generation performance.
/// </summary>
public sealed class GenerationStatistics
{
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int FailedCount { get; set; }
    public double SuccessPercentage { get; set; }
    public long MinDurationMs { get; set; }
    public long MaxDurationMs { get; set; }
    public double AverageDurationMs { get; set; }
    public int TotalCodeLines { get; set; }
    public long TotalCodeBytes { get; set; }
    public int EntitiesProcessed { get; set; }
    public Dictionary<string, int> ErrorCounts { get; } = [];
}

/// <summary>
/// Contains summary of file changes after comparing generated content with existing files on disk.
/// </summary>
public sealed class FileDiffSummary
{
    public int FilesAdded { get; set; }
    public int FilesChanged { get; set; }
    public int FilesUnchanged { get; set; }
    public int TotalFiles => FilesAdded + FilesChanged + FilesUnchanged;
    public int FilesCompared { get; set; }

    public bool HasChanges => FilesAdded > 0 || FilesChanged > 0;
    public bool IsEmpty => TotalFiles == 0;
}

/// <summary>
/// Analyzes and reports on code generation results and performance metrics.
/// </summary>
public sealed class GenerationResultAggregatorService : IGenerationResultAggregatorService
{
    private readonly ILogger<GenerationResultAggregatorService> _logger;
    private readonly IFileSystemService _fileSystemService;

    public GenerationResultAggregatorService(
        ILogger<GenerationResultAggregatorService> logger,
        IFileSystemService fileSystemService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    public GenerationReport Analyze(IEnumerable<GenerationResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var resultsList = results.ToList();
        _logger.LogInformation("Analyzing {Count} generation results", resultsList.Count);

        var report = new GenerationReport
        {
            TotalResults = resultsList.Count,
            SuccessCount = resultsList.Count(r => r.Status == GenerationStatus.Completed),
            FailureCount = resultsList.Count(r => r.Status == GenerationStatus.Failed),
            SkippedCount = resultsList.Count(r => r.Status == GenerationStatus.Skipped),
            TotalDurationMs = resultsList.Sum(r => r.GenerationDurationMs),
            TotalLinesGenerated = resultsList.Sum(r => r.CodeLineCount),
            TotalWarnings = resultsList.Sum(r => r.Warnings.Count),
            TotalErrors = resultsList.Sum(r => r.Errors.Count),
        };

        // Calculate success rate
        if (report.TotalResults > 0)
            report.SuccessRate = (double)report.SuccessCount / report.TotalResults * 100;

        // Calculate average duration
        if (report.TotalResults > 0)
            report.AverageDuration = TimeSpan.FromMilliseconds(report.TotalDurationMs / (double)report.TotalResults);

        // Group by type
        foreach (var type in Enum.GetValues(typeof(GeneratorType)).Cast<GeneratorType>())
        {
            var typeCount = resultsList.Count(r => r.GeneratorType == type);
            if (typeCount > 0)
                report.ResultsByType[type] = typeCount;
        }

        // Collect failed results
        report.FailedResults.AddRange(resultsList.Where(r => r.Status == GenerationStatus.Failed));

        _logger.LogInformation("Analysis complete: {Success} success, {Failure} failed, {Skipped} skipped",
            report.SuccessCount, report.FailureCount, report.SkippedCount);

        return report;
    }

    /// <summary>
    /// Analyzes a collection of generation results and includes file diff summary.
    /// </summary>
    /// <param name="results">Collection of generation results to analyze</param>
    /// <returns>GenerationReport with file diff summary included</returns>
    public async Task<GenerationReport> AnalyzeWithFileDiffAsync(IEnumerable<GenerationResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var report = Analyze(results);

        // Generate file diff summary for completed results with output paths
        var fileDiffSummary = await GenerateFileDiffSummaryAsync(results);

        report.FilesAdded = fileDiffSummary.FilesAdded;
        report.FilesChanged = fileDiffSummary.FilesChanged;
        report.FilesUnchanged = fileDiffSummary.FilesUnchanged;
        report.FilesCompared = fileDiffSummary.FilesCompared;

        _logger.LogInformation("Enhanced analysis complete with file diff: {Added} added, {Changed} changed, {Unchanged} unchanged",
            report.FilesAdded, report.FilesChanged, report.FilesUnchanged);

        return report;
    }

    public string GenerateReport(GenerationReport report)
    {
        if (report is null)
            throw new ArgumentNullException(nameof(report));

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("╔═══════════════════════════════════════════════════════════╗");
        sb.AppendLine("║ CODE GENERATION REPORT ║");
        sb.AppendLine("╚═══════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        // Summary section
        sb.AppendLine("SUMMARY");
        sb.AppendLine("─────────────────────────────────────────────────────────");
        sb.AppendLine($"Total Results: {report.TotalResults}");
        sb.AppendLine($"Successful: {report.SuccessCount}");
        sb.AppendLine($"Failed: {report.FailureCount}");
        sb.AppendLine($"Skipped: {report.SkippedCount}");
        sb.AppendLine($"Success Rate: {report.SuccessRate:F2}%");
        sb.AppendLine();

        // Performance section
        sb.AppendLine("PERFORMANCE");
        sb.AppendLine("─────────────────────────────────────────────────────────");
        sb.AppendLine($"Total Duration: {report.TotalDurationMs}ms");
        sb.AppendLine($"Average Duration: {report.AverageDuration.TotalMilliseconds:F2}ms");
        sb.AppendLine($"Total Code Lines: {report.TotalLinesGenerated:N0}");
        sb.AppendLine();

        // Results by type
        if (report.ResultsByType.Count > 0)
        {
            sb.AppendLine("BY GENERATOR TYPE");
            sb.AppendLine("─────────────────────────────────────────────────────────");
            foreach (var kvp in report.ResultsByType)
            {
                sb.AppendLine($"{kvp.Key,-20} {kvp.Value,5} results");
            }
            sb.AppendLine();
        }

        // File diff summary section
        if (report.FilesCompared > 0)
        {
            sb.AppendLine("FILE DIFF SUMMARY");
            sb.AppendLine("─────────────────────────────────────────────────────────");
            sb.AppendLine($"Files Compared: {report.FilesCompared}");
            sb.AppendLine($"Files Added: {report.FilesAdded}");
            sb.AppendLine($"Files Changed: {report.FilesChanged}");
            sb.AppendLine($"Files Unchanged: {report.FilesUnchanged}");

            var changePercentage = report.FilesCompared > 0
                ? ((double)(report.FilesAdded + report.FilesChanged) / report.FilesCompared * 100)
                : 0;
            sb.AppendLine($"Change Rate: {changePercentage:F1}%");
            sb.AppendLine();
        }

        // Issues section
        sb.AppendLine("ISSUES");
        sb.AppendLine("─────────────────────────────────────────────────────────");
        sb.AppendLine($"Total Warnings: {report.TotalWarnings}");
        sb.AppendLine($"Total Errors: {report.TotalErrors}");

        if (report.FailedResults.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Failed Entities:");
            foreach (var result in report.FailedResults)
            {
                sb.AppendLine($" • {result.EntityName} ({result.GeneratorType})");
                foreach (var error in result.Errors.Take(2))
                    sb.AppendLine($" - {error}");
                if (result.Errors.Count > 2)
                    sb.AppendLine($" ... and {result.Errors.Count - 2} more errors");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Report Generated: {report.ReportGeneratedAt:yyyy-MM-dd HH:mm:ss}");

        return sb.ToString();
    }

    public GenerationStatistics GetStatistics(IEnumerable<GenerationResult> results)
    {
        if (results is null)
            throw new ArgumentNullException(nameof(results));

        var resultsList = results.ToList();
        var completedResults = resultsList.Where(r => r.Status == GenerationStatus.Completed).ToList();

        var stats = new GenerationStatistics
        {
            TotalCount = resultsList.Count,
            CompletedCount = completedResults.Count,
            FailedCount = resultsList.Count(r => r.Status == GenerationStatus.Failed),
            TotalCodeLines = resultsList.Sum(r => r.CodeLineCount),
            TotalCodeBytes = resultsList.Sum(r => r.GeneratedCode?.Length ?? 0),
            EntitiesProcessed = resultsList.Select(r => r.EntityName).Distinct().Count(),
        };

        // Calculate percentages
        if (stats.TotalCount > 0)
        {
            stats.SuccessPercentage = (double)stats.CompletedCount / stats.TotalCount * 100;
        }

        // Duration statistics
        if (completedResults.Count > 0)
        {
            var durations = completedResults.Select(r => r.GenerationDurationMs).ToList();
            stats.MinDurationMs = durations.Min();
            stats.MaxDurationMs = durations.Max();
            stats.AverageDurationMs = durations.Average();
        }

        // Error aggregation
        foreach (var result in resultsList.Where(r => r.Errors.Count > 0))
        {
            foreach (var error in result.Errors)
            {
                var errorType = ExtractErrorType(error);
                if (stats.ErrorCounts.ContainsKey(errorType))
                    stats.ErrorCounts[errorType]++;
                else
                    stats.ErrorCounts[errorType] = 1;
            }
        }

        _logger.LogInformation("Generated statistics: {Success}% success rate", stats.SuccessPercentage);

        return stats;
    }

    public async Task<string> ExportToJsonAsync(GenerationReport report)
    {
        if (report is null)
            throw new ArgumentNullException(nameof(report));

        _logger.LogInformation("Exporting report to JSON format");

        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });

        return await Task.FromResult(json);
    }

    /// <summary>
    /// Compares generated SourceFile contents against existing files on disk and returns diff summary.
    /// </summary>
    /// <param name="generatedResults">Collection of generation results to analyze</param>
    /// <returns>Diff summary with added/changed/unchanged file counts</returns>
    public async Task<FileDiffSummary> GenerateFileDiffSummaryAsync(IEnumerable<GenerationResult> generatedResults)
    {
        if (generatedResults is null)
            throw new ArgumentNullException(nameof(generatedResults));

        var resultsList = generatedResults.Where(r => r.Status == GenerationStatus.Completed && !string.IsNullOrEmpty(r.OutputFilePath)).ToList();

        if (resultsList.Count == 0)
        {
            _logger.LogInformation("No completed generation results with output file paths to compare");
            return new FileDiffSummary();
        }

        _logger.LogInformation("Generating file diff summary for {Count} completed results", resultsList.Count);

        int filesAdded = 0;
        int filesChanged = 0;
        int filesUnchanged = 0;

        foreach (var result in resultsList)
        {
            var filePath = result.OutputFilePath;
            var generatedContent = result.GeneratedCode;

            if (!_fileSystemService.FileExists(filePath))
            {
                filesAdded++;
                _logger.LogDebug("File would be added: {FilePath}", filePath);
            }
            else
            {
                try
                {
                    var existingContent = await _fileSystemService.ReadFileAsync(filePath);

                    if (string.Equals(existingContent, generatedContent, StringComparison.Ordinal))
                    {
                        filesUnchanged++;
                        _logger.LogDebug("File unchanged: {FilePath}", filePath);
                    }
                    else
                    {
                        filesChanged++;
                        _logger.LogDebug("File changed: {FilePath}", filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading existing file for comparison: {FilePath}", filePath);
                    filesAdded++; // Treat as would-be-added if we can't read existing
                }
            }
        }

        var summary = new FileDiffSummary
        {
            FilesAdded = filesAdded,
            FilesChanged = filesChanged,
            FilesUnchanged = filesUnchanged,
            FilesCompared = resultsList.Count
        };

        _logger.LogInformation("File diff summary generated: {Added} added, {Changed} changed, {Unchanged} unchanged out of {Total} files",
            summary.FilesAdded, summary.FilesChanged, summary.FilesUnchanged, summary.TotalFiles);

        return summary;
    }

    private string ExtractErrorType(string error)
    {
        var parts = error.Split(':');
        return parts.Length > 0 ? parts[0] : "Unknown";
    }
}