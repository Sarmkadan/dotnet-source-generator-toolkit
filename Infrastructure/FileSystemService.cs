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
public class FileSystemService : IFileSystemService
{
    private readonly ILogger<FileSystemService> _logger;

    public FileSystemService(ILogger<FileSystemService> logger)
    {
        _logger = logger;
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
            throw new GenerationException($"File not found: {filePath}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
            throw new GenerationException($"Error reading file {filePath}: {ex.Message}", ex);
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
                Directory.CreateDirectory(directory);
                _logger.LogInformation("Created directory: {Directory}", directory);
            }

            _logger.LogInformation("Writing file: {FilePath}", filePath);
            await File.WriteAllTextAsync(filePath, content);
            _logger.LogInformation("Successfully wrote file: {FilePath} ({Bytes} bytes)", filePath, content?.Length ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file: {FilePath}", filePath);
            throw new GenerationException($"Error writing file {filePath}: {ex.Message}", ex);
        }
    }

    public async Task AppendFileAsync(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            _logger.LogInformation("Appending to file: {FilePath}", filePath);
            await File.AppendAllTextAsync(filePath, content);
            _logger.LogInformation("Successfully appended to file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error appending to file: {FilePath}", filePath);
            throw new GenerationException($"Error appending to file {filePath}: {ex.Message}", ex);
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
                File.Delete(filePath);
                _logger.LogInformation("Deleted file: {FilePath}", filePath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            throw new GenerationException($"Error deleting file {filePath}: {ex.Message}", ex);
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
                Directory.CreateDirectory(dirPath);
                _logger.LogInformation("Created directory: {DirectoryPath}", dirPath);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory: {DirectoryPath}", dirPath);
            throw new GenerationException($"Error creating directory {dirPath}: {ex.Message}", ex);
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
            throw new GenerationException($"Error reading directory {dirPath}: {ex.Message}", ex);
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
        if (segments == null || segments.Length == 0)
            return string.Empty;

        return Path.Combine(segments);
    }
}
