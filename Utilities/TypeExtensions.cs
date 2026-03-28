// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for Type operations and introspection.
/// Simplifies common type checking and conversion patterns.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Check if a type is a nullable reference type.
    /// </summary>
    public static bool IsNullable(this Type type)
    {
        return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
    }

    /// <summary>
    /// Check if a type is a numeric type.
    /// </summary>
    public static bool IsNumeric(this Type type)
    {
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
    public static bool IsSimpleType(this Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.IsValueType ||
               underlyingType == typeof(string) ||
               underlyingType == typeof(DateTime) ||
               underlyingType == typeof(Guid);
    }

    /// <summary>
    /// Get a friendly name for a type.
    /// </summary>
    public static string GetFriendlyName(this Type type)
    {
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
    public static object? GetDefaultValue(this Type type)
    {
        if (type == null)
            return null;

        if (type.IsValueType)
            return Activator.CreateInstance(type);

        return null;
    }

    /// <summary>
    /// Check if a type can be assigned to another type.
    /// </summary>
    public static bool IsAssignableTo(this Type type, Type targetType)
    {
        return targetType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Get all base types in the inheritance hierarchy.
    /// </summary>
    public static IEnumerable<Type> GetBaseTypes(this Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null && baseType != typeof(object))
        {
            yield return baseType;
            baseType = baseType.BaseType;
        }
    }

    /// <summary>
    /// Check if type is a collection type (but not string).
    /// </summary>
    public static bool IsCollection(this Type type)
    {
        if (type == typeof(string))
            return false;

        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }
}
