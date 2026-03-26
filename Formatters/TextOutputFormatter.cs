// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Human-readable text format for generation results.
/// Designed for console output and quick visual inspection.
/// </summary>
public class TextOutputFormatter : IOutputFormatter
{
    public string FileExtension => ".txt";
    public string FormatName => "Text";

    public string Format(IEnumerable<Domain.GenerationResult> results)
    {
        var sb = new StringBuilder();
        var resultsList = results.ToList();

        sb.AppendLine("╔════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║          Generation Results Summary                        ║");
        sb.AppendLine("╚════════════════════════════════════════════════════════════╝");
        sb.AppendLine();

        // Summary statistics
        var successCount = resultsList.Count(r => r.Status == Domain.GenerationStatus.Completed);
        var failureCount = resultsList.Count(r => r.Status == Domain.GenerationStatus.Failed);

        sb.AppendLine($"Total Results:    {resultsList.Count}");
        sb.AppendLine($"Successful:       {successCount}");
        sb.AppendLine($"Failed:           {failureCount}");
        sb.AppendLine($"Generated At:     {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine();

        // Detailed results
        if (resultsList.Count > 0)
        {
            sb.AppendLine("╔════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                     Detailed Results                       ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            var groupedByStatus = resultsList.GroupBy(r => r.Status);

            foreach (var statusGroup in groupedByStatus.OrderBy(g => g.Key))
            {
                sb.AppendLine($"[{statusGroup.Key}] ({statusGroup.Count()} items)");
                sb.AppendLine();

                foreach (var result in statusGroup)
                {
                    sb.AppendLine($"  Entity:         {result.EntityName}");
                    sb.AppendLine($"  Generator:      {result.GeneratorType}");
                    sb.AppendLine($"  Output File:    {result.OutputFilePath}");
                    sb.AppendLine($"  Code Size:      {result.GeneratedCode?.Length ?? 0} bytes");
                    sb.AppendLine($"  Execution Time: {result.ExecutionTimeMs}ms");

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        sb.AppendLine($"  Error:          {result.ErrorMessage}");
                    }

                    sb.AppendLine();
                }
            }
        }

        return sb.ToString();
    }

    public async Task FormatToFileAsync(IEnumerable<Domain.GenerationResult> results, string filePath)
    {
        var text = Format(results);
        await File.WriteAllTextAsync(filePath, text, Encoding.UTF8);
    }
}
