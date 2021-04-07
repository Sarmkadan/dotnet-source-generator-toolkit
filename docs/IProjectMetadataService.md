# IProjectMetadataService

`IProjectMetadataService` provides an abstraction for reading and validating metadata from .NET project files. It exposes asynchronous methods to retrieve project properties, dependencies, and validation results, enabling tooling and build-time tasks to inspect project configurations without direct file system access.

## API

### `ProjectName`
Gets the name of the project as defined in the project file.

- **Type**: `string`
- **Access**: Read-only

### `ProjectPath`
Gets the full file system path to the project file.

- **Type**: `string`
- **Access**: Read-only

### `TargetFramework`
Gets the target framework moniker (TFM) for the project.

- **Type**: `string`
- **Access**: Read-only

### `RootNamespace`
Gets the root namespace used for code generation in the project.

- **Type**: `string`
- **Access**: Read-only

### `ProjectVersion`
Gets the version of the project as defined in the project file.

- **Type**: `Version?`
- **Access**: Read-only

### `Dependencies`
Gets the list of project dependencies (NuGet packages or project references).

- **Type**: `List<Dependency>`
- **Access**: Read-only

### `Properties`
Gets a dictionary of additional project properties from the project file.

- **Type**: `Dictionary<string, string>`
- **Access**: Read-only

### `LoadedAt`
Gets the timestamp when the project metadata was loaded.

- **Type**: `DateTime`
- **Access**: Read-only

### `Name`
Gets the name of the dependency (for `Dependency` instances).

- **Type**: `string`
- **Access**: Read-only

### `Version`
Gets the version of the dependency (for `Dependency` instances).

- **Type**: `string`
- **Access**: Read-only

### `IsDevDependency`
Indicates whether the dependency is marked as a development dependency (for `Dependency` instances).

- **Type**: `bool`
- **Access**: Read-only

### `ProjectMetadataService`
Initializes a new instance of the `IProjectMetadataService` implementation.

- **Type**: Constructor

### `ReadProjectMetadataAsync`
Asynchronously reads and parses the project metadata from the project file.

- **Returns**: `Task<ProjectMetadata>`
- **Throws**: `InvalidOperationException` if the project file is invalid or inaccessible.

### `GetDependenciesAsync`
Asynchronously retrieves the list of project dependencies.

- **Returns**: `Task<IEnumerable<Dependency>>`
- **Throws**: `InvalidOperationException` if the project file cannot be read.

### `GetTargetFrameworkAsync`
Asynchronously retrieves the target framework moniker (TFM) for the project.

- **Returns**: `Task<string>`
- **Throws**: `InvalidOperationException` if the project file is invalid or the TFM is missing.

### `GetRootNamespaceAsync`
Asynchronously retrieves the root namespace for the project.

- **Returns**: `Task<string>`
- **Throws**: `InvalidOperationException` if the project file is invalid or the namespace is missing.

### `ValidateProjectAsync`
Asynchronously validates the project file for correctness and completeness.

- **Returns**: `Task<bool>`
- **Throws**: `InvalidOperationException` if the project file cannot be read.

## Usage

### Example 1: Reading Project Metadata
