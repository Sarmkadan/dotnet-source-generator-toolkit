using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Domain
{
    /// <summary>
    /// Provides extension methods for the <see cref="Entity"/> type.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Gets the fully qualified name of the entity (namespace + name).
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The qualified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="entity"/>.<see cref="Entity.Namespace"/> or <see cref="Entity.Name"/> is <c>null</c> or empty.
        /// </exception>
        public static string GetQualifiedName(this Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentException.ThrowIfNullOrEmpty(entity.Namespace);
            ArgumentException.ThrowIfNullOrEmpty(entity.Name);
            return $"{entity.Namespace}.{entity.Name}";
        }

        /// <summary>
        /// Determines whether the entity has the specified attribute.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="attribute">The attribute name to check.</param>
        /// <returns><c>true</c> if the attribute is present; otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> or <paramref name="attribute"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="attribute"/> is empty.</exception>
        public static bool HasAttribute(this Entity entity, string attribute)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentException.ThrowIfNullOrEmpty(attribute);
            return entity.Attributes.Contains(attribute);
        }

        /// <summary>
        /// Gets all validation messages for the entity as a read‑only list.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A read‑only list of validation messages.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
        public static IReadOnlyList<string> GetValidationMessages(this Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            return entity.Validate().ToList().AsReadOnly();
        }

        /// <summary>
        /// Creates a concise summary string for the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>A summary string describing the entity's access modifier, type, primary key, and property count.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is <c>null</c>.</exception>
        public static string GetSummary(this Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            var access = entity.AccessModifier.ToString().ToLowerInvariant();
            var modifiers = entity.IsAbstract ? "abstract " : entity.IsSealed ? "sealed " : string.Empty;
            var primaryKey = entity.GetPrimaryKeyProperty()?.GetClrTypeName() ?? "none";
            var propertyCount = entity.Properties.Count;

            return $"{access} {modifiers}{entity.Name} (PK: {primaryKey}, Props: {propertyCount})" ;
        }
    }
}