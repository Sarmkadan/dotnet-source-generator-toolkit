#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel;
using System.Reflection;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for enum operations and conversions.
/// Provides utilities for working with enum values and descriptions.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Get human-readable description of enum value from DescriptionAttribute.
    /// </summary>
    /// <param name="value">The enum value to get description for.</param>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>The DescriptionAttribute value if present; otherwise the enum value name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string GetDescription<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());
        if (field is null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Get all values of an enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>An enumerable of all enum values.</returns>
    /// <exception cref="ArgumentException">Thrown when <typeparamref name="T"/> is not an enum type.</exception>
    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Try to parse a string to enum value with case-insensitive matching.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <param name="result">When this method returns, contains the parsed enum value if successful; otherwise, the default value.</param>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>True if parsing succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool TryParse<T>(string value, out T result) where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        return Enum.TryParse<T>(value, ignoreCase: true, out result);
    }


    /// <summary>
    /// Convert enum values to CSV-friendly string representation.
    /// </summary>
    /// <param name="values">The enum values to convert.</param>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>A comma-separated string of enum values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public static string ToCsv<T>(this IEnumerable<T> values) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(values);
        return string.Join(",", values.Select(v => v?.ToString() ?? string.Empty));
    }
}
