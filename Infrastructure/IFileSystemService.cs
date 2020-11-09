#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Abstracts file system operations for reading, writing, and managing files.
/// </summary>
public interface IFileSystemService
{
    /// <summary>
    /// Reads the complete content of a file asynchronously.
    /// </summary>
    Task<string> ReadFileAsync(string filePath);

    /// <summary>
    /// Writes content to a file asynchronously, creating or overwriting as needed.
    /// </summary>
    Task WriteFileAsync(string filePath, string content);

    /// <summary>
    /// Appends content to an existing file asynchronously.
    /// </summary>
    Task AppendFileAsync(string filePath, string content);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    bool FileExists(string filePath);

    /// <summary>
    /// Deletes a file if it exists.
    /// </summary>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Creates a directory if it doesn't exist.
    /// </summary>
    Task CreateDirectoryAsync(string dirPath);

    /// <summary>
    /// Gets all files in a directory matching a pattern.
    /// </summary>
    Task<IEnumerable<string>> GetFilesAsync(string dirPath, string searchPattern);

    /// <summary>
    /// Gets the directory name from a file path.
    /// </summary>
    string GetDirectoryName(string filePath);

    /// <summary>
    /// Combines path segments into a single path.
    /// </summary>
    string CombinePath(params string[] segments);
}

