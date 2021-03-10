#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Exceptions;

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ValidationException"/> to simplify common operations
/// like error aggregation, filtering, and conversion to other formats.
/// </summary>
public static class ValidationExceptionExtensions
{
    /// <summary>
    /// Combines multiple <see cref="ValidationException"/> instances into a single exception.
    /// Useful when aggregating validation failures from multiple sources.
    /// </summary>
    /// <param name="exceptions">Collection of validation exceptions to combine</param>
    /// <returns>A new ValidationException containing all errors from all exceptions</returns>
    public static ValidationException Combine(this IEnumerable<ValidationException> exceptions)
    {
        if (exceptions == null)
        {
            throw new ArgumentNullException(nameof(exceptions));
        }

        var allErrors = new List<string>();

        foreach (var exception in exceptions.Where(e => e != null))
        {
            allErrors.AddRange(exception.Errors);
        }

        if (allErrors.Count == 0)
        {
            throw new ArgumentException("No validation errors found in the provided exceptions", nameof(exceptions));
        }

        return new ValidationException("Multiple validation failures occurred", allErrors);
    }

    /// <summary>
    /// Adds additional error messages to an existing ValidationException.
    /// Useful for accumulating errors during validation pipeline execution.
    /// </summary>
    /// <param name="exception">The validation exception to extend</param>
    /// <param name="errors">Additional error messages to add</param>
    /// <returns>The same ValidationException instance for method chaining</returns>
    public static ValidationException AddErrors(this ValidationException exception, params string[] errors)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (errors != null && errors.Length > 0)
        {
            exception.Errors.AddRange(errors.Where(e => !string.IsNullOrWhiteSpace(e)));
        }

        return exception;
    }

    /// <summary>
    /// Filters validation errors by a predicate.
    /// Useful for selective error handling or conditional validation.
    /// </summary>
    /// <param name="exception">The validation exception to filter</param>
    /// <param name="predicate">Predicate to filter errors</param>
    /// <returns>A new ValidationException containing only the filtered errors</returns>
    public static ValidationException FilterErrors(this ValidationException exception, Func<string, bool> predicate)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        var filteredErrors = exception.Errors.Where(predicate).ToList();

        return new ValidationException("Filtered validation errors", filteredErrors);
    }

    /// <summary>
    /// Converts a ValidationException to a dictionary of error messages grouped by error type or pattern.
    /// Useful for structured error reporting and API responses.
    /// </summary>
    /// <param name="exception">The validation exception to convert</param>
    /// <returns>Dictionary mapping error categories to lists of error messages</returns>
    public static Dictionary<string, List<string>> ToErrorDictionary(this ValidationException exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        var dictionary = new Dictionary<string, List<string>>();

        foreach (var error in exception.Errors)
        {
            var key = GetErrorCategory(error);
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = new List<string>();
                dictionary[key] = list;
            }
            list.Add(error);
        }

        return dictionary;
    }

    /// <summary>
    /// Gets whether the validation exception contains any errors matching a specific pattern.
    /// Useful for quick existence checks without enumerating all errors.
    /// </summary>
    /// <param name="exception">The validation exception to check</param>
    /// <param name="predicate">Predicate to test each error</param>
    /// <returns>True if any error matches the predicate, false otherwise</returns>
    public static bool HasError(this ValidationException exception, Func<string, bool> predicate)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return exception.Errors.Any(predicate);
    }

    /// <summary>
    /// Gets the first error message that matches a specific pattern.
    /// Useful for extracting specific error details when the pattern is known.
    /// </summary>
    /// <param name="exception">The validation exception to search</param>
    /// <param name="predicate">Predicate to test each error</param>
    /// <returns>The first matching error message, or null if no match found</returns>
    public static string? GetFirstError(this ValidationException exception, Func<string, bool> predicate)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return exception.Errors.FirstOrDefault(predicate);
    }

    private static string GetErrorCategory(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return "Unknown";
        }

        // Simple categorization based on error content
        if (error.Contains("null", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("required", StringComparison.OrdinalIgnoreCase))
        {
            return "NullOrRequired";
        }

        if (error.Contains("format", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("pattern", StringComparison.OrdinalIgnoreCase))
        {
            return "FormatOrPattern";
        }

        if (error.Contains("range", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("min", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("max", StringComparison.OrdinalIgnoreCase))
        {
            return "RangeValidation";
        }

        if (error.Contains("type", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("cast", StringComparison.OrdinalIgnoreCase))
        {
            return "TypeValidation";
        }

        return "General";
    }
}