// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Provides template processing and rendering functionality for code generation.
/// Supports variable substitution, conditional blocks, and loops.
/// </summary>
public interface ITemplateEngineService
{
    /// <summary>Renders a template with the provided context variables.</summary>
    Task<string> RenderAsync(string template, Dictionary<string, object> context);

    /// <summary>Loads a template from file and renders it.</summary>
    Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context);

    /// <summary>Validates template syntax.</summary>
    bool ValidateTemplate(string template);

    /// <summary>Registers a custom filter for template processing.</summary>
    void RegisterFilter(string filterName, Func<object, string> filterFunc);
}

public class TemplateEngineService : ITemplateEngineService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<TemplateEngineService> _logger;
    private readonly Dictionary<string, Func<object, string>> _filters = [];

    public TemplateEngineService(IFileSystemService fileSystemService, ILogger<TemplateEngineService> logger)
    {
        _fileSystemService = fileSystemService;
        _logger = logger;
        RegisterDefaultFilters();
    }

    public async Task<string> RenderAsync(string template, Dictionary<string, object> context)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        if (context == null)
            context = [];

        _logger.LogInformation("Rendering template with {Count} context variables", context.Count);

        var result = template;

        // Process variable substitutions
        foreach (var kvp in context)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            var value = kvp.Value?.ToString() ?? string.Empty;
            result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        // Process conditional blocks
        result = ProcessConditionals(result, context);

        // Process loops
        result = ProcessLoops(result, context);

        // Process filters
        result = ProcessFilters(result, context);

        _logger.LogInformation("Template rendered successfully ({0} characters)", result.Length);
        return await Task.FromResult(result);
    }

    public async Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            throw new ArgumentNullException(nameof(templatePath));

        _logger.LogInformation("Loading template from file: {TemplatePath}", templatePath);

        var template = await _fileSystemService.ReadFileAsync(templatePath);
        return await RenderAsync(template, context);
    }

    public bool ValidateTemplate(string template)
    {
        if (string.IsNullOrEmpty(template))
            return false;

        var errors = new List<string>();

        // Check for balanced conditional tags
        var ifCount = CountOccurrences(template, "{{#if");
        var endIfCount = CountOccurrences(template, "{{/if}}");
        if (ifCount != endIfCount)
            errors.Add($"Unmatched if tags: {ifCount} opening, {endIfCount} closing");

        // Check for balanced loop tags
        var forCount = CountOccurrences(template, "{{#for");
        var endForCount = CountOccurrences(template, "{{/for}}");
        if (forCount != endForCount)
            errors.Add($"Unmatched for tags: {forCount} opening, {endForCount} closing");

        if (errors.Count > 0)
        {
            _logger.LogWarning("Template validation found {Count} errors", errors.Count);
            return false;
        }

        _logger.LogInformation("Template validation passed");
        return true;
    }

    public void RegisterFilter(string filterName, Func<object, string> filterFunc)
    {
        if (string.IsNullOrWhiteSpace(filterName))
            throw new ArgumentNullException(nameof(filterName));

        if (filterFunc == null)
            throw new ArgumentNullException(nameof(filterFunc));

        _filters[filterName] = filterFunc;
        _logger.LogInformation("Registered custom filter: {FilterName}", filterName);
    }

    private void RegisterDefaultFilters()
    {
        _filters["upper"] = obj => obj?.ToString()?.ToUpper() ?? string.Empty;
        _filters["lower"] = obj => obj?.ToString()?.ToLower() ?? string.Empty;
        _filters["capitalize"] = obj =>
        {
            var str = obj?.ToString();
            return string.IsNullOrEmpty(str) ? str : char.ToUpper(str[0]) + str[1..];
        };
        _filters["pluralize"] = obj =>
        {
            var str = obj?.ToString();
            if (string.IsNullOrEmpty(str)) return str;
            return str.EndsWith("y") ? str[..^1] + "ies" : str + "s";
        };
        _filters["trim"] = obj => obj?.ToString()?.Trim() ?? string.Empty;

        _logger.LogInformation("Registered {Count} default filters", _filters.Count);
    }

    private string ProcessConditionals(string template, Dictionary<string, object> context)
    {
        var pattern = @"{{#if\s+(\w+)}}(.*?){{/if}}";
        var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline);

        return regex.Replace(template, match =>
        {
            var variable = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (context.TryGetValue(variable, out var value) && IsTruthy(value))
                return content;

            return string.Empty;
        });
    }

    private string ProcessLoops(string template, Dictionary<string, object> context)
    {
        var pattern = @"{{#for\s+(\w+)\s+in\s+(\w+)}}(.*?){{/for}}";
        var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline);

        return regex.Replace(template, match =>
        {
            var itemVar = match.Groups[1].Value;
            var collectionVar = match.Groups[2].Value;
            var content = match.Groups[3].Value;

            if (!context.TryGetValue(collectionVar, out var collection))
                return string.Empty;

            if (collection is not IEnumerable<object> enumerable)
                return string.Empty;

            var result = new StringBuilder();
            foreach (var item in enumerable)
            {
                var itemContext = new Dictionary<string, object>(context) { [itemVar] = item };
                result.Append(ProcessVariables(content, itemContext));
            }

            return result.ToString();
        });
    }

    private string ProcessFilters(string template, Dictionary<string, object> context)
    {
        var pattern = @"{{\s*(\w+)\s*\|\s*(\w+)\s*}}";
        var regex = new System.Text.RegularExpressions.Regex(pattern);

        return regex.Replace(template, match =>
        {
            var variable = match.Groups[1].Value;
            var filterName = match.Groups[2].Value;

            if (!context.TryGetValue(variable, out var value))
                return string.Empty;

            if (_filters.TryGetValue(filterName, out var filter))
                return filter(value);

            return value?.ToString() ?? string.Empty;
        });
    }

    private string ProcessVariables(string template, Dictionary<string, object> context)
    {
        foreach (var kvp in context)
        {
            var placeholder = $"{{{{{kvp.Key}}}}}";
            var value = kvp.Value?.ToString() ?? string.Empty;
            template = template.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private bool IsTruthy(object value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            string s => !string.IsNullOrEmpty(s),
            int i => i != 0,
            long l => l != 0,
            _ => true,
        };
    }

    private int CountOccurrences(string text, string search)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
            return 0;

        return (text.Length - text.Replace(search, string.Empty).Length) / search.Length;
    }
}
