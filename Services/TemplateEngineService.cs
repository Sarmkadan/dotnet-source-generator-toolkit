#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
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

public sealed class TemplateEngineService : ITemplateEngineService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<TemplateEngineService> _logger;
    private readonly Dictionary<string, Func<object, string>> _filters = [];

    public TemplateEngineService(IFileSystemService fileSystemService, ILogger<TemplateEngineService> logger)
    {
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RegisterDefaultFilters();
    }

    public async Task<string> RenderAsync(string template, Dictionary<string, object> context)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        if (context is null)
            context = [];

        _logger.LogInformation("Rendering template with {Count} context variables", context.Count);

        try
        {
            var result = template;

            // Process variable substitutions
            foreach (var kvp in context)
            {
                var placeholder = "{{" + kvp.Key + "}}";
                var value = kvp.Value?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
            }

            // Process conditional blocks
            result = ProcessConditionals(result, context);

            // Process loops
            result = ProcessLoops(result, context);

            // Process filters
            result = ProcessFilters(result, context);

            // Process special case transforms: {{snake_case}} and {{camelCase}}
            result = ProcessSpecialCaseTransforms(result);

            _logger.LogInformation("Template rendered successfully ({Length} characters)", new { Length = result.Length });
            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering template");
            throw new GenerationException("Error rendering template", ex);
        }
    }

    public async Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            throw new ArgumentNullException(nameof(templatePath));

        _logger.LogInformation("Loading template from file: {TemplatePath}", templatePath);

        try
        {
            var template = await _fileSystemService.ReadFileAsync(templatePath);
            return await RenderAsync(template, context);
        }
        catch (FileSystemException ex)
        {
            _logger.LogError(ex, "File system error loading template from file: {TemplatePath}", templatePath);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template from file: {TemplatePath}", templatePath);
            throw new GenerationException($"Error loading template from file {templatePath}", ex);
        }
    }

    public bool ValidateTemplate(string template)
    {
        if (string.IsNullOrEmpty(template))
            return false;

        try
        {
            var errors = new List<string>();

            // Check for balanced conditional tags
            var ifCount = CountOccurrences(template, "{{#if");
            var endIfCount = CountOccurrences(template, "{{/if}}");
            if (ifCount != endIfCount)
                errors.Add($"Unmatched if tags: {ifCount} opening, {endIfCount} closing");

            // {{#else}} must appear inside an if block, so its count cannot exceed the number of {{#if}}
            var elseCount = CountOccurrences(template, "{{#else}}");
            if (elseCount > ifCount)
                errors.Add($"More {{#else}} tags ({elseCount}) than {{#if}} blocks ({ifCount})");

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating template");
            throw new GenerationException("Error validating template syntax", ex);
        }
    }

    public void RegisterFilter(string filterName, Func<object, string> filterFunc)
    {
        if (string.IsNullOrWhiteSpace(filterName))
            throw new ArgumentNullException(nameof(filterName));

        if (filterFunc is null)
            throw new ArgumentNullException(nameof(filterFunc));

        try
        {
            _filters[filterName] = filterFunc;
            _logger.LogInformation("Registered custom filter: {FilterName}", filterName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering custom filter: {FilterName}", filterName);
            throw new GenerationException($"Error registering filter '{filterName}'", ex);
        }
    }

    private void RegisterDefaultFilters()
    {
        try
        {
            _filters["upper"] = obj => obj?.ToString()?.ToUpper() ?? string.Empty;
            _filters["lower"] = obj => obj?.ToString()?.ToLower() ?? string.Empty;
            _filters["capitalize"] = obj =>
            {
                var str = obj?.ToString();
                if (string.IsNullOrEmpty(str)) return string.Empty;
                return str.Length == 1 ? char.ToUpper(str[0]).ToString() : char.ToUpper(str[0]) + str[1..];
            };
            _filters["camelCase"] = obj =>
            {
                var str = obj?.ToString();
                if (string.IsNullOrEmpty(str)) return string.Empty;
                return str.Length == 1 ? char.ToLower(str[0]).ToString() : char.ToLower(str[0]) + str[1..];
            };
            _filters["pluralize"] = obj =>
            {
                var str = obj?.ToString();
                if (string.IsNullOrEmpty(str)) return string.Empty;
                if (str.EndsWith("ss") || str.EndsWith("sh") || str.EndsWith("ch") || str.EndsWith("x") || str.EndsWith("z"))
                    return str + "es";
                if (str.EndsWith("y") && str.Length > 1 && !"aeiou".Contains(str[^2]))
                    return str[..^1] + "ies";
                return str + "s";
            };
            _filters["trim"] = obj => obj?.ToString()?.Trim() ?? string.Empty;

            _logger.LogInformation("Registered {Count} default filters", _filters.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering default filters");
            throw new GenerationException("Error registering default filters", ex);
        }
    }

    private string ProcessConditionals(string template, Dictionary<string, object> context)
    {
        try
        {
            // Match {{#if var}}...{{#else}}...{{/if}} or {{#if var}}...{{/if}}
            var pattern = @"{{#if\s+(\w+)}}(.*?)(?:{{#else}}(.*?))?{{/if}}";
            var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.Singleline);

            return regex.Replace(template, match =>
            {
                var variable = match.Groups[1].Value;
                var trueContent = match.Groups[2].Value;
                var falseContent = match.Groups[3].Success ? match.Groups[3].Value : string.Empty;

                if (context.TryGetValue(variable, out var value) && IsTruthy(value))
                    return trueContent;

                return falseContent;
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing conditional blocks in template");
            return template;
        }
    }

    private string ProcessLoops(string template, Dictionary<string, object> context)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing loop blocks in template");
            return template;
        }
    }

    private string ProcessFilters(string template, Dictionary<string, object> context)
    {
        try
        {
            var pattern = @"{{\s*(\w+)\s*|\s*(\w+)\s*}}";
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing filters in template");
            return template;
        }
    }

    private string ProcessVariables(string template, Dictionary<string, object> context)
    {
        try
        {
            foreach (var kvp in context)
            {
                var placeholder = "{{" + kvp.Key + "}}";
                var value = kvp.Value?.ToString() ?? string.Empty;
                template = template.Replace(placeholder, value, StringComparison.OrdinalIgnoreCase);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing variables in template");
            return template;
        }
    }

    private string ProcessSpecialCaseTransforms(string template)
    {
        try
        {
            // Process {{snake_case}} transform
            var snakeCasePattern = @">{{\s*snake_case\s*}}";
            template = System.Text.RegularExpressions.Regex.Replace(
                template,
                snakeCasePattern,
                match => ToSnakeCase(match.Value.Replace("{{snake_case}}", "").Trim()),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Process {{camelCase}} transform
            var camelCasePattern = @">{{\s*camelCase\s*}}";
            template = System.Text.RegularExpressions.Regex.Replace(
                template,
                camelCasePattern,
                match => ToCamelCase(match.Value.Replace("{{camelCase}}", "").Trim()),
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing special case transforms in template");
            return template;
        }
    }

    private string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0 && !char.IsUpper(input[i - 1]))
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(input[i]));
        }
        return result.ToString();
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        bool firstChar = true;
        for (int i = 0; i < input.Length; i++)
        {
            if (firstChar)
            {
                result.Append(char.ToLowerInvariant(input[i]));
                firstChar = false;
            }
            else if (input[i] == '_')
            {
                if (i + 1 < input.Length)
                {
                    result.Append(char.ToUpperInvariant(input[i + 1]));
                    i++;
                }
            }
            else
            {
                result.Append(input[i]);
            }
        }
        return result.ToString();
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