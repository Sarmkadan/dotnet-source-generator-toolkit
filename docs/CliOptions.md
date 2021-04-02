# CliOptions

The `CliOptions` class serves as the primary data transfer object for configuring the execution context of the .NET Source Generator Toolkit command-line interface. It encapsulates all user-specified parameters required to locate target projects, define generator behavior, control output formatting, and manage execution modes such as validation or dry runs. This type acts as the central configuration hub, bridging raw command-line arguments and the internal logic of the source generation pipeline.

## API

### ProjectPath
*   **Type:** `public string ProjectPath`
*   **Purpose:** Specifies the absolute or relative file system path to the `.csproj` file that serves as the entry point for source generation.
*   **Behavior:** This property is mandatory for execution. It determines the working directory and the project context used to resolve dependencies and analyze syntax trees.
*   **Exceptions:** No exceptions are thrown by the property itself; however, invalid paths provided to this property may cause `FileNotFoundException` or `IOException` in downstream consumers during file resolution.

### OutputPath
*   **Type:** `public string? OutputPath`
*   **Purpose:** Defines the directory where generated source files should be written.
*   **Behavior:** If set to `null`, the toolkit typically defaults to a standard output location (e.g., an intermediate build folder). When specified, the directory must be writable.
*   **Exceptions:** No exceptions are thrown by the property accessor. Invalid paths may result in `UnauthorizedAccessException` or `DirectoryNotFoundException` during the write phase.

### GeneratorTypes
*   **Type:** `public List<string> GeneratorTypes`
*   **Purpose:** Contains a list of fully qualified type names or aliases identifying specific source generators to invoke within the toolkit.
*   **Behavior:** An empty list usually implies that all discovered generators should be executed. The list order may dictate the execution sequence.
*   **Exceptions:** No exceptions are thrown by the property. Passing `null` to this property (if allowed by the setter context) or including invalid type names will result in `ArgumentException` or `TypeLoadException` during the initialization phase.

### OutputFormat
*   **Type:** `public string OutputFormat`
*   **Purpose:** Determines the serialization format for the output (e.g., "CSharp", "Json", "Xml").
*   **Behavior:** The default is typically "CSharp" for standard source generation. Changing this affects how the generated content is structured on disk.
*   **Exceptions:** No exceptions are thrown by the property. Unsupported format strings will trigger an `InvalidOperationException` when the formatter factory attempts to resolve the provider.

### Verbose
*   **Type:** `public bool Verbose`
*   **Purpose:** Enables detailed logging and diagnostic information to the standard output.
*   **Behavior:** When `true`, the application emits step-by-step execution logs, timing information, and internal state changes. When `false`, only critical errors and summaries are shown.
*   **Exceptions:** None.

### ShowHelp
*   **Type:** `public bool ShowHelp`
*   **Purpose:** Indicates that the application should display the help manual and exit immediately without performing generation.
*   **Behavior:** If `true`, the main execution loop is bypassed in favor of rendering usage instructions.
*   **Exceptions:** None.

### ShowVersion
*   **Type:** `public bool ShowVersion`
*   **Purpose:** Indicates that the application should print the current version number and exit immediately.
*   **Behavior:** If `true`, version metadata is retrieved and displayed, skipping all generation logic.
*   **Exceptions:** None.

### NamespaceOverride
*   **Type:** `public string? NamespaceOverride`
*   **Purpose:** Forces all generated types to reside within a specific namespace, ignoring the namespace inferred from the project structure.
*   **Behavior:** If `null`, namespaces are derived automatically. If set, this value takes precedence for all generated artifacts.
*   **Exceptions:** No exceptions are thrown by the property. Invalid C# identifier characters in the string may cause `SyntaxException` during code emission.

### Recursive
*   **Type:** `public bool Recursive`
*   **Purpose:** Controls whether the tool should traverse subdirectories to discover additional project files or configuration contexts.
*   **Behavior:** When `true`, the search scope expands beyond the immediate `ProjectPath` directory.
*   **Exceptions:** None.

### GenerateDtos
*   **Type:** `public bool GenerateDtos`
*   **Purpose:** Toggles the automatic generation of Data Transfer Objects (DTOs) alongside standard source artifacts.
*   **Behavior:** Enabling this flag activates specific generator passes dedicated to creating serialization-friendly classes.
*   **Exceptions:** None.

### ConfigFile
*   **Type:** `public string? ConfigFile`
*   **Purpose:** Specifies the path to an external configuration file (e.g., JSON or XML) to supplement or override command-line arguments.
*   **Behavior:** If `null`, only command-line arguments and defaults are used. If provided, the file is parsed early in the startup sequence.
*   **Exceptions:** No exceptions are thrown by the property. Missing files or malformed content will result in `FileNotFoundException` or `JsonException`/`XmlException` during loading.

### DryRun
*   **Type:** `public bool DryRun`
*   **Purpose:** Executes the generation pipeline up to the point of file writing, validating logic without persisting changes to the disk.
*   **Behavior:** Useful for testing configurations. The tool performs analysis and code generation in memory but discards the output.
*   **Exceptions:** None.

### ValidateOnly
*   **Type:** `public bool ValidateOnly`
*   **Purpose:** Restricts execution to validation checks (syntax, semantic analysis, configuration correctness) without generating any code.
*   **Behavior:** Returns an exit code indicating success or failure of the validation pass.
*   **Exceptions:** None.

### DegreeOfParallelism
*   **Type:** `public int DegreeOfParallelism`
*   **Purpose:** Sets the maximum number of concurrent threads or tasks used during the generation process.
*   **Behavior:** A value of `-1` or `0` typically defaults to the number of logical processors. Positive integers constrain the concurrency level.
*   **Exceptions:** No exceptions are thrown by the property. Values less than `-1` may cause `ArgumentOutOfRangeException` when initializing the task scheduler.

## Usage

### Example 1: Standard Generation with Custom Output
The following example demonstrates configuring `CliOptions` for a standard generation run, specifying a custom output directory and enabling verbose logging for debugging purposes.

```csharp
using System.Collections.Generic;
using DotNetSourceGeneratorToolkit;

var options = new CliOptions
{
    ProjectPath = "./src/MyApplication/MyApplication.csproj",
    OutputPath = "./artifacts/generated",
    OutputFormat = "CSharp",
    Verbose = true,
    Recursive = false,
    GeneratorTypes = new List<string> { "MyCompany.Generators.RepositoryGenerator" },
    DegreeOfParallelism = 4
};

// The options object is now ready to be passed to the runner
// var runner = new SourceGeneratorRunner(options);
// await runner.ExecuteAsync();
```

### Example 2: Validation and Dry Run Mode
This example configures the toolkit to validate a project configuration and perform a dry run, ensuring no files are written while checking for errors across multiple generator types.

```csharp
using System.Collections.Generic;
using DotNetSourceGeneratorToolkit;

var options = new CliOptions
{
    ProjectPath = "./tests/ValidationTest/ValidationTest.csproj",
    ConfigFile = "./config/validation-settings.json",
    ValidateOnly = true,
    DryRun = true,
    Verbose = true,
    GeneratorTypes = new List<string> 
    { 
        "DotNetSourceGeneratorToolkit.Generators.DtoGenerator",
        "DotNetSourceGeneratorToolkit.Generators.ApiGenerator"
    },
    NamespaceOverride = "Test.Validation.Namespace",
    DegreeOfParallelism = -1 // Use default processor count
};

// Execution will analyze the project and report errors without writing files
// var result = await SourceGeneratorRunner.ValidateAsync(options);
```

## Notes

*   **Mutability:** The `CliOptions` class exposes mutable public properties. It is not thread-safe for concurrent modification. If an instance is shared across threads, it should be treated as immutable after initialization, or external synchronization must be applied.
*   **Collection Initialization:** The `GeneratorTypes` property is a `List<string>`. Callers must ensure this list is instantiated before adding items to avoid `NullReferenceException`. The toolkit does not automatically initialize this collection in the default constructor if not explicitly set.
*   **Path Resolution:** Properties accepting paths (`ProjectPath`, `OutputPath`, `ConfigFile`) do not perform immediate validation upon assignment. Validation occurs lazily during the execution phase. Relative paths are resolved against the current working directory of the process at runtime.
*   **Conflicting Flags:** Setting both `DryRun` and `ValidateOnly` to `true` is permissible and results in a validation pass that loads generators but skips both code emission and file I/O. However, setting `ShowHelp` or `ShowVersion` to `true` generally supersedes all other flags, causing immediate termination before other properties are processed.
*   **Parallelism Constraints:** The `DegreeOfParallelism` property accepts integers. While the property setter does not restrict values, passing values less than `1` (excluding standard sentinel values like `-1` if supported by the underlying runner) may lead to runtime exceptions when the `ParallelOptions` or Task Scheduler is configured.
