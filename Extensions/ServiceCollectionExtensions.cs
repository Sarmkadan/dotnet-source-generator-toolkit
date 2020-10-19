// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Caching;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Repositories;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetSourceGeneratorToolkit.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register toolkit services
/// with the dependency-injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the full set of source-generator toolkit services required for a complete
    /// generation run, including infrastructure, analysers, repositories, and all generator
    /// services. Uses <c>TryAdd</c> semantics so existing registrations are preserved.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance to allow call chaining.</returns>
    public static IServiceCollection AddSourceGeneratorToolkit(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Shared infrastructure — singletons to avoid redundant allocations across scopes
        services.TryAddSingleton<ICache, MemoryCache>();
        services.TryAddSingleton<IFileSystemService, FileSystemService>();
        services.TryAddSingleton<IConfigurationManager, ConfigurationManager>();

        // Analysers
        services.TryAddScoped<IAttributeAnalyzer, AttributeAnalyzer>();
        services.TryAddScoped<IEntityAnalyzer, EntityAnalyzer>();

        // Repositories
        services.TryAddScoped<IEntityRepository, EntityRepository>();
        services.TryAddScoped<IGenerationResultRepository, GenerationResultRepository>();

        // Generator services
        services.TryAddScoped<ISourceGeneratorService, SourceGeneratorService>();
        services.TryAddScoped<IRepositoryGeneratorService, RepositoryGeneratorService>();
        services.TryAddScoped<IMapperGeneratorService, MapperGeneratorService>();
        services.TryAddScoped<IValidatorGeneratorService, ValidatorGeneratorService>();
        services.TryAddScoped<ISerializerGeneratorService, SerializerGeneratorService>();

        // Incremental generation support
        services.TryAddScoped<IIncrementalGeneratorService, IncrementalGeneratorService>();

        return services;
    }

    /// <summary>
    /// Registers incremental generation services that enable selective regeneration by
    /// detecting source-file changes between consecutive builds. Automatically registers
    /// <see cref="ICache"/> and <see cref="IFileSystemService"/> if not already present.
    /// </summary>
    /// <remarks>
    /// Call this method when you want incremental support without registering the full
    /// toolkit. Call <see cref="AddSourceGeneratorToolkit"/> instead if you need everything.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <paramref name="services"/> instance to allow call chaining.</returns>
    public static IServiceCollection AddIncrementalGeneration(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.TryAddSingleton<ICache, MemoryCache>();
        services.TryAddSingleton<IFileSystemService, FileSystemService>();
        services.TryAddScoped<IIncrementalGeneratorService, IncrementalGeneratorService>();

        return services;
    }
}
