# ProjectInfo

A container class that holds metadata and analysis results for a .NET project during source generation. It tracks project identity, compilation details, discovered entities, generation templates, and validation outcomes.

## API

### `Id`
Gets the unique identifier of the project. Typically corresponds to the assembly name or project file name.

### `ProjectName`
Gets the display name of the project.

### `ProjectPath`
Gets the full file system path to the project file (e.g., `.csproj`).

### `TargetFramework`
Gets the target framework moniker (TFM) of the project (e.g., `net8.0`).

### `Entities`
Gets the list of `Entity` objects discovered during analysis. Each entity represents a type or member intended for code generation.

### `Templates`
Gets the list of `GenerationTemplate` objects available for generating code from entities.

### `GenerationResults`
Gets the list of `GenerationResult` objects produced by applying templates to entities.

### `ProjectProperties`
Gets a dictionary of MSBuild project properties (e.g., `$(TargetFramework)`, `$(RootNamespace)`) captured during analysis.

### `ReferencedAssemblies`
Gets the list of assembly names referenced by the project.

### `RootNamespace`
Gets the root namespace inferred from the project configuration.

### `AnalyzedAt`
Gets the timestamp when the project was analyzed.

### `HasAnalysisErrors`
Gets a value indicating whether the analysis phase encountered errors.

### `AnalysisErrors`
Gets the list of error messages collected during project analysis.

### `AddEntity(Entity entity)`
Adds a new entity to the project.

- **Parameters**: `entity` — The `Entity` to add.
- **Throws**: `ArgumentNullException` if `entity` is `null`.

### `AddTemplate(GenerationTemplate template)`
Adds a new code generation template to the project.

- **Parameters**: `template` — The `GenerationTemplate` to add.
- **Throws**: `ArgumentNullException` if `template` is `null`.

### `RecordGenerationResult(GenerationResult result)`
Records the outcome of applying a template to an entity.

- **Parameters**: `result` — The `GenerationResult` to record.
- **Throws**: `ArgumentNullException` if `result` is `null`.

### `GetTemplatesForType(string typeName)`
Returns the templates applicable to a given entity type.

- **Parameters**: `typeName` — The fully qualified name of the entity type.
- **Returns**: An `IEnumerable<GenerationTemplate>` of matching templates, possibly empty.
- **Throws**: `ArgumentNullException` if `typeName` is `null`.

### `GetStatistics()`
Computes and returns summary statistics about the project and its entities.

- **Returns**: A `ProjectStatistics` object containing counts and metadata.

### `Validate()`
Validates the current state of the project and its contents.

- **Returns**: An `IEnumerable<string>` of validation error messages, empty if valid.

### `TotalEntities`
Gets the total number of entities currently tracked in the project.

## Usage
