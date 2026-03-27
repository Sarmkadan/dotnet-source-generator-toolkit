// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Helper utilities for reflection-based operations.
/// Simplifies common reflection patterns used throughout the toolkit.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Get all public properties of a type.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Get all public methods of a type.
    /// </summary>
    public static IEnumerable<MethodInfo> GetPublicMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    /// <summary>
    /// Check if a type implements a specific interface.
    /// </summary>
    public static bool ImplementsInterface(Type type, Type interfaceType)
    {
        return type.GetInterfaces().Contains(interfaceType);
    }

    /// <summary>
    /// Get all types in an assembly that implement a specific interface.
    /// </summary>
    public static IEnumerable<Type> GetImplementations(Assembly assembly, Type interfaceType)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsInterface && !t.IsAbstract && ImplementsInterface(t, interfaceType));
    }

    /// <summary>
    /// Create an instance of a type using its parameterless constructor.
    /// </summary>
    public static object? CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get custom attributes of a specific type from a type.
    /// </summary>
    public static IEnumerable<T> GetCustomAttributes<T>(Type type) where T : Attribute
    {
        return type.GetCustomAttributes(typeof(T), inherit: true).Cast<T>();
    }

    /// <summary>
    /// Get all base types up the inheritance hierarchy.
    /// </summary>
    public static IEnumerable<Type> GetBaseTypes(Type type)
    {
        var current = type.BaseType;
        while (current != null && current != typeof(object))
        {
            yield return current;
            current = current.BaseType;
        }
    }

    /// <summary>
    /// Check if a property is auto-implemented (has backing field).
    /// </summary>
    public static bool IsAutoProperty(PropertyInfo property)
    {
        return property.GetGetMethod()?.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) == true;
    }
}
