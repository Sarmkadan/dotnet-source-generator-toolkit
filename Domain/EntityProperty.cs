// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents a property of an entity with metadata for code generation.
/// Includes type information, validation rules, and database mapping.
/// </summary>
public class EntityProperty
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = "string";

    public string? ColumnName { get; set; }

    public int? MaxLength { get; set; }

    public int? MinLength { get; set; }

    public string? RegexPattern { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsNullable { get; set; }

    public bool IsPrimaryKey { get; set; }

    public bool IsAutoIncrement { get; set; }

    public bool IsNavigationProperty { get; set; }

    public bool IsCollection { get; set; }

    public string? DefaultValue { get; set; }

    public string? Description { get; set; }

    public List<string> Attributes { get; } = [];

    public AccessModifier GetterAccess { get; set; } = AccessModifier.Public;

    public AccessModifier SetterAccess { get; set; } = AccessModifier.Public;

    public bool HasPrivateSetter { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the CLR type name formatted for C# code generation.
    /// </summary>
    public string GetClrTypeName()
    {
        var baseName = Type switch
        {
            "int" or "integer" => "int",
            "long" => "long",
            "decimal" => "decimal",
            "double" => "double",
            "float" => "float",
            "bool" or "boolean" => "bool",
            "datetime" or "DateTime" => "DateTime",
            "guid" or "Guid" => "Guid",
            _ => Type
        };

        if (IsCollection)
            return $"List<{baseName}>";

        return IsNullable ? $"{baseName}?" : baseName;
    }

    /// <summary>
    /// Determines if this property requires validation generation.
    /// </summary>
    public bool RequiresValidation()
    {
        return IsRequired || MaxLength.HasValue || MinLength.HasValue || !string.IsNullOrEmpty(RegexPattern);
    }

    /// <summary>
    /// Generates validation attributes for the property.
    /// </summary>
    public IEnumerable<string> GenerateValidationAttributes()
    {
        var attributes = new List<string>();

        if (IsRequired)
            attributes.Add("[Required]");

        if (MaxLength.HasValue)
            attributes.Add($"[MaxLength({MaxLength})]");

        if (MinLength.HasValue)
            attributes.Add($"[MinLength({MinLength})]");

        if (!string.IsNullOrEmpty(RegexPattern))
            attributes.Add($"[RegularExpression(@\"{RegexPattern}\")]");

        return attributes;
    }

    /// <summary>
    /// Validates the property definition.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Property name is required.");

        if (string.IsNullOrWhiteSpace(Type))
            errors.Add($"Property '{Name}' type is required.");

        if (MinLength.HasValue && MaxLength.HasValue && MinLength > MaxLength)
            errors.Add($"Property '{Name}' MinLength cannot be greater than MaxLength.");

        if (IsPrimaryKey && IsNullable)
            errors.Add($"Primary key property '{Name}' cannot be nullable.");

        return errors;
    }
}
