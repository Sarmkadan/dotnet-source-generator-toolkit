// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Formats generation results as CSV for import into spreadsheets and data analysis tools.
/// Handles proper escaping of values containing special characters.
/// </summary>
public class CsvOutputFormatter : IOutputFormatter
{
    public string FileExtension => ".csv";
    public string FormatName => "CSV";

    public string Format(IEnumerable<Domain.GenerationResult> results)
    {
        var sb = new StringBuilder();

        // Write header
        sb.AppendLine("EntityName,GeneratorType,Status,OutputFilePath,CodeLength,ExecutionTimeMs,ErrorMessage");

        // Write data rows
        foreach (var result in results)
        {
            var row = new[]
            {
                EscapeCsvField(result.EntityName),
                EscapeCsvField(result.GeneratorType.ToString()),
                EscapeCsvField(result.Status.ToString()),
                EscapeCsvField(result.OutputFilePath ?? string.Empty),
                result.GeneratedCode?.Length.ToString() ?? string.Empty,
                result.ExecutionTimeMs.ToString(),
                EscapeCsvField(result.ErrorMessage ?? string.Empty),
            };

            sb.AppendLine(string.Join(",", row));
        }

        return sb.ToString();
    }

    public async Task FormatToFileAsync(IEnumerable<Domain.GenerationResult> results, string filePath)
    {
        var csv = Format(results);
        await File.WriteAllTextAsync(filePath, csv, Encoding.UTF8);
    }

    /// <summary>
    /// Escape CSV field value by wrapping in quotes if needed.
    /// Handles commas, quotes, and newlines per RFC 4180.
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}
