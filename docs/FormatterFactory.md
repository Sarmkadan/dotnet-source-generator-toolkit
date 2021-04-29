# FormatterFactory

`FormatterFactory` is a registry and factory for output formatters. It allows consumers to register formatter implementations keyed by format name, then create an `IOutputFormatter` instance for a requested format. It also provides introspection methods to enumerate all available formats and to check whether a specific format has been registered.

## API

### `public FormatterFactory()`

Constructs a new, empty `FormatterFactory` instance. No formatters are registered by default.

- **Parameters:** none.
- **Returns:** a new `FormatterFactory`.
- **Throws:** nothing.

### `public void Register(string format, Func<IOutputFormatter> factory)`

Registers a factory delegate for the specified format. If a factory for the same format name already exists, it is overwritten.

- **Parameters:**
  - `format` — The name of the format (e.g. `"json"`, `"xml"`). Must not be `null` or empty.
  - `factory` — A delegate that, when invoked, returns a new `IOutputFormatter` instance. Must not be `null`.
- **Returns:** nothing.
- **Throws:**
  - `ArgumentNullException` if `format` is `null`.
  - `ArgumentException` if `format` is empty or consists only of whitespace.
  - `ArgumentNullException` if `factory` is `null`.

### `public IOutputFormatter Create(string format)`

Creates and returns an `IOutputFormatter` instance for the specified format by invoking the registered factory delegate.

- **Parameters:**
  - `format` — The name of the desired format. Must not be `null` or empty.
- **Returns:** a new `IOutputFormatter` instance.
- **Throws:**
  - `ArgumentNullException` if `format` is `null`.
  - `ArgumentException` if `format` is empty or consists only of whitespace.
  - `KeyNotFoundException` if no factory has been registered for the given format.

### `public IEnumerable<string> GetAvailableFormats()`

Returns an enumeration of all format names currently registered in the factory. The order is not guaranteed.

- **Parameters:** none.
- **Returns:** an `IEnumerable<string>` of format names.
- **Throws:** nothing.

### `public bool IsFormatAvailable(string format)`

Checks whether a factory has been registered for the specified format.

- **Parameters:**
  - `format` — The format name to check. Must not be `null`.
- **Returns:** `true` if the format is registered; otherwise `false`.
- **Throws:**
  - `ArgumentNullException` if `format` is `null`.

## Usage

### Example 1: Basic registration and creation

```csharp
var factory = new FormatterFactory();

// Register formatters
factory.Register("json", () => new JsonOutputFormatter());
factory.Register("xml",  () => new XmlOutputFormatter());

// Check availability before creating
if (factory.IsFormatAvailable("json"))
{
    IOutputFormatter formatter = factory.Create("json");
    formatter.Write(outputStream, data);
}
```

### Example 2: Enumerating formats and dynamic selection

```csharp
var factory = new FormatterFactory();
factory.Register("yaml", () => new YamlOutputFormatter());
factory.Register("csv",  () => new CsvOutputFormatter());

// List all available formats to the user
foreach (string fmt in factory.GetAvailableFormats())
{
    Console.WriteLine($"- {fmt}");
}

// Create formatter based on runtime configuration
string requestedFormat = Configuration.Get("output.format");
if (factory.IsFormatAvailable(requestedFormat))
{
    IOutputFormatter formatter = factory.Create(requestedFormat);
    formatter.Write(stream, payload);
}
else
{
    throw new NotSupportedException($"Format '{requestedFormat}' is not available.");
}
```

## Notes

- **Overwrite behavior:** Calling `Register` with an already-registered format name replaces the existing factory delegate without warning. If multiple registrations for the same format are possible in your workflow, consider checking `IsFormatAvailable` first to avoid unintentional overwrites.
- **Case sensitivity:** Format names are treated as case-sensitive strings. `"JSON"` and `"json"` are distinct entries. Normalize casing if case-insensitive matching is desired.
- **Thread safety:** `FormatterFactory` is not thread-safe by default. Concurrent calls to `Register` and `Create` or `IsFormatAvailable` may cause race conditions. If shared across threads, synchronize externally or register all formatters during a single-threaded initialization phase before concurrent reads begin.
- **Lazy instantiation:** `Create` invokes the factory delegate each time it is called, returning a new instance. The factory does not cache or reuse formatter instances. If formatters are expensive to construct, consider registering a factory that returns a shared or pooled instance.
- **Empty state:** Immediately after construction, `GetAvailableFormats` returns an empty sequence, `IsFormatAvailable` returns `false` for any input, and `Create` throws `KeyNotFoundException` for any format.
