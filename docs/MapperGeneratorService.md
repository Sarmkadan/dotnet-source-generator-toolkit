# MapperGeneratorService

The `MapperGeneratorService` is a service class that provides functionality to generate source code mappers at compile-time using C# source generators. It encapsulates the logic for processing mapper definitions, validating inputs, and producing compilation-ready `GenerationResult` objects that can be consumed by source generators to emit additional source files.

## API

### `MapperGeneratorService`

A sealed service class responsible for generating mapper source code. It is designed to be used as a singleton or scoped service within dependency injection containers.

### `public async Task<IEnumerable<GenerationResult>> GenerateAllMappersAsync()`

Generates source code for all mappers registered with the service.

- **Returns**: An `IEnumerable<GenerationResult>` containing the results of each mapper generation attempt. Each `GenerationResult` includes success status, generated source code (if successful), and diagnostic messages.
- **Throws**: `InvalidOperationException` if the service is not properly initialized or if no mappers are registered.

### `public async Task<GenerationResult> GenerateMapperAsync()`

Generates source code for a single mapper identified by its type.

- **Returns**: A `GenerationResult` containing the outcome of the generation attempt, including generated source code (if successful) and any diagnostic messages.
- **Throws**: `ArgumentNullException` if the mapper type is `null`.
- **Throws**: `InvalidOperationException` if the mapper type is not registered or if the service is not properly initialized.

### `public static IEnumerable<Type> GetRegisteredMapperTypes()`

Retrieves all mapper types currently registered with the service.

- **Returns**: An `IEnumerable<Type>` of all registered mapper types. The collection may be empty if no mappers are registered.

## Usage

### Example 1: Generating all registered mappers
