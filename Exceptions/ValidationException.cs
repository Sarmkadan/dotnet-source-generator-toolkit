#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Thrown when entity or configuration validation fails.
/// Aggregates multiple validation errors for detailed reporting.
/// </summary>
public sealed class ValidationException : GenerationException
{
    /// <summary>
    /// Gets the list of validation error messages.
    /// </summary>
    public List<string> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// with a specified error message and collection of validation errors.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="errors">The collection of validation error messages.</param>
    public ValidationException(string message, IEnumerable<string> errors)
        : base(message)
    {
        Errors = new List<string>(errors ?? Enumerable.Empty<string>());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// with a specified error message and a single validation error.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="error">The single validation error message.</param>
    public ValidationException(string message, string error)
        : base(message)
    {
        Errors = new List<string> { error };
    }

    /// <summary>
    /// Returns a string representation of the exception, including the base exception message
    /// and all validation error messages.
    /// </summary>
    /// <returns>A string that represents the current exception.</returns>
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
