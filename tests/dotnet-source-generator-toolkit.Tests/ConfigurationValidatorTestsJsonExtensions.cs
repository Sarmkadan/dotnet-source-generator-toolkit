#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;

namespace DotNetSourceGeneratorToolkit.Tests;

/// <summary>
/// Provides JSON serialization and deserialization extension methods for the <see cref="ConfigurationValidatorTests"/> type.
/// </summary>
public static class ConfigurationValidatorTestsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes the specified <see cref="ConfigurationValidatorTests"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string representation of the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this ConfigurationValidatorTests value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = indented };
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ConfigurationValidatorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <see cref="ConfigurationValidatorTests"/> instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static ConfigurationValidatorTests? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        return JsonSerializer.Deserialize<ConfigurationValidatorTests>(json, _jsonSerializerOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ConfigurationValidatorTests"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized value if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null, empty, or whitespace.</exception>
    public static bool TryFromJson(string json, out ConfigurationValidatorTests? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = JsonSerializer.Deserialize<ConfigurationValidatorTests>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}