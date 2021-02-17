#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Generates serialization/deserialization code.
/// </summary>
public interface ISerializerGeneratorService
{
    /// <summary>
    /// Generates serializers for all entities.
    /// </summary>
    Task<IEnumerable<GenerationResult>> GenerateAllSerializersAsync(List<Entity> entities);

    /// <summary>
    /// Generates a serializer for a specific entity.
    /// </summary>
    Task<GenerationResult> GenerateSerializerAsync(Entity entity, SerializerFormat format);
}
