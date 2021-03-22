# GenerationPipeline

A lightweight, asynchronous pipeline for orchestrating source generation workflows. It coordinates the discovery of source entities, their transformation via generators, and the writing of generated files, while providing detailed telemetry about the process.

## API

### `public GenerationPipeline`

Initializes a new instance of the `GenerationPipeline` class. The pipeline is configured with default settings and is ready to accept generators and source entities for execution.

### `public async Task<PipelineResult> ExecuteAsync()`

Executes the pipeline asynchronously. The pipeline discovers all registered source entities, invokes each registered generator with the discovered entities, and writes the generated files to disk. The operation is idempotent; subsequent executions with the same configuration will produce identical results.

- **Returns**: A `Task<PipelineResult>` that completes when the pipeline finishes. The result contains metadata about the execution, including success status, counts of entities and files, and any error message.
- **Throws**: `InvalidOperationException` if the pipeline has already been executed.

### `public bool IsSuccessful`

Gets a value indicating whether the last execution of the pipeline completed successfully. Returns `true` if all steps completed without error; otherwise, `false`.

- **Remarks**: This property reflects the outcome of the most recent call to `ExecuteAsync()`. It is `false` by default and remains `false` until `ExecuteAsync()` is called.

### `public int EntitiesFound`

Gets the number of source entities discovered during the last execution of the pipeline.

- **Remarks**: This value is populated only after a successful or partially successful execution. It is zero by default and remains zero until `ExecuteAsync()` is called.

### `public int GeneratedFiles`

Gets the number of files generated during the last execution of the pipeline.

- **Remarks**: This value reflects the total number of files produced by all registered generators. It is zero by default and remains zero until `ExecuteAsync()` is called.

### `public int FilesWritten`

Gets the number of files successfully written to disk during the last execution of the pipeline.

- **Remarks**: This value may be less than `GeneratedFiles` if some writes fail. It is zero by default and remains zero until `ExecuteAsync()` is called.

### `public string? ErrorMessage`

Gets the error message associated with the last execution of the pipeline, if any.

- **Remarks**: This property is non-null only if `IsSuccessful` is `false`. It contains the first error encountered during execution. It is `null` by default and remains `null` until `ExecuteAsync()` is called.

### `public DateTime ExecutedAt`

Gets the timestamp of the last execution of the pipeline.

- **Remarks**: This value is set to `DateTime.MinValue` by default and is updated only after a successful or failed execution of `ExecuteAsync()`.

## Usage

### Basic Usage
