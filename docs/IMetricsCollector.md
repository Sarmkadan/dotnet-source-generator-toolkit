# IMetricsCollector

`IMetricsCollector` is an interface used to collect and report metric data, including gauges, counters, and histograms, along with basic statistical properties such as count, sum, min, and max. It is designed to standardize metric collection in applications where performance and thread safety are considerations.

## API

### `public Dictionary<string, long> Gauges`
A dictionary mapping gauge names to their current values. Gauges represent instantaneous measurements, such as memory usage or temperature. The keys are case-sensitive and should uniquely identify each gauge. Modifying this dictionary after collection may lead to inconsistent or stale data.

### `public Dictionary<string, long> Counters`
A dictionary mapping counter names to their cumulative values. Counters track the number of occurrences of an event, such as requests processed or errors logged. The keys are case-sensitive and must uniquely identify each counter. Concurrent modifications to this dictionary are not thread-safe unless externally synchronized.

### `public Dictionary<string, HistogramData> Histograms`
A dictionary mapping histogram names to their aggregated data. Histograms record the distribution of values, such as request durations or payload sizes. Each `HistogramData` contains statistical summaries (e.g., count, sum, min, max) for the recorded values. The keys are case-sensitive and must uniquely identify each histogram. Concurrent access to this dictionary is not thread-safe unless externally synchronized.

### `public DateTime CapturedAt`
The timestamp indicating when the metrics were captured. This value is set once during collection and should not be modified afterward. It provides a reference point for when the metrics were observed.

### `public long Count`
The total number of observations across all histograms. This value is derived from the aggregated data and represents the sum of all individual measurements. It is read-only and reflects the state at the time of collection.

### `public long Sum`
The sum of all observed values across all histograms. This value is derived from the aggregated data and represents the total of all individual measurements. It is read-only and reflects the state at the time of collection.

### `public long Min`
The minimum observed value across all histograms. This value is derived from the aggregated data and represents the smallest individual measurement. It is read-only and reflects the state at the time of collection. If no values were observed, this may be an undefined or default value.

### `public long Max`
The maximum observed value across all histograms. This value is derived from the aggregated data and represents the largest individual measurement. It is read-only and reflects the state at the time of collection. If no values were observed, this may be an undefined or default value.

## Usage

### Example 1: Collecting and Reporting Metrics
