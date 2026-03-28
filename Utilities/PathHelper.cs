// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Helper utilities for working with file and directory paths safely.
/// Provides path normalization and validation across platforms.
/// </summary>
public static class PathHelper
{
    /// <summary>
    /// Normalize a path for the current platform.
    /// Converts forward slashes to backslashes on Windows.
    /// </summary>
    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Convert absolute path to relative path from a base directory.
    /// </summary>
    public static string? ToRelativePath(string absolutePath, string basePath)
    {
        try
        {
            var fullAbsolute = NormalizePath(absolutePath);
            var fullBase = NormalizePath(basePath);

            return Path.GetRelativePath(fullBase, fullAbsolute);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if a path is absolute.
    /// </summary>
    public static bool IsAbsolute(string path)
    {
        return Path.IsPathRooted(path);
    }

    /// <summary>
    /// Get the common parent directory of multiple paths.
    /// </summary>
    public static string? GetCommonPath(params string[] paths)
    {
        if (paths == null || paths.Length == 0)
            return null;

        if (paths.Length == 1)
            return NormalizePath(paths[0]);

        var normalizedPaths = paths.Select(NormalizePath).ToList();
        var parts = normalizedPaths[0].Split(Path.DirectorySeparatorChar);

        for (int i = 0; i < parts.Length; i++)
        {
            var currentPart = parts[i];

            if (!normalizedPaths.Skip(1).All(p =>
            {
                var pathParts = p.Split(Path.DirectorySeparatorChar);
                return pathParts.Length > i && pathParts[i] == currentPart;
            }))
            {
                return string.Join(Path.DirectorySeparatorChar.ToString(),
                    parts.Take(i).ToArray());
            }
        }

        return normalizedPaths[0];
    }

    /// <summary>
    /// Ensure a directory path ends with separator.
    /// </summary>
    public static string EnsureTrailingSeparator(string path)
    {
        if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return path + Path.DirectorySeparatorChar;

        return path;
    }

    /// <summary>
    /// Remove trailing separator from a path.
    /// </summary>
    public static string RemoveTrailingSeparator(string path)
    {
        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
