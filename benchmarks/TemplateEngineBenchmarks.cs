#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Benchmarks rendering 1000 entities through <see cref="TemplateEngineService"/>, contrasting a
/// warm, cache-reusing service against a service instance recreated per render (the shape of the
/// old, always-reparse behavior, since the parsed-template cache lives on the service instance).
/// Run with: <c>dotnet run -c Release --project benchmarks -- --filter *TemplateEngine*</c>.
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class TemplateEngineBenchmarks
{
    private const int EntityCount = 1000;

    private const string EntityTemplate =
        "public sealed class {{ClassName}}\n" +
        "{\n" +
        "{{#if HasId}}\n" +
        "    public int Id { get; set; }\n" +
        "{{#else}}\n" +
        "    // no identity column\n" +
        "{{/if}}\n" +
        "{{#for prop in Properties}}\n" +
        "    public string {{prop}} { get; set; }\n" +
        "{{/for}}\n" +
        "}";

    private IFileSystemService _fileSystemService = null!;
    private ITemplateEngineService _warmService = null!;
    private List<Dictionary<string, object>> _contexts = null!;

    /// <summary>Builds shared dependencies and the per-entity render contexts.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _fileSystemService = new FileSystemService(NullLogger<FileSystemService>.Instance);
        _warmService = new TemplateEngineService(_fileSystemService, NullLogger<TemplateEngineService>.Instance);

        _contexts = new List<Dictionary<string, object>>(EntityCount);
        for (var i = 0; i < EntityCount; i++)
        {
            _contexts.Add(new Dictionary<string, object>
            {
                ["ClassName"] = $"Entity{i}",
                ["HasId"] = i % 2 == 0,
                ["Properties"] = new List<object> { "Name", "Value", "CreatedAt" },
            });
        }
    }

    /// <summary>
    /// Renders 1000 entities using a single, long-lived <see cref="TemplateEngineService"/> so the
    /// parsed-template cache is populated once and reused for the remaining 999 renders (the "after" case).
    /// </summary>
    [Benchmark(Baseline = true)]
    [BenchmarkCategory("TemplateEngine")]
    public async Task RenderThousandEntities_WarmCache()
    {
        foreach (var context in _contexts)
        {
            var rendered = await _warmService.RenderAsync(EntityTemplate, context, GenerationOptions.Default);
            if (string.IsNullOrEmpty(rendered))
                throw new InvalidOperationException("Template rendering produced empty output");
        }
    }

    /// <summary>
    /// Renders 1000 entities using a fresh <see cref="TemplateEngineService"/> per entity, so the
    /// parsed-template cache is never reused across renders - reproducing the always-reparse cost
    /// the cache was added to eliminate (the "before" case).
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("TemplateEngine")]
    public async Task RenderThousandEntities_ColdCachePerRender()
    {
        foreach (var context in _contexts)
        {
            var coldService = new TemplateEngineService(_fileSystemService, NullLogger<TemplateEngineService>.Instance);
            var rendered = await coldService.RenderAsync(EntityTemplate, context, GenerationOptions.Default);
            if (string.IsNullOrEmpty(rendered))
                throw new InvalidOperationException("Template rendering produced empty output");
        }
    }
}
