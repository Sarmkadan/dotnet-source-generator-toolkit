using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetSourceGeneratorToolkit.Events
{
    /// <summary>
    /// Extension methods for <see cref="GenerationCompletedEvent"/>.
    /// </summary>
    public static class GenerationCompletedEventExtensions
    {
        /// <summary>
        /// Returns a concise, human‑readable summary of the event.
        /// </summary>
        public static string ToSummary(this GenerationCompletedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            return $"[GenerationCompleted] Id={@event.EventId}, Request={@event.RequestId}, " +
                   $"Success={@event.IsSuccessful}, Files={@event.FilesGenerated}, " +
                   $"TimeMs={@event.ExecutionTimeMs}, Errors={@event.Errors?.Count ?? 0}";
        }

        /// <summary>
        /// Indicates whether the event contains any error messages.
        /// </summary>
        public static bool HasErrors(this GenerationCompletedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            return @event.Errors != null && @event.Errors.Any();
        }

        /// <summary>
        /// Returns a single string that concatenates all error messages,
        /// or <c>null</c> if there are no errors.
        /// </summary>
        public static string? GetErrorReport(this GenerationCompletedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            return @event.Errors != null && @event.Errors.Any()
                ? string.Join(Environment.NewLine, @event.Errors)
                : null;
        }

        /// <summary>
        /// Gets the execution duration as a <see cref="TimeSpan"/>.
        /// </summary>
        public static TimeSpan GetExecutionDuration(this GenerationCompletedEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            return TimeSpan.FromMilliseconds(@event.ExecutionTimeMs);
        }
    }
}
