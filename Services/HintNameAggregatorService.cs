#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Aggregates multiple Roslyn source generator results and ensures unique hint names
/// by detecting and resolving collisions through deterministic prefixing.
/// </summary>
/// <remarks>
/// This service prevents Roslyn's ArgumentException when multiple generators emit source files
/// with the same hint name. It tracks collisions and resolves them by prefixing the generator
/// category (e.g., 'Customer.g.cs' becomes 'Customer.Mapper.g.cs' or 'Customer.Repository.g.cs').
/// </remarks>
public sealed class HintNameAggregatorService : IHintNameAggregatorService
{
    private readonly ILogger<HintNameAggregatorService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HintNameAggregatorService"/> class.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output (optional)</param>
    public HintNameAggregatorService(ILogger<HintNameAggregatorService>? logger = null)
    {
        _logger = logger ?? NullLogger<HintNameAggregatorService>.Instance;
    }

    /// <inheritdoc/>
    public IEnumerable<(string HintName, SourceText SourceText)> AggregateResults(
        IEnumerable<(string OriginalHintName, SourceText SourceText, string GeneratorCategory)> results)
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }

        var diagnostics = new List<Diagnostic>();
        return AggregateResults(results, diagnostics);
    }

    /// <inheritdoc/>
    public IEnumerable<(string HintName, SourceText SourceText)> AggregateResults(
        IEnumerable<(string OriginalHintName, SourceText SourceText, string GeneratorCategory)> results,
        List<Diagnostic> diagnostics)
    {
        if (results is null)
        {
            throw new ArgumentNullException(nameof(results));
        }
        if (diagnostics is null)
        {
            throw new ArgumentNullException(nameof(diagnostics));
        }

        _logger.LogInformation("Aggregating {Count} source generator results", results.Count());

        var processedResults = new List<(string HintName, SourceText SourceText, string OriginalHintName, string GeneratorCategory)>();
        var allHintNames = new HashSet<string>(StringComparer.Ordinal);
        var collisionTracker = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        // First pass: identify all collisions
        foreach (var (originalHintName, sourceText, generatorCategory) in results)
        {
            if (!collisionTracker.TryGetValue(originalHintName, out var collidingGenerators))
            {
                collidingGenerators = new List<string>();
                collisionTracker[originalHintName] = collidingGenerators;
            }
            collidingGenerators.Add(generatorCategory);
        }

        // Track which hint names we've already assigned
        var assignedHintNames = new HashSet<string>(StringComparer.Ordinal);

        // Second pass: resolve collisions by prefixing with generator category
        foreach (var (originalHintName, sourceText, generatorCategory) in results)
        {
            var finalHintName = originalHintName;
            var collisionInfo = collisionTracker[originalHintName];

            // Check if this original hint name has collisions
            bool hasCollision = collisionInfo.Count > 1;

            if (hasCollision)
            {
                // This is a collision - need to deduplicate
                var baseFileName = Path.GetFileNameWithoutExtension(originalHintName);
                var extension = Path.GetExtension(originalHintName);
                var directory = Path.GetDirectoryName(originalHintName) ?? string.Empty;

                // Try to find an available unique name by adding the generator category
                var prefixedName = $"{baseFileName}.{generatorCategory}{extension}";
                var candidatePath = Path.Combine(directory, prefixedName);

                // If that's already taken by a previously processed result, add a numeric suffix
                int suffix = 1;
                while (assignedHintNames.Contains(candidatePath))
                {
                    prefixedName = $"{baseFileName}.{generatorCategory}{suffix}{extension}";
                    candidatePath = Path.Combine(directory, prefixedName);
                    suffix++;
                }

                finalHintName = candidatePath;

                _logger.LogWarning(
                    "Hint name collision detected for '{OriginalHintName}'. " +
                    "Multiple generators ({Generators}) emitted the same hint name. " +
                    "Resolved to '{FinalHintName}'.",
                    originalHintName,
                    string.Join(", ", collisionInfo),
                    finalHintName);

                // Add diagnostic warning
                var diagnostic = Diagnostic.Create(
                    new DiagnosticDescriptor(
                        id: "SGTK050",
                        title: "Hint name collision detected",
                        messageFormat: "Multiple generators ({0}) emitted the same hint name '{1}'. Resolved to '{2}'.",
                        category: "DotNetSourceGeneratorToolkit.HintAggregation",
                        defaultSeverity: DiagnosticSeverity.Warning,
                        isEnabledByDefault: true,
                        description: "Source generators emitted duplicate hint names which were automatically resolved."),
                    Location.None,
                    string.Join(", ", collisionInfo),
                    originalHintName,
                    finalHintName);

                diagnostics.Add(diagnostic);
            }

            processedResults.Add((finalHintName, sourceText, originalHintName, generatorCategory));
            assignedHintNames.Add(finalHintName);
        }

        // Third pass: ensure stable ordering by sorting deterministically
        var orderedResults = processedResults
            .OrderBy(r => r.HintName, StringComparer.Ordinal)
            .ThenBy(r => r.GeneratorCategory, StringComparer.Ordinal)
            .Select(r => (r.HintName, r.SourceText));

        _logger.LogInformation(
            "Aggregation complete: {UniqueCount} unique hint names from {TotalCount} results",
            assignedHintNames.Count,
            processedResults.Count);

        return orderedResults;
    }
}
