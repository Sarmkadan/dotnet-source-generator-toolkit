using DotNetSourceGeneratorToolkit.Exceptions;

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Exception thrown when configuration validation fails.
/// </summary>
public sealed class ConfigurationException : DotNetSourceGeneratorToolkitException
{
    public ConfigurationException(string message) : base(message) { }
    public ConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}
