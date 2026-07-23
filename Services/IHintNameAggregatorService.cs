#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Defines the contract for a service that aggregates multiple Roslyn source generator
/// results and ensures unique hint names by detecting and resolving collisions.
/// </summary>
/// <remarks>
/// This service handles the case where multiple independent generators emit source files
/// with the same hint name (e.g., both a mapper generator and a repository generator
/// try to emit 'Customer.g.cs'). Roslyn's <see cref="GeneratorDriver.AddSource"/>
/// throws an ArgumentException when duplicate hint names are provided.
/// </remarks>
public interface IHintNameAggregatorService
{
    /// <summary>
    /// Aggregates multiple source generator results, ensuring each has a unique hint name.
    /// </summary>
    /// <param name="results">Collection of source generator results to aggregate</param>
    /// <returns>Collection of source text entries with unique hint names</returns>
    /// <exception cref="ArgumentNullException">Thrown when results is null</exception>
    IEnumerable<(string HintName, SourceText SourceText)> AggregateResults(
        IEnumerable<(string OriginalHintName, SourceText SourceText, string GeneratorCategory)> results);

    /// <summary>
    /// Aggregates multiple source generator results with diagnostics support.
    /// </summary>
    /// <param name="results">Collection of source generator results to aggregate</param>
    /// <param name="diagnostics">List to receive collision warnings</param>
    /// <returns>Collection of source text entries with unique hint names</returns>
    /// <exception cref="ArgumentNullException">Thrown when results or diagnostics is null</exception>
    IEnumerable<(string HintName, SourceText SourceText)> AggregateResults(
        IEnumerable<(string OriginalHintName, SourceText SourceText, string GeneratorCategory)> results,
        List<Diagnostic> diagnostics);
}
