#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Utilities;

/// <summary>
/// Extension methods for DateTime operations and formatting.
/// Provides common patterns for date/time manipulation.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="DateTime"/> is in the past.
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <returns><see langword="true"/> if the specified date and time is in the past; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="dateTime"/> is <see cref="DateTimeKind.Local"/> or <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime.Kind != DateTimeKind.Utc
            ? throw new ArgumentException("DateTime must be in UTC kind for comparison", nameof(dateTime))
            : dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the specified <see cref="DateTime"/> is in the future.
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <returns><see langword="true"/> if the specified date and time is in the future; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="dateTime"/> is <see cref="DateTimeKind.Local"/> or <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime.Kind != DateTimeKind.Utc
            ? throw new ArgumentException("DateTime must be in UTC kind for comparison", nameof(dateTime))
            : dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the elapsed time from the specified <see cref="DateTime"/> to the current UTC time.
    /// </summary>
    /// <param name="dateTime">The starting date and time.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time.</returns>
    /// <exception cref="ArgumentException"><paramref name="dateTime"/> is <see cref="DateTimeKind.Local"/> or <see cref="DateTimeKind.Unspecified"/>.</exception>
    public static TimeSpan ElapsedSince(this DateTime dateTime)
    {
        return dateTime.Kind != DateTimeKind.Utc
            ? throw new ArgumentException("DateTime must be in UTC kind for comparison", nameof(dateTime))
            : DateTime.UtcNow - dateTime;
    }

    /// <summary>
    /// Formats the specified <see cref="DateTime"/> as an ISO 8601 string.
    /// </summary>
    /// <param name="dateTime">The date and time to format.</param>
    /// <returns>An ISO 8601 formatted string.</returns>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString("o");
    }

    /// <summary>
    /// Formats the specified <see cref="DateTime"/> in human-readable relative format.
    /// </summary>
    /// <param name="dateTime">The date and time to format.</param>
    /// <returns>A human-readable relative time string.</returns>
    public static string ToRelativeFormat(this DateTime dateTime)
    {
        var elapsed = DateTime.UtcNow - dateTime;

        if (elapsed.TotalSeconds < 60)
            return "just now";

        if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes} minute{(elapsed.TotalMinutes > 1 ? "s" : "")} ago";

        if (elapsed.TotalHours < 24)
            return $"{(int)elapsed.TotalHours} hour{(elapsed.TotalHours > 1 ? "s" : "")} ago";

        if (elapsed.TotalDays < 7)
            return $"{(int)elapsed.TotalDays} day{(elapsed.TotalDays > 1 ? "s" : "")} ago";

        return dateTime.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// Gets the start of the day (midnight) for the specified <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time.</param>
    /// <returns>A <see cref="DateTime"/> representing midnight of the same day.</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999) for the specified <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time.</param>
    /// <returns>A <see cref="DateTime"/> representing the end of the day.</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the month (first day at midnight) for the specified <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the month.</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month (last day at 23:59:59.999) for the specified <see cref="DateTime"/>.
    /// </summary>
    /// <param name="dateTime">The date and time.</param>
    /// <returns>A <see cref="DateTime"/> representing the last day of the month.</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// Determines whether the specified <see cref="DateTime"/> is between two dates (inclusive).
    /// </summary>
    /// <param name="dateTime">The date and time to check.</param>
    /// <param name="startDate">The start date of the range.</param>
    /// <param name="endDate">The end date of the range.</param>
    /// <returns><see langword="true"/> if the date is within the range; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="startDate"/> or <paramref name="endDate"/> is not in UTC kind.</exception>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        return dateTime.Kind != DateTimeKind.Utc || startDate.Kind != DateTimeKind.Utc || endDate.Kind != DateTimeKind.Utc
            ? throw new ArgumentException("All DateTime values must be in UTC kind for comparison")
            : dateTime >= startDate && dateTime <= endDate;
    }
}