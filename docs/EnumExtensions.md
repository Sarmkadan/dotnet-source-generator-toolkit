# EnumExtensions
Provides a set of extension methods for working with .NET enumeration types, offering description retrieval, value enumeration, parsing, flag testing, and CSV formatting in a type‑safe, generic manner.

## API
### GetDescription<T>(this T value) where T : struct, Enum
- **Purpose**: Returns the description associated with the enum value via a `System.ComponentModel.DescriptionAttribute`. If no such attribute is present, the enum member's name is returned.
- **Parameters**: 
  - `value`: The enum instance whose description is requested.
- **Return**: A `string` containing the description or the enum name.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enumeration type.

### GetValues<T>() where T : struct, Enum
- **Purpose**: Enumerates all defined constants of the enumeration type `T`.
- **Parameters**: None.
- **Return**: An `IEnumerable<T>` yielding each enum value.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enumeration type.

### TryParse<T>(this string input, out T result) where T : struct, Enum
- **Purpose**: Attempts to convert the supplied string to an enum value of type `T`, ignoring case.
- **Parameters**: 
  - `input`: The string to parse.
  - `result`: When the method returns `true`, contains the parsed enum value; otherwise, the default value of `T`.
- **Return**: `true` if `input` was successfully parsed; otherwise `false`.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enumeration type.
  - The method does **not** throw for a `null` input; it returns `false` and assigns `default(T)` to `result`.

### HasFlag<T>(this T value, T flag) where T : struct, Enum
- **Purpose**: Determines whether all bits that are set in `flag` are also set in `value`.
- **Parameters**: 
  - `value`: The enum value to test.
  - `flag`: The flag whose bits are checked.
- **Return**: `true` if every bit set in `flag` is also set in `value`; otherwise `false`.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enumeration type.
  - `ArgumentException` if `flag` is not a defined value of the enum `T`.

### ToCsv<T>(this IEnumerable<T> values) where T : struct, Enum
- **Purpose**: Produces a comma‑separated string representing the supplied enum values, using each value's description (via `DescriptionAttribute`) when available, otherwise the enum name.
- **Parameters**: 
  - `values`: A sequence of enum values to format.
- **Return**: A `string` containing the formatted values separated by commas. Returns an empty string if `values` is `null` or contains no elements.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enumeration type.
  - `ArgumentNullException` if `values` is `null`.

## Usage
```csharp
using System;
using System.ComponentModel;
using System.Linq;

public enum Priority
{
    [Description("Low importance")]
    Low = 1,
    [Description("Medium importance")]
    Medium = 2,
    [Description("High importance")]
    High = 4
}

// Example 1: Retrieve description and parse a string.
Priority p = Priority.Medium;
string description = p.GetDescription(); // Returns "Medium importance"

bool success = "high".TryParse(out Priority parsed);
if (success)
{
    Console.WriteLine(parsed.GetDescription()); // Prints "High importance"
}

// Example 2: Test flags and produce CSV.
Priority combined = Priority.Low | Priority.High;
bool hasHigh = combined.HasFlag(Priority.High); // true

string csv = new[] { Priority.Low, Priority.Medium, Priority.High }
                .ToCsv(); // Returns "Low importance,Medium importance,High importance"
```
```csharp
using System;
using System.Collections.Generic;

public enum FileAccess : uint
{
    Read    = 0x00000001,
    Write   = 0x00000002,
    Execute = 0x00000004
}

// Example: Combine flags, test, and export to CSV.
FileAccess rights = FileAccess.Read | FileAccess.Write;
bool canWrite = rights.HasFlag(FileAccess.Write); // true

IEnumerable<FileAccess> all = Enum.GetValues<FileAccess>();
string list = all.ToCsv(); // Returns "Read,Write,Execute"
```

## Notes
- All methods are constrained to `struct, Enum`, ensuring that misuse with non‑enum types results in a compile‑time error; no additional runtime type checks are required beyond those documented.
- The methods are stateless and rely only on the supplied arguments, making them thread‑safe for concurrent invocation.
- `GetDescription` returns the first `DescriptionAttribute` found; if multiple are present, only the first is used. Absence of any attribute falls back to `Enum.GetName`.
- `TryParse` performs a case‑insensitive match against enum names; it does not accept numeric strings unless they exactly match an enum constant's underlying value.
- `HasFlag` works correctly for flags enumerations; for non‑flags enums it behaves as an equality test (`value == flag`).
- `ToCsv` enumerates the supplied sequence once; a `null` sequence throws `ArgumentNullException`, while an empty sequence yields an empty string. The output order follows the enumeration order of the input sequence.
