// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
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

    /// <summary>Generates a detailed report as formatted text.</summary>
    string GenerateReport(GenerationReport report);

    /// <summary>Gets statistics about generation performance.</summary>
    GenerationStatistics GetStatistics(IEnumerable<GenerationResult> results);

    /// <summary>Exports results to JSON format.</summary>
    Task<string> ExportToJsonAsync(GenerationReport report);
}

/// <summary>
/// Contains aggregated analysis of generation results.
/// </summary>
public class GenerationReport
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
}

/// <summary>
/// Contains statistical analysis of generation performance.
/// </summary>
public class GenerationStatistics
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
/// Analyzes and reports on code generation results and performance metrics.
/// </summary>
public class GenerationResultAggregatorService : IGenerationResultAggregatorService
{
    private readonly ILogger<GenerationResultAggregatorService> _logger;

    public GenerationResultAggregatorService(ILogger<GenerationResultAggregatorService> logger)
    {
        _logger = logger;
    }

    public GenerationReport Analyze(IEnumerable<GenerationResult> results)
    {
        if (results == null)
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

    public string GenerateReport(GenerationReport report)
    {
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        var sb = new System.Text.StringBuilder();

        sb.AppendLine("╔═══════════════════════════════════════════════════════════╗");
        sb.AppendLine("║          CODE GENERATION REPORT                          ║");
        sb.AppendLine("╚═══════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        // Summary section
        sb.AppendLine("SUMMARY");
        sb.AppendLine("─────────────────────────────────────────────────────────");
        sb.AppendLine($"Total Results:        {report.TotalResults}");
        sb.AppendLine($"Successful:           {report.SuccessCount}");
        sb.AppendLine($"Failed:               {report.FailureCount}");
        sb.AppendLine($"Skipped:              {report.SkippedCount}");
        sb.AppendLine($"Success Rate:         {report.SuccessRate:F2}%");
        sb.AppendLine();

        // Performance section
        sb.AppendLine("PERFORMANCE");
        sb.AppendLine("─────────────────────────────────────────────────────────");
        sb.AppendLine($"Total Duration:       {report.TotalDurationMs}ms");
        sb.AppendLine($"Average Duration:     {report.AverageDuration.TotalMilliseconds:F2}ms");
        sb.AppendLine($"Total Code Lines:     {report.TotalLinesGenerated:N0}");
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

        // Issues section
        sb.AppendLine("ISSUES");
        sb.AppendLine("─────────────────────────────────────────────────────────");
        sb.AppendLine($"Total Warnings:       {report.TotalWarnings}");
        sb.AppendLine($"Total Errors:         {report.TotalErrors}");

        if (report.FailedResults.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("Failed Entities:");
            foreach (var result in report.FailedResults)
            {
                sb.AppendLine($"  • {result.EntityName} ({result.GeneratorType})");
                foreach (var error in result.Errors.Take(2))
                    sb.AppendLine($"    - {error}");
                if (result.Errors.Count > 2)
                    sb.AppendLine($"    ... and {result.Errors.Count - 2} more errors");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Report Generated: {report.ReportGeneratedAt:yyyy-MM-dd HH:mm:ss}");

        return sb.ToString();
    }

    public GenerationStatistics GetStatistics(IEnumerable<GenerationResult> results)
    {
        if (results == null)
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
        if (report == null)
            throw new ArgumentNullException(nameof(report));

        _logger.LogInformation("Exporting report to JSON format");

        var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });

        return await Task.FromResult(json);
    }

    private string ExtractErrorType(string error)
    {
        var parts = error.Split(':');
        return parts.Length > 0 ? parts[0] : "Unknown";
    }
}
