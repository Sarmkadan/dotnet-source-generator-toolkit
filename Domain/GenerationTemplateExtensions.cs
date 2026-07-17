using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Domain
{
    /// <summary>
    /// Provides extension methods for <see cref="GenerationTemplate"/> to enhance functionality and readability.
    /// </summary>
    public static class GenerationTemplateExtensions
    {
        /// <summary>
        /// Determines whether the template is valid based on its validation rules.
        /// </summary>
        /// <param name="template">The template to validate.</param>
        /// <returns>True if the template is valid; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        public static bool IsValid(this GenerationTemplate template)
        {
            ArgumentNullException.ThrowIfNull(template);
            return !template.Validate().Any();
        }

        /// <summary>
        /// Gets a display-friendly name for the template that includes its name and generator type.
        /// </summary>
        /// <param name="template">The template to format.</param>
        /// <returns>A display string in the format "Name (GeneratorType)".</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        public static string GetDisplayName(this GenerationTemplate template)
        {
            ArgumentNullException.ThrowIfNull(template);
            return $"{template.Name} ({template.GeneratorType})";
        }

        /// <summary>
        /// Determines whether the template supports a specific programming language.
        /// </summary>
        /// <param name="template">The template to check.</param>
        /// <param name="language">The language to verify support for.</param>
        /// <returns>True if the template supports the specified language; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> or <paramref name="language"/> is null.</exception>
        public static bool IsSupportedLanguage(this GenerationTemplate template, string language)
        {
            ArgumentNullException.ThrowIfNull(template);
            ArgumentNullException.ThrowIfNull(language);
            return template.SupportedLanguages.Contains(language, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the configuration option keys defined in the template.
        /// </summary>
        /// <param name="template">The template to extract keys from.</param>
        /// <returns>An enumerable of configuration option keys.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="template"/> is null.</exception>
        public static IEnumerable<string> GetConfigurationOptionKeys(this GenerationTemplate template)
        {
            ArgumentNullException.ThrowIfNull(template);
            return template.ConfigurationOptions.Keys;
        }
    }
}