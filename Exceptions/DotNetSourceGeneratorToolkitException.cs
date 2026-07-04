namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Base exception for all domain-specific errors in the toolkit.
/// </summary>
public abstract class DotNetSourceGeneratorToolkitException : Exception
{
    protected DotNetSourceGeneratorToolkitException(string message) : base(message) { }
    protected DotNetSourceGeneratorToolkitException(string message, Exception innerException) : base(message, innerException) { }
}
