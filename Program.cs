#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetSourceGeneratorToolkit.Batch;
using DotNetSourceGeneratorToolkit.CLI;
using DotNetSourceGeneratorToolkit.Caching;
using DotNetSourceGeneratorToolkit.Events;
using DotNetSourceGeneratorToolkit.Extensions;
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

            // Parse CLI arguments
            var argumentParser = provider.GetRequiredService<ICliArgumentParser>();
            var cliOptions = argumentParser.Parse(args);

            // Handle help and version flags
            if (cliOptions.ShowHelp)
            {
                Console.WriteLine(argumentParser.GetHelpMessage());
                return;
            }

            if (cliOptions.ShowVersion)
            {
                Console.WriteLine(argumentParser.GetVersionInfo());
                return;
            }

            // Validate options
            var validationErrors = argumentParser.Validate(cliOptions).ToList();
            if (validationErrors.Count > 0)
            {
                foreach (var error in validationErrors)
                {
                    Console.Error.WriteLine($"Error: {error}");
                }
                Environment.Exit(1);
            }

            // Set dry-run flag on FileSystemService if enabled
            var fileSystemService = provider.GetRequiredService<IFileSystemService>() as FileSystemService;
            if (fileSystemService != null && cliOptions.DryRun)
            {
                fileSystemService.SetDryRun(true);
            }

            // Create middleware context with CLI options
            var context = new MiddlewareContext
            {
                CliOptions = cliOptions,
                RequestId = Guid.NewGuid().ToString(),
                StartTime = DateTime.UtcNow
            };

            _logger.LogInformation("╔════════════════════════════════════════════════╗");
            _logger.LogInformation("║ .NET Source Generator Toolkit ║");
            _logger.LogInformation("║ Roslyn-powered code generation from attributes║");
            _logger.LogInformation("╚════════════════════════════════════════════════╝");
            _logger.LogInformation("Starting source generation toolkit...");
            _logger.LogInformation("Analyzing project: {ProjectPath}", cliOptions.ProjectPath);

            if (cliOptions.DryRun)
            {
                _logger.LogInformation("🔍 Dry-run mode enabled - no files will be written");
            }

            var middlewarePipeline = provider.GetRequiredService<IMiddlewarePipeline>();

            await middlewarePipeline.ExecuteAsync(context);

            if (context.Errors.Count > 0)
            {
                foreach (var error in context.Errors)
                {
                    Console.Error.WriteLine($"Error: {error}");
                }
                Environment.Exit(1);
            }

            if (context.GenerationResults.Count == 0 && !cliOptions.DryRun)
            {
                _logger.LogWarning("No generation results produced");
            }

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

        // Core toolkit registrations (infrastructure, analyzers, repositories, generators)
        // live in one place so the CLI host and embedding hosts cannot drift apart.
        services.AddSourceGeneratorToolkit();

        // CLI-host specific pieces on top of the toolkit
        services.AddLogging();
        services.AddSingleton<IEventPublisher, EventAggregator>();
        services.AddHttpClient<IHttpClientService, HttpClientService>();
        services.AddSingleton<IFormatterFactory, FormatterFactory>();
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IMiddlewarePipeline, MiddlewarePipeline>();
        services.AddScoped(typeof(IBatchProcessor<>), typeof(BatchProcessor<>));

        // Register CLI services
        services.AddSingleton<ICliArgumentParser, CliArgumentParser>();

        return services;
    }
}