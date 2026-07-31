#nullable enable

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using Microsoft.Extensions.Logging;
using System.IO;
using DotNetSourceGeneratorToolkit.Infrastructure;

namespace DotNetSourceGeneratorToolkit.Benchmarks;

/// <summary>
/// Performance benchmarks for the FileSystemService class.
/// Measures throughput and memory allocation for file system operations.
/// </summary>
[MemoryDiagnoser]
public class FileSystemServiceBenchmarks
{
    private string _tempDir = null!;
    private IFileSystemService _fileSystemService = null!;
    private ILogger<FileSystemService> _logger = null!;

    // Parameters for benchmark variations
    [Params(100, 1000, 10000)] // File size in bytes
    public int FileSize;

    [Params(10, 100, 1000)] // Number of files
    public int FileCount;

    [GlobalSetup]
    public void Setup()
    {
        // Create a temporary directory and set it as current to control base path
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        Environment.CurrentDirectory = _tempDir;

        // Set up minimal logging for benchmarks
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddFilter("Microsoft", LogLevel.Warning);
            builder.AddFilter("System", LogLevel.Warning);
            builder.AddFilter("DotNetSourceGeneratorToolkit", LogLevel.Warning);
            builder.AddConsole();
        });
        var serviceProvider = serviceCollection.BuildServiceProvider();
        _logger = serviceProvider.GetRequiredService<ILogger<FileSystemService>>();

        _fileSystemService = new FileSystemService(_logger);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        // Reset current directory to avoid affecting other benchmarks
        Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        // Clean up any files from previous iteration
        if (Directory.Exists(_tempDir))
        {
            foreach (var file in Directory.GetFiles(_tempDir))
            {
                File.Delete(file);
            }
            foreach (var dir in Directory.GetDirectories(_tempDir))
            {
                Directory.Delete(dir, true);
            }
        }
    }

    [Benchmark]
    public async Task WriteFileAsync_Benchmark()
    {
        var filePath = Path.Combine(_tempDir, $"test_{FileSize}.txt");
        var content = new string('a', FileSize);
        await _fileSystemService.WriteFileAsync(filePath, content);
    }

    [Benchmark]
    public async Task ReadFileAsync_Benchmark()
    {
        var filePath = Path.Combine(_tempDir, $"test_{FileSize}.txt");
        var content = new string('a', FileSize);
        await File.WriteAllTextAsync(filePath, content); // Setup file
        await _fileSystemService.ReadFileAsync(filePath);
    }

    [Benchmark]
    public async Task AppendFileAsync_Benchmark()
    {
        var filePath = Path.Combine(_tempDir, $"test_{FileSize}.txt");
        var content = new string('a', FileSize);
        await File.WriteAllTextAsync(filePath, string.Empty); // Initialize empty file
        await _fileSystemService.AppendFileAsync(filePath, content);
    }

    [Benchmark]
    public async Task CreateDirectoryAsync_Benchmark()
    {
        var dirPath = Path.Combine(_tempDir, $"dir_{new string('a', FileSize)}");
        await _fileSystemService.CreateDirectoryAsync(dirPath);
    }

    [Benchmark]
    public async Task GetFilesAsync_Benchmark()
    {
        // Create test files
        for (int i = 0; i < FileCount; i++)
        {
            var filePath = Path.Combine(_tempDir, $"file{i}.txt");
            await File.WriteAllTextAsync(filePath, "test content");
        }

        // Execute the method under test
        await _fileSystemService.GetFilesAsync(_tempDir, "*.txt");
    }
}