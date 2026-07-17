#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Examples;

namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// Provides extension methods for <see cref="BasicExample.User"/> entities.
/// </summary>
public static class BasicExampleExtensions
{
    /// <summary>
    /// Checks if a <see cref="BasicExample.User"/> is active and has a valid email.
    /// </summary>
    /// <param name="user">The <see cref="BasicExample.User"/> to check.</param>
    /// <returns><c>true</c> if the user is active and has a valid email; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is <c>null</c>.</exception>
    public static bool IsValidActiveUser(this BasicExample.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.IsActive && !string.IsNullOrEmpty(user.Email);
    }

    /// <summary>
    /// Gets a formatted string representation of a <see cref="BasicExample.User"/>.
    /// </summary>
    /// <param name="user">The <see cref="BasicExample.User"/> to format.</param>
    /// <returns>A formatted string representation of the user.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="user"/> is <c>null</c>.</exception>
    public static string GetFormattedString(this BasicExample.User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return $"ID: {user.Id}, Name: {user.FirstName} {user.LastName}, Email: {user.Email}, Created At: {user.CreatedAt:O}, Is Active: {user.IsActive}";
    }

    /// <summary>
    /// Checks if a <see cref="BasicExample.UserDto"/> has valid data.
    /// </summary>
    /// <param name="dto">The <see cref="BasicExample.UserDto"/> to check.</param>
    /// <returns><c>true</c> if the DTO has valid data; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dto"/> is <c>null</c>.</exception>
    public static bool HasValidData(this BasicExample.UserDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return !string.IsNullOrEmpty(dto.Email) && !string.IsNullOrEmpty(dto.FirstName);
    }
}
