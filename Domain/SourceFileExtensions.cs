namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Extension methods for <see cref="SourceFile"/>.
/// </summary>
public static class SourceFileExtensions
{
    /// <summary>
    /// Determines whether the source file has any using directives.
    /// </summary>
    /// <param name="sourceFile">The source file to check.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sourceFile"/> is <see langword="null"/>.</exception>
    /// <returns><see langword="true"/> if the source file has any using directives; otherwise, <see langword="false"/>.</returns>
    public static bool HasUsings(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.Usings.Count > 0;
    }

    /// <summary>
    /// Determines whether the source file declares any namespaces.
    /// </summary>
    /// <param name="sourceFile">The source file to check.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sourceFile"/> is <see langword="null"/>.</exception>
    /// <returns><see langword="true"/> if the source file declares any namespaces; otherwise, <see langword="false"/>.</returns>
    public static bool HasNamespaces(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.Namespaces.Count > 0;
    }

    /// <summary>
    /// Gets the total number of metadata elements (types, attributes, and usings) in the source file.
    /// </summary>
    /// <param name="sourceFile">The source file to analyze.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sourceFile"/> is <see langword="null"/>.</exception>
    /// <returns>The sum of type names, attribute names, and using directives in the file.</returns>
    public static int GetMetadataCount(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.TypeNames.Count + sourceFile.Attributes.Count + sourceFile.Usings.Count;
    }

    /// <summary>
    /// Determines whether the source file exceeds the large file threshold (> 1000 lines).
    /// </summary>
    /// <param name="sourceFile">The source file to evaluate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sourceFile"/> is <see langword="null"/>.</exception>
    /// <returns><see langword="true"/> if the file has more than 1000 lines; otherwise, <see langword="false"/>.</returns>
    public static bool IsLargeFile(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.LineCount > 1000;
    }
}
