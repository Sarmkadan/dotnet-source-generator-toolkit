// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Service for formatting generated C# code to match style guidelines.
/// Handles indentation, spacing, and code organization.
/// </summary>
public interface IFormattingService
{
    /// <summary>
    /// Format C# code according to configured style.
    /// </summary>
    string FormatCode(string code);

    /// <summary>
    /// Get current formatting rules.
    /// </summary>
    FormattingRules GetRules();

    /// <summary>
    /// Update formatting rules.
    /// </summary>
    void SetRules(FormattingRules rules);
}

/// <summary>
/// Implementation of C# code formatting service.
/// </summary>
public class FormattingService : IFormattingService
{
    private FormattingRules _rules = new();
    private readonly ILogger<FormattingService> _logger;

    public FormattingService(ILogger<FormattingService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string FormatCode(string code)
    {
        if (string.IsNullOrEmpty(code))
            return code;

        try
        {
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var formatted = new List<string>();
            int indentLevel = 0;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (string.IsNullOrEmpty(trimmed))
                {
                    formatted.Add(string.Empty);
                    continue;
                }

                // Decrease indent for closing braces
                if (trimmed.StartsWith("}") || trimmed.StartsWith("]"))
                {
                    indentLevel = Math.Max(0, indentLevel - 1);
                }

                var indent = new string(' ', indentLevel * _rules.IndentSize);
                formatted.Add(indent + trimmed);

                // Increase indent for opening braces
                if (trimmed.EndsWith("{") || trimmed.EndsWith("("))
                {
                    indentLevel++;
                }
            }

            var result = string.Join(Environment.NewLine, formatted);
            _logger.LogDebug("Code formatting completed: {LineCount} lines", formatted.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Code formatting failed, returning original");
            return code;
        }
    }

    public FormattingRules GetRules()
    {
        return _rules;
    }

    public void SetRules(FormattingRules rules)
    {
        _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        _logger.LogInformation("Formatting rules updated");
    }
}

/// <summary>
/// Configuration for code formatting.
/// </summary>
public class FormattingRules
{
    public int IndentSize { get; set; } = 4;
    public bool UseTabs { get; set; }
    public int LineLength { get; set; } = 100;
    public bool AddBlankLineBeforeMethod { get; set; } = true;
    public bool RemoveTrailingWhitespace { get; set; } = true;
}
