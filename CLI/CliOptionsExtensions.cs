namespace DotNetSourceGeneratorToolkit.CLI;

public static class CliOptionsExtensions
{
    /// <summary>
    /// Determines whether the CLI options require a recursive search.
    /// </summary>
    /// <param name="options">The CLI options to check.</param>
    /// <returns>True if the options require a recursive search; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static bool RequiresRecursiveSearch(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Recursive;
    }

    /// <summary>
    /// Gets the generator types as a read-only list.
    /// </summary>
    /// <param name="options">The CLI options to get the generator types from.</param>
    /// <returns>A read-only list of generator types.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static IReadOnlyList<string> GetGeneratorTypes(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.GeneratorTypes.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the CLI options are set to validate only.
    /// </summary>
    /// <param name="options">The CLI options to check.</param>
    /// <returns>True if the options are set to validate only; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static bool IsValidateOnly(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ValidateOnly;
    }

    /// <summary>
    /// Determines whether the CLI options are set to perform a dry run.
    /// </summary>
    /// <param name="options">The CLI options to check.</param>
    /// <returns>True if the options are set to perform a dry run; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
    public static bool IsDryRun(this CliOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.DryRun;
    }
}
