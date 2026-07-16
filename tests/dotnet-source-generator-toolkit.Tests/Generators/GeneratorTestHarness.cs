#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetSourceGeneratorToolkit.Tests.Generators;

/// <summary>
/// Small helper that drives an <see cref="IIncrementalGenerator"/> against in-memory source
/// and exposes the run result so tests can assert generated text, diagnostics, and caching.
/// </summary>
internal static class GeneratorTestHarness
{
    private static readonly ImmutableArray<MetadataReference> References = BuildReferences();

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        var trusted = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");
        var refs = new List<MetadataReference>();

        if (trusted is not null)
        {
            foreach (var path in trusted.Split(Path.PathSeparator))
            {
                if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && File.Exists(path))
                    refs.Add(MetadataReference.CreateFromFile(path));
            }
        }
        else
        {
            refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        }

        return refs.ToImmutableArray();
    }

    public static CSharpCompilation CreateCompilation(params string[] sources)
    {
        var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s)).ToArray();
        return CSharpCompilation.Create(
            assemblyName: "GeneratorTests",
            syntaxTrees: trees,
            references: References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    public static GeneratorDriver CreateDriver(IIncrementalGenerator generator)
    {
        return CSharpGeneratorDriver.Create(
            generators: new[] { generator.AsSourceGenerator() },
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));
    }

    public static GeneratorDriverRunResult Run(IIncrementalGenerator generator, params string[] sources)
    {
        var compilation = CreateCompilation(sources);
        var driver = CreateDriver(generator).RunGenerators(compilation);
        return driver.GetRunResult();
    }

    /// <summary>
    /// Returns the generated text for a hint-name that ends with the supplied suffix.
    /// Trailing whitespace on each line is trimmed and newlines normalised so snapshots are
    /// stable across platforms.
    /// </summary>
    public static string GetGeneratedText(GeneratorDriverRunResult result, string hintSuffix)
    {
        var source = result.Results
            .SelectMany(r => r.GeneratedSources)
            .Single(s => s.HintName.EndsWith(hintSuffix, StringComparison.Ordinal));

        return Normalize(source.SourceText.ToString());
    }

    public static string Normalize(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd());
        return string.Join("\n", lines).TrimEnd('\n');
    }
}
