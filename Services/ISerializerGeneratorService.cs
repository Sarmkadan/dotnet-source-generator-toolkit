// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Contract for generating serialization/deserialization code.
/// Creates JSON, XML, and binary serializers with custom handling.
/// </summary>
public interface ISerializerGeneratorService
{
    /// <summary>
    /// Generate serializer for a single entity.
    /// </summary>
    Task<GenerationResult> GenerateSerializerAsync(Entity entity, SerializationFormat format);

    /// <summary>
    /// Generate serializers for multiple entities in specified format.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllSerializersAsync(
        IEnumerable<Entity> entities,
        SerializationFormat format);

    /// <summary>
    /// Generate serialization methods only.
    /// </summary>
    Task<string> GenerateSerializationMethodsAsync(Entity entity, SerializationFormat format);

    /// <summary>
    /// Generate deserialization methods only.
    /// </summary>
    Task<string> GenerateDeserializationMethodsAsync(Entity entity, SerializationFormat format);
}

/// <summary>
/// Supported serialization formats.
/// </summary>
public enum SerializationFormat
{
    Json,
    Xml,
    Binary,
    Csv,
}
