# SourceGeneratorService

The `SourceGeneratorService` is a utility class designed to analyze C# projects, validate their configuration, and generate source code using .NET source generators. It provides asynchronous methods to inspect project metadata, validate project settings, and produce generated code for entire projects or specific entities.

## API

### `SourceGeneratorService`

The primary class providing source generation capabilities. This service orchestrates project analysis, validation, and code generation workflows.

### `public async Task<ProjectInfo> AnalyzeProjectAsync`

Analyzes the target project to extract metadata and configuration required for source generation. This includes project references, compilation options, and generator configurations.

- **Returns**: A `ProjectInfo` object containing detailed project metadata, compilation settings, and generator configurations.
- **Throws**:
  - `InvalidOperationException` if the project cannot be loaded or analyzed.
  - `ArgumentNullException` if the project path is null.
  - `FileNotFoundException` if the project file does not exist.

### `public async Task<IEnumerable<GenerationResult>> GenerateAllAsync`

Generates source code for all configured generators in the project. This method performs a full generation pass across all entities and configurations defined in the project.

- **Returns**: An enumerable of `GenerationResult` objects, each representing the output of a single generator invocation.
- **Throws**:
  - `InvalidOperationException` if the project has not been analyzed or is invalid.
  - `OperationCanceledException` if the operation is canceled via the provided cancellation token.

### `public async Task<IEnumerable<GenerationResult>> GenerateForEntityAsync`

Generates source code for a specific entity within the project. This method targets only the specified entity, reducing generation scope and improving performance for targeted scenarios.

- **Parameters**:
  - `entityName`: The name of the entity (e.g., class or struct) for which to generate code.
- **Returns**: An enumerable of `GenerationResult` objects representing the output of generator invocations for the specified entity.
- **Throws**:
  - `InvalidOperationException` if the project has not been analyzed or is invalid.
  - `ArgumentException` if `entityName` is null or empty.
  - `KeyNotFoundException` if the specified entity does not exist in the project.

### `public async Task<ValidationResult> ValidateProjectAsync`

Validates the project configuration and environment to ensure source generation can proceed safely and correctly. This includes checking generator configurations, project references, and tooling compatibility.

- **Returns**: A `ValidationResult` object indicating whether validation passed and any encountered issues.
- **Throws**:
  - `InvalidOperationException` if the project cannot be loaded.
  - `OperationCanceledException` if the operation is canceled via the provided cancellation token.

## Usage

### Example 1: Full Project Generation
