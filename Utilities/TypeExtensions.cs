#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for Type operations and introspection.
/// Simplifies common type checking and conversion patterns.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Check if a type is a nullable reference type or nullable value type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is nullable; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsNullable(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return !type.IsValueType || (Nullable.GetUnderlyingType(type) is not null);
    }

    /// <summary>
    /// Check if a type is a numeric type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a numeric type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsNumeric(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        return type.IsValueType &&
            (type == typeof(byte) || type == typeof(sbyte) ||
             type == typeof(short) || type == typeof(ushort) ||
             type == typeof(int) || type == typeof(uint) ||
             type == typeof(long) || type == typeof(ulong) ||
             type == typeof(float) || type == typeof(double) ||
             type == typeof(decimal));
    }

    /// <summary>
    /// Check if a type is a simple type (primitive or common value type).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a simple type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsSimpleType(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsValueType ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(Guid);
    }

    /// <summary>
    /// Get a friendly name for a type.
    /// </summary>
    /// <param name="type">The type to get the friendly name for.</param>
    /// <returns>A friendly name representation of the type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static string GetFriendlyName(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments()
                .Select(t => t.GetFriendlyName())
                .ToArray();

            var genericTypeName = type.Name.Substring(0, type.Name.IndexOf('`'));
            return $"{genericTypeName}<{string.Join(", ", genericArgs)}>";
        }

        return type.Name;
    }

    /// <summary>
    /// Get the default value for a type.
    /// </summary>
    /// <param name="type">The type to get the default value for.</param>
    /// <returns>The default value for the type, or null for reference types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static object? GetDefaultValue(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    /// <summary>
    /// Check if a type can be assigned to another type.
    /// </summary>
    /// <param name="type">The source type.</param>
    /// <param name="targetType">The target type to check assignability to.</param>
    /// <returns>True if the type can be assigned to the target type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> or <paramref name="targetType"/> is null.</exception>
    public static bool IsAssignableTo(this Type type, Type targetType)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(targetType);

        return targetType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Get all base types in the inheritance hierarchy.
    /// </summary>
    /// <param name="type">The type to get base types for.</param>
    /// <returns>An enumerable of base types in the inheritance hierarchy.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var baseType = type.BaseType;
        while (baseType is not null && baseType != typeof(object))
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

    /// <summary>
    /// Check if type is a collection type (but not string).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is a collection type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is null.</exception>
    public static bool IsCollection(this Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (type == typeof(string))
        {
            return false;
        }

        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }
}