# ToolkitOptions

`ToolkitOptions` is a configuration class used to control the behavior of source generators in the `dotnet-source-generator-toolkit` project. It allows fine-tuning of generation processes, caching, formatting, parallelism, and output settings to adapt the toolkit to different project needs.

## API

### `EnableCaching`
Enables or disables the caching mechanism for generated files. When enabled, the toolkit caches generated outputs to avoid reprocessing unchanged source files, improving performance in incremental builds.

### `CacheExpirationMinutes`
Specifies the duration (in minutes) after which cached entries expire. Only relevant when `EnableCaching` is `true`. Defaults to `60` minutes if not set.

### `EnableCodeFormatting`
Determines whether generated code should be automatically formatted according to the project's coding style. When enabled, the toolkit applies formatting rules such as indentation and line wrapping.

### `CodeFormattingLineLength`
Sets the maximum line length (in characters) for code formatting. Only applicable when `EnableCodeFormatting` is `true`. Defaults to `120` characters if not set.

### `VerboseLogging`
Controls the verbosity of logging output during generation. When `true`, detailed logs are emitted to aid debugging. When `false`, only essential logs are produced.

### `MaxDegreeOfParallelism`
Limits the number of concurrent operations the toolkit may perform during generation. A value of `-1` indicates no limit. Defaults to `-1` if not set.

### `OperationTimeoutSeconds`
Defines the maximum duration (in seconds) allowed for individual generation operations before timing out. A value of `-1` indicates no timeout. Defaults to `-1` if not set.

### `GenerateDtos`
Indicates whether Data Transfer Objects (DTOs) should be generated for models. When `true`, the toolkit creates DTO classes alongside or instead of model classes.

### `DefaultNamespace`
Specifies the default namespace to use for generated code. If `null` or empty, the toolkit uses the project's default namespace.

### `OutputDirectory`
Sets the directory where generated files are written. If `null` or empty, files are generated in the project's output directory.

### `BackupExistingFiles`
Determines whether existing files in the output directory should be backed up before generation. Backups are stored with a `.bak` suffix. Defaults to `false`.

### `GenerateInterfaces`
Indicates whether interfaces should be generated for models or services. When `true`, the toolkit creates interface definitions alongside implementations.

### `GenerateXmlComments`
Controls whether XML documentation comments are generated for public members in generated code. When `true`, the toolkit includes XML comments for classes, methods, and properties.

## Usage

### Basic Configuration
```csharp
var options = new ToolkitOptions
{
    EnableCaching = true,
    CacheExpirationMinutes = 30,
    EnableCodeFormatting = true,
    CodeFormattingLineLength = 100,
    VerboseLogging = true,
    MaxDegreeOfParallelism = 4,
    OperationTimeoutSeconds = 30,
    GenerateDtos = true,
    DefaultNamespace = "MyProject.Models",
    OutputDirectory = "Generated",
    BackupExistingFiles = true,
    GenerateInterfaces = true,
    GenerateXmlComments = true
};
```

### Minimal Configuration
```csharp
var options = new ToolkitOptions
{
    EnableCaching = false,
    GenerateDtos = true
};
```

## Notes

- **Thread Safety**: `ToolkitOptions` is designed to be immutable after construction. All properties are read-only once the instance is created. Modifications require creating a new instance.
- **Default Values**: Unset numeric properties default to `-1` (no limit/timeout), while string properties default to `null`. Boolean properties default to `false`.
- **Caching**: Enabling caching (`EnableCaching = true`) without setting `CacheExpirationMinutes` uses a 60-minute default. Ensure the cache directory is writable and persists between builds for optimal performance.
- **Parallelism**: Setting `MaxDegreeOfParallelism` to a value greater than the available CPU cores may not improve performance and could increase memory usage. Monitor system resources when tuning this value.
- **Timeouts**: `OperationTimeoutSeconds` applies per-operation. A value of `0` is invalid and may cause immediate timeouts. Negative values disable timeouts.
- **Output Conflicts**: If `BackupExistingFiles = true`, existing files are renamed with a `.bak` suffix. If a backup already exists, it is overwritten without warning.
