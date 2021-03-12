# GenerationStartedEventExtensions

`GenerationStartedEventExtensions` provides a set of utility extension methods for the `GenerationStartedEvent` class, facilitating common operations such as cloning, querying, and modifying generator type information within the context of source generator execution events. These methods simplify interactions with `GenerationStartedEvent` instances, providing streamlined access to diagnostic and state-management functionality required during source generation analysis.

## API

### DeepCopy

Creates a deep copy of a `GenerationStartedEvent` instance to prevent unintended side effects on the original event.

*   **Parameters:**
    *   `source`: The `GenerationStartedEvent` instance to clone.
*   **Returns:** A new `GenerationStartedEvent` instance containing a deep copy of the data from the source.
*   **Throws:** `ArgumentNullException` if `source` is null.

### HasGeneratorType

Checks if the `GenerationStartedEvent` contains information regarding a specific generator type.

*   **Parameters:**
    *   `source`: The `GenerationStartedEvent` instance to query.
    *   `generatorType`: The `Type` to check for.
*   **Returns:** `true` if the event contains the specified generator type; otherwise, `false`.
*   **Throws:** `ArgumentNullException` if `source` or `generatorType` is null.

### ToSummaryString

Generates a concise string representation of the `GenerationStartedEvent` for logging and diagnostic purposes.

*   **Parameters:**
    *   `source`: The `GenerationStartedEvent` instance to summarize.
*   **Returns:** A string containing a summary of the event details.
*   **Throws:** `ArgumentNullException` if `source` is null.

### AddGeneratorType

Adds a generator type to the `GenerationStartedEvent` instance.

*   **Parameters:**
    *   `source`: The `GenerationStartedEvent` instance to modify.
    *   `generatorType`: The `Type` of the generator to add.
*   **Returns:** The `GenerationStartedEvent` instance with the added generator type.
*   **Throws:** `ArgumentNullException` if `source` or `generatorType` is null.

## Usage

```csharp
// Example 1: Cloning and modifying an event
GenerationStartedEvent originalEvent = GetEvent();
GenerationStartedEvent clonedEvent = originalEvent.DeepCopy();

clonedEvent.AddGeneratorType(typeof(MySourceGenerator));
```

```csharp
// Example 2: Checking for a type and logging a summary
GenerationStartedEvent evt = GetEvent();
if (evt.HasGeneratorType(typeof(MySourceGenerator)))
{
    Console.WriteLine(evt.ToSummaryString());
}
```

## Notes

*   **Thread Safety:** These extension methods do not guarantee thread safety for the `GenerationStartedEvent` instance itself. If the underlying `GenerationStartedEvent` is mutable and accessed concurrently, appropriate locking mechanisms should be employed by the caller to ensure data integrity.
*   **Null Handling:** All methods strictly enforce non-null arguments and will throw an `ArgumentNullException` if the `source` instance or any required parameter is null.
