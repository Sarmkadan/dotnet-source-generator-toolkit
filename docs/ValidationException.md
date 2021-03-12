# ValidationException

`ValidationException` is an exception type used in the `dotnet-source-generator-toolkit` to encapsulate and report validation failures encountered during source generation processes. It holds a collection of descriptive error messages, allowing consumers to handle multiple validation issues simultaneously.

## API

### Errors
`public List<string> Errors { get; }`

Gets the collection of validation error messages associated with this exception.

### ValidationException(string message, IEnumerable<string> errors)
Initializes a new instance of the `ValidationException` class with a specified error message and a collection of validation errors.

- **Parameters:**
    - `message`: The error message that explains the reason for the exception.
    - `errors`: A collection of validation error messages.

### ValidationException(string message, string error)
Initializes a new instance of the `ValidationException` class with a specified error message and a single validation error.

- **Parameters:**
    - `message`: The error message that explains the reason for the exception.
    - `error`: A single validation error message.

### ToString()
`public override string ToString()`

Returns a string representation of the exception, including the base exception information and the individual error messages contained in the `Errors` property if any exist.

- **Returns:** A string containing the formatted exception details.

## Usage

Example 1: Throwing with a collection of errors.

```csharp
var errors = new List<string> { "Invalid namespace.", "Missing required property." };
throw new ValidationException("Failed to validate source generation configuration.", errors);
```

Example 2: Catching and iterating through errors.

```csharp
try 
{
    // ... source generation logic ...
} 
catch (ValidationException ex) 
{
    Console.WriteLine(ex.Message);
    foreach (var error in ex.Errors) 
    {
        Console.WriteLine($"  - {error}");
    }
}
```

## Notes

- **Edge Cases:** If an empty or null collection is provided to the constructor, the `Errors` list will be empty, and `ToString()` will return only the base exception message.
- **Thread Safety:** The `ValidationException` class is intended to be used in synchronous workflows. While the exception object itself is immutable once thrown, the `Errors` list property is a mutable `List<string>`. If the exception instance is shared across threads, standard thread-safety practices for accessing mutable collections should be observed.
