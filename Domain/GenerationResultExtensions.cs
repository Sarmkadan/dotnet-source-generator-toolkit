namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Extension methods for <see cref="GenerationResult"/>.
/// </summary>
public static class GenerationResultExtensions
{
    /// <summary>
    /// Gets a human-readable summary of the generation result, including warnings and errors.
    /// </summary>
    /// <param name="result">The generation result. Cannot be <see langword="null"/>.</param>
    /// <returns>A human-readable summary of the generation result.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
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
    /// <param name="result">The generation result. Cannot be <see langword="null"/>.</param>
    /// <returns><c>true</c> if the generation result has any warnings or errors; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static bool HasIssues(this GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Warnings.Count > 0 || result.Errors.Count > 0;
    }

    /// <summary>
    /// Gets the duration of the generation process in a human-readable format.
    /// </summary>
    /// <param name="result">The generation result. Cannot be <see langword="null"/>.</param>
    /// <returns>The duration of the generation process in a human-readable format using invariant culture formatting.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="result"/> is <see langword="null"/>.</exception>
    public static string GetDurationString(this GenerationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var duration = result.GenerationDurationMs;
        return duration < 1000
            ? $"{duration} ms"
            : $"{duration / 1000.0:F2} s";
    }
}
