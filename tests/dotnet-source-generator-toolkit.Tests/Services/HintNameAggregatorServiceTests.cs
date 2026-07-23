#nullable enable

// =============================================================================
// Author: Test Generator
// Hint name collision detection and resolution tests
// =============================================================================

using DotNetSourceGeneratorToolkit.Services;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Services;

/// <summary>
/// Tests for <see cref="HintNameAggregatorService"/> which handles hint name deduplication
/// when multiple Roslyn source generators emit the same hint name.
/// </summary>
public sealed class HintNameAggregatorServiceTests
{
    private readonly IHintNameAggregatorService _aggregator;

    public HintNameAggregatorServiceTests()
    {
        _aggregator = new HintNameAggregatorService();
    }

    [Fact]
    public void AggregateResults_WithNullResults_ThrowsArgumentNullException()
    {
        // Arrange
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _aggregator.AggregateResults(null!, diagnostics));
    }

    [Fact]
    public void AggregateResults_WithNullDiagnostics_ThrowsArgumentNullException()
    {
        // Arrange
        var results = new List<(string, SourceText, string)>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _aggregator.AggregateResults(results, null!));
    }

    [Fact]
    public void AggregateResults_WithEmptyResults_ReturnsEmptyCollection()
    {
        // Arrange
        var results = new List<(string, SourceText, string)>();
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics);

        // Assert
        Assert.Empty(aggregated);
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void AggregateResults_WithNoCollisions_ReturnsOriginalHintNames()
    {
        // Arrange
        var results = new List<(string, SourceText, string)>
        {
            ("Customer.g.cs", SourceText.From("// Customer code", System.Text.Encoding.UTF8), "Mapper"),
            ("Product.g.cs", SourceText.From("// Product code", System.Text.Encoding.UTF8), "Repository"),
            ("Order.g.cs", SourceText.From("// Order code", System.Text.Encoding.UTF8), "Validator"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert
        Assert.Equal(3, aggregated.Count);
        Assert.Empty(diagnostics);

        Assert.Equal("Customer.g.cs", aggregated[0].HintName);
        Assert.Equal("Order.g.cs", aggregated[1].HintName);
        Assert.Equal("Product.g.cs", aggregated[2].HintName);
    }

    [Fact]
    public void AggregateResults_WithSimpleCollision_ResolvesWithPrefix()
    {
        // Arrange - Two generators emit the same hint name
        var results = new List<(string, SourceText, string)>
        {
            ("Customer.g.cs", SourceText.From("// Customer mapper", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer repository", System.Text.Encoding.UTF8), "Repository"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert
        Assert.Equal(2, aggregated.Count);
        Assert.Equal(1, diagnostics.Count);

        // Verify collision was detected
        var diagnostic = diagnostics[0];
        Assert.Equal("SGTK050", diagnostic.Id);
        Assert.Equal("Hint name collision detected", diagnostic.Descriptor.Title.ToString());

        // Verify unique hint names
        var hintNames = aggregated.Select(r => r.HintName).ToHashSet();
        Assert.Equal(2, hintNames.Count);
        Assert.Contains("Customer.g.cs", hintNames);
        Assert.Contains("Customer.Mapper.g.cs", hintNames);

        // Verify stable ordering (sorted by hint name)
        Assert.Equal("Customer.g.cs", aggregated[0].HintName);
        Assert.Equal("Customer.Mapper.g.cs", aggregated[1].HintName);
    }

    [Fact]
    public void AggregateResults_WithThreeWayCollision_ResolvesAllWithPrefixes()
    {
        // Arrange - Three generators emit the same hint name
        var results = new List<(string, SourceText, string)>
        {
            ("Customer.g.cs", SourceText.From("// Customer mapper", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer repository", System.Text.Encoding.UTF8), "Repository"),
            ("Customer.g.cs", SourceText.From("// Customer validator", System.Text.Encoding.UTF8), "Validator"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert
        Assert.Equal(3, aggregated.Count);
        Assert.Equal(2, diagnostics.Count); // 2 additional collisions after first resolution

        // Verify all hint names are unique
        var hintNames = aggregated.Select(r => r.HintName).ToHashSet();
        Assert.Equal(3, hintNames.Count);
        Assert.Contains("Customer.g.cs", hintNames);
        Assert.Contains("Customer.Mapper.g.cs", hintNames);
        Assert.Contains("Customer.Repository.g.cs", hintNames);
        Assert.Contains("Customer.Validator.g.cs", hintNames);

        // Verify stable ordering
        Assert.Equal("Customer.g.cs", aggregated[0].HintName);
        Assert.Equal("Customer.Mapper.g.cs", aggregated[1].HintName);
        Assert.Equal("Customer.Repository.g.cs", aggregated[2].HintName);
        Assert.Equal("Customer.Validator.g.cs", aggregated[3].HintName);
    }

    [Fact]
    public void AggregateResults_WithDifferentFileExtensions_PreservesExtension()
    {
        // Arrange - Different file extensions
        var results = new List<(string, SourceText, string)>
        {
            ("Customer.cs", SourceText.From("// Customer code", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.cs", SourceText.From("// Customer code", System.Text.Encoding.UTF8), "Repository"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert
        Assert.Equal(2, aggregated.Count);

        var hintNames = aggregated.Select(r => r.HintName).ToHashSet();
        Assert.Contains("Customer.cs", hintNames);
        Assert.Contains("Customer.Mapper.cs", hintNames);
    }

    [Fact]
    public void AggregateResults_WithDirectoryPaths_PreservesDirectoryStructure()
    {
        // Arrange - With directory paths
        var results = new List<(string, SourceText, string)>
        {
            ("Mappers/Customer.g.cs", SourceText.From("// Mapper", System.Text.Encoding.UTF8), "Mapper"),
            ("Mappers/Customer.g.cs", SourceText.From("// Mapper", System.Text.Encoding.UTF8), "Mapper"),
            ("Repositories/Customer.g.cs", SourceText.From("// Repository", System.Text.Encoding.UTF8), "Repository"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert
        Assert.Equal(3, aggregated.Count);

        // Verify directory structure is preserved
        Assert.Contains("Mappers/Customer.g.cs", aggregated.Select(r => r.HintName));
        Assert.Contains("Mappers/Customer.Mapper.g.cs", aggregated.Select(r => r.HintName));
        Assert.Contains("Repositories/Customer.g.cs", aggregated.Select(r => r.HintName));
    }

    [Fact]
    public void AggregateResults_WithStableOrdering_ProducesDeterministicOutput()
    {
        // Arrange - Multiple results with various collisions
        var results = new List<(string, SourceText, string)>
        {
            ("Order.g.cs", SourceText.From("// Order 1", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer 1", System.Text.Encoding.UTF8), "Mapper"),
            ("Product.g.cs", SourceText.From("// Product 1", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer 2", System.Text.Encoding.UTF8), "Repository"),
            ("Order.g.cs", SourceText.From("// Order 2", System.Text.Encoding.UTF8), "Repository"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act - Run multiple times
        var aggregated1 = _aggregator.AggregateResults(results, diagnostics).ToList();
        var aggregated2 = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert - Same order both times
        Assert.Equal(5, aggregated1.Count);
        Assert.Equal(5, aggregated2.Count);

        for (int i = 0; i < aggregated1.Count; i++)
        {
            Assert.Equal(aggregated1[i].HintName, aggregated2[i].HintName);
        }

        // Verify expected order: sorted by hint name
        Assert.Equal("Customer.g.cs", aggregated1[0].HintName);
        Assert.Equal("Customer.Mapper.g.cs", aggregated1[1].HintName);
        Assert.Equal("Customer.Repository.g.cs", aggregated1[2].HintName);
        Assert.Equal("Order.g.cs", aggregated1[3].HintName);
        Assert.Equal("Order.Repository.g.cs", aggregated1[4].HintName);
        Assert.Equal("Product.g.cs", aggregated1[5].HintName);
    }

    [Fact]
    public void AggregateResults_WithoutDiagnosticsParameter_StillWorks()
    {
        // Arrange
        var results = new List<(string, SourceText, string)>
        {
            ("Customer.g.cs", SourceText.From("// Customer", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer", System.Text.Encoding.UTF8), "Repository"),
        };

        // Act
        var aggregated = _aggregator.AggregateResults(results).ToList();

        // Assert
        Assert.Equal(2, aggregated.Count);
        var hintNames = aggregated.Select(r => r.HintName).ToHashSet();
        Assert.Contains("Customer.g.cs", hintNames);
        Assert.Contains("Customer.Mapper.g.cs", hintNames);
    }

    [Fact]
    public void AggregateResults_WithSameGeneratorCategory_MultipleCollisions()
    {
        // Arrange - Same generator category emitting same hint multiple times
        var results = new List<(string, SourceText, string)>
        {
            ("Customer.g.cs", SourceText.From("// Customer 1", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer 2", System.Text.Encoding.UTF8), "Mapper"),
            ("Customer.g.cs", SourceText.From("// Customer 3", System.Text.Encoding.UTF8), "Mapper"),
        };
        var diagnostics = new List<Microsoft.CodeAnalysis.Diagnostic>();

        // Act
        var aggregated = _aggregator.AggregateResults(results, diagnostics).ToList();

        // Assert
        Assert.Equal(3, aggregated.Count);
        Assert.Equal(2, diagnostics.Count);

        var hintNames = aggregated.Select(r => r.HintName).ToHashSet();
        Assert.Equal(3, hintNames.Count);
        Assert.Contains("Customer.g.cs", hintNames);
        Assert.Contains("Customer.Mapper.g.cs", hintNames);
        Assert.Contains("Customer.Mapper1.g.cs", hintNames);
    }
}
