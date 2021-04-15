namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Extension methods for <see cref="SourceFile"/>.
/// </summary>
public static class SourceFileExtensions
{
    /// <summary>
    /// Determines whether the source file has any usings.
    /// </summary>
    /// <param name="sourceFile">The source file.</param>
    /// <returns><c>true</c> if the source file has any usings; otherwise, <c>false</c>.</returns>
    public static bool HasUsings(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.Usings.Count > 0;
    }

    /// <summary>
    /// Determines whether the source file has any namespaces.
    /// </summary>
    /// <param name="sourceFile">The source file.</param>
    /// <returns><c>true</c> if the source file has any namespaces; otherwise, <c>false</c>.</returns>
    public static bool HasNamespaces(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.Namespaces.Count > 0;
    }

    /// <summary>
    /// Gets the total number of types, attributes, and usings in the source file.
    /// </summary>
    /// <param name="sourceFile">The source file.</param>
    /// <returns>The total number of types, attributes, and usings.</returns>
    public static int GetMetadataCount(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.TypeNames.Count + sourceFile.Attributes.Count + sourceFile.Usings.Count;
    }

    /// <summary>
    /// Determines whether the source file is a large file (more than 1000 lines).
    /// </summary>
    /// <param name="sourceFile">The source file.</param>
    /// <returns><c>true</c> if the source file is a large file; otherwise, <c>false</c>.</returns>
    public static bool IsLargeFile(this SourceFile sourceFile)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        return sourceFile.LineCount > 1000;
    }
}
