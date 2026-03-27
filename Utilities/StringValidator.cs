// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Provides string validation utilities for common patterns.
/// Used to validate identifiers, namespaces, and other textual input.
/// </summary>
public static class StringValidator
{
    // C# identifier pattern: letters, digits, underscores, not starting with digit
    private static readonly Regex IdentifierPattern = new(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    // Namespace pattern: dot-separated identifiers
    private static readonly Regex NamespacePattern = new(@"^[a-zA-Z_][a-zA-Z0-9_]*(\.[a-zA-Z_][a-zA-Z0-9_]*)*$", RegexOptions.Compiled);

    /// <summary>
    /// Validate that a string is a valid C# identifier.
    /// </summary>
    public static bool IsValidIdentifier(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && IdentifierPattern.IsMatch(value);
    }

    /// <summary>
    /// Validate that a string is a valid C# namespace.
    /// </summary>
    public static bool IsValidNamespace(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && NamespacePattern.IsMatch(value);
    }

    /// <summary>
    /// Validate that a string is not null, empty, or whitespace.
    /// </summary>
    public static bool IsNotEmpty(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Validate that a string matches a maximum length.
    /// </summary>
    public static bool IsMaxLength(string? value, int maxLength)
    {
        return value?.Length <= maxLength;
    }

    /// <summary>
    /// Sanitize a string for use in file paths by removing invalid characters.
    /// </summary>
    public static string SanitizeForFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unnamed";

        var invalidChars = new string(Path.GetInvalidFileNameChars());
        var sanitized = new Regex($"[{Regex.Escape(invalidChars)}]").Replace(value, "_");
        return sanitized;
    }

    /// <summary>
    /// Get validation error message for invalid identifier.
    /// </summary>
    public static string GetIdentifierError(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Identifier cannot be empty";

        if (!IdentifierPattern.IsMatch(value))
            return "Identifier must start with letter or underscore and contain only alphanumeric characters and underscores";

        return string.Empty;
    }
}
