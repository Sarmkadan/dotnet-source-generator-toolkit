// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Provides file path validation and manipulation utilities.
/// Ensures paths are safe, normalized, and accessible.
/// </summary>
public static class FilePathValidator
{
    /// <summary>
    /// Validate that a file path is accessible and safe.
    /// </summary>
    public static bool IsValidPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // Check for invalid characters
            var invalidChars = Path.GetInvalidPathChars();
            if (path.Any(c => invalidChars.Contains(c)))
                return false;

            // Try to normalize it
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensure a directory path is safe and create it if it doesn't exist.
    /// </summary>
    public static bool EnsureDirectoryExists(string path)
    {
        if (!IsValidPath(path))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(path);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get safe relative path from base directory to target.
    /// </summary>
    public static string? GetRelativePath(string basePath, string targetPath)
    {
        try
        {
            var fullBase = Path.GetFullPath(basePath);
            var fullTarget = Path.GetFullPath(targetPath);

            // Ensure target is under base for security
            if (!fullTarget.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                return null;

            return Path.GetRelativePath(fullBase, fullTarget);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Combine path segments safely, preventing path traversal attacks.
    /// </summary>
    public static string? CombineSafePath(string basePath, params string[] segments)
    {
        try
        {
            var combined = Path.Combine(basePath, Path.Combine(segments));
            var fullPath = Path.GetFullPath(combined);
            var fullBase = Path.GetFullPath(basePath);

            // Ensure result is under base directory
            if (!fullPath.StartsWith(fullBase, StringComparison.OrdinalIgnoreCase))
                return null;

            return fullPath;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get the directory containing a file, with validation.
    /// </summary>
    public static string? GetDirectory(string filePath)
    {
        try
        {
            if (!IsValidPath(filePath))
                return null;

            return Path.GetDirectoryName(Path.GetFullPath(filePath));
        }
        catch
        {
            return null;
        }
    }
}
