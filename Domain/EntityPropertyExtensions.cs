using System;

namespace DotNetSourceGeneratorToolkit.Domain
{
    /// <summary>
    /// Provides extension methods for the <see cref="EntityProperty"/> class.
    /// </summary>
    public static class EntityPropertyExtensions
    {
        /// <summary>
        /// Determines whether the entity property has any validation constraints.
        /// </summary>
        /// <param name="property">The entity property.</param>
        /// <returns><c>true</c> if the property has validation constraints; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
        public static bool HasValidationConstraints(this EntityProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            return property.IsRequired
                || !string.IsNullOrEmpty(property.RegexPattern)
                || property.MinValue is not null
                || property.MaxValue is not null
                || property.MaxLength is not null
                || property.MinLength is not null;
        }

        /// <summary>
        /// Gets a display name for the entity property, combining the <see cref="EntityProperty.Description"/>
        /// and <see cref="EntityProperty.Name"/> if available.
        /// </summary>
        /// <param name="property">The entity property.</param>
        /// <returns>A formatted display name.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
        public static string GetDisplayName(this EntityProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            return string.IsNullOrEmpty(property.Description)
                ? property.Name
                : $"{property.Description} ({property.Name})";
        }

        /// <summary>
        /// Determines whether the entity property is of type string and has length constraints.
        /// </summary>
        /// <param name="property">The entity property.</param>
        /// <returns><c>true</c> if the property is a string with length constraints; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property"/> is <c>null</c>.</exception>
        public static bool IsStringWithLengthConstraints(this EntityProperty property)
        {
            ArgumentNullException.ThrowIfNull(property);
            return property.Type == "string"
                && (property.MaxLength.HasValue || property.MinLength.HasValue);
        }
    }
}
