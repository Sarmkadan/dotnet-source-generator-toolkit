#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extensions for ConfigurationManager.
/// </summary>
public static class ConfigurationManagerJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Serializes the ConfigurationManager instance to a JSON string.
    /// </summary>
    /// <param name="value">The ConfigurationManager instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the ConfigurationManager.</returns>
    public static string ToJson(this ConfigurationManager value, bool indented = false)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        try
        {
            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value.GetAllConfig(), options);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to serialize ConfigurationManager to JSON", ex);
        }
    }

    /// <summary>
    /// Deserializes a JSON string to a ConfigurationManager instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A ConfigurationManager instance populated with the deserialized configuration.</returns>
    public static ConfigurationManager? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var configDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonSerializerOptions);

            if (configDict is null)
            {
                return null;
            }

            var loggerFactory = LoggerFactory.Create(builder => { });
            var logger = loggerFactory.CreateLogger<ConfigurationManager>();
            var configManager = new ConfigurationManager(logger);

            foreach (var kvp in configDict)
            {
                configManager.SetValue(kvp.Key, kvp.Value);
            }

            return configManager;
        }
        catch (JsonException)
        {
            return null;
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new InvalidOperationException("Failed to deserialize JSON to ConfigurationManager", ex);
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a ConfigurationManager instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The resulting ConfigurationManager instance, or null if deserialization fails.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    public static bool TryFromJson(string json, out ConfigurationManager? value)
    {
        try
        {
            value = FromJson(json);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}