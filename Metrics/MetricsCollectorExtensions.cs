#nullable enable

namespace DotNetSourceGeneratorToolkit.Metrics;

/// <summary>
/// Extension methods for <see cref="MetricsCollector"/> to provide convenient metric collection operations.
/// </summary>
public static class MetricsCollectorExtensions
{
    /// <summary>
    /// Records a counter increment with optional tags for categorization.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="metricName">Name of the counter metric.</param>
    /// <param name="amount">Amount to increment by (defaults to 1).</param>
    /// <param name="tags">Optional tags to categorize the metric.</param>
    public static void IncrementCounter(this MetricsCollector collector, string metricName, long amount, Dictionary<string, string>? tags = null)
    {
        if (tags != null && tags.Count > 0)
        {
            // Create a tagged version of the metric name
            var taggedName = $"{metricName}[{string.Join(",", tags.Select(t => $"{t.Key}={t.Value}"))}]";
            collector.IncrementCounter(taggedName, amount);
        }
        else
        {
            collector.IncrementCounter(metricName, amount);
        }
    }

    /// <summary>
    /// Records a gauge value with optional tags for categorization.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="metricName">Name of the gauge metric.</param>
    /// <param name="value">The gauge value to record.</param>
    /// <param name="tags">Optional tags to categorize the metric.</param>
    public static void RecordGauge(this MetricsCollector collector, string metricName, long value, Dictionary<string, string>? tags = null)
    {
        if (tags != null && tags.Count > 0)
        {
            // Create a tagged version of the metric name
            var taggedName = $"{metricName}[{string.Join(",", tags.Select(t => $"{t.Key}={t.Value}"))}]";
            collector.RecordGauge(taggedName, value);
        }
        else
        {
            collector.RecordGauge(metricName, value);
        }
    }

    /// <summary>
    /// Measures and records the execution time of an action using a timer.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="operationName">Name of the operation being measured.</param>
    /// <param name="action">The action to measure.</param>
    /// <returns>The elapsed time as a TimeSpan.</returns>
    public static TimeSpan MeasureTime(this MetricsCollector collector, string operationName, Action action)
    {
        using var timer = collector.StartTimer(operationName);
        action();
        return timer.Elapsed;
    }

    /// <summary>
    /// Measures and records the execution time of a function using a timer.
    /// </summary>
    /// <typeparam name="T">Return type of the function.</typeparam>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="operationName">Name of the operation being measured.</param>
    /// <param name="func">The function to measure.</param>
    /// <returns>The result of the function and the elapsed time.</returns>
    public static (T Result, TimeSpan Elapsed) MeasureTime<T>(this MetricsCollector collector, string operationName, Func<T> func)
    {
        using var timer = collector.StartTimer(operationName);
        var result = func();
        return (result, timer.Elapsed);
    }

    /// <summary>
    /// Measures and records the execution time of an async action using a timer.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="operationName">Name of the operation being measured.</param>
    /// <param name="action">The async action to measure.</param>
    /// <returns>A task representing the completion.</returns>
    public static async Task MeasureTimeAsync(this MetricsCollector collector, string operationName, Func<Task> action)
    {
        using var timer = collector.StartTimer(operationName);
        await action();
    }

    /// <summary>
    /// Measures and records the execution time of an async function using a timer.
    /// </summary>
    /// <typeparam name="T">Return type of the function.</typeparam>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="operationName">Name of the operation being measured.</param>
    /// <param name="func">The async function to measure.</param>
    /// <returns>The result of the function and the elapsed time.</returns>
    public static async Task<(T Result, TimeSpan Elapsed)> MeasureTimeAsync<T>(this MetricsCollector collector, string operationName, Func<Task<T>> func)
    {
        using var timer = collector.StartTimer(operationName);
        var result = await func();
        return (result, timer.Elapsed);
    }

    /// <summary>
    /// Records a histogram value with additional context information.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="metricName">Name of the histogram metric.</param>
    /// <param name="value">The value to record in the histogram.</param>
    /// <param name="context">Optional context information for the measurement.</param>
    public static void RecordHistogram(this MetricsCollector collector, string metricName, long value, string? context = null)
    {
        if (context != null)
        {
            var contextMetricName = $"{metricName}_with_context";
            collector.RecordHistogram(contextMetricName, value);
        }

        collector.RecordHistogram(metricName, value);
    }

    /// <summary>
    /// Gets a snapshot and resets the collector in one atomic operation.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <returns>A metrics snapshot.</returns>
    public static MetricsSnapshot GetSnapshotAndReset(this MetricsCollector collector)
    {
        var snapshot = collector.GetSnapshot();
        collector.Reset();
        return snapshot;
    }

    /// <summary>
    /// Increments a counter and records a histogram value in a single operation.
    /// Useful for tracking both count and duration of operations.
    /// </summary>
    /// <param name="collector">The metrics collector instance.</param>
    /// <param name="counterName">Name of the counter metric.</param>
    /// <param name="histogramName">Name of the histogram metric.</param>
    /// <param name="amount">Amount to increment the counter by.</param>
    /// <param name="durationMs">Duration value to record in the histogram.</param>
    public static void RecordOperation(this MetricsCollector collector, string counterName, string histogramName, long amount, long durationMs)
    {
        collector.IncrementCounter(counterName, amount);
        collector.RecordHistogram(histogramName, durationMs);
    }
}