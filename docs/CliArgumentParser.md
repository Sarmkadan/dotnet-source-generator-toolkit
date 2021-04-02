# CliArgumentParser

`CliArgumentParser` provides a structured mechanism for parsing command-line arguments into a strongly typed options class. It handles tokenization, binding, validation, and generation of help and version information, serving as the primary entry point for converting raw string arrays into application configuration objects.

## API

### `CliOptions Parse`

Parses the provided command-line arguments and populates an instance of the options type.

- **Parameters:**  
  Accepts the raw `string[]` arguments typically passed to `Main` and, optionally, a configuration delegate that customizes parser behavior (e.g., casing rules, prefix conventions).
- **Return Value:**  
  Returns a `CliOptions` object whose properties are bound to the corresponding argument values. The exact type is determined by the generic parameter supplied when constructing the parser.
- **Throws:**  
  Throws a `CliArgumentParserException` when required arguments are missing, when a value cannot be converted to the target property type, or when an unrecognized argument is encountered and strict mode is enabled.

### `string GetHelpMessage`

Generates a formatted help string describing all available arguments, their aliases, descriptions, and whether they are required.

- **Parameters:**  
  None.
- **Return Value:**  
  A `string` containing the complete help text, suitable for display in a console window. The layout includes argument names, shorthand notations, and descriptive text derived from attributes on the options class.
- **Throws:**  
  Does not throw under normal circumstances; returns an empty string if no metadata is available.

### `string GetVersionInfo`

Produces a version information string based on the assembly metadata associated with the options type or the entry assembly.

- **Parameters:**  
  None.
- **Return Value:**  
  A `string` containing the version number and, where available, additional build information such as commit hash or informational version attributes.
- **Throws:**  
  Does not throw; falls back to a default representation if version attributes cannot be read.

### `IEnumerable<string> Validate`

Validates the parsed options against all declared constraints and returns any violations found.

- **Parameters:**  
  Accepts the `CliOptions` instance returned by `Parse`.
- **Return Value:**  
  An `IEnumerable<string>` where each element is a human-readable error message describing a validation failure. An empty sequence indicates successful validation.
- **Throws:**  
  Throws `ArgumentNullException` if the options instance is `null`. Does not throw for validation failures themselves; those are communicated through the returned sequence.

## Usage

### Example 1: Basic Parse with Help Fallback

```csharp
var parser = new CliArgumentParser<MyOptions>();
MyOptions options;

try
{
    options = parser.Parse(args);
}
catch (CliArgumentParserException)
{
    Console.WriteLine(parser.GetHelpMessage());
    return 1;
}

foreach (var error in parser.Validate(options))
{
    Console.WriteLine($"Validation error: {error}");
    return 1;
}

// Proceed with validated options.
Console.WriteLine($"Input file: {options.InputFile}");
```

### Example 2: Version Display and Custom Configuration

```csharp
var parser = new CliArgumentParser<AppOptions>();

// Check for version flag early.
if (args.Length == 1 && args[0] is "--version" or "-v")
{
    Console.WriteLine(parser.GetVersionInfo());
    return 0;
}

var options = parser.Parse(args, config =>
{
    config.AllowUnknownArguments = false;
    config.CaseSensitive = true;
});

var errors = parser.Validate(options).ToList();
if (errors.Any())
{
    foreach (var error in errors)
    {
        Console.Error.WriteLine(error);
    }
    Console.Error.WriteLine(parser.GetHelpMessage());
    return 1;
}

// Application logic here.
```

## Notes

- **Thread Safety:** `CliArgumentParser` is designed to be used once per application invocation. Its methods are not guaranteed to be thread-safe for concurrent calls on the same instance; create separate instances if parsing must occur in parallel contexts.
- **Edge Cases:** When the arguments array is empty, `Parse` returns an options instance with all properties set to their default values. Validation may then report missing required options. `GetHelpMessage` and `GetVersionInfo` remain callable regardless of whether parsing has occurred.
- **Validation Order:** `Validate` should be called after `Parse`; calling it on an uninitialized or manually constructed `CliOptions` object may produce misleading results if internal state flags set during parsing are absent.
- **Custom Attributes:** The content returned by `GetHelpMessage` depends entirely on the metadata attributes decorating the options class. If no descriptions are provided, the help message will contain only argument names and type information.
