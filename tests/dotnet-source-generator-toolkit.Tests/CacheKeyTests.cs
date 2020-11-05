// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Caching;
using FluentAssertions;

namespace DotNetSourceGeneratorToolkit.Tests;

public class CacheKeyTests
{
    [Fact]
    public void ForGenerationResult_ReturnsCorrectFormat()
    {
        var key = CacheKey.ForGenerationResult("UserEntity", "Repository");
        key.Should().Be("generated::UserEntity::Repository");
    }

    [Fact]
    public void ForGenerationResult_DifferentEntities_ProduceDifferentKeys()
    {
        var key1 = CacheKey.ForGenerationResult("User", "Repository");
        var key2 = CacheKey.ForGenerationResult("Order", "Repository");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void ForGenerationResult_DifferentGenerators_ProduceDifferentKeys()
    {
        var key1 = CacheKey.ForGenerationResult("User", "Repository");
        var key2 = CacheKey.ForGenerationResult("User", "Validator");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void ForEntityAnalysis_SameContent_ProducesSameKey()
    {
        var key1 = CacheKey.ForEntityAnalysis("User.cs", "public class User {}");
        var key2 = CacheKey.ForEntityAnalysis("User.cs", "public class User {}");
        key1.Should().Be(key2);
    }

    [Fact]
    public void ForEntityAnalysis_DifferentContent_ProducesDifferentKeys()
    {
        var key1 = CacheKey.ForEntityAnalysis("User.cs", "public class User { }");
        var key2 = CacheKey.ForEntityAnalysis("User.cs", "public class User { int Id; }");
        key1.Should().NotBe(key2);
    }

    [Fact]
    public void ForEntityAnalysis_ContainsFileName()
    {
        var key = CacheKey.ForEntityAnalysis("/path/to/User.cs", "content");
        key.Should().Contain("User.cs");
    }

    [Fact]
    public void ForConfiguration_ProducesConsistentKey()
    {
        var key1 = CacheKey.ForConfiguration("config.json");
        var key2 = CacheKey.ForConfiguration("config.json");
        key1.Should().Be(key2);
    }

    [Fact]
    public void ForConfiguration_StartsWithConfigPrefix()
    {
        var key = CacheKey.ForConfiguration("settings.json");
        key.Should().StartWith("config::");
    }
}
