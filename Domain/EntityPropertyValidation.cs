#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Provides validation helpers for <see cref="EntityProperty"/> instances.
/// Includes methods for validating property values against their metadata rules.
/// </summary>
public static class EntityPropertyValidation
{
    /// <summary>
    /// Validates the specified <see cref="EntityProperty"/> value against its metadata rules.
    /// </summary>
    /// <param name="value">The entity property to validate.</param>
    /// <returns>A list of human-readable validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EntityProperty value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate string properties using pattern matching
        if (value.IsRequired && string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Property name is required.");
        }

        if (string.IsNullOrWhiteSpace(value.Type))
        {
            errors.Add("Property type is required.");
        }

        // Validate string length constraints
        if (value.MinLength.HasValue && value.MaxLength.HasValue && value.MinLength > value.MaxLength)
        {
            errors.Add("MinLength cannot be greater than MaxLength.");
        }

        if (value.MaxLength.HasValue && value.MaxLength < 0)
        {
            errors.Add("MaxLength must be non-negative.");
        }

        if (value.MinLength.HasValue && value.MinLength < 0)
        {
            errors.Add("MinLength must be non-negative.");
        }

        // Validate numeric range constraints
        if (value.MinValue.HasValue && value.MaxValue.HasValue && value.MinValue > value.MaxValue)
        {
            errors.Add("MinValue cannot be greater than MaxValue.");
        }

        // Validate primary key constraints
        if (value.IsPrimaryKey)
        {
            if (value.IsNullable)
            {
                errors.Add("Primary key property cannot be nullable.");
            }

            if (string.IsNullOrWhiteSpace(value.Name))
            {
                errors.Add("Primary key property must have a name.");
            }
        }

        // Validate default value format for numeric types
        if (!string.IsNullOrEmpty(value.DefaultValue))
        {
            bool isNumericType = value.Type is not null &&
                (value.Type.Equals("decimal", StringComparison.OrdinalIgnoreCase) ||
                 value.Type.Equals("double", StringComparison.OrdinalIgnoreCase) ||
                 value.Type.Equals("float", StringComparison.OrdinalIgnoreCase) ||
                 value.Type.Equals("int", StringComparison.OrdinalIgnoreCase));

            if (isNumericType && !decimal.TryParse(value.DefaultValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                errors.Add("DefaultValue must be a valid number for numeric property types.");
            }
        }

        // Validate regex pattern with proper exception handling
        if (!string.IsNullOrEmpty(value.RegexPattern))
        {
            try
            {
                _ = System.Text.RegularExpressions.Regex.IsMatch("test", value.RegexPattern);
            }
            catch (System.Text.RegularExpressions.RegexParseException)
            {
                errors.Add("RegexPattern is not a valid regular expression.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EntityProperty"/> value is valid according to its metadata rules.
    /// </summary>
    /// <param name="value">The entity property to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this EntityProperty value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="EntityProperty"/> value is valid according to its metadata rules.
    /// </summary>
    /// <param name="value">The entity property to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this EntityProperty value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"EntityProperty validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }
}