# StringExtensionsTests

The `StringExtensionsTests` class provides a comprehensive suite of unit tests designed to validate the functionality of string manipulation and utility extension methods within the `dotnet-source-generator-toolkit`. These tests ensure the reliability, edge-case handling, and expected behaviors of string transformations required for robust C# code generation.

## API

*   **`ToPascalCase_WithUnderscoreDelimiters_ReturnsPascalCase`**: Verifies that input strings with underscore delimiters are correctly converted to PascalCase.
*   **`ToCamelCase_WithMultipleWords_ReturnsFirstWordLowercased`**: Validates that multi-word strings are converted to camelCase, ensuring the first word is correctly lowercased.
*   **`ToSnakeCase_WithPascalCaseString_InsertsUnderscoresBetweenWords`**: Confirms that PascalCase strings are converted to snake_case by inserting underscores between words.
*   **`Truncate_WhenStringExceedsMaxLength_AppendsEllipsis`**: Tests that strings exceeding the defined maximum length are truncated and appended with an ellipsis.
*   **`Repeat_WithCountThree_ReturnsRepeatedString`**: Checks that the string is repeated the specified number of times (three).
*   **`Repeat_WithCountZero_ReturnsEmpty`**: Confirms that repeating a string zero times results in an empty string.
*   **`IsNumeric_WithOnlyDigits_ReturnsTrue`**: Validates that strings containing only digit characters are correctly identified as numeric.
*   **`IsNumeric_WithLetters_ReturnsFalse`**: Verifies that strings containing alphabetical characters return false when checked for numeric values.
*   **`IsLettersOnly_WithOnlyLetters_ReturnsTrue`**: Ensures that strings consisting solely of alphabetical characters are correctly identified.
*   **`IsLettersOnly_WithDigits_ReturnsFalse`**: Validates that strings containing digits return false when checked for purely alphabetical content.
*   **`CountWord_WithOccurrences_ReturnsCount`**: Verifies that the occurrence count of a specified word within a string is calculated accurately.
*   **`Truncate_WhenEllipsisDisabled_ReturnsPlainTruncation`**: Tests that truncation occurs without appending an ellipsis when the ellipsis feature is explicitly disabled.
*   **`IsValidIdentifier_WithValidCSharpIdentifier_ReturnsTrue`**: Validates that strings conforming to C# identifier naming rules are identified as valid.
*   **`IsValidIdentifier_WhenStartsWithDigit_ReturnsFalse`**: Confirms that identifiers beginning with a digit are correctly rejected as invalid.
*   **`IsValidNamespace_WithDotSeparatedIdentifiers_ReturnsTrue`**: Validates that dot-separated identifier strings are correctly identified as valid namespace structures.
*   **`SanitizeForFileName_WithNullOrWhiteSpaceValue_ReturnsUnnamedFallback`**: Verifies that null or whitespace-only inputs result in a default "unnamed" fallback string when sanitized for file system compatibility.

## Usage

### Example 1: Running Tests via CLI
To execute these tests using the .NET CLI, navigate to the project directory and run the following command:

```bash
dotnet test
```

### Example 2: Sample Test Case Implementation
The following demonstrates how these tests are structured within the test project, utilizing standard testing framework conventions:

```csharp
using Xunit;
using Toolkit.Extensions; // Assume this is the extension namespace

public class StringExtensionsTests
{
    [Fact]
    public void ToPascalCase_WithUnderscoreDelimiters_ReturnsPascalCase()
    {
        // Arrange
        string input = "hello_world_example";
        string expected = "HelloWorldExample";

        // Act
        string result = input.ToPascalCase();

        // Assert
        Assert.Equal(expected, result);
    }
}
```

## Notes

*   **Edge Case Coverage:** These tests specifically target edge cases for string manipulation, including empty strings, null values (where applicable), and non-standard naming conventions (e.g., snake_case, PascalCase).
*   **Thread Safety:** The underlying extension methods being tested are designed to be stateless and thread-safe. The test suite itself is designed to run in parallel where supported by the test runner (e.g., xUnit).
*   **Environment Assumptions:** It is assumed that the test environment has the necessary .NET SDK installed to compile and execute the test project.
