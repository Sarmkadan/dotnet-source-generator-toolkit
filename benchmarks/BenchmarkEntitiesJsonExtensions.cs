#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Provides System.Text.Json serialization extensions for BenchmarkEntities nested types
/// </summary>
public static class BenchmarkEntitiesJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Serializes a BenchmarkEntities nested entity instance to a JSON string
    /// </summary>
    /// <typeparam name="T">The BenchmarkEntities nested entity type (e.g., SimpleEntity, ComplexEntity).</typeparam>
    /// <param name="value">The entity instance to serialize. If <see langword="null"/>, throws <see cref="ArgumentNullException"/>.</param>
    /// <param name="indented">Whether to format the JSON with indentation for human-readable output.</param>
    /// <returns>A JSON string representation of the entity.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson<T>(this T value, bool indented = false) where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions)
            {
                WriteIndented = true
            }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a BenchmarkEntities nested entity from JSON string
    /// </summary>
    /// <typeparam name="T">The BenchmarkEntities nested entity type to deserialize.</typeparam>
    /// <param name="json">JSON string to deserialize. If <see langword="null"/>, throws <see cref="ArgumentNullException"/>.</param>
    /// <returns>Deserialized entity instance or <see langword="null"/> if invalid JSON or input is whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static T? FromJson<T>(string json) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a BenchmarkEntities nested entity from JSON string
    /// </summary>
    /// <typeparam name="T">The BenchmarkEntities nested entity type to deserialize.</typeparam>
    /// <param name="json">JSON string to deserialize. If <see langword="null"/> or whitespace, returns <see langword="false"/>.</param>
    /// <param name="value">Output parameter for deserialized value.</param>
    /// <returns>True if deserialization succeeded, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson<T>(string json, out T? value) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        try
        {
            value = JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}