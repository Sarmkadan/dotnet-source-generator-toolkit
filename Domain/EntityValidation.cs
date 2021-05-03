#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Provides validation helpers for <see cref="Entity"/> instances.
/// </summary>
public static class EntityValidation
{
    /// <summary>
    /// Validates an entity and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The entity to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if the entity is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this Entity value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate required string properties
        if (string.IsNullOrWhiteSpace(value.Id))
            errors.Add("Entity Id is required.");
        else if (!Guid.TryParse(value.Id, out _))
            errors.Add("Entity Id must be a valid GUID.");

        if (string.IsNullOrWhiteSpace(value.Name))
            errors.Add("Entity name is required.");
        else if (value.Name.Length > 100)
            errors.Add("Entity name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(value.Namespace))
            errors.Add("Entity namespace is required.");
        else if (value.Namespace.Length > 255)
            errors.Add("Entity namespace cannot exceed 255 characters.");

        // Validate optional string properties
        if (!string.IsNullOrWhiteSpace(value.Description) && value.Description.Length > 1000)
            errors.Add("Entity description cannot exceed 1000 characters.");

        if (!string.IsNullOrWhiteSpace(value.TableName) && value.TableName.Length > 128)
            errors.Add("Entity table name cannot exceed 128 characters.");

        if (!string.IsNullOrWhiteSpace(value.BaseClass) && value.BaseClass.Length > 255)
            errors.Add("Entity base class name cannot exceed 255 characters.");

        // Validate collections
        if (value.Properties is null)
            errors.Add("Entity Properties collection cannot be null.");
        else if (value.Properties.Count == 0)
            errors.Add(string.Format("Entity '{0}' must have at least one property.", value.Name));
        else if (value.Properties.Count > 1000)
            errors.Add("Entity cannot have more than 1000 properties.");

        if (value.Attributes is null)
            errors.Add("Entity Attributes collection cannot be null.");
        else if (value.Attributes.Count > 100)
            errors.Add("Entity cannot have more than 100 attributes.");

        if (value.Interfaces is null)
            errors.Add("Entity Interfaces collection cannot be null.");
        else if (value.Interfaces.Count > 50)
            errors.Add("Entity cannot implement more than 50 interfaces.");

        // Validate dates - CreatedAt should not be default
        if (value.CreatedAt == default)
            errors.Add("Entity CreatedAt date is required.");
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Entity CreatedAt date cannot be in the future.");

        // UpdatedAt should not be default
        if (value.UpdatedAt == default)
            errors.Add("Entity UpdatedAt date is required.");
        else if (value.UpdatedAt > DateTime.UtcNow.AddMinutes(5))
            errors.Add("Entity UpdatedAt date cannot be in the future.");

        // Validate AccessModifier
        if (value.AccessModifier < AccessModifier.Public || value.AccessModifier > AccessModifier.ProtectedInternal)
            errors.Add("Entity AccessModifier has an invalid value.");

        // Validate property names for duplicates
        var duplicatePropertyNames = value.Properties
            .GroupBy(p => p.Name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var duplicateName in duplicatePropertyNames)
            errors.Add(string.Format("Duplicate property name: '{0}' in entity '{1}'.", duplicateName, value.Name));

        // Validate each property
        var propertyErrors = new List<string>();
        foreach (var property in value.Properties)
        {
            propertyErrors.AddRange(property.Validate());
        }

        if (propertyErrors.Count > 0)
            errors.AddRange(propertyErrors);

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified entity is valid.
    /// </summary>
    /// <param name="value">The entity to check.</param>
    /// <returns><see langword="true"/> if the entity is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this Entity value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified entity is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The entity to check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the entity is invalid. The exception message contains all validation errors.</exception>
    public static void EnsureValid(this Entity value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
            return;

        var errorMessage = string.Join(Environment.NewLine + "- ", errors);
        throw new ArgumentException(
            $"Entity validation failed:{Environment.NewLine}- {errorMessage}",
            nameof(value));
    }
}