// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Result of a validation operation with errors and warnings.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; } = true;
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();

    /// <summary>
    /// Add an error and mark validation as failed.
    /// </summary>
    public void AddError(string message)
    {
        Errors.Add(message);
        IsValid = false;
    }

    /// <summary>
    /// Add a warning without affecting validity.
    /// </summary>
    public void AddWarning(string message)
    {
        Warnings.Add(message);
    }

    /// <summary>
    /// Check if validation has any issues.
    /// </summary>
    public bool HasIssues => Errors.Count > 0 || Warnings.Count > 0;

    /// <summary>
    /// Get all messages (errors and warnings).
    /// </summary>
    public IEnumerable<string> GetAllMessages()
    {
        return Errors.Concat(Warnings);
    }

    /// <summary>
    /// Format validation result as readable string.
    /// </summary>
    public override string ToString()
    {
        if (IsValid && Warnings.Count == 0)
            return "Validation passed.";

        var lines = new List<string>();

        if (Errors.Count > 0)
        {
            lines.Add($"Errors ({Errors.Count}):");
            foreach (var error in Errors)
            {
                lines.Add($"  - {error}");
            }
        }

        if (Warnings.Count > 0)
        {
            lines.Add($"Warnings ({Warnings.Count}):");
            foreach (var warning in Warnings)
            {
                lines.Add($"  - {warning}");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }
}
