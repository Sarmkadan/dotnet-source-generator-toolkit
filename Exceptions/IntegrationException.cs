using DotNetSourceGeneratorToolkit.Exceptions;

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Exception thrown when integration with external services fails.
/// </summary>
public sealed class IntegrationException : DotNetSourceGeneratorToolkitException
{
    public IntegrationException(string message) : base(message) { }
    public IntegrationException(string message, Exception innerException) : base(message, innerException) { }
}
