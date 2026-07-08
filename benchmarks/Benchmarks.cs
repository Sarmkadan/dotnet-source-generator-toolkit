#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Infrastructure;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Performance benchmarks for the .NET Source Generator Toolkit
/// Measures throughput and memory allocation for critical operations
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class Benchmarks
{
    private ServiceProvider? _serviceProvider;
    private ISourceGeneratorService? _generatorService;
    private IEntityAnalyzer? _entityAnalyzer;
    private IRepositoryGeneratorService? _repositoryGenerator;
    private IMapperGeneratorService? _mapperGenerator;
    private IValidatorGeneratorService? _validatorGenerator;
    private ISerializerGeneratorService? _serializerGenerator;
    private string? _tempProjectPath;

    /// <summary>
    /// Setup benchmark dependencies and create test project structure
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        // Create a temporary directory for test projects
        _tempProjectPath = Path.Combine(Path.GetTempPath(), "DotNetSourceGeneratorToolkit.Benchmarks");
        if (Directory.Exists(_tempProjectPath))
        {
            Directory.Delete(_tempProjectPath, true);
        }
        Directory.CreateDirectory(_tempProjectPath);

        // Setup DI container
        var services = new ServiceCollection();

        // Add logging (minimal for benchmarks)
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
            builder.AddFilter("DotNetSourceGeneratorToolkit", LogLevel.Warning);
            builder.AddConsole();
        });

        // Register all services
        services.AddSingleton<IFileSystemService, FileSystemService>();
        services.AddSingleton<IEntityAnalyzer, EntityAnalyzer>();
        services.AddSingleton<IRepositoryGeneratorService, RepositoryGeneratorService>();
        services.AddSingleton<IMapperGeneratorService, MapperGeneratorService>();
        services.AddSingleton<IValidatorGeneratorService, ValidatorGeneratorService>();
        services.AddSingleton<ISerializerGeneratorService, SerializerGeneratorService>();
        services.AddSingleton<ISourceGeneratorService, SourceGeneratorService>();

        _serviceProvider = services.BuildServiceProvider();

        _generatorService = _serviceProvider.GetRequiredService<ISourceGeneratorService>();
        _entityAnalyzer = _serviceProvider.GetRequiredService<IEntityAnalyzer>();
        _repositoryGenerator = _serviceProvider.GetRequiredService<IRepositoryGeneratorService>();
        _mapperGenerator = _serviceProvider.GetRequiredService<IMapperGeneratorService>();
        _validatorGenerator = _serviceProvider.GetRequiredService<IValidatorGeneratorService>();
        _serializerGenerator = _serviceProvider.GetRequiredService<ISerializerGeneratorService>();

        // Create test entity files
        CreateTestEntityFiles();
    }

    /// <summary>
    /// Cleanup temporary files
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        if (_tempProjectPath != null && Directory.Exists(_tempProjectPath))
        {
            Directory.Delete(_tempProjectPath, true);
        }

        _serviceProvider?.Dispose();
    }

    /// <summary>
    /// Create test entity files in the temporary project directory
    /// </summary>
    private void CreateTestEntityFiles()
    {
        var entities = new[]
        {
            typeof(BenchmarkEntities.SimpleEntity),
            typeof(BenchmarkEntities.ComplexEntity),
            typeof(BenchmarkEntities.GenericEntity),
            typeof(BenchmarkEntities.LargeEntity),
            typeof(BenchmarkEntities.FullFeaturedEntity)
        };

        foreach (var entityType in entities)
        {
            var fileName = $"{entityType.Name}.cs";
            var filePath = Path.Combine(_tempProjectPath!, fileName);
            var fileContent = GetEntityFileContent(entityType);
            File.WriteAllText(filePath, fileContent);
        }
    }

    /// <summary>
    /// Get the C# file content for a given entity type
    /// </summary>
    private static string GetEntityFileContent(Type entityType)
    {
        return $@"#nullable enable

using DotNetSourceGeneratorToolkit.Domain;

namespace BenchmarkTestProject;

{GetEntityClassContent(entityType)}
";
    }

    /// <summary>
    /// Get the entity class content with proper attributes
    /// </summary>
    private static string GetEntityClassContent(Type entityType)
    {
        var className = entityType.Name;
        var attributes = new List<string>();

        if (entityType.Name.Contains("Simple") || entityType.Name.Contains("Complex") ||
            entityType.Name.Contains("Large") || entityType.Name.Contains("FullFeatured"))
        {
            attributes.Add("[Repository]");
            attributes.Add("[Mapper]");
            attributes.Add("[Validator]");
        }

        if (entityType.Name.Contains("Serializable") || entityType.Name.Contains("FullFeatured"))
        {
            attributes.Add("[Serializer(Formats = new[] { \"Json\", \"Xml\", \"Csv\" })]");
        }

        var attributesText = string.Join("\n", attributes);

        return $@"{attributesText}
public sealed class {className}
{{
    public int Id {{ get; set; }}
    public string Name {{ get; set; }} = string.Empty;
    public string Description {{ get; set; }} = string.Empty;
    public decimal Price {{ get; set; }}
    public int Quantity {{ get; set; }}
    public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;
    public DateTime? UpdatedAt {{ get; set; }}
    public bool IsActive {{ get; set; }} = true;
}}";
    }

    /// <summary>
    /// Benchmark: Entity analysis performance for a single file
    /// Measures the time to parse and extract entities from a C# file
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Analysis")]
    public async Task EntityAnalysis_SingleFile()
    {
        var filePath = Path.Combine(_tempProjectPath!, "SimpleEntity.cs");
        var content = await File.ReadAllTextAsync(filePath);

        var entities = await _entityAnalyzer!.AnalyzeFileAsync(filePath, content);

        if (entities.Count() != 1)
        {
            throw new InvalidOperationException("Expected exactly 1 entity");
        }
    }

    /// <summary>
    /// Benchmark: Entity analysis performance for multiple files
    /// Measures the time to parse and extract entities from multiple C# files
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Analysis")]
    public async Task EntityAnalysis_MultipleFiles()
    {
        var filePaths = Directory.GetFiles(_tempProjectPath!, "*.cs");
        var tasks = new List<Task>();

        foreach (var filePath in filePaths)
        {
            var content = await File.ReadAllTextAsync(filePath);
            tasks.Add(_entityAnalyzer!.AnalyzeFileAsync(filePath, content));
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Benchmark: Repository generation performance
    /// Measures the time to generate repository code for an entity
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Generation")]
    public async Task RepositoryGeneration_SingleEntity()
    {
        var entity = new Entity
        {
            Name = "TestEntity",
            Namespace = "TestNamespace",
            Attributes = { "Repository" }
        };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsRequired = true });
        entity.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = false });

        var result = await _repositoryGenerator!.GenerateRepositoryAsync(entity);

        if (string.IsNullOrEmpty(result.GeneratedCode))
        {
            throw new InvalidOperationException("Repository generation failed");
        }
    }

    /// <summary>
    /// Benchmark: Mapper generation performance
    /// Measures the time to generate mapper code for an entity
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Generation")]
    public async Task MapperGeneration_SingleEntity()
    {
        var entity = new Entity
        {
            Name = "TestEntity",
            Namespace = "TestNamespace",
            Attributes = { "Mapper" }
        };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsRequired = true });
        entity.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = false });

        var result = await _mapperGenerator!.GenerateMapperAsync(entity, entity);

        if (string.IsNullOrEmpty(result.GeneratedCode))
        {
            throw new InvalidOperationException("Mapper generation failed");
        }
    }

    /// <summary>
    /// Benchmark: Validator generation performance
    /// Measures the time to generate validator code for an entity
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Generation")]
    public async Task ValidatorGeneration_SingleEntity()
    {
        var entity = new Entity
        {
            Name = "TestEntity",
            Namespace = "TestNamespace",
            Attributes = { "Validator" }
        };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsRequired = true });
        entity.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = false });
        entity.AddProperty(new EntityProperty { Name = "Price", Type = "decimal", IsRequired = false });

        var result = await _validatorGenerator!.GenerateValidatorAsync(entity);

        if (string.IsNullOrEmpty(result.GeneratedCode))
        {
            throw new InvalidOperationException("Validator generation failed");
        }
    }

    /// <summary>
    /// Benchmark: Serializer generation performance
    /// Measures the time to generate serializer code for an entity
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Generation")]
    public async Task SerializerGeneration_SingleEntity()
    {
        var entity = new Entity
        {
            Name = "TestEntity",
            Namespace = "TestNamespace",
            Attributes = { "Serializer" }
        };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsRequired = true });
        entity.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = false });

        var result = await _serializerGenerator!.GenerateSerializerAsync(entity, SerializerFormat.Json);

        if (string.IsNullOrEmpty(result.GeneratedCode))
        {
            throw new InvalidOperationException("Serializer generation failed");
        }
    }

    /// <summary>
    /// Benchmark: Full project analysis
    /// Measures the time to analyze a complete project with multiple entities
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Project")]
    public async Task ProjectAnalysis_FullProject()
    {
        var projectInfo = await _generatorService!.AnalyzeProjectAsync(_tempProjectPath!);

        if (projectInfo.Entities.Count < 3)
        {
            throw new InvalidOperationException("Expected at least 3 entities");
        }
    }

    /// <summary>
    /// Benchmark: Full code generation for a project
    /// Measures the time to generate all code for a complete project
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Project")]
    public async Task ProjectGeneration_FullProject()
    {
        var projectInfo = await _generatorService!.AnalyzeProjectAsync(_tempProjectPath!);
        var results = await _generatorService.GenerateAllAsync(projectInfo);

        if (results.Count() < 3)
        {
            throw new InvalidOperationException("Expected generation results for at least 3 entities");
        }
    }

    /// <summary>
    /// Benchmark: Batch generation with parallel processing
    /// Measures the throughput when generating code for multiple entities in parallel
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Batch")]
    public async Task BatchGeneration_ParallelProcessing()
    {
        var projectInfo = await _generatorService!.AnalyzeProjectAsync(_tempProjectPath!);
        var entities = projectInfo.Entities.ToList();

        var tasks = new List<Task<IEnumerable<GenerationResult>>>();
        foreach (var entity in entities)
        {
            tasks.Add(_generatorService.GenerateForEntityAsync(entity, projectInfo));
        }

        var results = await Task.WhenAll(tasks);

        if (results.Sum(r => r.Count()) < entities.Count)
        {
            throw new InvalidOperationException("Batch generation produced insufficient results");
        }
    }

    /// <summary>
    /// Benchmark: Memory allocation for entity analysis
    /// Measures memory allocations during entity parsing
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Memory")]
    public async Task Memory_EntityAnalysis()
    {
        var filePath = Path.Combine(_tempProjectPath!, "ComplexEntity.cs");
        var content = await File.ReadAllTextAsync(filePath);

        var entities = await _entityAnalyzer!.AnalyzeFileAsync(filePath, content);

        GC.KeepAlive(entities);
    }

    /// <summary>
    /// Benchmark: Memory allocation for repository generation
    /// Measures memory allocations during repository code generation
    /// </summary>
    [Benchmark]
    [BenchmarkCategory("Memory")]
    public async Task Memory_RepositoryGeneration()
    {
        var entity = new Entity
        {
            Name = "MemoryTestEntity",
            Namespace = "MemoryTestNamespace",
            Attributes = { "Repository" }
        };
        entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsRequired = true });
        entity.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = false });
        entity.AddProperty(new EntityProperty { Name = "Description", Type = "string", IsRequired = false });

        var result = await _repositoryGenerator!.GenerateRepositoryAsync(entity);

        GC.KeepAlive(result);
    }
}

/// <summary>
/// Entry point for running benchmarks from command line
/// </summary>
public static class BenchmarkRunner
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Benchmarks).Assembly).Run(args);
    }
}