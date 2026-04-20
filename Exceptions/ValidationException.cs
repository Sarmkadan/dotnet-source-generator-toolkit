// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Thrown when entity or configuration validation fails.
/// Aggregates multiple validation errors for detailed reporting.
/// </summary>
public class ValidationException : GenerationException
{
    public List<string> Errors { get; }

    public ValidationException(string message, IEnumerable<string> errors)
        : base(message)
    {
        Errors = new List<string>(errors ?? Enumerable.Empty<string>());
    }

    public ValidationException(string message, string error)
        : base(message)
    {
        Errors = new List<string> { error };
    }

    public override string ToString()
    {
        var message = base.ToString();

        if (Errors.Count > 0)
        {
            var errorList = string.Join("\n  - ", Errors);
            return $"{message}\n\nValidation Errors:\n  - {errorList}";
        }

        return message;
    }
}
