using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Provides extension methods for <see cref="FileSystemService"/>.
/// </summary>
public static class FileSystemServiceExtensions
{
    /// <summary>
    /// Reads all lines from the specified file.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains a list of strings containing all lines of the file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="filePath"/> is null.</exception>
    public static async Task<IReadOnlyList<string>> ReadAllLinesAsync(this FileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = await service.ReadFileAsync(filePath);
        return content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    }

    /// <summary>
    /// Deletes the specified file if it exists.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>A task that represents the asynchronous delete operation. Returns true if the file was successfully deleted or did not exist, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="filePath"/> is null.</exception>
    public static async Task<bool> SafeDeleteFileAsync(this FileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!service.FileExists(filePath))
            return true;

        await service.DeleteFileAsync(filePath);
        return !service.FileExists(filePath);
    }
}
