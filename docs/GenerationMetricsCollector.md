# GenerationMetricsCollector

The `GenerationMetricsCollector` class tracks and aggregates performance metrics for source generation operations within the `dotnet-source-generator-toolkit`. It records key statistics such as generation counts (total, successful, failed), durations, and timestamps to provide insights into the efficiency and reliability of source generation processes over time. The collector is designed to be used in asynchronous workflows and supports thread-safe updates.

## API

### `public GenerationMetricsCollector`
The constructor initializes a new instance of the `GenerationMetricsCollector` with default values for all metrics. No parameters are required.

---

### `public MetricsSnapshot GetSnapshot()`
Returns a read-only snapshot of the current metrics as a `MetricsSnapshot` value type.

**Returns:**
- A `MetricsSnapshot` containing the following fields:
  - `TotalGenerations`: Total number of generation attempts.
  - `SuccessfulGenerations`: Number of successful generations.
  - `FailedGenerations`: Number of failed generations.
  - `TotalDurationMs`: Cumulative duration of all generations in milliseconds.
  - `AverageDurationMs`: Average duration per generation in milliseconds (0 if no generations have occurred).
  - `FirstGenerationStart`: Timestamp of the first generation start (`null` if no generations have occurred).
  - `LastGenerationEnd`: Timestamp of the last generation end (`DateTime.MinValue` if no generations have occurred).
  - `GenerationRatePerHour`: Rate of generations per hour (0 if no generations have occurred).

**Throws:**
- None.

---

### `public Task HandleAsync(SourceGenerationStatus status, TimeSpan duration)`
Records the outcome of a single source generation attempt along with its duration.

**Parameters:**
- `status` (`SourceGenerationStatus`): The outcome of the generation (`Success` or `Failure`).
- `duration` (`TimeSpan`): The time taken for the generation attempt.

**Returns:**
- A `Task` representing the asynchronous operation.

**Throws:**
- `ArgumentOutOfRangeException`: If `duration` is negative.
- `ArgumentException`: If `status` is not a valid `SourceGenerationStatus` value.

---

### `public Task HandleAsync(SourceGenerationStatus status, DateTime start, DateTime end)`
Records the outcome of a single source generation attempt along with its start and end timestamps.

**Parameters:**
- `status` (`SourceGenerationStatus`): The outcome of the generation (`Success` or `Failure`).
- `start` (`DateTime`): The timestamp when the generation started.
- `end` (`DateTime`): The timestamp when the generation ended.

**Returns:**
- A `Task` representing the asynchronous operation.

**Throws:**
- `ArgumentException`: If `start` is later than `end` or if `status` is not a valid `SourceGenerationStatus` value.

---

### `public int TotalGenerations`
Gets the total number of generation attempts (successful and failed).

**Returns:**
- The total count of generations.

---

### `public int SuccessfulGenerations`
Gets the number of successful generation attempts.

**Returns:**
- The count of successful generations.

---

### `public int FailedGenerations`
Gets the number of failed generation attempts.

**Returns:**
- The count of failed generations.

---

### `public long TotalDurationMs`
Gets the cumulative duration of all generation attempts in milliseconds.

**Returns:**
- The total duration in milliseconds.

---

### `public double AverageDurationMs`
Gets the average duration of generation attempts in milliseconds. Returns `0` if no generations have occurred.

**Returns:**
- The average duration in milliseconds.

---

### `public DateTime? FirstGenerationStart`
Gets the timestamp of the first generation start. Returns `null` if no generations have occurred.

**Returns:**
- The start timestamp of the first generation or `null`.

---

### `public DateTime LastGenerationEnd`
Gets the timestamp of the last generation end. Returns `DateTime.MinValue` if no generations have occurred.

**Returns:**
- The end timestamp of the last generation or `DateTime.MinValue`.

---

### `public double GenerationRatePerHour`
Gets the rate of generations per hour, calculated based on the time elapsed between the first and last generation. Returns `0` if fewer than two generations have occurred.

**Returns:**
- The generation rate per hour.

---

### `public override string ToString()`
Returns a formatted string representation of the current metrics, including counts, durations, and timestamps.

**Returns:**
- A string summarizing the metrics.

**Example Output:**
