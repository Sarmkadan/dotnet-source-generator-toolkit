// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetSourceGeneratorToolkit;

class Program
{
    static async Task Main(string[] args)
    {
        var services = ConfigureServices();
        var provider = services.BuildServiceProvider();

        try
        {
            var generatorService = provider.GetRequiredService<ISourceGeneratorService>();
            var projectPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║  .NET Source Generator Toolkit                 ║");
            Console.WriteLine("║  Roslyn-powered code generation from attributes║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.WriteLine($"Analyzing project: {projectPath}");
            var projectInfo = await generatorService.AnalyzeProjectAsync(projectPath);

            if (projectInfo.Entities.Count == 0)
            {
                Console.WriteLine("No entities found for generation.");
                return;
            }

            Console.WriteLine($"Found {projectInfo.Entities.Count} entities to process.");
            Console.WriteLine();

            // Generate repositories
            var repoGenerator = provider.GetRequiredService<IRepositoryGeneratorService>();
            foreach (var entity in projectInfo.Entities)
            {
                var repository = await repoGenerator.GenerateRepositoryAsync(entity);
                Console.WriteLine($"✓ Generated repository for: {repository.EntityName}");
            }

            // Generate mappers
            var mapperGenerator = provider.GetRequiredService<IMapperGeneratorService>();
            var mappers = await mapperGenerator.GenerateAllMappersAsync(projectInfo.Entities);
            Console.WriteLine($"✓ Generated {mappers.Count} mapper(s)");

            // Generate validators
            var validatorGenerator = provider.GetRequiredService<IValidatorGeneratorService>();
            var validators = await validatorGenerator.GenerateAllValidatorsAsync(projectInfo.Entities);
            Console.WriteLine($"✓ Generated {validators.Count} validator(s)");

            Console.WriteLine();
            Console.WriteLine("Generation completed successfully!");
        }
        catch (Exception ex)
        {
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

        // Register core services
        services.AddScoped<ISourceGeneratorService, SourceGeneratorService>();
        services.AddScoped<IRepositoryGeneratorService, RepositoryGeneratorService>();
        services.AddScoped<IMapperGeneratorService, MapperGeneratorService>();
        services.AddScoped<IValidatorGeneratorService, ValidatorGeneratorService>();
        services.AddScoped<ISerializerGeneratorService, SerializerGeneratorService>();

        // Register repositories
        services.AddScoped<IEntityRepository, EntityRepository>();
        services.AddScoped<IGenerationResultRepository, GenerationResultRepository>();

        // Register analyzers
        services.AddScoped<IAttributeAnalyzer, AttributeAnalyzer>();
        services.AddScoped<IEntityAnalyzer, EntityAnalyzer>();

        return services;
    }
}
