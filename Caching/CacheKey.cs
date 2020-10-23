// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetSourceGeneratorToolkit.Caching;

/// <summary>
/// Generates consistent cache keys for generation results and analysis data.
/// Ensures same inputs always produce the same key.
/// </summary>
public static class CacheKey
{
    private const string Delimiter = "::";

    /// <summary>
    /// Generate cache key for entity analysis results.
    /// </summary>
    public static string ForEntityAnalysis(string filePath, string content)
    {
        var hash = ComputeHash(content);
        return $"analysis{Delimiter}{Path.GetFileName(filePath)}{Delimiter}{hash}";
    }

    /// <summary>
    /// Generate cache key for generation result.
    /// </summary>
    public static string ForGenerationResult(string entityName, string generatorType)
    {
        return $"generated{Delimiter}{entityName}{Delimiter}{generatorType}";
    }

    /// <summary>
    /// Generate cache key for project metadata.
    /// </summary>
    public static string ForProjectMetadata(string projectPath)
    {
        var normalizedPath = Path.GetFullPath(projectPath);
        return $"project{Delimiter}{normalizedPath}";
    }

    /// <summary>
    /// Generate cache key for configuration.
    /// </summary>
    public static string ForConfiguration(string configPath)
    {
        var hash = ComputeHash(configPath);
        return $"config{Delimiter}{hash}";
    }

    /// <summary>
    /// Compute SHA256 hash of content for cache invalidation.
    /// </summary>
    private static string ComputeHash(string content)
    {
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
            return Convert.ToHexString(hash)[..16]; // Use first 16 chars for readability
        }
    }
}
