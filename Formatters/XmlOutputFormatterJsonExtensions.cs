#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for <see cref="XmlOutputFormatter"/>.
/// </summary>
public static class XmlOutputFormatterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes the <see cref="XmlOutputFormatter"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="XmlOutputFormatter"/> instance to serialize. Must not be <see langword="null"/>.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the <see cref="XmlOutputFormatter"/> instance.
    /// The serialized JSON includes the <see cref="XmlOutputFormatter.FileExtension"/> and <see cref="XmlOutputFormatter.FormatName"/> properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this XmlOutputFormatter value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to an <see cref="XmlOutputFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/> or empty.</param>
    /// <returns>An <see cref="XmlOutputFormatter"/> instance deserialized from JSON, or <see langword="null"/> if the JSON represents a null value.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="JsonException">The JSON is invalid or cannot be deserialized to an <see cref="XmlOutputFormatter"/> instance.</exception>
    public static XmlOutputFormatter? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<XmlOutputFormatter>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to an <see cref="XmlOutputFormatter"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize. Must not be <see langword="null"/> or empty.</param>
    /// <param name="value">Receives the deserialized <see cref="XmlOutputFormatter"/> instance if successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out XmlOutputFormatter? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<XmlOutputFormatter>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}