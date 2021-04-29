#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetSourceGeneratorToolkit.Integration;

/// <summary>
/// Provides validation helpers for <see cref="HttpClientService"/> instances.
/// </summary>
public static class HttpClientServiceValidation
{
    /// <summary>
    /// Validates the specified <see cref="HttpClientService"/> instance.
    /// </summary>
    /// <param name="value">The <see cref="HttpClientService"/> instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this HttpClientService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // The HttpClientService is validated by its constructor which already ensures
        // HttpClient and ILogger are not null. Once constructed, the service is valid.
        // Additional runtime validation would require public properties which don't exist.

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="HttpClientService"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="HttpClientService"/> instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this HttpClientService? value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="HttpClientService"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="HttpClientService"/> instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance has validation problems.</exception>
    public static void EnsureValid(this HttpClientService? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"HttpClientService is invalid:{Environment.NewLine}- {
                    string.Join(Environment.NewLine + "- ", problems)
                }");
        }
    }
}