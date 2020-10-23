// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.CLI;

/// <summary>
/// Represents command-line options parsed from user input.
/// Encapsulates all configuration needed to execute the toolkit.
/// </summary>
public class CliOptions
{
    /// <summary>
    /// Path to the project to analyze. Defaults to current working directory.
    /// </summary>
    public string ProjectPath { get; set; } = Directory.GetCurrentDirectory();

    /// <summary>
    /// Output directory for generated files. Defaults to project path.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Types of generators to run (Repository, Mapper, Validator, Serializer).
    /// If empty, all generators run.
    /// </summary>
    public List<string> GeneratorTypes { get; set; } = new();

    /// <summary>
    /// Output format for generation summary (Json, Csv, Xml, Text).
    /// </summary>
    public string OutputFormat { get; set; } = "Text";

    /// <summary>
    /// Enable verbose logging output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// Show help message and exit.
    /// </summary>
    public bool ShowHelp { get; set; }

    /// <summary>
    /// Show version information and exit.
    /// </summary>
    public bool ShowVersion { get; set; }

    /// <summary>
    /// Namespace override for generated code. Uses project namespace if not specified.
    /// </summary>
    public string? NamespaceOverride { get; set; }

    /// <summary>
    /// Recursively search subdirectories for entities.
    /// </summary>
    public bool Recursive { get; set; } = true;

    /// <summary>
    /// Generate DTOs alongside other artifacts.
    /// </summary>
    public bool GenerateDtos { get; set; }

    /// <summary>
    /// Configuration file path for advanced settings.
    /// </summary>
    public string? ConfigFile { get; set; }

    /// <summary>
    /// Enable dry-run mode - analyze without writing files.
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Validate configuration and entities without code generation.
    /// </summary>
    public bool ValidateOnly { get; set; }

    /// <summary>
    /// Parallel execution degree of freedom. Default is environment processor count.
    /// </summary>
    public int DegreeOfParallelism { get; set; } = Environment.ProcessorCount;
}
