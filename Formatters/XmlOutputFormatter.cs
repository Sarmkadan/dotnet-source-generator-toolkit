// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Xml;
using System.Xml.Linq;

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Formats generation results as XML with proper element structure and attributes.
/// Useful for integration with tools that consume XML documents.
/// </summary>
public class XmlOutputFormatter : IOutputFormatter
{
    public string FileExtension => ".xml";
    public string FormatName => "XML";

    public string Format(IEnumerable<Domain.GenerationResult> results)
    {
        var resultsList = results.ToList();

        var root = new XElement("GenerationResults",
            new XAttribute("timestamp", DateTime.UtcNow.ToString("O")),
            new XAttribute("resultCount", resultsList.Count),
            new XElement("Metadata",
                new XElement("SuccessCount", resultsList.Count(r => r.Status == Domain.GenerationStatus.Completed)),
                new XElement("FailureCount", resultsList.Count(r => r.Status == Domain.GenerationStatus.Failed))
            ),
            new XElement("Results",
                resultsList.Select(r => new XElement("Result",
                    new XAttribute("entityName", r.EntityName),
                    new XAttribute("generatorType", r.GeneratorType),
                    new XAttribute("status", r.Status),
                    new XElement("OutputPath", r.OutputFilePath ?? string.Empty),
                    new XElement("CodeLength", r.GeneratedCode?.Length ?? 0),
                    new XElement("ExecutionTimeMs", r.ExecutionTimeMs),
                    r.ErrorMessage != null ? new XElement("Error", r.ErrorMessage) : null
                ).RemoveEmptyNodes())
            )
        );

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = System.Text.Encoding.UTF8,
        };

        using (var sw = new StringWriter())
        using (var writer = XmlWriter.Create(sw, settings))
        {
            root.WriteTo(writer);
            return sw.ToString();
        }
    }

    public async Task FormatToFileAsync(IEnumerable<Domain.GenerationResult> results, string filePath)
    {
        var xml = Format(results);
        await File.WriteAllTextAsync(filePath, xml, System.Text.Encoding.UTF8);
    }
}

/// <summary>
/// Extension method to remove empty elements from XML tree.
/// </summary>
internal static class XmlExtensions
{
    public static XElement? RemoveEmptyNodes(this XElement? element)
    {
        if (element == null) return null;

        // Remove elements that are null
        element.Elements().Where(e => e.IsEmpty).Remove();

        return element;
    }
}
