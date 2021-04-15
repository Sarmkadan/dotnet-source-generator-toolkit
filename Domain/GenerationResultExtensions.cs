namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Extension methods for <see cref="GenerationResult"/>.
/// </summary>
public static class GenerationResultExtensions
{
    /// <summary>
    /// Gets a human-readable summary of the generation result, including warnings and errors.
    /// </summary>
    /// <param name="result">The generation result.</param>
    /// <returns>A human-readable summary of the generation result.</returns>
    public static string GetDetailedSummary(this GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var summary = result.GetSummary();
        if (result.Warnings.Count > 0)
        {
            summary += Environment.NewLine + "Warnings:" + Environment.NewLine + string.Join(Environment.NewLine, result.Warnings);
        }
        if (result.Errors.Count > 0)
        {
            summary += Environment.NewLine + "Errors:" + Environment.NewLine + string.Join(Environment.NewLine, result.Errors);
        }
        return summary;
    }

    /// <summary>
    /// Determines whether the generation result has any warnings or errors.
    /// </summary>
    /// <param name="result">The generation result.</param>
    /// <returns><c>true</c> if the generation result has any warnings or errors; otherwise, <c>false</c>.</returns>
    public static bool HasIssues(this GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Warnings.Count > 0 || result.Errors.Count > 0;
    }

    /// <summary>
    /// Gets the duration of the generation process in a human-readable format.
    /// </summary>
    /// <param name="result">The generation result.</param>
    /// <returns>The duration of the generation process in a human-readable format.</returns>
    public static string GetDurationString(this GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var duration = result.GenerationDurationMs;
        if (duration < 1000)
        {
            return $"{duration} ms";
        }
        else
        {
            var seconds = duration / 1000.0;
            return $"{seconds:F2} s";
        }
    }
}
