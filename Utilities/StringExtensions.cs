#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for string manipulation and formatting.
/// </summary>
public static class StringExtensions
{
    /// <summary>Converts string to PascalCase.</summary>
    public static string ToPascalCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var words = str.Split(new[] { '_', ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(string.Empty, words.Select(w => char.ToUpper(w[0]) + w[1..]));
    }

    /// <summary>Converts string to camelCase.</summary>
    public static string ToCamelCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var pascalCase = ToPascalCase(str);
        return char.ToLower(pascalCase[0]) + pascalCase[1..];
    }

    /// <summary>Converts string to snake_case.</summary>
    public static string ToSnakeCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var result = System.Text.RegularExpressions.Regex.Replace(str, @"([A-Z])", "_$1").ToLower();
        return result.StartsWith("_") ? result[1..] : result;
    }

    /// <summary>Converts string to kebab-case.</summary>
    public static string ToKebabCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var result = System.Text.RegularExpressions.Regex.Replace(str, @"([A-Z])", "-$1").ToLower();
        return result.StartsWith("-") ? result[1..] : result;
    }

    /// <summary>Repeats the string n times.</summary>
    public static string Repeat(this string str, int count)
    {
        if (string.IsNullOrEmpty(str) || count <= 0)
            return string.Empty;

        return string.Concat(System.Linq.Enumerable.Repeat(str, count));
    }

    /// <summary>Truncates string to specified length with optional ellipsis.</summary>
    public static string Truncate(this string str, int maxLength, bool addEllipsis = true)
    {
        if (string.IsNullOrEmpty(str) || str.Length <= maxLength)
            return str;

        var truncated = str[..maxLength];
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>Checks if string is numeric.</summary>
    public static bool IsNumeric(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        return str.All(char.IsDigit);
    }

    /// <summary>Checks if string contains only letters.</summary>
    public static bool IsLettersOnly(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return false;

        return str.All(char.IsLetter);
    }

    /// <summary>Counts word occurrences in string.</summary>
    public static int CountWord(this string str, string word)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(word))
            return 0;

        return System.Text.RegularExpressions.Regex.Matches(str, System.Text.RegularExpressions.Regex.Escape(word), System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
    }

    /// <summary>Gets lines from string, optionally removing empty lines.</summary>
    public static string[] GetLines(this string str, bool removeEmpty = false)
    {
        if (string.IsNullOrEmpty(str))
            return [];

        var lines = str.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.None);
        return removeEmpty ? lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray() : lines;
    }

    /// <summary>Indents all lines by the specified number of spaces.</summary>
    public static string Indent(this string str, int spaces = 4)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        var indent = new string(' ', spaces);
        return indent + str.Replace(Environment.NewLine, Environment.NewLine + indent);
    }

    /// <summary>Wraps text to specified line width.</summary>
    public static string Wrap(this string str, int lineWidth = 80)
    {
        if (string.IsNullOrEmpty(str) || lineWidth <= 0)
            return str;

        var words = str.Split(' ');
        var lines = new System.Collections.Generic.List<string>();
        var currentLine = string.Empty;

        foreach (var word in words)
        {
            if ((currentLine + word).Length > lineWidth && !string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
            }
        }

        if (!string.IsNullOrEmpty(currentLine))
            lines.Add(currentLine);

        return string.Join(Environment.NewLine, lines);
    }
}


