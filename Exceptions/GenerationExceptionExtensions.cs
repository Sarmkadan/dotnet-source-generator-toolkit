using System;

namespace DotNetSourceGeneratorToolkit.Exceptions
{
    /// <summary>
    /// Provides extension methods for the <see cref="GenerationException"/> class.
    /// </summary>
    public static class GenerationExceptionExtensions
    {
        /// <summary>
        /// Checks if the exception is related to the specified generator type.
        /// </summary>
        /// <param name="exception">The generation exception to check.</param>
        /// <param name="generatorType">The generator type to compare against.</param>
        /// <returns>True if the generator type matches (case-insensitive); otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="generatorType"/> is <see langword="null"/>.</exception>
        public static bool IsGeneratorType(this GenerationException exception, string generatorType)
        {
            ArgumentNullException.ThrowIfNull(exception);
            ArgumentNullException.ThrowIfNull(generatorType);

            return exception.GeneratorType?.Equals(generatorType, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Gets a formatted error message that includes the GeneratorType and EntityName details.
        /// </summary>
        /// <param name="exception">The generation exception.</param>
        /// <returns>A string containing the original message combined with the GeneratorType and EntityName.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
        public static string GetErrorMessageWithDetails(this GenerationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return $"{exception.Message} (GeneratorType: {exception.GeneratorType}, EntityName: {exception.EntityName})";
        }

        /// <summary>
        /// Checks if the exception has an associated entity name.
        /// </summary>
        /// <param name="exception">The generation exception to check.</param>
        /// <returns>True if the EntityName is not null, empty, or whitespace; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exception"/> is <see langword="null"/>.</exception>
        public static bool HasEntityName(this GenerationException exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return !string.IsNullOrWhiteSpace(exception.EntityName);
        }
    }
}