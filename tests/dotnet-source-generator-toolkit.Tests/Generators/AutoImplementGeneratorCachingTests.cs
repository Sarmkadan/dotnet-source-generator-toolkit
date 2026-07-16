#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Generators;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Generators;

/// <summary>
/// Proves that <see cref="AutoImplementGenerator"/> is a well-behaved incremental generator:
/// when the compilation changes in a way unrelated to its inputs, the pipeline reuses cached
/// outputs instead of re-running the transform. This is the property that keeps IDE typing fast.
/// </summary>
public sealed class AutoImplementGeneratorCachingTests
{
    private const string TargetSource = """
        using DotNetSourceGeneratorToolkit.Generated;
        namespace Shop
        {
            [GenerateToString]
            public partial class Order
            {
                public int Id { get; set; }
                public string Customer { get; set; }
            }
        }
        """;

    // An unrelated file that carries no marker attributes. Editing it must not disturb the
    // generator's cached output for Order.
    private const string UnrelatedSourceV1 = """
        namespace Shop
        {
            public class Unrelated
            {
                public int A { get; set; }
            }
        }
        """;

    private const string UnrelatedSourceV2 = """
        namespace Shop
        {
            public class Unrelated
            {
                public int A { get; set; }
                public int B { get; set; }
                public string Note = "changed";
            }
        }
        """;

    [Fact]
    public void UnrelatedChange_DoesNotReRunGenerationStep()
    {
        var generator = new AutoImplementGenerator();
        var driver = GeneratorTestHarness.CreateDriver(generator);

        var compilation1 = GeneratorTestHarness.CreateCompilation(TargetSource, UnrelatedSourceV1);
        driver = driver.RunGenerators(compilation1);

        // Replace only the unrelated syntax tree with an edited version.
        var oldTree = compilation1.SyntaxTrees.Single(t => t.ToString().Contains("class Unrelated"));
        var newTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(UnrelatedSourceV2);
        var compilation2 = compilation1.ReplaceSyntaxTree(oldTree, newTree);

        driver = driver.RunGenerators(compilation2);

        var steps = driver.GetRunResult().Results
            .SelectMany(r => r.TrackedOutputSteps)
            .SelectMany(kvp => kvp.Value)
            .SelectMany(step => step.Outputs);

        // Every output for the second run must have come from cache, not a fresh computation.
        steps.Should().NotBeEmpty();
        steps.Should().OnlyContain(o =>
            o.Reason == IncrementalStepRunReason.Cached || o.Reason == IncrementalStepRunReason.Unchanged);
    }

    [Fact]
    public void RelatedChange_ReRunsAndProducesNewOutput()
    {
        var generator = new AutoImplementGenerator();
        var driver = GeneratorTestHarness.CreateDriver(generator);

        var compilation1 = GeneratorTestHarness.CreateCompilation(TargetSource, UnrelatedSourceV1);
        driver = driver.RunGenerators(compilation1);
        var before = GeneratorTestHarness.GetGeneratedText(driver.GetRunResult(), "Order.ToString.g.cs");

        // Add a property to the annotated type - this is a relevant change.
        var editedTarget = TargetSource.Replace(
            "public string Customer { get; set; }",
            "public string Customer { get; set; }\n        public decimal Total { get; set; }");

        var oldTree = compilation1.SyntaxTrees.Single(t => t.ToString().Contains("class Order"));
        var newTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(editedTarget);
        var compilation2 = compilation1.ReplaceSyntaxTree(oldTree, newTree);

        driver = driver.RunGenerators(compilation2);
        var after = GeneratorTestHarness.GetGeneratedText(driver.GetRunResult(), "Order.ToString.g.cs");

        before.Should().NotContain("Total");
        after.Should().Contain("Total = {Total}");
    }
}
