// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Configuration options for the toolkit loaded from config file or environment.
/// Provides defaults and allows fine-tuning of generator behavior.
/// </summary>
public class ToolkitOptions
{
    /// <summary>
    /// Enable caching of analysis results across runs.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache expiration time in minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Enable automatic formatting of generated code.
    /// </summary>
    public bool EnableCodeFormatting { get; set; } = true;

    /// <summary>
    /// Line length for code formatting (default: 100 chars).
    /// </summary>
    public int CodeFormattingLineLength { get; set; } = 100;

    /// <summary>
    /// Enable verbose output for all operations.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Degree of parallelism for generation tasks.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Timeout for long-running operations in seconds.
    /// </summary>
    public int OperationTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Enable generation of DTOs for entities.
    /// </summary>
    public bool GenerateDtos { get; set; } = false;

    /// <summary>
    /// Default namespace for generated code.
    /// </summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>
    /// Output directory for generated files.
    /// </summary>
    public string OutputDirectory { get; set; } = "./Generated";

    /// <summary>
    /// Backup existing files before overwriting.
    /// </summary>
    public bool BackupExistingFiles { get; set; } = true;

    /// <summary>
    /// Enable generation of interface abstractions.
    /// </summary>
    public bool GenerateInterfaces { get; set; } = true;

    /// <summary>
    /// Include XML documentation comments in generated code.
    /// </summary>
    public bool GenerateXmlComments { get; set; } = true;
}
