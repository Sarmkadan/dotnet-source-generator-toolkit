#nullable enable

// =============================================================================
// Author: Test Generator
// Verifies that template compilation caching does not alter render output.
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Services;

/// <summary>
/// Tests for <see cref="TemplateEngineService"/> covering the parsed-template cache and
/// StringBuilder pooling introduced for repeated-render performance.
/// </summary>
public sealed class TemplateEngineServiceCachingTests
{
    private const string Template =
        "public class {{ClassName}}\n" +
        "{\n" +
        "{{#if HasId}}\n" +
        "    public int Id { get; set; }\n" +
        "{{#else}}\n" +
        "    // no id\n" +
        "{{/if}}\n" +
        "{{#for prop in Properties}}\n" +
        "    public string {{prop}} { get; set; }\n" +
        "{{/for}}\n" +
        "}";

    private static TemplateEngineService CreateService() =>
        new(new FileSystemService(NullLogger<FileSystemService>.Instance), NullLogger<TemplateEngineService>.Instance);

    private static Dictionary<string, object> CreateContext() => new()
    {
        ["ClassName"] = "Widget",
        ["HasId"] = true,
        ["Properties"] = new List<object> { "Name", "Price" },
    };

    /// <summary>
    /// Rendering the same template text repeatedly must populate the internal template cache
    /// exactly once while still producing identical output on every call.
    /// </summary>
    [Fact]
    public async Task RenderAsync_SameTemplateRenderedTwice_UsesCacheAndProducesIdenticalOutput()
    {
        var service = CreateService();
        var context = CreateContext();

        var firstRender = await service.RenderAsync(Template, context, GenerationOptions.Default);
        Assert.True(service.TemplateCache.ContainsKey(Template));
        var cachedInstanceAfterFirstRender = service.TemplateCache[Template];

        var secondRender = await service.RenderAsync(Template, context, GenerationOptions.Default);
        var cachedInstanceAfterSecondRender = service.TemplateCache[Template];

        Assert.Equal(firstRender, secondRender);
        Assert.Same(cachedInstanceAfterFirstRender, cachedInstanceAfterSecondRender);
        Assert.Single(service.TemplateCache);
    }

    /// <summary>
    /// A render produced by a service instance that already cached the compiled template must
    /// be byte-for-byte identical to a render produced by a fresh, uncached service instance.
    /// </summary>
    [Fact]
    public async Task RenderAsync_CachedVersusUncached_ProducesIdenticalOutput()
    {
        var context = CreateContext();

        var uncachedService = CreateService();
        var uncachedResult = await uncachedService.RenderAsync(Template, context, GenerationOptions.Default);

        var warmedService = CreateService();
        await warmedService.RenderAsync(Template, context, GenerationOptions.Default);
        var cachedResult = await warmedService.RenderAsync(Template, context, GenerationOptions.Default);

        Assert.Equal(uncachedResult, cachedResult);
    }

    /// <summary>
    /// Rendering the same template with different context values must still resolve conditional
    /// branches and loop bodies correctly, proving the cached structure is context-agnostic.
    /// </summary>
    [Fact]
    public async Task RenderAsync_SameTemplateDifferentContext_ResolvesBranchesFromCachedStructure()
    {
        var service = CreateService();

        var withId = await service.RenderAsync(Template, CreateContext(), GenerationOptions.Default);
        var withoutIdContext = CreateContext();
        withoutIdContext["HasId"] = false;
        var withoutId = await service.RenderAsync(Template, withoutIdContext, GenerationOptions.Default);

        Assert.Contains("public int Id", withId);
        Assert.DoesNotContain("public int Id", withoutId);
        Assert.Contains("// no id", withoutId);
        Assert.Single(service.TemplateCache);
    }
}
