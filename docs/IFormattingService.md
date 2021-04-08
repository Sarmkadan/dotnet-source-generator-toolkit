# IFormattingService

The `IFormattingService` interface defines a contract for services that provide code formatting capabilities in C# projects. It allows configuring and applying formatting rules to source code, including indentation, line breaks, whitespace handling, and method organization.

## API

### `FormattingService`

The default implementation of `IFormattingService`. This class provides concrete functionality for formatting C# code according to specified rules.

### `string FormatCode(string code)`

Formats the provided C# source code according to the current formatting rules.

- **Parameters**:
  - `code` (string): The C# source code to format.
- **Return value**: The formatted C# source code as a string.
- **Exceptions**: Throws `ArgumentNullException` if `code` is `null`.

### `FormattingRules GetRules()`

Retrieves the current formatting rules being applied by the service.

- **Return value**: A `FormattingRules` object containing the current formatting configuration.

### `void SetRules(FormattingRules rules)`

Updates the formatting rules used by the service.

- **Parameters**:
  - `rules` (`FormattingRules`): The new formatting rules to apply.
- **Exceptions**: Throws `ArgumentNullException` if `rules` is `null`.

### `int IndentSize`

Gets or sets the number of spaces used for each indentation level. Must be a positive integer.

- **Exceptions**: Throws `ArgumentOutOfRangeException` if set to a value less than 1.

### `bool UseTabs`

Gets or sets whether indentation should use tab characters instead of spaces.

### `int LineLength`

Gets or sets the maximum line length before line wrapping is applied. Must be a positive integer.

- **Exceptions**: Throws `ArgumentOutOfRangeException` if set to a value less than 1.

### `bool AddBlankLineBeforeMethod`

Gets or sets whether a blank line should be added before each method declaration.

### `bool RemoveTrailingWhitespace`

Gets or sets whether trailing whitespace should be removed from lines.

## Usage

```csharp
// Example 1: Basic formatting with default rules
var formattingService = new FormattingService();
string unformattedCode = "public class Example{public void Method(){Console.WriteLine(\"Hello\");}}";
string formattedCode = formattingService.FormatCode(unformattedCode);

// Example 2: Custom formatting configuration
var rules = new FormattingRules
{
    IndentSize = 4,
    UseTabs = false,
    LineLength = 120,
    AddBlankLineBeforeMethod = true,
    RemoveTrailingWhitespace = true
};
var customFormattingService = new FormattingService();
customFormattingService.SetRules(rules);
string customFormattedCode = customFormattingService.FormatCode(unformattedCode);
```

## Notes

- The service is not thread-safe. Concurrent access to the same `FormattingService` instance requires external synchronization.
- Changing `IndentSize` or `LineLength` while formatting is in progress may lead to inconsistent results.
- The `FormatCode` method does not modify the original `FormattingRules` instance; it uses a snapshot of the current rules at the time of invocation.
- Invalid rule values (e.g., negative `IndentSize`) will throw exceptions rather than being silently corrected.
