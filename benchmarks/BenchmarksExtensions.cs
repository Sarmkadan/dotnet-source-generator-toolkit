#nullable enable

using System.Text;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Extension methods for the Benchmarks class providing additional functionality
/// for benchmark analysis, reporting, and configuration management
/// </summary>
public static class BenchmarksExtensions
{
    /// <summary>
    /// Formats benchmark results as a markdown table for documentation purposes
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="results">Benchmark results to format</param>
    /// <returns>Markdown formatted table with benchmark results</returns>
    public static string FormatBenchmarkResultsAsMarkdown(this Benchmarks benchmarks, Summary results)
    {
        if (results == null || results.Benchmarks == null || !results.Benchmarks.Any())
        {
            return "No benchmark results available.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("| Method | Mean | Error | StdDev | Median | Gen0 | Allocated |");
        sb.AppendLine("|-------|-------|-------|-------|-------|------|-----------|");

        foreach (var benchmark in results.Benchmarks)
        {
            var method = benchmark.Descriptor.WorkloadMethodDisplayInfo;
            var statistics = benchmark.ResultStatistics;
            var memory = benchmark.Memory;

            var mean = statistics?.Mean.ToString("N3") ?? "N/A";
            var error = statistics?.StandardError.ToString("N3") ?? "N/A";
            var stdDev = statistics?.StandardDeviation.ToString("N3") ?? "N/A";
            var median = statistics?.Median.ToString("N3") ?? "N/A";
            var gen0 = memory?.GetValue(GcStats.Gen0Collections)?.ToString() ?? "N/A";
            var allocated = memory?.GetValue(GcStats.BytesAllocatedPerOperation)?.ToString("N0") ?? "N/A";

            sb.AppendLine($"| {method} | {mean} | {error} | {stdDev} | {median} | {gen0} | {allocated} |");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the slowest benchmark from the results
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="results">Benchmark results to analyze</param>
    /// <returns>Benchmark with the highest mean execution time, or null if no results</returns>
    public static BenchmarkReport? GetSlowestBenchmark(this Benchmarks benchmarks, Summary results)
    {
        if (results?.Benchmarks == null || !results.Benchmarks.Any())
        {
            return null;
        }

        return results.Benchmarks
            .OrderByDescending(b => b.ResultStatistics?.Mean)
            .First();
    }

    /// <summary>
    /// Gets the fastest benchmark from the results
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="results">Benchmark results to analyze</param>
    /// <returns>Benchmark with the lowest mean execution time, or null if no results</returns>
    public static BenchmarkReport? GetFastestBenchmark(this Benchmarks benchmarks, Summary results)
    {
        if (results?.Benchmarks == null || !results.Benchmarks.Any())
        {
            return null;
        }

        return results.Benchmarks
            .OrderBy(b => b.ResultStatistics?.Mean)
            .First();
    }

    /// <summary>
    /// Calculates the total execution time for all benchmarks in the results
    /// </summary>
    /// <param name="benchmarks">The benchmarks instance</param>
    /// <param name="results">Benchmark results to analyze</param>
    /// <returns>Total execution time in milliseconds, or 0 if no results</returns>
    public static double CalculateTotalExecutionTime(this Benchmarks benchmarks, Summary results)
    {
        if (results?.Benchmarks == null || !results.Benchmarks.Any())
        {
            return 0;
        }

        return results.Benchmarks.Sum(b => b.ResultStatistics?.Mean ?? 0);
    }
}