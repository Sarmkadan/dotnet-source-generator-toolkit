# ICodeFormatterService

Provides a set of utilities for manipulating and validating C# source code strings, such as formatting, header insertion, whitespace trimming, line‑ending normalization, and syntax verification.

## API
### CodeFormatterService
**Purpose**  
Obtains an instance of the concrete formatter implementation that backs this interface.

**Parameters**  
None.

**Return value**  
A `CodeFormatterService` object that implements `ICodeFormatterService`.

**Exceptions**  
None.

### FormatCode
**Purpose**  
Applies standard C# formatting rules to the supplied source code.

**Parameters**  
- `source`: The C# code to format.

**Return value**  
A string containing the formatted code.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

### AddFileHeader
**Purpose**  
Prepends a file header comment to the supplied source code.

**Parameters**  
- `source`: The C# code to which the header will be added.  
- `header`: The header text to prepend (typically including comment delimiters).

**Return value**  
A string with the header inserted at the beginning of `source`.

**Exceptions**  
- `ArgumentNullException` if either `source` or `header` is `null`.

### TrimWhitespace
**Purpose**  
Removes leading and trailing whitespace from each line of the supplied source code.

**Parameters**  
- `source`: The C# code to process.

**Return value**  
A string with whitespace trimmed from the start and end of every line.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

### NormalizeLineEndings
**Purpose**  
Converts all line endings in the supplied source code to the platform‑default newline sequence (`Environment.NewLine`).

**Parameters**  
- `source`: The C# code to normalize.

**Return value**  
A string with uniform line endings.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

### ValidateSyntax
**Purpose**  
Checks whether the supplied source code is syntactically valid C#.

**Parameters**  
- `source`: The C# code to validate.

**Return value**  
`true` if the code parses without errors; otherwise `false`.

**Exceptions**  
- `ArgumentNullException` if `source` is `null`.

## Usage
```csharp
ICodeFormatterService formatter = new CodeFormatterService();

string raw = @"    public class Foo {    }";
string formatted = formatter.FormatCode(raw);
// formatted => "public class Foo { }"

string withHeader = formatter.AddFileHeader(formatted, "// Copyright 2025");
// withHeader => "// Copyright 2025\r\npublic class Foo { }"

string trimmed = formatter.TrimWhitespace(withHeader);
// trimmed => "// Copyright 2025\r\npublic class Foo { }"

string normalized = formatter.NormalizeLineEndings(trimmed);
// normalized => "// Copyright 2025\npublic class Foo { }"

bool isValid = formatter.ValidateSyntax(normalized);
// isValid => true
```
```csharp
// Example using the factory method to obtain a service instance
ICodeFormatterService service = formatter.CodeFormatterService();
string safe = service.FormatCode(@"using System;");
```

## Notes
- All methods that accept a `source` argument treat a `null` value as an error and throw `ArgumentNullException`.  
- The `ValidateSyntax` method does not throw on invalid syntax; it simply returns `false`.  
- The formatter does not modify the original string; it returns a new instance.  
- Implementations are expected to be stateless and therefore thread‑safe for concurrent calls.  
- The `CodeFormatterService` member is intended to provide access to the underlying concrete type when additional members not exposed through the interface are required.  
- Line‑ending normalization uses `Environment.NewLine` as the target format; callers requiring a specific sequence should apply further processing after invoking this method.
