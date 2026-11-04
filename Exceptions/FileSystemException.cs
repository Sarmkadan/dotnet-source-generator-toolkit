using DotNetSourceGeneratorToolkit.Exceptions;

namespace DotNetSourceGeneratorToolkit.Exceptions;

/// <summary>
/// Exception thrown when file system operations fail.
/// </summary>
public sealed class FileSystemException : DotNetSourceGeneratorToolkitException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FileSystemException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FileSystemException(string message, Exception innerException) : base(message, innerException) { }
}
