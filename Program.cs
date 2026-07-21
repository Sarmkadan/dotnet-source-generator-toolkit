#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using DotNetSourceGeneratorToolkit.Batch;
using DotNetSourceGeneratorToolkit.CLI;
using DotNetSourceGeneratorToolkit.Caching;
using DotNetSourceGeneratorToolkit.Domain;
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

            // Handle stats flag
            if (cliOptions.Stats)
            {
                await HandleStatsCommandAsync(provider, cliOptions);
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

    static async Task HandleStatsCommandAsync(IServiceProvider provider, CliOptions cliOptions)
    {
        _logger?.LogInformation("📊 Collecting statistics...");

        var formatterFactory = provider.GetRequiredService<IFormatterFactory>();
        var metricsCollector = provider.GetService<GenerationMetricsCollector>();
        var projectInfo = provider.GetService<ProjectInfo>();

        // Create stats data
        var statsData = new StatsData
        {
            Timestamp = DateTime.UtcNow,
            ProjectPath = cliOptions.ProjectPath,
            EntityCount = projectInfo?.Entities.Count ?? 0,
            PropertyCount = projectInfo?.Entities.Sum(e => e.Properties.Count) ?? 0,
            GenerationMetrics = metricsCollector?.GetSnapshot()
        };

        if (projectInfo != null)
        {
            statsData.ProjectStatistics = projectInfo.GetStatistics();
        }

        // Format output
        var format = cliOptions.OutputFormat ?? "Text";
        var formatter = formatterFactory.Create(format);

        var statsResult = new GenerationResult
        {
            EntityName = "stats",
            GeneratorType = GeneratorType.Repository,
            GeneratedCode = statsData.ToString(),
            OutputFilePath = "stats.txt",
            Status = GenerationStatus.Completed,
            CodeLineCount = statsData.ToString().Split(Environment.NewLine).Length,
            GenerationDurationMs = 0
        };

        var output = formatter.Format([statsResult]);

        Console.WriteLine(output);
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
        services.AddSingleton<GenerationMetricsCollector>();
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

/// <summary>
/// Data structure for holding statistics to display
/// </summary>
public sealed class StatsData
{
    public DateTime Timestamp { get; set; }
    public string ProjectPath { get; set; } = string.Empty;
    public int EntityCount { get; set; }
    public int PropertyCount { get; set; }
    public GenerationMetricsCollector.MetricsSnapshot? GenerationMetrics { get; set; }
    public ProjectStatistics? ProjectStatistics { get; set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("📊 Source Generator Toolkit Statistics");
        sb.AppendLine("====================================");
        sb.AppendLine($"Timestamp: {Timestamp:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Project Path: {ProjectPath}");
        sb.AppendLine();

        sb.AppendLine("📈 Entity & Property Counts:");
        sb.AppendLine($"  Entities: {EntityCount}");
        sb.AppendLine($"  Properties: {PropertyCount}");

        if (ProjectStatistics != null)
        {
            sb.AppendLine();
            sb.AppendLine("📊 Project Statistics:");
            sb.AppendLine($"  Total Entities: {ProjectStatistics.TotalEntities}");
            sb.AppendLine($"  Total Properties: {ProjectStatistics.TotalProperties}");
            sb.AppendLine($"  Successful Generations: {ProjectStatistics.TotalGenerated}");
            sb.AppendLine($"  Failed Generations: {ProjectStatistics.TotalFailed}");
            sb.AppendLine($"  Success Rate: {ProjectStatistics.SuccessRate:F2}%");
            sb.AppendLine($"  Total Code Lines: {ProjectStatistics.TotalCodeLines}");
            sb.AppendLine($"  Total Generation Time: {ProjectStatistics.TotalGenerationTime}ms");
        }

        if (GenerationMetrics != null)
        {
            sb.AppendLine();
            sb.AppendLine("⚡ Generation Metrics:");
            sb.AppendLine($"  Total Generations: {GenerationMetrics.TotalGenerations}");
            sb.AppendLine($"  Successful: {GenerationMetrics.SuccessfulGenerations} ({GenerationMetrics.SuccessRate:F1}%)");
            sb.AppendLine($"  Failed: {GenerationMetrics.FailedGenerations}");
            sb.AppendLine($"  Total Duration: {GenerationMetrics.TotalDurationMs}ms");
            sb.AppendLine($"  Average Duration: {GenerationMetrics.AverageDurationMs:F2}ms");
            sb.AppendLine($"  First Generation: {GenerationMetrics.FirstGenerationStart?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}");
            sb.AppendLine($"  Last Generation: {GenerationMetrics.LastGenerationEnd.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine($"  Generation Rate: {GenerationMetrics.GenerationRatePerHour:F2} gen/hour");
        }

        return sb.ToString();
    }
}
