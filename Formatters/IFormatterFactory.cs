#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Contract for formatter factory.
/// </summary>
public interface IFormatterFactory
{
    IOutputFormatter Create(string format);
    IEnumerable<string> GetAvailableFormats();
    bool IsFormatAvailable(string format);
}
