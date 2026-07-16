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
/// Verifies that <see cref="AutoImplementGenerator"/> reports the expected compiler diagnostics
/// - correct IDs and severities - for invalid inputs, and stays silent for valid ones.
/// </summary>
public sealed class AutoImplementGeneratorDiagnosticTests
{
    [Fact]
    public void NonPartialClass_ReportsSGTK001_AsError()
    {
        const string source = """
            using DotNetSourceGeneratorToolkit.Generated;
            namespace Acme
            {
                [GenerateToString]
                public class NotPartial
                {
                    public int Id { get; set; }
                }
            }
            """;

        var result = GeneratorTestHarness.Run(new AutoImplementGenerator(), source);

        var diagnostic = result.Diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SGTK001");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("NotPartial");
    }

    [Fact]
    public void NonPartialClass_DoesNotEmitSource()
    {
        const string source = """
            using DotNetSourceGeneratorToolkit.Generated;
            namespace Acme
            {
                [GenerateEquals]
                public class NotPartial
                {
                    public int Id { get; set; }
                }
            }
            """;

        var result = GeneratorTestHarness.Run(new AutoImplementGenerator(), source);

        // Only the post-initialization attribute file should be present, no generated members.
        result.Results.SelectMany(r => r.GeneratedSources)
            .Select(s => s.HintName)
            .Should().OnlyContain(name => name == "AutoImplementAttributes.g.cs");
    }

    [Fact]
    public void PartialClassWithNoPublicProperties_ReportsSGTK002_AsWarning()
    {
        const string source = """
            using DotNetSourceGeneratorToolkit.Generated;
            namespace Acme
            {
                [GenerateToString]
                public partial class Empty
                {
                    private int _hidden;
                }
            }
            """;

        var result = GeneratorTestHarness.Run(new AutoImplementGenerator(), source);

        var diagnostic = result.Diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SGTK002");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void PartialClassWithNoPublicProperties_StillEmitsTrivialSource()
    {
        const string source = """
            using DotNetSourceGeneratorToolkit.Generated;
            namespace Acme
            {
                [GenerateToString]
                public partial class Empty
                {
                }
            }
            """;

        var result = GeneratorTestHarness.Run(new AutoImplementGenerator(), source);
        var generated = GeneratorTestHarness.GetGeneratedText(result, "Empty.ToString.g.cs");

        generated.Should().Contain("public override string ToString() => \"Empty { }\";");
    }

    [Fact]
    public void StaticClass_ReportsSGTK003_AsError()
    {
        const string source = """
            using DotNetSourceGeneratorToolkit.Generated;
            namespace Acme
            {
                [GenerateToString]
                public static partial class Helpers
                {
                    public static int Value { get; set; }
                }
            }
            """;

        var result = GeneratorTestHarness.Run(new AutoImplementGenerator(), source);

        // Static + no public *instance* properties -> both SGTK003 and SGTK002.
        result.Diagnostics.Select(d => d.Id).Should().Contain("SGTK003");
        result.Diagnostics.Single(d => d.Id == "SGTK003").Severity.Should().Be(DiagnosticSeverity.Error);
    }

    [Fact]
    public void ValidPartialClass_ReportsNoDiagnostics()
    {
        const string source = """
            using DotNetSourceGeneratorToolkit.Generated;
            namespace Acme
            {
                [GenerateToString]
                public partial class Ok
                {
                    public int Id { get; set; }
                }
            }
            """;

        var result = GeneratorTestHarness.Run(new AutoImplementGenerator(), source);

        result.Diagnostics.Should().BeEmpty();
    }
}
