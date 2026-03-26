// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Analyzes C# source code to extract attribute information.
/// Uses regex patterns for quick parsing without full compilation.
/// </summary>
public class AttributeAnalyzer : IAttributeAnalyzer
{
    private readonly ILogger<AttributeAnalyzer> _logger;

    // Pattern to match attributes: [AttributeName] or [AttributeName(...)]
    private static readonly Regex AttributePattern = new(
        @"\[\s*(\w+(?:\.\w+)*)\s*(?:\((.*?)\))?\s*\]",
        RegexOptions.Compiled | RegexOptions.Singleline);

    public AttributeAnalyzer(ILogger<AttributeAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IEnumerable<AttributeInfo> AnalyzeAttributes(string sourceCode)
    {
        if (string.IsNullOrEmpty(sourceCode))
            return new List<AttributeInfo>();

        var attributes = new List<AttributeInfo>();
        var matches = AttributePattern.Matches(sourceCode);

        foreach (Match match in matches)
        {
            var attributeName = match.Groups[1].Value;
            var parametersText = match.Groups[2].Value;

            var attributeInfo = new AttributeInfo
            {
                Name = attributeName,
                Parameters = ParseParameters(parametersText),
            };

            attributes.Add(attributeInfo);
        }

        _logger.LogDebug("Found {Count} attributes in source code", attributes.Count);
        return attributes;
    }

    public bool HasAttribute(string sourceCode, string attributeName)
    {
        if (string.IsNullOrEmpty(sourceCode) || string.IsNullOrEmpty(attributeName))
            return false;

        return sourceCode.Contains($"[{attributeName}]", StringComparison.OrdinalIgnoreCase) ||
               AttributePattern.IsMatch(sourceCode) &&
               AnalyzeAttributes(sourceCode).Any(a =>
                   a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
    }

    public Dictionary<string, string>? GetAttributeParameters(string sourceCode, string attributeName)
    {
        var attributes = AnalyzeAttributes(sourceCode);
        var attribute = attributes.FirstOrDefault(a =>
            a.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

        return attribute?.Parameters;
    }

    private static Dictionary<string, string> ParseParameters(string parametersText)
    {
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(parametersText))
            return parameters;

        // Simple parameter parsing: supports "key = value" pairs
        var paramPattern = new Regex(@"(\w+)\s*=\s*[""']?(.*?)[""']?(?:,|$)", RegexOptions.Compiled);
        var paramMatches = paramPattern.Matches(parametersText);

        foreach (Match match in paramMatches)
        {
            if (match.Groups.Count >= 3)
            {
                var key = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();
                parameters[key] = value;
            }
        }

        return parameters;
    }
}
