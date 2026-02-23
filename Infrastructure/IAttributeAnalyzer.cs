// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Contract for analyzing attributes on entities to determine generation requirements.
/// Extracts metadata from custom attributes to drive code generation.
/// </summary>
public interface IAttributeAnalyzer
{
    /// <summary>
    /// Extract attributes from source code text.
    /// </summary>
    /// <param name="sourceCode">C# source code to analyze</param>
    /// <returns>List of found attributes with their configuration</returns>
    IEnumerable<AttributeInfo> AnalyzeAttributes(string sourceCode);

    /// <summary>
    /// Check if source code contains a specific attribute.
    /// </summary>
    /// <param name="sourceCode">C# source code to search</param>
    /// <param name="attributeName">Attribute name to find (case-insensitive)</param>
    /// <returns>True if attribute is present</returns>
    bool HasAttribute(string sourceCode, string attributeName);

    /// <summary>
    /// Extract attribute parameters from the source code.
    /// </summary>
    /// <param name="sourceCode">C# source code</param>
    /// <param name="attributeName">Attribute to get parameters for</param>
    /// <returns>Attribute parameters if found</returns>
    Dictionary<string, string>? GetAttributeParameters(string sourceCode, string attributeName);
}

/// <summary>
/// Information about a discovered attribute.
/// </summary>
public class AttributeInfo
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, string> Parameters { get; set; } = new();
    public int LineNumber { get; set; }
}
