#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Events;

/// <summary>
/// Collects and aggregates metrics about generation events including counts, durations,
/// success/failure rates, and performance statistics.
/// </summary>
public sealed class GenerationMetricsCollector :
    IEventHandler<GenerationStartedEvent>,
    IEventHandler<GenerationCompletedEvent>
{
    private readonly ILogger<GenerationMetricsCollector> _logger;
    private readonly object _lock = new();

    private int _totalGenerations = 0;
    private int _successfulGenerations = 0;
    private int _failedGenerations = 0;
    private long _totalDurationMs = 0;
    private readonly List<long> _durations = new();
    private DateTime? _firstGenerationStart;
    private DateTime? _lastGenerationEnd;

    public GenerationMetricsCollector(ILogger<GenerationMetricsCollector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a snapshot of current metrics.
    /// </summary>
    /// <returns>Metrics snapshot with totals and statistics</returns>
    public MetricsSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            var now = DateTime.UtcNow;
            var totalDuration = _totalDurationMs;
            var durationCount = _durations.Count;
            var avgDuration = durationCount > 0 ? (double)totalDuration / durationCount : 0;

            return new MetricsSnapshot
            {
                TotalGenerations = _totalGenerations,
                SuccessfulGenerations = _successfulGenerations,
                FailedGenerations = _failedGenerations,
                TotalDurationMs = totalDuration,
                AverageDurationMs = avgDuration,
                FirstGenerationStart = _firstGenerationStart,
                LastGenerationEnd = _lastGenerationEnd ?? now,
                GenerationRatePerHour = _totalGenerations > 0 && _firstGenerationStart.HasValue
                    ? CalculateGenerationsPerHour(_firstGenerationStart.Value, now)
                    : 0
            };
        }
    }

    public Task HandleAsync(GenerationStartedEvent @event)
    {
        lock (_lock)
        {
            _totalGenerations++;
            _firstGenerationStart ??= @event.OccurredAt;
            _lastGenerationEnd = null; // Reset on new generation

            _logger.LogDebug(
                "[{RequestId}] METRICS: Generation started. Total: {Total}, Success: {Success}, Failed: {Failed}",
                @event.RequestId,
                _totalGenerations,
                _successfulGenerations,
                _failedGenerations
            );
        }

        return Task.CompletedTask;
    }

    public Task HandleAsync(GenerationCompletedEvent @event)
    {
        lock (_lock)
        {
            var stopwatch = Stopwatch.StartNew();

            if (@event.IsSuccessful)
            {
                _successfulGenerations++;
            }
            else
            {
                _failedGenerations++;
            }

            _totalDurationMs += @event.ExecutionTimeMs;
            _durations.Add(@event.ExecutionTimeMs);
            _lastGenerationEnd = @event.OccurredAt;

            _logger.LogDebug(
                "[{RequestId}] METRICS: Generation completed. Success: {IsSuccessful}, Duration: {DurationMs}ms, " +
                "Total: {Total}, Success: {Success}, Failed: {Failed}, Avg: {AvgDuration}ms",
                @event.RequestId,
                @event.IsSuccessful,
                @event.ExecutionTimeMs,
                _totalGenerations,
                _successfulGenerations,
                _failedGenerations,
                _durations.Count > 0 ? (double)_totalDurationMs / _durations.Count : 0
            );

            stopwatch.Stop();
        }

        return Task.CompletedTask;
    }

    private double CalculateGenerationsPerHour(DateTime firstStart, DateTime now)
    {
        var totalHours = (now - firstStart).TotalHours;
        return totalHours > 0 ? _totalGenerations / totalHours : 0;
    }

    /// <summary>
    /// Snapshot of metrics collected by GenerationMetricsCollector.
    /// </summary>
    public sealed class MetricsSnapshot
    {
        /// <summary>
        /// Total number of generation events processed.
        /// </summary>
        public int TotalGenerations { get; init; }

        /// <summary>
        /// Number of successful generation events.
        /// </summary>
        public int SuccessfulGenerations { get; init; }

        /// <summary>
        /// Number of failed generation events.
        /// </summary>
        public int FailedGenerations { get; init; }

        /// <summary>
        /// Total duration of all generations in milliseconds.
        /// </summary>
        public long TotalDurationMs { get; init; }

        /// <summary>
        /// Average duration per generation in milliseconds.
        /// </summary>
        public double AverageDurationMs { get; init; }

        /// <summary>
        /// Timestamp of the first generation event.
        /// </summary>
        public DateTime? FirstGenerationStart { get; init; }

        /// <summary>
        /// Timestamp of the last generation event.
        /// </summary>
        public DateTime LastGenerationEnd { get; init; }

        /// <summary>
        /// Average generation rate per hour.
        /// </summary>
        public double GenerationRatePerHour { get; init; }

        /// <summary>
        /// Success rate as a percentage (0-100).
        /// </summary>
        public double SuccessRate => TotalGenerations > 0
            ? (double)SuccessfulGenerations / TotalGenerations * 100
            : 0;

        /// <summary>
        /// Returns a string representation of the metrics.
        /// </summary>
        public override string ToString()
        {
            return $@"Generation Metrics:
  Total: {TotalGenerations}
  Success: {SuccessfulGenerations} ({SuccessRate:F1}%)
  Failed: {FailedGenerations}
  Total Duration: {TotalDurationMs}ms
  Average Duration: {AverageDurationMs:F2}ms
  First Start: {FirstGenerationStart?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A"}
  Last End: {LastGenerationEnd.ToString("yyyy-MM-dd HH:mm:ss")}
  Rate: {GenerationRatePerHour:F2} gen/hour";
        }
    }
}
