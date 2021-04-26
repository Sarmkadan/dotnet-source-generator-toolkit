# StringValidator

Utility class providing validation and sanitization methods for C# identifiers, namespaces, and file names. Designed to enforce language specification constraints and common file system restrictions.

## API

### `public static bool IsValidIdentifier(string? value)`

Determines whether the provided string is a valid C# identifier according to the language specification.

- **Parameters**:
  - `value` – The string to validate; `null` is considered invalid.
- **Return value**:
  - `true` if the string is a valid C# identifier; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static bool IsValidNamespace(string? value)`

Determines whether the provided string is a valid C# namespace identifier according to the language specification.

- **Parameters**:
  - `value` – The string to validate; `null` is considered invalid.
- **Return value**:
  - `true` if the string is a valid C# namespace; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static bool IsNotEmpty(string? value)`

Determines whether the provided string is neither `null` nor empty.

- **Parameters**:
  - `value` – The string to validate.
- **Return value**:
  - `true` if the string is neither `null` nor empty; otherwise, `false`.
- **Exceptions**:
  - None.

### `public static bool IsMaxLength(string? value, int maxLength)`

Determines whether the provided string is neither `null`, empty, nor exceeds the specified maximum length.

- **Parameters**:
  - `value` – The string to validate.
  - `maxLength` – The maximum allowed length (must be non-negative).
- **Return value**:
  - `true` if the string is valid and within length bounds; otherwise, `false`.
- **Exceptions**:
  - Throws `ArgumentOutOfRangeException` if `maxLength` is negative.

### `public static string SanitizeForFileName(string value)`

Sanitizes a string to make it safe for use as a file name by replacing invalid characters with underscores.

- **Parameters**:
  - `value` – The string to sanitize; must not be `null`.
- **Return value**:
  - A sanitized string suitable for file system use.
- **Exceptions**:
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static string GetIdentifierError(string? value)`

Generates a human-readable error message describing why a string is not a valid C# identifier.

- **Parameters**:
  - `value` – The string to analyze; `null` is considered invalid.
- **Return value**:
  - An error message if the string is invalid; otherwise, an empty string.
- **Exceptions**:
  - None.

## Usage
