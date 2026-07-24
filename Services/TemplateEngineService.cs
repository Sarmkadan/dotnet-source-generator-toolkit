#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.Extensions.Logging;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("dotnet-source-generator-toolkit.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DotNetSourceGeneratorToolkit.Benchmarks")]

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Represents a single <c>{{#if}}</c>/<c>{{#else}}</c>/<c>{{/if}}</c> block extracted from a
/// raw template during compilation, together with the literal text that precedes it.
/// </summary>
/// <param name="LiteralBefore">Literal text found between the previous block (or start of template) and this block.</param>
/// <param name="Variable">Name of the context variable that controls the branch selected at render time.</param>
/// <param name="TrueContent">Content emitted when <see cref="Variable"/> resolves to a truthy value.</param>
/// <param name="FalseContent">Content emitted when <see cref="Variable"/> resolves to a falsy value (or is absent).</param>
internal readonly record struct ConditionalNode(string LiteralBefore, string Variable, string TrueContent, string FalseContent);

/// <summary>
/// Immutable, pre-parsed representation of a template's conditional structure.
/// Building one requires scanning the raw template text with a regular expression; once built it
/// can be reused across an unlimited number of <see cref="TemplateEngineService.RenderAsync(string, Dictionary{string, object}, GenerationOptions)"/>
/// calls for the same template text without repeating that scan.
/// </summary>
internal sealed class CompiledTemplate
{
    /// <summary>Initializes a new compiled template.</summary>
    /// <param name="rawTemplate">The original, unmodified template text this instance was compiled from.</param>
    /// <param name="conditionalNodes">The ordered list of conditional blocks found in <paramref name="rawTemplate"/>.</param>
    /// <param name="trailingLiteral">Literal text following the last conditional block, if any.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rawTemplate"/>, <paramref name="conditionalNodes"/> or <paramref name="trailingLiteral"/> is <see langword="null"/>.</exception>
    public CompiledTemplate(string rawTemplate, IReadOnlyList<ConditionalNode> conditionalNodes, string trailingLiteral)
    {
        ArgumentNullException.ThrowIfNull(rawTemplate);
        ArgumentNullException.ThrowIfNull(conditionalNodes);
        ArgumentNullException.ThrowIfNull(trailingLiteral);

        RawTemplate = rawTemplate;
        ConditionalNodes = conditionalNodes;
        TrailingLiteral = trailingLiteral;
    }

    /// <summary>The original, unmodified template text this instance was compiled from.</summary>
    public string RawTemplate { get; }

    /// <summary>The ordered list of conditional blocks found in the raw template.</summary>
    public IReadOnlyList<ConditionalNode> ConditionalNodes { get; }

    /// <summary>Literal text following the last conditional block, if any.</summary>
    public string TrailingLiteral { get; }
}

/// <summary>
/// Lightweight pool of reusable <see cref="StringBuilder"/> instances used to avoid repeated
/// large-object allocations while rendering templates. Rented builders must be returned via
/// <see cref="Return(StringBuilder)"/> once they are no longer needed.
/// </summary>
internal static class StringBuilderPool
{
    private const int MaxPooledCapacity = 8192;

    private static readonly ConcurrentBag<StringBuilder> Pool = [];

    /// <summary>Rents a cleared <see cref="StringBuilder"/> from the pool, creating a new one if the pool is empty.</summary>
    /// <returns>An empty <see cref="StringBuilder"/> ready for use.</returns>
    public static StringBuilder Rent() => Pool.TryTake(out var builder) ? builder : new StringBuilder(256);

    /// <summary>Returns a <see cref="StringBuilder"/> to the pool for reuse.</summary>
    /// <param name="builder">The builder to return.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <see langword="null"/>.</exception>
    public static void Return(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.Capacity > MaxPooledCapacity)
            return;

        builder.Clear();
        Pool.Add(builder);
    }
}

/// <summary>
/// Provides template processing and rendering functionality for code generation.
/// Supports variable substitution, conditional blocks, and loops.
/// </summary>
public interface ITemplateEngineService
{
    /// <summary>Renders a template with the provided context variables.</summary>
    Task<string> RenderAsync(string template, Dictionary<string, object> context);

    /// <summary>Renders a template with the provided context variables and generation options.</summary>
    /// <param name="template">Template to render.</param>
    /// <param name="context">Context variables.</param>
    /// <param name="options">Generation options to apply.</param>
    /// <returns>Rendered template with generation options applied.</returns>
    Task<string> RenderAsync(string template, Dictionary<string, object> context, GenerationOptions options);

    /// <summary>Loads a template from file and renders it.</summary>
    Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context);

    /// <summary>Loads a template from file and renders it with generation options.</summary>
    /// <param name="templatePath">Path to template file.</param>
    /// <param name="context">Context variables.</param>
    /// <param name="options">Generation options to apply.</param>
    /// <returns>Rendered template with generation options applied.</returns>
    Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context, GenerationOptions options);

    /// <summary>Validates template syntax.</summary>
    bool ValidateTemplate(string template);

    /// <summary>Registers a custom filter for template processing.</summary>
    void RegisterFilter(string filterName, Func<object, string> filterFunc);
}

public sealed class TemplateEngineService : ITemplateEngineService
{
    private static readonly Regex ConditionalsRegex = new(
        @"{{#if\s+(\w+)}}(.*?)(?:{{#else}}(.*?))?{{/if}}",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex LoopsRegex = new(
        @"{{#for\s+(\w+)\s+in\s+(\w+)}}(.*?){{/for}}",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex FiltersRegex = new(
        @"{{\s*(\w+)\s*|\s*(\w+)\s*}}",
        RegexOptions.Compiled);

    private static readonly Regex SnakeCasePattern = new(
        @">{{\s*snake_case\s*}}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex CamelCasePattern = new(
        @">{{\s*camelCase\s*}}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<TemplateEngineService> _logger;
    private readonly Dictionary<string, Func<object, string>> _filters = [];

    /// <summary>
    /// Cache of parsed template structures keyed by the raw template text. Since source generators
    /// re-render the same templates repeatedly (often once per keystroke in the IDE), this avoids
    /// re-scanning a template's conditional structure with a regular expression on every render.
    /// Entries live for the lifetime of this service instance (typically one compilation).
    /// </summary>
    internal readonly ConcurrentDictionary<string, CompiledTemplate> TemplateCache = new();

    public TemplateEngineService(IFileSystemService fileSystemService, ILogger<TemplateEngineService> logger)
    {
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        RegisterDefaultFilters();
    }

    public async Task<string> RenderAsync(string template, Dictionary<string, object> context)
    {
        return await RenderAsync(template, context, GenerationOptions.Default);
    }

    public async Task<string> RenderAsync(string template, Dictionary<string, object> context, GenerationOptions options)
    {
        if (string.IsNullOrEmpty(template))
            return template;

        if (context is null)
            context = [];

        _logger.LogInformation("Rendering template with {Count} context variables", context.Count);

        try
        {
            // Parse (or fetch from cache) the template's conditional structure. Parsing happens
            // once per distinct template text for the lifetime of this service instance; variable
            // substitution and conditional branch selection - which are context-dependent - still
            // run on every call.
            var compiled = TemplateCache.GetOrAdd(template, CompileTemplate);

            // Process variable substitutions and conditional blocks together
            var result = RenderConditionals(compiled, context);

            // Process loops
            result = ProcessLoops(result, context);

            // Process filters
            result = ProcessFilters(result, context);

            // Process special case transforms: {{snake_case}} and {{camelCase}}
            result = ProcessSpecialCaseTransforms(result);

            // Apply generation options to the result
            result = ApplyGenerationOptions(result, options);

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
        return await RenderFromFileAsync(templatePath, context, GenerationOptions.Default);
    }

    public async Task<string> RenderFromFileAsync(string templatePath, Dictionary<string, object> context, GenerationOptions options)
    {
        if (string.IsNullOrWhiteSpace(templatePath))
            throw new ArgumentNullException(nameof(templatePath));

        _logger.LogInformation("Loading template from file: {TemplatePath}", templatePath);

        try
        {
            var template = await _fileSystemService.ReadFileAsync(templatePath);
            return await RenderAsync(template, context, options);
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

    /// <summary>
    /// Parses the conditional (<c>{{#if}}</c>/<c>{{#else}}</c>/<c>{{/if}}</c>) structure out of a raw
    /// template exactly once. The result is cached in <see cref="TemplateCache"/> so subsequent renders
    /// of the same template text reuse the parse instead of re-running the regular expression.
    /// </summary>
    /// <param name="rawTemplate">The raw, unmodified template text.</param>
    /// <returns>A <see cref="CompiledTemplate"/> describing the template's conditional structure.</returns>
    private CompiledTemplate CompileTemplate(string rawTemplate)
    {
        var nodes = new List<ConditionalNode>();
        var lastIndex = 0;

        foreach (Match match in ConditionalsRegex.Matches(rawTemplate))
        {
            var literalBefore = rawTemplate[lastIndex..match.Index];
            var variable = match.Groups[1].Value;
            var trueContent = match.Groups[2].Value;
            var falseContent = match.Groups[3].Success ? match.Groups[3].Value : string.Empty;

            nodes.Add(new ConditionalNode(literalBefore, variable, trueContent, falseContent));
            lastIndex = match.Index + match.Length;
        }

        var trailingLiteral = rawTemplate[lastIndex..];
        return new CompiledTemplate(rawTemplate, nodes, trailingLiteral);
    }

    /// <summary>
    /// Reassembles the rendered text from a <see cref="CompiledTemplate"/>: applies variable
    /// substitution to each literal segment and picks the true/false branch of every conditional
    /// block based on <paramref name="context"/>. This replaces re-scanning the raw template for
    /// conditional tags on every render.
    /// </summary>
    /// <param name="compiled">The pre-parsed template structure.</param>
    /// <param name="context">Context variables used for substitution and branch selection.</param>
    /// <returns>The template text with variables substituted and conditionals resolved.</returns>
    private string RenderConditionals(CompiledTemplate compiled, Dictionary<string, object> context)
    {
        var builder = StringBuilderPool.Rent();
        try
        {
            foreach (var node in compiled.ConditionalNodes)
            {
                builder.Append(ProcessVariables(node.LiteralBefore, context));

                var branch = context.TryGetValue(node.Variable, out var value) && IsTruthy(value)
                    ? node.TrueContent
                    : node.FalseContent;
                builder.Append(ProcessVariables(branch, context));
            }

            builder.Append(ProcessVariables(compiled.TrailingLiteral, context));
            return builder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing conditional blocks in template");
            return compiled.RawTemplate;
        }
        finally
        {
            StringBuilderPool.Return(builder);
        }
    }

    private string ProcessLoops(string template, Dictionary<string, object> context)
    {
        try
        {
            return LoopsRegex.Replace(template, match =>
            {
                var itemVar = match.Groups[1].Value;
                var collectionVar = match.Groups[2].Value;
                var content = match.Groups[3].Value;

                if (!context.TryGetValue(collectionVar, out var collection))
                    return string.Empty;

                if (collection is not IEnumerable<object> enumerable)
                    return string.Empty;

                var result = StringBuilderPool.Rent();
                try
                {
                    foreach (var item in enumerable)
                    {
                        var itemContext = new Dictionary<string, object>(context) { [itemVar] = item };
                        result.Append(ProcessVariables(content, itemContext));
                    }

                    return result.ToString();
                }
                finally
                {
                    StringBuilderPool.Return(result);
                }
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
            return FiltersRegex.Replace(template, match =>
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
            template = SnakeCasePattern.Replace(
                template,
                match => ToSnakeCase(match.Value.Replace("{{snake_case}}", "").Trim())
            );

            // Process {{camelCase}} transform
            template = CamelCasePattern.Replace(
                template,
                match => ToCamelCase(match.Value.Replace("{{camelCase}}", "").Trim())
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

        var result = StringBuilderPool.Rent();
        try
        {
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
        finally
        {
            StringBuilderPool.Return(result);
        }
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = StringBuilderPool.Rent();
        try
        {
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
        finally
        {
            StringBuilderPool.Return(result);
        }
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

    /// <summary>
    /// Applies generation options to the rendered template to produce a complete source file.
    /// </summary>
    /// <param name="renderedTemplate">The rendered template content.</param>
    /// <param name="options">Generation options to apply.</param>
    /// <returns>Complete source file with all generation options applied.</returns>
    private string ApplyGenerationOptions(string renderedTemplate, GenerationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var lines = new List<string>();

        // Add language version directive if specified
        var langVersionDirective = options.GetLangVersionDirective();
        if (!string.IsNullOrEmpty(langVersionDirective))
        {
            lines.Add(langVersionDirective);
        }

        // Add nullable directive
        var nullableDirective = options.GetNullableDirective();
        if (!string.IsNullOrEmpty(nullableDirective))
        {
            lines.Add(nullableDirective);
        }

        // Add header comment
        var header = options.GetHeaderComment();
        if (!string.IsNullOrEmpty(header))
        {
            lines.Add(header);
        }

        // Add [GeneratedCode] attribute if enabled
        var generatedCodeAttribute = options.GetGeneratedCodeAttribute("TemplateEngine", "1.0.0");
        if (!string.IsNullOrEmpty(generatedCodeAttribute))
        {
            lines.Add(generatedCodeAttribute);
        }

        // Add the rendered template content
        lines.Add(renderedTemplate);

        return string.Join(Environment.NewLine, lines);
    }

    private int CountOccurrences(string text, string search)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
            return 0;

        return (text.Length - text.Replace(search, string.Empty).Length) / search.Length;
    }
}