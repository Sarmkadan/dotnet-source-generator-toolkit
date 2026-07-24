#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetSourceGeneratorToolkit.Exceptions;
using Microsoft.Extensions.Logging;
using System.IO;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Provides file system operations including reading, writing, and directory management.
/// Includes error handling, logging, and security validation for all operations.
/// </summary>
public sealed class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;
    private readonly IRetryPolicy _retryPolicy;
    private bool _dryRun;
    private readonly string _baseOutputPath;
    private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    public FileSystemService(ILogger<FileSystemService> logger, IRetryPolicy? retryPolicy = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _retryPolicy = retryPolicy ?? new RetryPolicy();
        _baseOutputPath = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Sets the dry-run mode for this service instance.
    /// When enabled, write operations will be skipped and logged instead.
    /// </summary>
    /// <param name="dryRun">True to enable dry-run mode, false to disable.</param>
    public void SetDryRun(bool dryRun)
    {
        _dryRun = dryRun;
        if (_dryRun)
        {
            _logger.LogInformation("Dry-run mode enabled: file writes will be simulated only");
        }
    }

    /// <summary>
    /// Validates that a file path is safe and does not contain path traversal sequences.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="operation">The type of operation being performed.</param>
    /// <exception cref="ArgumentException">Thrown when the path is invalid or contains path traversal sequences.</exception>
    /// <exception cref="ArgumentNullException">Thrown when filePath is null.</exception>
    private void ValidatePath(string filePath, string operation)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        // Check for null bytes which can be used in path traversal attacks
        if (filePath.Contains('\0'))
        {
            throw new FileSystemException($"Path contains null byte character: {filePath}");
        }

        // Normalize the path to resolve any path traversal sequences
        var fullPath = Path.GetFullPath(filePath);

        // Check for path traversal attempts (../ or ..\)
        if (filePath.Contains("..") && fullPath.Contains(".."))
        {
            throw new FileSystemException($"Path traversal detected in {operation} operation: {filePath}");
        }

        // Check for absolute paths that could escape the intended directory
        if (Path.IsPathRooted(filePath) && !filePath.StartsWith(_baseOutputPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new FileSystemException($"Absolute path outside base directory detected in {operation} operation: {filePath}");
        }

        // Check for invalid characters in the path
        if (filePath.IndexOfAny(InvalidPathChars) >= 0)
        {
            throw new FileSystemException($"Path contains invalid characters in {operation} operation: {filePath}");
        }

        // Check if the resolved path stays within the base directory
        if (!fullPath.StartsWith(_baseOutputPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new FileSystemException($"Resolved path escapes base directory in {operation} operation. Base: {_baseOutputPath}, Attempted: {fullPath}");
        }
    }

    /// <summary>
    /// Validates that a filename is safe and does not contain invalid characters.
    /// </summary>
    /// <param name="fileName">The filename to validate.</param>
    /// <param name="operation">The type of operation being performed.</param>
    /// <exception cref="ArgumentException">Thrown when the filename is invalid.</exception>
    private void ValidateFileName(string fileName, string operation)
    {
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        // Check for null bytes
        if (fileName.Contains('\0'))
        {
            throw new FileSystemException($"Filename contains null byte character in {operation} operation: {fileName}");
        }

        // Check for invalid filename characters
        if (fileName.IndexOfAny(InvalidFileNameChars) >= 0)
        {
            throw new FileSystemException($"Filename contains invalid characters in {operation} operation: {fileName}");
        }

        // Check for path separators in filename
        if (fileName.Contains(Path.DirectorySeparatorChar) || fileName.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new FileSystemException($"Filename contains path separators in {operation} operation: {fileName}");
        }

        // Check for Windows reserved names
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        if (IsReservedWindowsName(nameWithoutExt))
        {
            throw new FileSystemException($"Filename uses reserved Windows name in {operation} operation: {fileName}");
        }
    }

    /// <summary>
    /// Checks if a filename is a Windows reserved name that could cause issues.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <returns>True if the name is reserved; otherwise false.</returns>
    private static bool IsReservedWindowsName(string name)
    {
        // List of Windows reserved names
        var reservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        return reservedNames.Contains(name);
    }

    public async Task<string> ReadFileAsync(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            ValidatePath(filePath, "read");
            _logger.LogInformation("Reading file: {FilePath}", filePath);
            var content = await File.ReadAllTextAsync(filePath);
            _logger.LogInformation("Successfully read file: {FilePath} ({Bytes} bytes)", filePath, content.Length);
            return content;
        }
        catch (FileSystemException)
        {
            // Re-throw FileSystemException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw new FileSystemException($"Error reading file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task WriteFileAsync(string filePath, string content)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            ValidatePath(filePath, "write");
            ValidateFileName(Path.GetFileName(filePath), "write");

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                if (_dryRun)
                {
                    _logger.LogInformation("[DRY-RUN] Would create directory: {Directory}", directory);
                }
                else
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation("Created directory: {Directory}", directory);
                }
            }

            if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Would write file: {FilePath} ({Bytes} bytes)\n{Content}",
                    filePath, content?.Length ?? 0, content);
            }
            else
            {
                _logger.LogInformation("Writing file: {FilePath}", filePath);
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await File.WriteAllTextAsync(filePath, content);
                    _logger.LogInformation("Successfully wrote file: {FilePath} ({Bytes} bytes)", filePath, content?.Length ?? 0);
                }, filePath);
            }
        }
        catch (FileSystemException)
        {
            // Re-throw FileSystemException as-is
            throw;
        }
        catch (Exception ex) when (ex is not FileSystemException)
        {
            _logger.LogError(ex, "Error writing file: {FilePath}", filePath);
            throw new FileSystemException($"Error writing file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task AppendFileAsync(string filePath, string content)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            ValidatePath(filePath, "append");
            ValidateFileName(Path.GetFileName(filePath), "append");

            if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Would append to file: {FilePath}\n{Content}", filePath, content);
            }
            else
            {
                _logger.LogInformation("Appending to file: {FilePath}", filePath);
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await File.AppendAllTextAsync(filePath, content);
                    _logger.LogInformation("Successfully appended to file: {FilePath}", filePath);
                }, filePath);
            }
        }
        catch (FileSystemException)
        {
            // Re-throw FileSystemException as-is
            throw;
        }
        catch (Exception ex) when (ex is not FileSystemException)
        {
            _logger.LogError(ex, "Error appending to file: {FilePath}", filePath);
            throw new FileSystemException($"Error appending to file {filePath}: {ex.Message}", ex);
        }
    }

    public bool FileExists(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            ValidatePath(filePath, "file existence check");
            return File.Exists(filePath);
        }
        catch (FileSystemException)
        {
            // If path is invalid, file doesn't exist in a safe way
            return false;
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            ValidatePath(filePath, "delete");

            if (File.Exists(filePath))
            {
                if (_dryRun)
                {
                    _logger.LogInformation("[DRY-RUN] Would delete file: {FilePath}", filePath);
                }
                else
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        File.Delete(filePath);
                        _logger.LogInformation("Deleted file: {FilePath}", filePath);
                    }, filePath);
                }
            }
            else if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Would delete file (does not exist): {FilePath}", filePath);
            }

            await Task.CompletedTask;
        }
        catch (FileSystemException)
        {
            // Re-throw FileSystemException as-is
            throw;
        }
        catch (Exception ex) when (ex is not FileSystemException)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw new FileSystemException($"Error deleting file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task CreateDirectoryAsync(string dirPath)
    {
        ArgumentNullException.ThrowIfNull(dirPath);

        try
        {
            ValidatePath(dirPath, "directory creation");

            if (!Directory.Exists(dirPath))
            {
                if (_dryRun)
                {
                    _logger.LogInformation("[DRY-RUN] Would create directory: {DirectoryPath}", dirPath);
                }
                else
                {
                    await _retryPolicy.ExecuteAsync(async () =>
                    {
                        Directory.CreateDirectory(dirPath);
                        _logger.LogInformation("Created directory: {DirectoryPath}", dirPath);
                    }, dirPath);
                }
            }
            else if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Directory already exists: {DirectoryPath}", dirPath);
            }

            await Task.CompletedTask;
        }
        catch (FileSystemException)
        {
            // Re-throw FileSystemException as-is
            throw;
        }
        catch (Exception ex) when (ex is not FileSystemException)
        {
            _logger.LogError(ex, "Error creating directory: {DirectoryPath}", dirPath);
            throw new FileSystemException($"Error creating directory {dirPath}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string dirPath, string searchPattern)
    {
        ArgumentNullException.ThrowIfNull(dirPath);
        ArgumentException.ThrowIfNullOrEmpty(searchPattern);

        try
        {
            ValidatePath(dirPath, "file listing");

            if (!Directory.Exists(dirPath))
                return [];

            var files = Directory.GetFiles(dirPath, searchPattern, SearchOption.AllDirectories);
            _logger.LogInformation("Found {Count} files matching pattern '{Pattern}' in {Directory}", files.Length, searchPattern, dirPath);
            return await Task.FromResult(files);
        }
        catch (FileSystemException)
        {
            // Re-throw FileSystemException as-is
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files from directory: {DirectoryPath}", dirPath);
            throw new FileSystemException($"Error reading directory {dirPath}: {ex.Message}", ex);
        }
    }

    public string GetDirectoryName(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        try
        {
            ValidatePath(filePath, "directory name extraction");
            return Path.GetDirectoryName(filePath) ?? string.Empty;
        }
        catch (FileSystemException)
        {
            return string.Empty;
        }
    }

    public string CombinePath(params string[] segments)
    {
        ArgumentNullException.ThrowIfNull(segments);
        if (segments.Length == 0)
            return string.Empty;

        return Path.Combine(segments);
    }
}
