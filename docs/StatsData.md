# StatsData

Represents a snapshot of statistical data collected during the code generation process for a specific project. This type aggregates metrics such as entity counts, property counts, and performance measurements, along with contextual information like the project path and timestamp. It is primarily used for reporting, diagnostics, and analysis of the source generation pipeline.

## API

### `public DateTime Timestamp`
The point in time when the statistics were captured. This value is automatically set when the `StatsData` instance is created and reflects the local system time.

### `public string ProjectPath`
The full filesystem path to the project file (e.g., `.csproj`) associated with the collected statistics. This path is used to uniquely identify the project within the reporting context. Throws `ArgumentNullException` if assigned a `null` value.

### `public int EntityCount`
The total number of entities (e.g., classes, structs, interfaces) processed during the generation phase. This count includes all entities discovered in the project, regardless of whether they were modified or generated.

### `public int PropertyCount`
The total number of properties (e.g., fields, properties, methods) processed across all entities in the project. This metric provides insight into the complexity of the generated codebase.

### `public GenerationMetricsCollector.MetricsSnapshot? GenerationMetrics`
An optional snapshot of performance metrics collected during the generation process, such as execution time, memory usage, or iteration counts. This value is `null` if no metrics were recorded for the current snapshot.

### `public ProjectStatistics? ProjectStatistics`
An optional reference to detailed project-level statistics, including aggregated counts of files, namespaces, or other project-specific metrics. This value is `null` if no project statistics were collected.

### `public override string ToString()`
Returns a human-readable string representation of the `StatsData` instance, including all non-null public fields. The format is optimized for logging and debugging purposes. This method does not throw exceptions.

## Usage

### Example 1: Capturing and Logging Statistics
