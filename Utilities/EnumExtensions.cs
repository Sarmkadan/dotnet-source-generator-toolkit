// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static string GetDescription<T>(this T value) where T : Enum
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var attribute = field.GetCustomAttribute<System.ComponentModel.DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Get all values of an enum type.
    /// </summary>
    public static IEnumerable<T> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Try to parse a string to enum value with case-insensitive matching.
    /// </summary>
    public static bool TryParse<T>(string value, out T result) where T : Enum
    {
        return Enum.TryParse<T>(value, ignoreCase: true, out result);
    }

    /// <summary>
    /// Check if enum has a specific flag.
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        return value.HasFlag(flag);
    }

    /// <summary>
    /// Convert enum to CSV-friendly string representation.
    /// </summary>
    public static string ToCsv<T>(this IEnumerable<T> values) where T : Enum
    {
        return string.Join(",", values.Select(v => v.ToString()));
    }
}
