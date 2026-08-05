#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Configuration;

/// <summary>
/// Configuration options for the toolkit loaded from config file or environment.
/// Provides defaults and allows fine-tuning of generator behavior.
/// </summary>
public sealed class ToolkitOptions
{
    /// <summary>
    /// When <c>true</c>, analysis results are cached and reused across runs instead of
    /// being recomputed, reducing generation time for unchanged inputs. Default: <c>true</c>.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Number of minutes a cached analysis result remains valid before it is treated as
    /// stale and recomputed. Only relevant when <see cref="EnableCaching"/> is <c>true</c>.
    /// Default: <c>60</c> minutes.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// When <c>true</c>, generated source files are run through the code formatter before
    /// being written to disk, normalizing indentation and layout. Default: <c>true</c>.
    /// </summary>
    public bool EnableCodeFormatting { get; set; } = true;

    /// <summary>
    /// Maximum line length, in characters, enforced by the formatter when
    /// <see cref="EnableCodeFormatting"/> is <c>true</c>; longer lines are wrapped.
    /// Default: <c>100</c> characters.
    /// </summary>
    public int CodeFormattingLineLength { get; set; } = 100;

    /// <summary>
    /// When <c>true</c>, additional diagnostic and progress messages are emitted for every
    /// operation, useful for troubleshooting. Default: <c>false</c>.
    /// </summary>
    public bool VerboseLogging { get; set; } = false;

    /// <summary>
    /// Maximum number of generation tasks that may run concurrently. Higher values increase
    /// throughput on multi-core machines at the cost of memory usage. Default: the number of
    /// logical processors on the host (<see cref="Environment.ProcessorCount"/>).
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Maximum number of seconds a single long-running operation (e.g. analysis or
    /// generation pass) is allowed to run before it is aborted as timed out.
    /// Default: <c>300</c> seconds (5 minutes).
    /// </summary>
    public int OperationTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// When <c>true</c>, Data Transfer Object classes are generated alongside entities for
    /// use at API/service boundaries. Default: <c>false</c>.
    /// </summary>
    public bool GenerateDtos { get; set; } = false;

    /// <summary>
    /// Namespace applied to generated code when the source type does not otherwise dictate
    /// one. When <c>null</c>, the generator falls back to inferring a namespace from the
    /// target project or source file. Default: <c>null</c>.
    /// </summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>
    /// Relative or absolute path of the directory generated files are written to; the
    /// directory is created automatically if it does not already exist.
    /// Default: <c>"./Generated"</c>.
    /// </summary>
    public string OutputDirectory { get; set; } = "./Generated";

    /// <summary>
    /// When <c>true</c>, an existing file is copied to a backup location before it is
    /// overwritten by newly generated output, guarding against accidental data loss.
    /// Default: <c>true</c>.
    /// </summary>
    public bool BackupExistingFiles { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, an interface abstraction is generated alongside each concrete type
    /// to support mocking and dependency injection. Default: <c>true</c>.
    /// </summary>
    public bool GenerateInterfaces { get; set; } = true;

    /// <summary>
    /// When <c>true</c>, generated members are annotated with XML documentation comments
    /// summarizing their purpose. Default: <c>true</c>.
    /// </summary>
    public bool GenerateXmlComments { get; set; } = true;
}
