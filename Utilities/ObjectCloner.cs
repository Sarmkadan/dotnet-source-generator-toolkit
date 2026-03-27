// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Provides deep cloning functionality using JSON serialization.
/// Useful for creating independent copies of entity objects.
/// </summary>
public static class ObjectCloner
{
    /// <summary>
    /// Create a deep clone of an object using JSON serialization.
    /// Requires the type to be JSON serializable.
    /// </summary>
    /// <typeparam name="T">Type of object to clone</typeparam>
    /// <param name="source">Original object to clone</param>
    /// <returns>Deep copy of the object</returns>
    public static T DeepClone<T>(T source) where T : class
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        try
        {
            var json = JsonSerializer.Serialize(source);
            var cloned = JsonSerializer.Deserialize<T>(json);
            return cloned ?? throw new InvalidOperationException("Failed to deserialize cloned object");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Object of type {typeof(T).Name} cannot be cloned via JSON serialization",
                ex);
        }
    }

    /// <summary>
    /// Try to create a deep clone, returning false if cloning fails.
    /// </summary>
    /// <typeparam name="T">Type of object to clone</typeparam>
    /// <param name="source">Original object to clone</param>
    /// <param name="cloned">Cloned object if successful</param>
    /// <returns>True if cloning succeeded</returns>
    public static bool TryDeepClone<T>(T source, out T? cloned) where T : class
    {
        cloned = null;

        if (source == null)
            return false;

        try
        {
            var json = JsonSerializer.Serialize(source);
            cloned = JsonSerializer.Deserialize<T>(json);
            return cloned != null;
        }
        catch
        {
            return false;
        }
    }
}
