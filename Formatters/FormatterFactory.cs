// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Formatters;

/// <summary>
/// Factory for creating output formatter instances based on format name.
/// Provides type-safe formatter selection and extensibility.
/// </summary>
public class FormatterFactory : IFormatterFactory
{
    private readonly Dictionary<string, Func<IOutputFormatter>> _formatters = new(StringComparer.OrdinalIgnoreCase);

    public FormatterFactory()
    {
        // Register built-in formatters
        Register("json", () => new JsonOutputFormatter());
        Register("csv", () => new CsvOutputFormatter());
        Register("xml", () => new XmlOutputFormatter());
        Register("text", () => new TextOutputFormatter());
    }

    public void Register(string format, Func<IOutputFormatter> factory)
    {
        _formatters[format] = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    public IOutputFormatter Create(string format)
    {
        if (!_formatters.TryGetValue(format, out var factory))
        {
            throw new ArgumentException(
                $"Unknown format: {format}. Available formats: {string.Join(", ", _formatters.Keys)}",
                nameof(format));
        }

        return factory();
    }

    public IEnumerable<string> GetAvailableFormats()
    {
        return _formatters.Keys.OrderBy(k => k);
    }

    public bool IsFormatAvailable(string format)
    {
        return _formatters.ContainsKey(format);
    }
}

/// <summary>
/// Contract for formatter factory.
/// </summary>
public interface IFormatterFactory
{
    IOutputFormatter Create(string format);
    IEnumerable<string> GetAvailableFormats();
    bool IsFormatAvailable(string format);
}
