using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Events
{
    /// <summary>
    /// Provides extension methods for <see cref="GenerationCompletedEvent"/> to enhance functionality
    /// and improve developer experience when working with generation completion events.
    /// </summary>
    public static class GenerationCompletedEventExtensions
    {
        /// <summary>
        /// Returns a concise, human‑readable summary of the event.
        /// </summary>
        /// <param name="@event">The event to summarize.</param>
        /// <returns>A formatted summary string containing key event information.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="@event"/> is null.</exception>
        public static string ToSummary(this GenerationCompletedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return $"[GenerationCompleted] Id={@event.EventId}, Request={@event.RequestId}, " +
                   $"Success={@event.IsSuccessful}, Files={@event.FilesGenerated}, " +
                   $"TimeMs={@event.ExecutionTimeMs}, Errors={@event.Errors?.Count ?? 0}";
        }

        /// <summary>
        /// Indicates whether the event contains any error messages.
        /// </summary>
        /// <param name="@event">The event to check for errors.</param>
        /// <returns>True if the event contains errors; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="@event"/> is null.</exception>
        public static bool HasErrors(this GenerationCompletedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return @event.Errors?.Count > 0;
        }

        /// <summary>
        /// Returns a single string that concatenates all error messages,
        /// or <c>null</c> if there are no errors.
        /// </summary>
        /// <param name="@event">The event containing error messages to report.</param>
        /// <returns>A concatenated error report string, or null if no errors exist.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="@event"/> is null.</exception>
        public static string? GetErrorReport(this GenerationCompletedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return @event.Errors?.Count > 0
                ? string.Join(Environment.NewLine, @event.Errors)
                : null;
        }

        /// <summary>
        /// Gets the execution duration as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="@event">The event containing execution timing information.</param>
        /// <returns>The execution duration as a <see cref="TimeSpan"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="@event"/> is null.</exception>
        public static TimeSpan GetExecutionDuration(this GenerationCompletedEvent @event)
        {
            ArgumentNullException.ThrowIfNull(@event);

            return TimeSpan.FromMilliseconds(@event.ExecutionTimeMs);
        }
    }
}
