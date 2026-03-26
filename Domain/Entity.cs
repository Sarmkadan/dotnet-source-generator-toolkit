// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents a domain entity that will be processed by the source generator toolkit.
/// Contains metadata about the entity and its properties for generation.
/// </summary>
public class Entity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public string Namespace { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? TableName { get; set; }

    public List<EntityProperty> Properties { get; } = [];

    public List<string> Attributes { get; } = [];

    public List<string> Interfaces { get; } = [];

    public string? BaseClass { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsAbstract { get; set; }

    public bool IsSealed { get; set; }

    public AccessModifier AccessModifier { get; set; } = AccessModifier.Public;

    /// <summary>
    /// Adds a new property to the entity with validation.
    /// </summary>
    public void AddProperty(EntityProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property));

        if (Properties.Any(p => p.Name == property.Name))
            throw new InvalidOperationException($"Property '{property.Name}' already exists on entity '{Name}'.");

        Properties.Add(property);
    }

    /// <summary>
    /// Removes a property from the entity by name.
    /// </summary>
    public bool RemoveProperty(string propertyName)
    {
        var property = Properties.FirstOrDefault(p => p.Name == propertyName);
        if (property != null)
        {
            Properties.Remove(property);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets primary key property if it exists.
    /// </summary>
    public EntityProperty? GetPrimaryKeyProperty()
    {
        return Properties.FirstOrDefault(p => p.IsPrimaryKey);
    }

    /// <summary>
    /// Gets all navigation properties for relationships.
    /// </summary>
    public IEnumerable<EntityProperty> GetNavigationProperties()
    {
        return Properties.Where(p => p.IsNavigationProperty);
    }

    /// <summary>
    /// Validates the entity structure for code generation.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Entity name is required.");

        if (string.IsNullOrWhiteSpace(Namespace))
            errors.Add("Entity namespace is required.");

        if (Properties.Count == 0)
            errors.Add($"Entity '{Name}' must have at least one property.");

        var duplicateProps = Properties.GroupBy(p => p.Name).Where(g => g.Count() > 1);
        foreach (var group in duplicateProps)
            errors.Add($"Duplicate property name: '{group.Key}'.");

        return errors;
    }
}

public enum AccessModifier
{
    Public,
    Private,
    Protected,
    Internal,
    ProtectedInternal,
}
