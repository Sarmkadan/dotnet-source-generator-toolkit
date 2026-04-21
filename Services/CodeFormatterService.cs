// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Formats and normalizes generated C# code with consistent indentation,
/// spacing, and code style conventions.
/// </summary>
public interface ICodeFormatterService
{
    /// <summary>Formats C# code with proper indentation and spacing.</summary>
    string FormatCode(string code);

    /// <summary>Adds header comments to generated code.</summary>
    string AddFileHeader(string code, string generatorType, string entityName);

    /// <summary>Removes trailing whitespace from code.</summary>
    string TrimWhitespace(string code);

    /// <summary>Normalizes line endings to Unix format.</summary>
    string NormalizeLineEndings(string code);

    /// <summary>Validates C# code syntax correctness.</summary>
    bool ValidateSyntax(string code);
}

public class CodeFormatterService : ICodeFormatterService
{
    private const int INDENT_SIZE = 4;
    private readonly ILogger<CodeFormatterService> _logger;

    public CodeFormatterService(ILogger<CodeFormatterService> logger)
    {
        _logger = logger;
    }

    public string FormatCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        _logger.LogInformation("Formatting code ({0} characters)", code.Length);

        var lines = code.Split(Environment.NewLine);
        var formatted = new StringBuilder();
        var indentLevel = 0;
        var inMultilineComment = false;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Handle multi-line comments
            if (trimmed.StartsWith("/*"))
                inMultilineComment = true;
            if (inMultilineComment && trimmed.EndsWith("*/"))
                inMultilineComment = false;

            // Adjust indent for closing braces
            if (trimmed.StartsWith("}") || trimmed.StartsWith("]") || trimmed.StartsWith(")"))
                indentLevel = Math.Max(0, indentLevel - 1);

            // Skip empty lines but preserve structure
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                var indent = new string(' ', indentLevel * INDENT_SIZE);
                formatted.AppendLine(indent + trimmed);
            }
            else if (formatted.Length > 0)
            {
                formatted.AppendLine();
            }

            // Adjust indent for opening braces
            if (trimmed.EndsWith("{") || trimmed.EndsWith("[") || trimmed.EndsWith("("))
                indentLevel++;
        }

        var result = formatted.ToString().TrimEnd();
        _logger.LogInformation("Code formatted successfully");
        return result;
    }

    public string AddFileHeader(string code, string generatorType, string entityName)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        var header = new StringBuilder();
        header.AppendLine("// =============================================================================");
        header.AppendLine("// Author: Vladyslav Zaiets | https://sarmkadan.com");
        header.AppendLine("// CTO & Software Architect");
        header.AppendLine("// =============================================================================");
        header.AppendLine();
        header.AppendLine($"// Generated {generatorType} for entity: {entityName}");
        header.AppendLine($"// Generation timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
        header.AppendLine();

        _logger.LogInformation("Added file header for {GeneratorType} - {EntityName}", generatorType, entityName);

        return header.ToString() + code;
    }

    public string TrimWhitespace(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        var lines = code.Split(Environment.NewLine);
        var trimmed = lines.Select(l => l.TrimEnd()).ToList();

        // Remove trailing empty lines
        while (trimmed.Count > 0 && string.IsNullOrWhiteSpace(trimmed[^1]))
            trimmed.RemoveAt(trimmed.Count - 1);

        var result = string.Join(Environment.NewLine, trimmed);
        _logger.LogDebug("Trimmed whitespace from code");
        return result;
    }

    public string NormalizeLineEndings(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        // Convert all line endings to Unix format (LF)
        var normalized = code
            .Replace("\r\n", "\n")  // Windows to Unix
            .Replace("\r", "\n");   // Old Mac to Unix

        _logger.LogDebug("Normalized line endings to Unix format");
        return normalized;
    }

    public bool ValidateSyntax(string code)
    {
        if (string.IsNullOrEmpty(code))
            return false;

        var errors = new List<string>();

        // Check for matching braces
        var openBraces = code.Count(c => c == '{');
        var closeBraces = code.Count(c => c == '}');
        if (openBraces != closeBraces)
            errors.Add($"Mismatched braces: {openBraces} open, {closeBraces} close");

        // Check for matching parentheses
        var openParens = code.Count(c => c == '(');
        var closeParens = code.Count(c => c == ')');
        if (openParens != closeParens)
            errors.Add($"Mismatched parentheses: {openParens} open, {closeParens} close");

        // Check for matching brackets
        var openBrackets = code.Count(c => c == '[');
        var closeBrackets = code.Count(c => c == ']');
        if (openBrackets != closeBrackets)
            errors.Add($"Mismatched brackets: {openBrackets} open, {closeBrackets} close");

        // Check for namespace and using statements
        if (!code.Contains("namespace ") && !code.Contains("using "))
            errors.Add("Code must contain namespace or using declarations");

        if (errors.Count > 0)
        {
            _logger.LogWarning("Syntax validation found {Count} errors", errors.Count);
            return false;
        }

        _logger.LogInformation("Syntax validation passed");
        return true;
    }
}
