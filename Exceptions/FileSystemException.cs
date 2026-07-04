using DotNetSourceGeneratorToolkit.Exceptions;

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Exception thrown when file system operations fail.
/// </summary>
public sealed class FileSystemException : DotNetSourceGeneratorToolkitException
{
    public FileSystemException(string message) : base(message) { }
    public FileSystemException(string message, Exception innerException) : base(message, innerException) { }
}
