# ValidationExceptionExtensions

The `ValidationExceptionExtensions` class provides a suite of extension methods for the `ValidationException` type, facilitating the manipulation, aggregation, and querying of validation error collections. These utilities simplify the handling of complex validation scenarios by offering a fluent and robust approach to merging exceptions, filtering error sets, and extracting diagnostic information for reporting or further processing.

## API

### Combine
Merges the errors from two `ValidationException` instances into a new `ValidationException`.

*   **Parameters:**
    *   `exception` (this): The base `ValidationException`.
    *   `other`: The `ValidationException` to merge with the base.
*   **Returns:** A new `ValidationException` containing the combined errors from both source exceptions.
*   **Throws:** `ArgumentNullException` if either `exception` or `other` is null.

### AddErrors
Appends a collection of errors to an existing `ValidationException`.

*   **Parameters:**
    *   `exception` (this): The target `ValidationException`.
    *   `errors`: An `IEnumerable` of key-value pairs representing property keys and their corresponding lists of error messages.
*   **Returns:** A new `ValidationException` containing the original errors plus the newly added errors.
*   **Throws:** `ArgumentNullException` if `exception` or `errors` is null.

### FilterErrors
Returns a new `ValidationException` containing only the errors that satisfy the provided predicate.

*   **Parameters:**
    *   `exception` (this): The source `ValidationException`.
    *   `predicate`: A `Func<string, bool>` used to filter the error keys.
*   **Returns:** A new `ValidationException` containing only the entries for which the predicate returns `true`.
*   **Throws:** `ArgumentNullException` if `exception` or `predicate` is null.

### ToErrorDictionary
Converts the errors contained within a `ValidationException` into a standard dictionary.

*   **Parameters:**
    *   `exception` (this): The source `ValidationException`.
*   **Returns:** A `Dictionary<string, List<string>>` where the keys are property names and the values are lists of error messages.
*   **Throws:** `ArgumentNullException` if `exception` is null.

### HasError
Determines whether the `ValidationException` contains any errors associated with a specific key.

*   **Parameters:**
    *   `exception` (this): The source `ValidationException`.
    *   `key`: The string key to check.
*   **Returns:** `true` if the exception contains errors for the specified key; otherwise, `false`.
*   **Throws:** `ArgumentNullException` if `exception` or `key` is null.

### GetFirstError
Retrieves the first error message associated with the specified key.

*   **Parameters:**
    *   `exception` (this): The source `ValidationException`.
    *   `key`: The string key to look up.
*   **Returns:** The first error message string if the key exists and has associated errors; otherwise, `null`.
*   **Throws:** `ArgumentNullException` if `exception` or `key` is null.

## Usage

### Example 1: Aggregating and Filtering Errors
This example demonstrates how to combine validation results from two distinct sources and filter out irrelevant errors before presenting them.

```csharp
try 
{
    // Assume these methods return ValidationException instances
    var ex1 = Validator.ValidateUser(user);
    var ex2 = Validator.ValidatePreferences(preferences);

    // Combine both exceptions
    var combinedEx = ex1.Combine(ex2);

    // Filter only to retain errors related to specific 'Contact' properties
    var filteredEx = combinedEx.FilterErrors(key => key.StartsWith("Contact"));

    throw filteredEx;
}
catch (ValidationException ex)
{
    var errorDict = ex.ToErrorDictionary();
    // Handle errorDict as needed
}
```

### Example 2: Querying Specific Error States
This example shows how to perform targeted checks on a validation exception to customize the user response.

```csharp
var ex = Validator.ValidateForm(form);

if (ex.HasError("Password"))
{
    var firstPasswordError = ex.GetFirstError("Password");
    Console.WriteLine($"Password error: {firstPasswordError}");
}

// Convert all errors to a dictionary for API response serialization
var responseErrors = ex.ToErrorDictionary();
```

## Notes

*   **Immutability:** Methods that return a new `ValidationException` do not modify the original exception instance. They return a new instance containing the resulting state, adhering to functional programming principles to prevent side effects.
*   **Thread Safety:** These extension methods are thread-safe, provided the `ValidationException` instances themselves are treated as immutable once created. They do not maintain any mutable internal state during the operation.
*   **Null Handling:** All methods strictly validate input parameters. Passing `null` to any required parameter will result in an `ArgumentNullException`.
*   **Performance:** While these methods are optimized, frequent creation of new `ValidationException` objects in a tight loop should be avoided if performance is critical.
