#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// Validation helpers for the <see cref="AdvancedExample.BlogPost"/> type
/// </summary>
public static class AdvancedExampleValidation
{
    /// <summary>
    /// Validates a BlogPost instance and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The BlogPost to validate</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this AdvancedExample.BlogPost? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add("Id must be a positive integer.");
        }

        // Validate Title
        if (string.IsNullOrWhiteSpace(value.Title))
        {
            errors.Add("Title cannot be null or whitespace.");
        }
        else if (value.Title.Length > 200)
        {
            errors.Add("Title cannot exceed 200 characters.");
        }

        // Validate Content
        if (string.IsNullOrWhiteSpace(value.Content))
        {
            errors.Add("Content cannot be null or whitespace.");
        }
        else if (value.Content.Length > 10000)
        {
            errors.Add("Content cannot exceed 10000 characters.");
        }

        // Validate AuthorId
        if (value.AuthorId <= 0)
        {
            errors.Add("AuthorId must be a positive integer.");
        }

        // Validate PublishedAt
        if (value.PublishedAt == default)
        {
            errors.Add("PublishedAt must be set to a valid date.");
        }
        else if (value.PublishedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("PublishedAt cannot be in the future.");
        }

        // Validate Tags
        ArgumentNullException.ThrowIfNull(value.Tags);

        if (value.Tags.Count > 50)
        {
            errors.Add("Tags collection cannot exceed 50 items.");
        }

        foreach (var tag in value.Tags)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                errors.Add("Tags cannot contain null or whitespace values.");
                continue;
            }

            if (tag.Length > 50)
            {
                errors.Add("Individual tags cannot exceed 50 characters.");
                continue;
            }

            // Check for invalid characters in tag
            if (tag.IndexOfAny([';', ',', '|', '\n', '\r']) >= 0)
            {
                errors.Add("Tags cannot contain special characters (;, ,, |, newlines).");
                continue;
            }
        }

        // Validate ViewCount
        if (value.ViewCount < 0)
        {
            errors.Add("ViewCount cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a BlogPost instance is valid.
    /// </summary>
    /// <param name="value">The BlogPost to check</param>
    /// <returns>True if valid; otherwise false</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this AdvancedExample.BlogPost? value)
    {
        return value is null ? throw new ArgumentNullException(nameof(value)) : Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a BlogPost instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The BlogPost to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid, containing validation errors</exception>
    public static void EnsureValid(this AdvancedExample.BlogPost? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"BlogPost validation failed. {string.Join(" ", errors)}",
                nameof(value));
        }
    }
}