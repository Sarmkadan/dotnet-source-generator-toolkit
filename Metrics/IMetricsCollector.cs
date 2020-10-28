// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Metrics;

/// <summary>
/// Contract for collecting performance metrics throughout generation.
/// Enables monitoring of execution time, resource usage, and throughput.
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Start timing a named operation.
    /// </summary>
    ITimer StartTimer(string operationName);

    /// <summary>
    /// Record a gauge metric (instantaneous measurement).
    /// </summary>
    void RecordGauge(string metricName, long value);

    /// <summary>
    /// Increment a counter metric.
    /// </summary>
    void IncrementCounter(string metricName, long amount = 1);

    /// <summary>
    /// Record a histogram metric (distribution).
    /// </summary>
    void RecordHistogram(string metricName, long value);

    /// <summary>
    /// Get all collected metrics.
    /// </summary>
    MetricsSnapshot GetSnapshot();

    /// <summary>
    /// Clear all collected metrics.
    /// </summary>
    void Reset();
}

/// <summary>
/// Timer for measuring elapsed time of operations.
/// </summary>
public interface ITimer : IDisposable
{
    TimeSpan Elapsed { get; }
    void Stop();
}

/// <summary>
/// Snapshot of metrics at a point in time.
/// </summary>
public class MetricsSnapshot
{
    public Dictionary<string, long> Gauges { get; set; } = new();
    public Dictionary<string, long> Counters { get; set; } = new();
    public Dictionary<string, HistogramData> Histograms { get; set; } = new();
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Histogram metric data including statistics.
/// </summary>
public class HistogramData
{
    public long Count { get; set; }
    public long Sum { get; set; }
    public long Min { get; set; } = long.MaxValue;
    public long Max { get; set; } = long.MinValue;
    public double Average => Count > 0 ? (double)Sum / Count : 0;
}
