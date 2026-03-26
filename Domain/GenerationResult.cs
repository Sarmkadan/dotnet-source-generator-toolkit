// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents the result of a source code generation operation, including
/// generated content, status, and any errors or warnings that occurred.
/// </summary>
public class GenerationResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string EntityName { get; set; } = string.Empty;

    public GeneratorType GeneratorType { get; set; }

    public string GeneratedCode { get; set; } = string.Empty;

    public string OutputFilePath { get; set; } = string.Empty;

    public GenerationStatus Status { get; set; } = GenerationStatus.Pending;

    public List<string> Warnings { get; } = [];

    public List<string> Errors { get; } = [];

    public int CodeLineCount { get; set; }

    public long GenerationDurationMs { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime CompletedAt { get; set; }

    public string? CreatedBy { get; set; }

    public Dictionary<string, string> Metadata { get; } = [];

    /// <summary>
    /// Marks the generation as completed and sets the duration.
    /// </summary>
    public void MarkAsCompleted(GenerationStatus status, long durationMs)
    {
        Status = status;
        GenerationDurationMs = durationMs;
        CompletedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(GeneratedCode))
            CodeLineCount = GeneratedCode.Split(Environment.NewLine).Length;
    }

    /// <summary>
    /// Adds a warning message to the generation result.
    /// </summary>
    public void AddWarning(string warning)
    {
        if (!string.IsNullOrWhiteSpace(warning))
            Warnings.Add(warning);
    }

    /// <summary>
    /// Adds an error message to the generation result.
    /// </summary>
    public void AddError(string error)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            Errors.Add(error);
            Status = GenerationStatus.Failed;
        }
    }

    /// <summary>
    /// Checks if generation was successful without errors.
    /// </summary>
    public bool IsSuccessful() => Status == GenerationStatus.Completed && Errors.Count == 0;

    /// <summary>
    /// Validates that the generation result contains valid content.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(EntityName))
            errors.Add("Entity name is required.");

        if (Status == GenerationStatus.Completed && string.IsNullOrWhiteSpace(GeneratedCode))
            errors.Add("Generated code cannot be empty for completed generation.");

        if (Status == GenerationStatus.Completed && string.IsNullOrWhiteSpace(OutputFilePath))
            errors.Add("Output file path is required for completed generation.");

        if (Errors.Count > 0 && Status != GenerationStatus.Failed)
            errors.Add("Result has errors but status is not marked as Failed.");

        return errors;
    }

    /// <summary>
    /// Gets a summary of the generation result.
    /// </summary>
    public string GetSummary()
    {
        return $"{EntityName} ({GeneratorType}): {Status} - {CodeLineCount} lines, {Warnings.Count} warnings, {Errors.Count} errors";
    }
}

public enum GenerationStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Skipped,
}
