namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// Provides extension methods for <see cref="AdvancedExample"/>.
/// </summary>
public static class AdvancedExampleExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="AdvancedExample"/> is published and has a non-empty title.
    /// </summary>
    /// <param name="example">The <see cref="AdvancedExample"/> instance.</param>
    /// <returns><c>true</c> if the example is published and has a non-empty title; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="example"/> is <c>null</c>.</exception>
    public static bool IsValidPublishedExample(this AdvancedExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        return example.IsPublished && !string.IsNullOrEmpty(example.Title);
    }

    /// <summary>
    /// Retrieves a read-only list of tags for the specified <see cref="AdvancedExample"/> sorted alphabetically.
    /// </summary>
    /// <param name="example">The <see cref="AdvancedExample"/> instance.</param>
    /// <returns>A read-only list of tags sorted alphabetically using invariant culture comparison.</returns>
    /// <remarks>
    /// Returns an empty read-only list if <paramref name="example"/>.Tags is <c>null</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="example"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetSortedTags(this AdvancedExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        return example.Tags is null
            ? Array.Empty<string>()
            : example.Tags
                .Where(t => !string.IsNullOrEmpty(t))
                .OrderBy(t => t, StringComparer.InvariantCulture)
                .ToList()
                .AsReadOnly();
    }
}
