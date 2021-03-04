#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetSourceGeneratorToolkit.Batch;
using DotNetSourceGeneratorToolkit.Caching;
using DotNetSourceGeneratorToolkit.Events;
using DotNetSourceGeneratorToolkit.Formatters;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Integration;
using DotNetSourceGeneratorToolkit.Middleware;
using DotNetSourceGeneratorToolkit.Repositories;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit;

class Program
{
    private static ILogger<Program>? _logger;

    static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var provider = services.BuildServiceProvider();

        try
        {
            _logger = provider.GetRequiredService<ILogger<Program>>();
            var generatorService = provider.GetRequiredService<ISourceGeneratorService>();
            var projectPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

            _logger.LogInformation("╔════════════════════════════════════════════════╗");
            _logger.LogInformation("║ .NET Source Generator Toolkit ║");
            _logger.LogInformation("║ Roslyn-powered code generation from attributes║");
            _logger.LogInformation("╚════════════════════════════════════════════════╝");
            _logger.LogInformation("Starting source generation toolkit...");
            _logger.LogInformation("Analyzing project: {ProjectPath}", projectPath);

            var projectInfo = await generatorService.AnalyzeProjectAsync(projectPath);

            if (projectInfo.Entities.Count == 0)
            {
                _logger.LogWarning("No entities found for generation.");
                return;
            }

            _logger.LogInformation("Found {EntityCount} entities to process.", projectInfo.Entities.Count);

            // Generate repositories
            var repoGenerator = provider.GetRequiredService<IRepositoryGeneratorService>();
            foreach (var entity in projectInfo.Entities)
            {
                var repository = await repoGenerator.GenerateRepositoryAsync(entity);
                _logger.LogInformation("✓ Generated repository for: {EntityName}", repository.EntityName);
            }

            // Generate mappers
            var mapperGenerator = provider.GetRequiredService<IMapperGeneratorService>();
            var mappers = await mapperGenerator.GenerateAllMappersAsync(projectInfo.Entities);
            _logger.LogInformation("✓ Generated {MapperCount} mapper(s)", mappers.Count());

            // Generate validators
            var validatorGenerator = provider.GetRequiredService<IValidatorGeneratorService>();
            var validators = await validatorGenerator.GenerateAllValidatorsAsync(projectInfo.Entities);
            _logger.LogInformation("✓ Generated {ValidatorCount} validator(s)", validators.Count());

            _logger.LogInformation("Generation completed successfully!");
        }
        catch (Exception ex)
        {
            var logger = provider.GetService<ILogger<Program>>();
            logger?.LogError(ex, "Application error occurred");
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            await provider.DisposeAsync();
        }
    }

    static IServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register infrastructure
        services.AddLogging();
        services.AddSingleton<IConfigurationManager, ConfigurationManager>();
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IEventPublisher, EventAggregator>();
        services.AddHttpClient<IHttpClientService, HttpClientService>();
        services.AddSingleton<IFormatterFactory, FormatterFactory>();

        // Register core services
        services.AddScoped<ISourceGeneratorService, SourceGeneratorService>();
        services.AddScoped<IRepositoryGeneratorService, RepositoryGeneratorService>();
        services.AddScoped<IMapperGeneratorService, MapperGeneratorService>();
        services.AddScoped<IValidatorGeneratorService, ValidatorGeneratorService>();
        services.AddScoped<ISerializerGeneratorService, SerializerGeneratorService>();
        services.AddScoped<IWebhookService, WebhookService>();

        // Register repositories
        services.AddScoped<IEntityRepository, EntityRepository>();
        services.AddScoped<IGenerationResultRepository, GenerationResultRepository>();

        // Register analyzers
        services.AddScoped<IAttributeAnalyzer, AttributeAnalyzer>();
        services.AddScoped<IEntityAnalyzer, EntityAnalyzer>();

        // Register Caching
        services.AddSingleton<ICache, MemoryCache>();

        // Register Middleware Pipeline
        services.AddScoped<IMiddlewarePipeline, MiddlewarePipeline>();

        // Register Batch Processor
        services.AddScoped(typeof(IBatchProcessor<>), typeof(BatchProcessor<>));

        return services;
    }
}
