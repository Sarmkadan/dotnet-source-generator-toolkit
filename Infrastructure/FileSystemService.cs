#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Provides file system operations including reading, writing, and directory management.
/// Includes error handling and logging for all operations.
/// </summary>
public sealed class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;
    private bool _dryRun;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
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

    public async Task<string> ReadFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            _logger.LogInformation("Reading file: {FilePath}", filePath);
            var content = await File.ReadAllTextAsync(filePath);
            _logger.LogInformation("Successfully read file: {FilePath} ({Bytes} bytes)", filePath, content.Length);
            return content;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found: {FilePath}", filePath);
            throw new FileSystemException($"File not found: {filePath}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw new FileSystemException($"Error reading file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task WriteFileAsync(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
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
                await File.WriteAllTextAsync(filePath, content);
                _logger.LogInformation("Successfully wrote file: {FilePath} ({Bytes} bytes)", filePath, content?.Length ?? 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file: {FilePath}", filePath);
            throw new FileSystemException($"Error writing file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task AppendFileAsync(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Would append to file: {FilePath}\n{Content}", filePath, content);
            }
            else
            {
                _logger.LogInformation("Appending to file: {FilePath}", filePath);
                await File.AppendAllTextAsync(filePath, content);
                _logger.LogInformation("Successfully appended to file: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error appending to file: {FilePath}", filePath);
            throw new FileSystemException($"Error appending to file {filePath}: {ex.Message}", ex);
        }
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        return File.Exists(filePath);
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            if (File.Exists(filePath))
            {
                if (_dryRun)
                {
                    _logger.LogInformation("[DRY-RUN] Would delete file: {FilePath}", filePath);
                }
                else
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted file: {FilePath}", filePath);
                }
            }
            else if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Would delete file (does not exist): {FilePath}", filePath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw new FileSystemException($"Error deleting file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task CreateDirectoryAsync(string dirPath)
    {
        if (string.IsNullOrWhiteSpace(dirPath))
            throw new ArgumentNullException(nameof(dirPath));

        try
        {
            if (!Directory.Exists(dirPath))
            {
                if (_dryRun)
                {
                    _logger.LogInformation("[DRY-RUN] Would create directory: {DirectoryPath}", dirPath);
                }
                else
                {
                    Directory.CreateDirectory(dirPath);
                    _logger.LogInformation("Created directory: {DirectoryPath}", dirPath);
                }
            }
            else if (_dryRun)
            {
                _logger.LogInformation("[DRY-RUN] Directory already exists: {DirectoryPath}", dirPath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory: {DirectoryPath}", dirPath);
            throw new FileSystemException($"Error creating directory {dirPath}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<string>> GetFilesAsync(string dirPath, string searchPattern)
    {
        if (string.IsNullOrWhiteSpace(dirPath))
            throw new ArgumentNullException(nameof(dirPath));

        try
        {
            if (!Directory.Exists(dirPath))
                return [];

            var files = Directory.GetFiles(dirPath, searchPattern, SearchOption.AllDirectories);
            _logger.LogInformation("Found {Count} files matching pattern '{Pattern}' in {Directory}", files.Length, searchPattern, dirPath);
            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files from directory: {DirectoryPath}", dirPath);
            throw new FileSystemException($"Error reading directory {dirPath}: {ex.Message}", ex);
        }
    }

    public string GetDirectoryName(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return string.Empty;

        return Path.GetDirectoryName(filePath) ?? string.Empty;
    }

    public string CombinePath(params string[] segments)
    {
        if (segments is null || segments.Length == 0)
            return string.Empty;

        return Path.Combine(segments);
    }
}
