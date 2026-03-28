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
    /// Check if a DateTime is in the past.
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Check if a DateTime is in the future.
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Get elapsed time from a DateTime to now.
    /// </summary>
    public static TimeSpan ElapsedSince(this DateTime dateTime)
    {
        return DateTime.UtcNow - dateTime;
    }

    /// <summary>
    /// Format DateTime as ISO 8601 string.
    /// </summary>
    public static string ToIso8601(this DateTime dateTime)
    {
        return dateTime.ToString("o");
    }

    /// <summary>
    /// Format DateTime in human-readable relative format.
    /// </summary>
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
    /// Get start of day (midnight).
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Get end of day (23:59:59.999).
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Get start of month.
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Get end of month.
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddDays(-1);
    }

    /// <summary>
    /// Check if DateTime is between two dates (inclusive).
    /// </summary>
    public static bool IsBetween(this DateTime dateTime, DateTime startDate, DateTime endDate)
    {
        return dateTime >= startDate && dateTime <= endDate;
    }
}
