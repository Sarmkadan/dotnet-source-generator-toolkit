#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for string manipulation and formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>Converts string to PascalCase.</summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The PascalCase string, or the original string if null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static string ToPascalCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return str;

        var words = str.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(string.Empty, words.Select(w =>
            w.Length > 0
                ? char.ToUpperInvariant(w[0]) + w[1..]
                : string.Empty));
    }

    /// <summary>Converts string to camelCase.</summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The camelCase string, or the original string if null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static string ToCamelCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return str;

        var pascalCase = ToPascalCase(str);
        return char.ToLowerInvariant(pascalCase[0]) + pascalCase[1..];
    }

    /// <summary>Converts string to snake_case.</summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The snake_case string, or the original string if null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static string ToSnakeCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return str;

        var result = System.Text.RegularExpressions.Regex.Replace(str, @"([A-Z])", "_$1").ToLowerInvariant();
        return result.StartsWith("_") ? result[1..] : result;
    }

    /// <summary>Converts string to kebab-case.</summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The kebab-case string, or the original string if null or empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static string ToKebabCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return str;

        var result = System.Text.RegularExpressions.Regex.Replace(str, @"([A-Z])", "-$1").ToLowerInvariant();
        return result.StartsWith("-") ? result[1..] : result;
    }

    /// <summary>Repeats the string n times.</summary>
    /// <param name="str">The string to repeat.</param>
    /// <param name="count">The number of times to repeat.</param>
    /// <returns>The repeated string, or empty string if count is zero or negative.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static string Repeat(this string str, int count)
    {
        ArgumentNullException.ThrowIfNull(str);

        return count <= 0 ? string.Empty : string.Concat(Enumerable.Repeat(str, count));
    }

    /// <summary>Truncates string to specified length with optional ellipsis.</summary>
    /// <param name="str">The string to truncate.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="addEllipsis">Whether to add ellipsis.</param>
    /// <returns>The truncated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string str, int maxLength, bool addEllipsis = true)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;

        var truncated = str[..maxLength];
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>Checks if string represents a numeric value.</summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string represents a numeric value; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static bool IsNumeric(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return false;

        // Support integers, decimals, and negative numbers
        return double.TryParse(str, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _);
    }

    /// <summary>Checks if string contains only letters (using invariant culture).</summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string contains only letters; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static bool IsLettersOnly(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return false;

        return str.All(char.IsLetter);
    }

    /// <summary>Counts word occurrences in string (case-insensitive).</summary>
    /// <param name="str">The string to search in.</param>
    /// <param name="word">The word to count.</param>
    /// <returns>The number of occurrences.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> or <paramref name="word"/> is null.</exception>
    public static int CountWord(this string str, string word)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentNullException.ThrowIfNull(word);

        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(word))
            return 0;

        // Use word boundaries to avoid partial matches
        var pattern = $"\\b{System.Text.RegularExpressions.Regex.Escape(word)}\\b";
        return System.Text.RegularExpressions.Regex.Matches(str, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
    }

    /// <summary>Gets lines from string, optionally removing empty lines.</summary>
    /// <param name="str">The string to split into lines.</param>
    /// <param name="removeEmpty">Whether to remove empty lines.</param>
    /// <returns>An array of lines.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    public static string[] GetLines(this string str, bool removeEmpty = false)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (string.IsNullOrEmpty(str))
            return [];

        var lines = str.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.None);
        return removeEmpty
            ? lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray()
            : lines;
    }

    /// <summary>Indents all lines by the specified number of spaces.</summary>
    /// <param name="str">The string to indent.</param>
    /// <param name="spaces">The number of spaces to indent.</param>
    /// <returns>The indented string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="spaces"/> is negative.</exception>
    public static string Indent(this string str, int spaces = 4)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentOutOfRangeException.ThrowIfNegative(spaces);

        if (string.IsNullOrEmpty(str))
            return str;

        var indent = new string(' ', spaces);
        return indent + str.Replace(Environment.NewLine, Environment.NewLine + indent);
    }

    /// <summary>Wraps text to specified line width.</summary>
    /// <param name="str">The text to wrap.</param>
    /// <param name="lineWidth">The maximum line width.</param>
    /// <returns>The wrapped text with line breaks.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="str"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="lineWidth"/> is zero or negative.</exception>
    public static string Wrap(this string str, int lineWidth = 80)
    {
        ArgumentNullException.ThrowIfNull(str);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(lineWidth);

        if (string.IsNullOrEmpty(str))
            return str;

        var words = str.Split(' ');
        var lines = new System.Collections.Generic.List<string>();
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            var potentialLine = string.IsNullOrEmpty(currentLine)
                ? word
                : currentLine + " " + word;

            if (potentialLine.Length > lineWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = potentialLine;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine);

        return string.Join(Environment.NewLine, lines);
    }
}