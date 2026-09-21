#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace DotNetSourceGeneratorToolkit.Infrastructure;

/// <summary>
/// Defines limits for project analysis to prevent unbounded resource consumption.
/// </summary>
public sealed class AnalysisLimits
{
    /// <summary>
    /// Maximum number of files to analyze. Default is 10000.
    /// </summary>
    public int MaxFileCount { get; init; } = 10000;

    /// <summary>
    /// Maximum recursion depth for directory traversal. Default is 10.
    /// </summary>
    public int MaxRecursionDepth { get; init; } = 10;

    /// <summary>
    /// Maximum time in seconds allowed for analysis. Default is 30 seconds.
    /// </summary>
    public int AnalysisTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Gets a preset with no limits (effectively disabled).
    /// </summary>
    public static AnalysisLimits None => new AnalysisLimits
    {
        MaxFileCount = int.MaxValue,
        MaxRecursionDepth = int.MaxValue,
        AnalysisTimeoutSeconds = int.MaxValue
    };
}