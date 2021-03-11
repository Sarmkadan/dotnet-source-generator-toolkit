# GenerationException

The `GenerationException` class and its derived types serve as the base exception hierarchy for errors encountered during source code generation processes within the `dotnet-source-generator-toolkit`. These exceptions encapsulate metadata about the failure, such as the type of generator involved and the specific entity associated with the error, facilitating more granular error handling and diagnostic reporting during build-time generation.

## API

### GenerationException
Represents the base exception for generation-related failures.

#### Properties
*   `public string? GeneratorType`: Gets or sets the type name of the generator that produced the exception.
*   `public string? EntityName`: Gets or sets the name of the entity being processed when the exception occurred.

#### Constructors
*   `public GenerationException(string message)`: Initializes a new instance with a specified error message.
*   `public GenerationException()`: Initializes a new instance.

### Derived Exceptions
These specialized exceptions inherit from `GenerationException` to categorize failures by domain.

*   `public EntityAnalysisException(string message)`: Thrown when an error occurs during the analysis phase of an entity.
*   `public EntityAnalysisException()`: Initializes a new instance.
*   `public RepositoryGenerationException(string message)`: Thrown when an error occurs during repository source generation.
*   `public RepositoryGenerationException()`: Initializes a new instance.
*   `public MapperGenerationException(string message)`: Thrown when an error occurs during mapper source generation.
*   `public MapperGenerationException()`: Initializes a new instance.
*   `public ValidatorGenerationException(string message)`: Thrown when an error occurs during validator source generation.
*   `public ValidatorGenerationException()`: Initializes a new instance.
*   `public GenerationConfigurationException(string message)`: Thrown when the generation configuration is invalid.
*   `public GenerationConfigurationException()`: Initializes a new instance.

## Usage

### Throwing a specialized exception
```csharp
public void AnalyzeEntity(EntityModel entity)
{
    if (string.IsNullOrWhiteSpace(entity.Name))
    {
        throw new EntityAnalysisException("Entity name cannot be null or empty.")
        {
            GeneratorType = nameof(EntityAnalyzer),
            EntityName = entity.Name
        };
    }
}
```

### Catching generation errors
```csharp
try
{
    _generator.Execute(context);
}
catch (GenerationException ex)
{
    // Log details and rethrow or handle appropriately
    Logger.LogError("Generation failed in {Generator}: {Message}", ex.GeneratorType, ex.Message);
    throw;
}
```

## Notes

*   **Immutability and Thread Safety:** These exception classes are intended to be used as transient error indicators. While the `GeneratorType` and `EntityName` properties have public setters for convenience during exception instantiation, modification after throwing is discouraged. They are thread-safe for concurrent read access once thrown.
*   **Serialization:** As standard .NET exceptions, these types support serialization. Ensure that any custom derived exceptions correctly implement the required serialization constructors if they introduce new state that must be preserved across application domains or process boundaries.
