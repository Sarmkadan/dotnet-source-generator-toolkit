#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Reflection;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Generators;

/// <summary>
/// Verifier-style snapshot tests that pin the exact source emitted by each attribute-driven
/// generator service for the shared <see cref="SnapshotFixtures.Product"/> fixture. The expected
/// output lives in the embedded <c>*.verified.txt</c> files; a diff there is an intentional
/// generation change and must be reviewed. These snapshots capture current behaviour as-is,
/// including any templating quirks, so regressions are caught immediately.
/// </summary>
public sealed class GeneratorServiceSnapshotTests
{
    [Fact]
    public async Task RepositoryGenerator_MatchesSnapshot()
    {
        var service = new RepositoryGeneratorService(NullLogger<RepositoryGeneratorService>.Instance);
        var result = await service.GenerateRepositoryAsync(SnapshotFixtures.Product());

        AssertMatchesSnapshot(result.GeneratedCode, "Product.Repository.verified.txt");
        result.Status.Should().Be(GenerationStatus.Completed);
    }

    [Fact]
    public async Task MapperGenerator_MatchesSnapshot()
    {
        var service = new MapperGeneratorService(NullLogger<MapperGeneratorService>.Instance);
        var entity = SnapshotFixtures.Product();
        var result = await service.GenerateMapperAsync(entity, entity);

        AssertMatchesSnapshot(result.GeneratedCode, "Product.Mapper.verified.txt");
        result.Status.Should().Be(GenerationStatus.Completed);
    }

    [Fact]
    public async Task ValidatorGenerator_MatchesSnapshot()
    {
        var service = new ValidatorGeneratorService(NullLogger<ValidatorGeneratorService>.Instance);
        var result = await service.GenerateValidatorAsync(SnapshotFixtures.Product());

        AssertMatchesSnapshot(result.GeneratedCode, "Product.Validator.verified.txt");
        result.Status.Should().Be(GenerationStatus.Completed);
    }

    [Fact]
    public async Task JsonSerializerGenerator_MatchesSnapshot()
    {
        var service = new SerializerGeneratorService(NullLogger<SerializerGeneratorService>.Instance);
        var result = await service.GenerateSerializerAsync(SnapshotFixtures.Product(), SerializerFormat.Json);

        AssertMatchesSnapshot(result.GeneratedCode, "Product.JsonSerializer.verified.txt");
        result.Status.Should().Be(GenerationStatus.Completed);
    }

    private static void AssertMatchesSnapshot(string actual, string snapshotFileSuffix)
    {
        var expected = LoadSnapshot(snapshotFileSuffix);
        GeneratorTestHarness.Normalize(actual).Should().Be(GeneratorTestHarness.Normalize(expected));
    }

    private static string LoadSnapshot(string suffix)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames()
            .Single(n => n.EndsWith(suffix, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
