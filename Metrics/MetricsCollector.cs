// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Metrics;

/// <summary>
/// Collects performance metrics including timers, counters, gauges, and histograms.
/// Thread-safe for concurrent metric collection from parallel tasks.
/// </summary>
public class MetricsCollector : IMetricsCollector
{
    private readonly Dictionary<string, long> _gauges = new();
    private readonly Dictionary<string, long> _counters = new();
    private readonly Dictionary<string, HistogramData> _histograms = new();
    private readonly object _lock = new();

    public ITimer StartTimer(string operationName)
    {
        return new StopWatch(this, operationName);
    }

    public void RecordGauge(string metricName, long value)
    {
        lock (_lock)
        {
            _gauges[metricName] = value;
        }
    }

    public void IncrementCounter(string metricName, long amount = 1)
    {
        lock (_lock)
        {
            if (!_counters.TryGetValue(metricName, out var current))
            {
                _counters[metricName] = amount;
            }
            else
            {
                _counters[metricName] = current + amount;
            }
        }
    }

    public void RecordHistogram(string metricName, long value)
    {
        lock (_lock)
        {
            if (!_histograms.TryGetValue(metricName, out var histogram))
            {
                histogram = new HistogramData();
                _histograms[metricName] = histogram;
            }

            histogram.Count++;
            histogram.Sum += value;
            histogram.Min = Math.Min(histogram.Min, value);
            histogram.Max = Math.Max(histogram.Max, value);
        }
    }

    public MetricsSnapshot GetSnapshot()
    {
        lock (_lock)
        {
            return new MetricsSnapshot
            {
                Gauges = new Dictionary<string, long>(_gauges),
                Counters = new Dictionary<string, long>(_counters),
                Histograms = _histograms.ToDictionary(
                    k => k.Key,
                    v => new HistogramData
                    {
                        Count = v.Value.Count,
                        Sum = v.Value.Sum,
                        Min = v.Value.Min,
                        Max = v.Value.Max,
                    }),
            };
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _gauges.Clear();
            _counters.Clear();
            _histograms.Clear();
        }
    }

    /// <summary>
    /// Timer implementation using System.Diagnostics.Stopwatch.
    /// </summary>
    private class StopWatch : ITimer
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();
        private readonly MetricsCollector _collector;
        private readonly string _operationName;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public StopWatch(MetricsCollector collector, string operationName)
        {
            _collector = collector;
            _operationName = operationName;
        }

        public void Stop()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
                _collector.RecordHistogram(_operationName, _stopwatch.ElapsedMilliseconds);
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
