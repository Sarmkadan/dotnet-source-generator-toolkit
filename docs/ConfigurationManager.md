# ConfigurationManager

The `ConfigurationManager` class provides a centralized store for key-value string configuration data used by the dotnet-source-generator-toolkit. It allows reading, writing, and querying configuration entries, as well as retrieving well-known directory paths (output, template, project root). This class is typically instantiated once per generator execution and is not intended for long-lived, concurrent access.

## API

### `public ConfigurationManager()`

Initializes a new instance of the `ConfigurationManager` with an empty configuration store.

### `public string GetValue(string key)`

Retrieves the configuration value associated with the specified key.

- **Parameters**  
  `key` – The configuration key to look up. Must not be `null` or empty.

- **Returns**  
  The string value associated with the key.

- **Throws**  
  `ArgumentNullException` if `key` is `null`.  
  `ArgumentException` if `key` is empty or consists only of whitespace.  
  `KeyNotFoundException` if the key does not exist in the configuration.

### `public string GetValue(string key, string defaultValue)`

Retrieves the configuration value associated with the specified key, or returns a default value if the key is not present.

- **Parameters**  
  `key` – The configuration key to look up. Must not be `null` or empty.  
  `defaultValue` – The value to return if the key is not found.

- **Returns**  
  The string value associated with the key, or `defaultValue` if the key does not exist.

- **Throws**  
  `ArgumentNullException` if `key` is `null`.  
  `ArgumentException` if `key` is empty or consists only of whitespace.

### `public void SetValue(string key, string value)`

Sets the configuration value for the specified key. If the key already exists, its value is overwritten.

- **Parameters**  
  `key` – The configuration key. Must not be `null` or empty.  
  `value` – The value to store. May be `null`.

- **Throws**  
  `ArgumentNullException` if `key` is `null`.  
  `ArgumentException` if `key` is empty or consists only of whitespace.

### `public bool HasKey(string key)`

Determines whether the configuration contains the specified key.

- **Parameters**  
  `key` – The configuration key to check. Must not be `null` or empty.

- **Returns**  
  `true` if the key exists; otherwise, `false`.

- **Throws**  
  `ArgumentNullException` if `key` is `null`.  
  `ArgumentException` if `key` is empty or consists only of whitespace.

### `public string GetOutputDirectory()`

Returns the configured output directory path. This value is typically set during initialization and may be derived from project settings.

- **Returns**  
  The absolute or relative path to the output directory. May be empty if not configured.

- **Throws**  
  This method does not throw.

### `public string GetTemplateDirectory()`

Returns the configured template directory path. This value is typically set during initialization and may be derived from project settings.

- **Returns**  
  The absolute or relative path to the template directory. May be empty if not configured.

- **Throws**  
  This method does not throw.

### `public string GetProjectRoot()`

Returns the root directory of the project being processed. This value is typically set during initialization.

- **Returns**  
  The absolute path to the project root directory. May be empty if not configured.

- **Throws**  
  This method does not throw.

### `public IReadOnlyDictionary<string, string> GetAllConfig()`

Returns a read-only snapshot of all current configuration key-value pairs.

- **Returns**  
  A dictionary containing all configuration entries. The returned dictionary is a copy of the internal state at the time of the call.

- **Throws**  
  This method does not throw.

## Usage

### Example 1: Basic configuration read and write

```csharp
var config = new ConfigurationManager();

// Set configuration values
config.SetValue("Namespace", "MyApp.Generated");
config.SetValue("DebugMode", "true");

// Read with default fallback
string outputDir = config.GetValue("OutputDirectory", "./Generated");
Console.WriteLine($"Output directory: {outputDir}");

// Check existence and read
if (config.HasKey("DebugMode"))
{
    string debug = config.GetValue("DebugMode");
    Console.WriteLine($"Debug mode: {debug}");
}
```

### Example 2: Using directory methods and GetAllConfig

```csharp
var config = new ConfigurationManager();

// Assume these are set externally (e.g., by the generator host)
// config.SetValue("OutputDirectory", @"C:\temp\output");
// config.SetValue("TemplateDirectory", @".\Templates");
// config.SetValue("ProjectRoot", @"C:\Projects\MyApp");

string output = config.GetOutputDirectory();
string templates = config.GetTemplateDirectory();
string root = config.GetProjectRoot();

Console.WriteLine($"Output: {output}");
Console.WriteLine($"Templates: {templates}");
Console.WriteLine($"Project root: {root}");

// Dump all configuration
foreach (var kvp in config.GetAllConfig())
{
    Console.WriteLine($"{kvp.Key} = {kvp.Value}");
}
```

## Notes

- **Key validation**: All methods that accept a `key` parameter throw `ArgumentNullException` for `null` and `ArgumentException` for empty or whitespace-only strings. This includes `GetValue`, `SetValue`, and `HasKey`.
- **Missing keys**: The single-parameter `GetValue` overload throws `KeyNotFoundException` if the key does not exist. Use the two-parameter overload with a default value to avoid exceptions.
- **Directory methods**: `GetOutputDirectory`, `GetTemplateDirectory`, and `GetProjectRoot` return whatever value was set during initialization. If no value was provided, they return an empty string. They do not validate whether the returned path exists.
- **Thread safety**: `ConfigurationManager` is not thread-safe. Concurrent reads and writes from multiple threads may result in inconsistent state or exceptions. If concurrent access is required, external synchronization (e.g., a lock) must be used.
- **Snapshot semantics**: `GetAllConfig` returns a copy of the internal dictionary at the time of the call. Subsequent modifications to the `ConfigurationManager` instance will not be reflected in the returned dictionary.
