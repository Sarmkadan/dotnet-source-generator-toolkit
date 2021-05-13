#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Pipeline;

/// <summary>
/// Extension methods for <see cref="PipelineResult"/> that provide additional functionality
/// for pipeline execution analysis, reporting, and result processing.
/// </summary>
public static class GenerationPipelineExtensions
{
    /// <summary>
    /// Creates a summary report of the pipeline execution results.
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>A formatted string containing execution statistics and summary</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string CreateSummaryReport(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Generation Pipeline Execution Report ===");
        report.AppendLine($"Status: {(result.IsSuccessful ? "SUCCESSFUL" : "FAILED")}");
        report.AppendLine($"Executed At: {result.ExecutedAt:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine();

        if (!result.IsSuccessful)
        {
            report.AppendLine("ERROR:");
            report.AppendLine(result.ErrorMessage ?? "Unknown error occurred");
            report.AppendLine();
        }

        report.AppendLine("STATISTICS:");
        report.AppendLine($" Entities Found: {result.EntitiesFound}");
        report.AppendLine($" Generated Files: {result.GeneratedFiles}");
        report.AppendLine($" Files Written: {result.FilesWritten}");

        if (result.EntitiesFound > 0)
        {
            var successRate = result.GeneratedFiles > 0
                ? Math.Round((double)result.GeneratedFiles / result.EntitiesFound * 100, 2)
                : 0;
            report.AppendLine($" Generation Rate: {successRate}% ({result.GeneratedFiles}/{result.EntitiesFound})");
        }

        if (result.GeneratedFiles > 0)
        {
            var writeRate = Math.Round((double)result.FilesWritten / result.GeneratedFiles * 100, 2);
            report.AppendLine($" Write Success Rate: {writeRate}% ({result.FilesWritten}/{result.GeneratedFiles})");
        }

        report.AppendLine();
        report.AppendLine("=== End of Report ===");

        return report.ToString();
    }

    /// <summary>
    /// Determines if the pipeline execution was efficient based on the number of entities and files.
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <param name="threshold">Minimum entities required to consider efficiency calculation</param>
    /// <returns>True if execution was efficient; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool WasExecutionEfficient(this PipelineResult result, int threshold = 5)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.EntitiesFound < threshold)
        {
            return true; // Small projects are always considered efficient
        }

        // Consider execution efficient if at least 80% of entities resulted in generated files
        var efficiencyThreshold = result.EntitiesFound * 0.8;
        return result.GeneratedFiles >= efficiencyThreshold;
    }

    /// <summary>
    /// Gets a human-readable status message for the pipeline execution.
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>A status message indicating the execution outcome</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static string GetStatusMessage(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsSuccessful)
        {
            return $"Pipeline failed: {result.ErrorMessage ?? "Unknown error"}";
        }

        if (result.EntitiesFound == 0)
        {
            return "Pipeline completed with no entities found";
        }

        if (result.GeneratedFiles == 0)
        {
            return "Pipeline completed but no files were generated";
        }

        if (result.FilesWritten == result.GeneratedFiles)
        {
            return $"Pipeline successful: {result.EntitiesFound} entities → {result.FilesWritten} files written";
        }

        return $"Pipeline successful: {result.EntitiesFound} entities → {result.GeneratedFiles} files generated ({result.FilesWritten} written)";
    }

    /// <summary>
    /// Creates a detailed performance metrics object from the pipeline execution.
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>Performance metrics including timing and efficiency calculations</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static PipelinePerformanceMetrics GetPerformanceMetrics(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var duration = DateTime.UtcNow - result.ExecutedAt;
        var totalSeconds = duration.TotalSeconds > 0 ? duration.TotalSeconds : 1;

        return new PipelinePerformanceMetrics
        {
            ExecutionDuration = duration,
            EntitiesPerSecond = result.EntitiesFound / totalSeconds,
            FilesPerEntity = result.EntitiesFound > 0
                ? (double)result.GeneratedFiles / result.EntitiesFound
                : 0,
            WriteSuccessRate = result.GeneratedFiles > 0
                ? (double)result.FilesWritten / result.GeneratedFiles * 100
                : 100,
            WasSuccessful = result.IsSuccessful,
            ErrorMessage = result.ErrorMessage
        };
    }

    /// <summary>
    /// Checks if the pipeline execution produced any output files.
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>True if files were written; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool HasOutputFiles(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.FilesWritten > 0;
    }

    /// <summary>
    /// Gets the success ratio of file generation (files written vs files generated).
    /// </summary>
    /// <param name="result">The pipeline result</param>
    /// <returns>A value between 0 and 1 representing the success ratio</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static double GetFileWriteSuccessRatio(this PipelineResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.GeneratedFiles == 0)
        {
            return 1.0; // No files generated means perfect ratio
        }

        return (double)result.FilesWritten / result.GeneratedFiles;
    }
}

/// <summary>
/// Performance metrics for pipeline execution.
/// </summary>
public sealed class PipelinePerformanceMetrics
{
    /// <summary>
    /// Total duration of the pipeline execution.
    /// </summary>
    public TimeSpan ExecutionDuration { get; set; }

    /// <summary>
    /// Entities processed per second.
    /// </summary>
    public double EntitiesPerSecond { get; set; }

    /// <summary>
    /// Ratio of generated files to entities found.
    /// </summary>
    public double FilesPerEntity { get; set; }

    /// <summary>
    /// Percentage of successfully written files.
    /// </summary>
    public double WriteSuccessRate { get; set; }

    /// <summary>
    /// Indicates if the pipeline execution was successful.
    /// </summary>
    public bool WasSuccessful { get; set; }

    /// <summary>
    /// Error message if execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}