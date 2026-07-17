#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for string validation utilities.
/// </summary>
public static class StringValidatorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes an object to JSON.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the object.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an instance of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A deserialized instance of type T, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static T? FromJson<T>(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an instance of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized instance if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson<T>(string json, out T? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        value = default;

        if (string.IsNullOrWhiteSpace(json))
        {
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}