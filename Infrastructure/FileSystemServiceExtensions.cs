using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Provides extension methods for <see cref="IFileSystemService"/>.
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
    /// <exception cref="FileSystemException">Thrown when the file cannot be read.</exception>
    public static async Task<IReadOnlyList<string>> ReadAllLinesAsync(this IFileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var content = await service.ReadFileAsync(filePath);
        return content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Writes multiple lines to a file asynchronously.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="lines">The lines to write to the file.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="lines"/> is null.</exception>
    /// <exception cref="FileSystemException">Thrown when the file cannot be written.</exception>
    public static async Task WriteAllLinesAsync(this IFileSystemService service, string filePath, IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);
        ArgumentNullException.ThrowIfNull(lines);

        await service.WriteFileAsync(filePath, string.Join(Environment.NewLine, lines));
    }

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="dirPath">The path to the directory.</param>
    /// <returns>True if the directory exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dirPath"/> is null or empty.</exception>
    public static bool DirectoryExists(this IFileSystemService service, string dirPath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(dirPath);

        return Directory.Exists(dirPath);
    }

    /// <summary>
    /// Deletes the specified file if it exists.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>A task that represents the asynchronous delete operation. Returns true if the file was successfully deleted or did not exist, false if deletion was attempted but failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="FileSystemException">Thrown when the file exists but cannot be deleted.</exception>
    public static async Task<bool> SafeDeleteFileAsync(this IFileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        if (!service.FileExists(filePath))
            return true;

        try
        {
            await service.DeleteFileAsync(filePath);
            return !service.FileExists(filePath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes the specified directory if it exists.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="dirPath">The path to the directory.</param>
    /// <param name="recursive">True to delete the directory and all its contents; false to delete only empty directories.</param>
    /// <returns>A task that represents the asynchronous delete operation. Returns true if the directory was successfully deleted or did not exist, false if deletion was attempted but failed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="dirPath"/> is null.</exception>
    /// <exception cref="FileSystemException">Thrown when the directory exists but cannot be deleted.</exception>
    public static async Task<bool> SafeDeleteDirectoryAsync(this IFileSystemService service, string dirPath, bool recursive = true)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(dirPath);

        if (!service.DirectoryExists(dirPath))
            return true;

        try
        {
            if (recursive)
            {
                Directory.Delete(dirPath, true);
            }
            else
            {
                Directory.Delete(dirPath);
            }

            return !service.DirectoryExists(dirPath);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Reads all text from the specified file.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>A task that represents the asynchronous read operation. The task result contains the file content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="filePath"/> is null.</exception>
    /// <exception cref="FileSystemException">Thrown when the file cannot be read.</exception>
    public static async Task<string> ReadAllTextAsync(this IFileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return await service.ReadFileAsync(filePath);
    }

    /// <summary>
    /// Gets the file name from a file path.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The file name with extension.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    public static string GetFileName(this IFileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return Path.GetFileName(filePath);
    }

    /// <summary>
    /// Gets the file name without extension from a file path.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The file name without extension.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    public static string GetFileNameWithoutExtension(this IFileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Gets the file extension from a file path.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <returns>The file extension including the dot, or an empty string if no extension exists.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
    public static string GetFileExtension(this IFileSystemService service, string filePath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        return Path.GetExtension(filePath);
    }

    /// <summary>
    /// Gets the relative path from a base directory to a target path.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="basePath">The base directory path.</param>
    /// <param name="targetPath">The target file or directory path.</param>
    /// <returns>The relative path from basePath to targetPath.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="basePath"/>, or <paramref name="targetPath"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="basePath"/> or <paramref name="targetPath"/> is null or empty.</exception>
    public static string GetRelativePath(this IFileSystemService service, string basePath, string targetPath)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(basePath);
        ArgumentException.ThrowIfNullOrEmpty(targetPath);

        return Path.GetRelativePath(basePath, targetPath);
    }

    /// <summary>
    /// Gets the absolute path from a relative path.
    /// </summary>
    /// <param name="service">The file system service.</param>
    /// <param name="path">The path to convert.</param>
    /// <returns>The absolute path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="path"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="path"/> is null or empty.</exception>
    public static string GetFullPath(this IFileSystemService service, string path)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrEmpty(path);

        return Path.GetFullPath(path);
    }
}
