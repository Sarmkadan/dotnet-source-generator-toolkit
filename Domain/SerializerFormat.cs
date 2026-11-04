#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

public enum SerializerFormat
{
    /// <summary>
    /// JavaScript Object Notation (JSON) format.
    /// </summary>
    Json,
    
    /// <summary>
    /// Extensible Markup Language (XML) format.
    /// </summary>
    Xml,
    
    /// <summary>
    /// Binary format.
    /// </summary>
    Binary,
}
