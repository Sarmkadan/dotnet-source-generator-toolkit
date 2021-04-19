# MetricsCollector
The `MetricsCollector` type is designed to collect and manage metrics data, providing a set of methods to record various types of metrics, such as timers, gauges, counters, and histograms. It allows users to track performance and other relevant metrics in their applications, and provides a way to retrieve a snapshot of the collected metrics.

## API
* `public ITimer StartTimer`: Starts a new timer, returning an `ITimer` object that can be used to stop the timer. The timer measures the elapsed time between the start and stop calls.
* `public void RecordGauge(double value)`: Records a gauge metric with the specified value. A gauge is a metric that can increase or decrease over time.
* `public void IncrementCounter(string name, int increment = 1)`: Increments a counter metric with the specified name by the given increment value (default is 1).
* `public void RecordHistogram(string name, double value)`: Records a histogram metric with the specified name and value. A histogram is a metric that tracks the distribution of values.
* `public MetricsSnapshot GetSnapshot()`: Returns a snapshot of the collected metrics, providing a point-in-time view of the metrics data.
* `public void Reset()`: Resets all collected metrics to their initial state.
* `public StopWatch`: A property that provides a stopwatch object, which can be used to measure elapsed time.
* `public void Stop()`: Stops the metrics collector, preventing further metric collection.
* `public void Dispose()`: Disposes of the metrics collector, releasing any resources it holds. May throw an exception if the collector is already disposed.

## Usage
The following examples demonstrate how to use the `MetricsCollector` type:
```csharp
// Example 1: Recording metrics
var collector = new MetricsCollector();
collector.RecordGauge(10.5);
collector.IncrementCounter("requests", 2);
var snapshot = collector.GetSnapshot();
Console.WriteLine(snapshot);

// Example 2: Using a timer
var collector = new MetricsCollector();
var timer = collector.StartTimer();
// Perform some operation
timer.Stop();
collector.RecordHistogram("operation-time", timer.ElapsedMilliseconds);
```

## Notes
When using the `MetricsCollector` type, note that:
* The `StartTimer` method returns an `ITimer` object that must be stopped using the `Stop` method to ensure accurate timing.
* The `RecordGauge`, `IncrementCounter`, and `RecordHistogram` methods do not throw exceptions, but may log errors if the metric collection fails.
* The `GetSnapshot` method returns a snapshot of the metrics data at the time of the call, and does not reflect any subsequent changes to the metrics.
* The `Reset` method resets all collected metrics, including timers, gauges, counters, and histograms.
* The `StopWatch` property provides a stopwatch object that can be used to measure elapsed time, but is not thread-safe.
* The `Dispose` method should be called when the metrics collector is no longer needed to release any resources it holds.
* The `MetricsCollector` type is not thread-safe, and should be used from a single thread to ensure accurate metric collection.
