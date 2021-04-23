# StringExtensions
Provides a collection of pure‑function extension methods for `System.String` that perform common text transformations and inspections without modifying the original instance.

## API
### ToPascalCase
**Purpose** – Converts the input string to PascalCase (each word capitalized, no separators).  
**Parameters**  
- `input`: The string to convert.  
**Return value** – A new string in PascalCase; returns an empty string if `input` is empty or consists only of whitespace.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### ToCamelCase
**Purpose** – Converts the input string to camelCase (first word lower‑cased, subsequent words capitalized, no separators).  
**Parameters**  
- `input`: The string to convert.  
**Return value** – A new string in camelCase; returns an empty string for empty or whitespace‑only input.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### ToSnakeCase
**Purpose** – Converts the input string to snake_case (words lower‑cased and separated by underscores).  
**Parameters**  
- `input`: The string to convert.  
**Return value** – A new string in snake_case; returns an empty string for empty or whitespace‑only input.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### ToKebabCase
**Purpose** – Converts the input string to kebab-case (words lower‑cased and separated by hyphens).  
**Parameters**  
- `input`: The string to convert.  
**Return value** – A new string in kebab-case; returns an empty string for empty or whitespace‑only input.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`.

### Repeat
**Purpose** – Produces a new string consisting of the input repeated a specified number of times.  
**Parameters**  
- `input`: The string to repeat.  
- `count`: The number of repetitions; must be zero or positive.  
**Return value** – A new string containing `input` concatenated `count` times; returns an empty string if `count` is zero or `input` is empty.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`. Throws `ArgumentOutOfRangeException` if `count` is negative.

### Truncate
**Purpose** – Shortens the input string to a maximum length, optionally appending an ellipsis.  
**Parameters**  
- `input`: The string to truncate.  
- `maxLength`: The maximum allowed length of the result; must be non‑negative.  
- `ellipsis` (optional): The string to append when truncation occurs; defaults to `"…"`.  
**Return value** – If `input.Length <= maxLength`, returns `input` unchanged; otherwise returns the first `maxLength - ellipsis.Length` characters followed by `ellipsis`. Returns an empty string when `maxLength` is zero.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`. Throws `ArgumentOutOfRangeException` if `maxLength` is negative.

### IsNumeric
**Purpose** – Determines whether the input string represents a numeric value (integral or floating‑point) using invariant culture rules.  
**Parameters**  
- `input`: The string to test.  
**Return value** – `true` if `input` can be parsed as a number; otherwise `false`. Returns `false` for `null`, empty, or whitespace‑only strings.  
**Exceptions** – None.

### IsLettersOnly
**Purpose** – Determines whether the input string consists solely of Unicode letter characters.  
**Parameters**  
- `input`: The string to test.  
**Return value** – `true` if every character in `input` is a letter; otherwise `false`. Returns `false` for `null` or empty strings.  
**Exceptions** – None.

### CountWord
**Purpose** – Counts the number of words in the input string, where words are delimited by whitespace.  
**Parameters**  
- `input`: The string to analyze.  
**Return value** – The number of words; returns `0` for `null`, empty, or whitespace‑only strings.  
**Exceptions** – None.

### GetLines
**Purpose** – Splits the input string into an array of lines, removing line‑break characters.  
**Parameters**  
- `input`: The string to split.  
**Return value** – An array of strings, each representing a line. Returns an empty array for `null` or empty input. Line breaks recognized are `\r\n`, `\n`, and `\r`.  
**Exceptions** – None.

### Indent
**Purpose** – Indents each line of the input string by prepending a specified indentation string.  
**Parameters**  
- `input`: The string to indent.  
- `indentation`: The string to prepend to each line; typically spaces or tabs.  
**Return value** – A new string where each line begins with `indentation`. Returns an empty string if `input` is empty.  
**Exceptions** – Throws `ArgumentNullException` if `input` or `indentation` is `null`.

### Wrap
**Purpose** – Wraps the input string so that no line exceeds a specified width, inserting line breaks as needed.  
**Parameters**  
- `input`: The string to wrap.  
- `width`: The maximum line width; must be positive.  
**Return value** – A new string with line breaks inserted to respect the width limit. Words longer than `width` are placed on their own line and may exceed the limit. Returns an empty string for empty input.  
**Exceptions** – Throws `ArgumentNullException` if `input` is `null`. Throws `ArgumentOutOfRangeException` if `width` is less than or equal to zero.

## Usage
```csharp
using static MyNamespace.StringExtensions;

string title = "hello_world-example";
string pascal = title.ToPascalCase();   // "HelloWorldExample"
string kebab  = title.ToKebabCase();    // "hello-world-example"

string paragraph = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";
string wrapped   = paragraph.Wrap(40);
// Result:
// Lorem ipsum dolor sit amet,
// consectetur adipiscing elit.
```

```csharp
string[] lines = GetLines("First line\nSecond line\r\nThird line\rFourth line");
foreach (var line in lines)
{
    Console.WriteLine(Indent(line, "    "));
}
// Output:
//     First line
//     Second line
//     Third line
//     Fourth line
```

## Notes
- All methods are stateless and thread‑safe; they depend only on their input parameters and do not modify any shared state.  
- Null arguments are explicitly guarded and will raise `ArgumentNullException` where noted; empty strings are treated as valid input and produce sensible defaults (empty result, false boolean, zero count, etc.).  
- Word counting and line splitting treat any Unicode whitespace character as a delimiter; consecutive delimiters are considered to separate empty words, which are not counted.  
- Case conversion methods (`ToPascalCase`, `ToCamelCase`, `ToSnakeCase`, `ToKebabCase`) operate on the invariant culture and do not perform locale‑specific rules; they simply split on non‑letter/digit boundaries and apply the requested casing.  
- `IsNumeric` accepts formats recognized by `double.TryParse` with `NumberStyles.Any` and `CultureInfo.InvariantCulture`; it does not consider currency symbols or group separators unless they are part of the invariant numeric pattern.  
- `Wrap` preserves existing line breaks in the input; it treats the input as a sequence of words separated by whitespace and inserts breaks only when the accumulated line length would exceed the specified width. Words longer than the width are not hyphenated.  
- `Repeat` and `Truncate` return the original string unchanged when the operation would have no effect (repeat count zero, truncation length greater or equal to the input length).  
- No allocations are made beyond the necessary return strings or arrays; callers should consider pooling or reuse if these methods are invoked in tight performance‑critical loops.
