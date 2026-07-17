#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Provides System.Text.Json serialization support for <see cref="FilePathValidator"/> operations.
/// </summary>
public static class FilePathValidatorJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    /// <summary>
    /// Serializes a file path validation result to a JSON string.
    /// </summary>
    /// <param name="path">The file path to validate and serialize.</param>
    /// <param name="indented">Whether to indent the JSON for readability.</param>
    /// <returns>A JSON string containing the validation result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> is empty or consists only of whitespace.</exception>
    public static string ToJson(this string path, bool indented = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);

        var result = new FilePathValidationResult
        {
            IsValid = FilePathValidator.IsValidPath(path),
            Path = path,
        };

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true }
            : _jsonOptions;

        return JsonSerializer.Serialize(result, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a file path validation result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A validation result containing the path and its validity.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of whitespace.</exception>
    public static FilePathValidationResult? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        return JsonSerializer.Deserialize<FilePathValidationResult>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a file path validation result.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized validation result if successful.</param>
    /// <returns>True if deserialization succeeds; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="json"/> is empty or consists only of whitespace.</exception>
    public static bool TryFromJson(string json, out FilePathValidationResult? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            value = JsonSerializer.Deserialize<FilePathValidationResult>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Represents a file path validation result for serialization.
    /// </summary>
    public sealed class FilePathValidationResult
    {
        /// <summary>
        /// Gets or sets the file path that was validated.
        /// </summary>
        public required string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the path is valid.
        /// </summary>
        public required bool IsValid { get; set; }
    }
}