# ValidatorGeneratorServiceExtensions

Extension methods for generating and validating validators for entities using source generators. These methods provide asynchronous operations for generating validation code, validating entities, and collecting statistics about the generation and validation processes.

## API

### `GenerateAllValidatorsAsync`

Generates validation code for all entities in the project.

- **Parameters**:
  - `projectPath` (string): The path to the project containing entities to validate.
  - `outputPath` (string): The directory where generated validator files should be written.
  - `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.
- **Return value**: `Task<IEnumerable<GenerationResult>>` – A collection of results indicating success or failure for each entity.
- **Throws**:
  - `ArgumentNullException` if `projectPath` or `outputPath` is null.
  - `DirectoryNotFoundException` if the project directory does not exist.
  - `IOException` if file operations fail.

---

### `GenerateValidatorAsync`

Generates validation code for a single entity.

- **Parameters**:
  - `entityName` (string): The name of the entity to generate a validator for.
  - `projectPath` (string): The path to the project containing the entity.
  - `outputPath` (string): The directory where the generated validator file should be written.
  - `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.
- **Return value**: `Task<GenerationResult>` – A result indicating success or failure of the generation.
- **Throws**:
  - `ArgumentNullException` if any required string parameter is null.
  - `ArgumentException` if `entityName` is empty or whitespace.
  - `DirectoryNotFoundException` if the project or output directory does not exist.
  - `FileNotFoundException` if the entity is not found in the project.

---

### `GenerateValidatorsWithStatsAsync`

Generates validation code for all entities and returns detailed statistics about the process.

- **Parameters**:
  - `projectPath` (string): The path to the project containing entities to validate.
  - `outputPath` (string): The directory where generated validator files should be written.
  - `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.
- **Return value**: `Task<ValidatorGenerationStats>` – An object containing statistics about the generation process.
- **Throws**:
  - `ArgumentNullException` if `projectPath` or `outputPath` is null.
  - `DirectoryNotFoundException` if the project directory does not exist.
  - `IOException` if file operations fail.

---

### `ValidateEntityAsync`

Validates a single entity instance against its generated validator.

- **Parameters**:
  - `entity` (object): The entity instance to validate.
  - `entityName` (string): The name of the entity type.
  - `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.
- **Return value**: `Task<bool>` – `true` if the entity is valid; otherwise, `false`.
- **Throws**:
  - `ArgumentNullException` if `entity` or `entityName` is null.
  - `ArgumentException` if `entityName` is empty or whitespace.
  - `InvalidOperationException` if no validator is available for the given entity type.

---

### `ValidateEntitiesAsync`

Validates a collection of entity instances.

- **Parameters**:
  - `entities` (IEnumerable<object>): The entities to validate.
  - `entityName` (string): The name of the entity type.
  - `cancellationToken` (CancellationToken, optional): A token to monitor for cancellation requests.
- **Return value**: `Task<IEnumerable<EntityValidationResult>>` – A collection of validation results, one per entity.
- **Throws**:
  - `ArgumentNullException` if `entities` or `entityName` is null.
  - `ArgumentException` if `entityName` is empty or whitespace.

---

### `TotalEntities` (property)

Gets the total number of entities processed.

- **Type**: `int`
- **Access**: Read-only

---

### `Successful` (property)

Gets the number of entities successfully validated or generated.

- **Type**: `int`
- **Access**: Read-only

---
### `Failed` (property)

Gets the number of entities that failed validation or generation.

- **Type**: `int`
- **Access**: Read-only

---
### `Skipped` (property)

Gets the number of entities skipped during processing.

- **Type**: `int`
- **Access**: Read-only

---
### `InProgress` (property)

Gets the number of entities currently being processed.

- **Type**: `int`
- **Access**: Read-only

---
### `TotalLinesGenerated` (property)

Gets the total number of lines of generated validator code.

- **Type**: `int`
- **Access**: Read-only

---
### `AverageExecutionTimeMs` (property)

Gets the average execution time per entity in milliseconds.

- **Type**: `double`
- **Access**: Read-only

---
### `EntityName` (property)

Gets the name of the entity currently being processed.

- **Type**: `string`
- **Access**: Read-only

---
### `IsValid` (property)

Gets a value indicating whether the last validation operation succeeded.

- **Type**: `bool`
- **Access**: Read-only

---
### `Errors` (property)

Gets a list of error messages from the last validation or generation operation.

- **Type**: `List<string>`
- **Access**: Read-only

## Usage

### Example 1: Generate validators for all entities in a project
