namespace YourNamespace; // replace with actual namespace

/// <summary>
/// Provides extension methods for <see cref="BasicExample"/>.
/// </summary>
public static class BasicExampleExtensions
{
    /// <summary>
    /// Checks if a <see cref="BasicExample"/> is active and has a valid email.
    /// </summary>
    /// <param name="example">The <see cref="BasicExample"/> to check.</param>
    /// <returns><c>true</c> if the example is active and has a valid email; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="example"/> is <c>null</c>.</exception>
    public static bool IsValidActiveExample(this BasicExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        return example.IsActive && !string.IsNullOrEmpty(example.Email);
    }

    /// <summary>
    /// Gets a formatted string representation of a <see cref="BasicExample"/>.
    /// </summary>
    /// <param name="example">The <see cref="BasicExample"/> to format.</param>
    /// <returns>A formatted string representation of the example.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="example"/> is <c>null</c>.</exception>
    public static string GetFormattedString(this BasicExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        return $"ID: {example.Id}, Name: {example.FirstName} {example.LastName}, Email: {example.Email}, Created At: {example.CreatedAt:O}, Is Active: {example.IsActive}";
    }
}
