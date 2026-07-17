#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Repositories;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for EntityRepository.
/// </summary>
public static class EntityRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    /// <summary>
    /// Serializes the EntityRepository instance to a JSON string.
    /// </summary>
    /// <param name="value">The repository instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the repository.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this EntityRepository value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string into an EntityRepository instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized EntityRepository instance, or null if the JSON is empty or whitespace.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static EntityRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<EntityRepository>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string into an EntityRepository instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="result">Receives the deserialized EntityRepository instance if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static bool TryFromJson(string json, out EntityRepository? result)
    {
        ArgumentNullException.ThrowIfNull(json);

        result = null;

        return !string.IsNullOrWhiteSpace(json)
            && TryDeserialize(json, out result);

        static bool TryDeserialize(string json, out EntityRepository? result)
        {
            try
            {
                result = JsonSerializer.Deserialize<EntityRepository>(json, _jsonOptions);
                return true;
            }
            catch (JsonException)
            {
                result = null;
                return false;
            }
        }
    }
}