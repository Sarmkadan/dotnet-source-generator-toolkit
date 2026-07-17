namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Provides extension methods for <see cref="ValidationResult"/>.
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// Determines whether the validation result has any errors or warnings.
    /// </summary>
    /// <param name="validationResult">The validation result.</param>
    /// <returns><c>true</c> if the validation result has any errors or warnings; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationResult"/> is <c>null</c>.</exception>
    public static bool HasMessages(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.HasIssues;
    }

    /// <summary>
    /// Gets a read-only list of all messages (errors and warnings) in the validation result.
    /// </summary>
    /// <param name="validationResult">The validation result.</param>
    /// <returns>A read-only list of all messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationResult"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> GetAllMessagesList(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.Errors.Concat(validationResult.Warnings).ToList().AsReadOnly();
    }

    /// <summary>
    /// Determines whether the validation result is valid and has no warnings.
    /// </summary>
    /// <param name="validationResult">The validation result.</param>
    /// <returns><c>true</c> if the validation result is valid and has no warnings; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="validationResult"/> is <c>null</c>.</exception>
    public static bool IsValidAndClean(this ValidationResult validationResult)
        => validationResult is null
            ? throw new ArgumentNullException(nameof(validationResult))
            : validationResult.IsValid && !validationResult.HasMessages();
}
