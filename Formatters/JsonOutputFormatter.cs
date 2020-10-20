// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Formats generation results as JSON with pretty-printing for readability.
/// Includes metadata about generation execution and results.
/// </summary>
public class JsonOutputFormatter : IOutputFormatter
{
    public string FileExtension => ".json";
    public string FormatName => "JSON";

    public string Format(IEnumerable<Domain.GenerationResult> results)
    {
        var resultsList = results.ToList();

        var output = new
        {
            metadata = new
            {
                generatedAt = DateTime.UtcNow,
                resultCount = resultsList.Count,
                successCount = resultsList.Count(r => r.Status == Domain.GenerationStatus.Completed),
                failureCount = resultsList.Count(r => r.Status == Domain.GenerationStatus.Failed),
            },
            results = resultsList.Select(r => new
            {
                r.EntityName,
                r.GeneratorType,
                r.Status,
                r.OutputFilePath,
                r.GeneratedCode?.Length,
                r.ExecutionTimeMs,
                r.ErrorMessage,
            }),
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return JsonSerializer.Serialize(output, options);
    }

    public async Task FormatToFileAsync(IEnumerable<Domain.GenerationResult> results, string filePath)
    {
        var json = Format(results);
        await File.WriteAllTextAsync(filePath, json);
    }
}
