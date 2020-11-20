#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Analyzes C# source files using Regex patterns to extract entity definitions
/// including class names, properties, and metadata.
/// </summary>
public sealed class EntityAnalyzer : IEntityAnalyzer
{
    private readonly ILogger<EntityAnalyzer> _logger;

    public EntityAnalyzer(ILogger<EntityAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Entity>> AnalyzeFileAsync(string filePath, string content)
    {
        if (string.IsNullOrEmpty(content))
            return [];

        _logger.LogInformation("Analyzing file: {FilePath}", filePath);

        var entities = new List<Entity>();

        try
        {
            // Extract namespace
            var namespaceMatch = Regex.Match(content, @"namespace\s+(\S+)");
            var fileNamespace = namespaceMatch.Success ? namespaceMatch.Groups[1].Value : "UnknownNamespace";

            // Find all public classes
            var classMatches = Regex.Matches(content, @"(?:public\s+)?(?:partial\s+)?(?:abstract\s+)?(?:sealed\s+)?class\s+(\w+)\s*(?::\s*([^{]+))?\s*{", RegexOptions.Multiline);

            foreach (Match classMatch in classMatches)
            {
                var className = classMatch.Groups[1].Value;
                var baseClass = classMatch.Groups[2].Value?.Trim();

                _logger.LogInformation("Found class: {ClassName}", className);

                var entity = new Entity
                {
                    Name = className,
                    Namespace = fileNamespace,
                    BaseClass = baseClass,
                };

                // Extract properties — the \?? at the end of the type group captures nullable
                // annotations such as `AddressRecord?` so they are not silently dropped.
                var propPattern = @"public\s+(\w+(?:<[^>]+>)?\??)\s+(\w+)\s*{\s*get;?\s*set;?\s*}";
                var propMatches = Regex.Matches(content[classMatch.Index..], propPattern);

                foreach (Match propMatch in propMatches)
                {
                    var rawPropType = propMatch.Groups[1].Value;
                    var propName = propMatch.Groups[2].Value;
                    var isNullable = rawPropType.EndsWith('?');
                    // Store the base type without the nullable suffix; IsNullable carries that flag.
                    var propType = isNullable ? rawPropType[..^1] : rawPropType;

                    var property = new EntityProperty
                    {
                        Name = propName,
                        Type = propType,
                        IsNullable = isNullable,
                        IsRequired = !isNullable,
                    };

                    entity.AddProperty(property);
                }

                // Extract attributes
                var attrPattern = @"\[(\w+(?:\([^)]*\))?)\]";
                var attrMatches = Regex.Matches(content[Math.Max(0, classMatch.Index - 200)..classMatch.Index], attrPattern);

                foreach (Match attrMatch in attrMatches)
                {
                    var attr = attrMatch.Groups[1].Value;
                    entity.Attributes.Add(attr);
                }

                entities.Add(entity);
            }

            _logger.LogInformation("Extracted {Count} entities from file", entities.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file: {FilePath}", filePath);
            throw new EntityAnalysisException($"Failed to analyze file {filePath}", ex);
        }

        return await Task.FromResult(entities);
    }

    public async Task<Entity> ParseClassAsync(string className, string classContent, string fileNamespace)
    {
        if (string.IsNullOrWhiteSpace(className))
            throw new ArgumentNullException(nameof(className));

        if (string.IsNullOrWhiteSpace(classContent))
            throw new ArgumentNullException(nameof(classContent));

        _logger.LogInformation("Parsing class: {ClassName}", className);

        var entity = new Entity
        {
            Name = className,
            Namespace = fileNamespace,
        };

        // Extract properties — the \?? at the end of the type group captures nullable
        // annotations such as `AddressRecord?` so they are not silently dropped.
        var propPattern = @"(?:\w+\s+)?public\s+(\w+(?:<[^>]+>)?\??)\s+(\w+)\s*{\s*get;?\s*set;?\s*}";
        var matches = Regex.Matches(classContent, propPattern);

        foreach (Match match in matches)
        {
            var rawType = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            var isNullable = rawType.EndsWith('?');
            var type = isNullable ? rawType[..^1] : rawType;

            var property = new EntityProperty
            {
                Name = name,
                Type = type,
                IsNullable = isNullable,
                IsRequired = !isNullable,
            };

            entity.AddProperty(property);
        }

        return await Task.FromResult(entity);
    }
}


