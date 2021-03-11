# ConfigurationManagerExtensions

`ConfigurationManagerExtensions` provides a set of extension methods for the `IConfiguration` interface, designed to simplify the retrieval of configuration settings within .NET applications. These methods offer streamlined handling of missing keys, default value injection, and type-safe conversion, reducing boilerplate code when accessing configuration data.

## API

### GetValueOrDefault
Retrieves a string value associated with the specified key.
*   **Parameters:**
    *   `key` (string): The configuration key to retrieve.
    *   `defaultValue` (string, optional): The value to return if the key is not found. Defaults to `null`.
*   **Returns:** The value associated with the key, or `defaultValue` if the key does not exist.

### GetRequiredValue
Retrieves a string value associated with the specified key.
*   **Parameters:**
    *   `key` (string): The configuration key to retrieve.
*   **Returns:** The string value associated with the key.
*   **Throws:** `InvalidOperationException` if the key is not found or the value is null/empty.

### GetValue&lt;T&gt;
Retrieves a value associated with the specified key and converts it to the requested type `T`.
*   **Parameters:**
    *   `key` (string): The configuration key to retrieve.
*   **Returns:** The value associated with the key, converted to type `T`.
*   **Throws:** `InvalidOperationException` if the key is not found, or `InvalidCastException` if the value cannot be converted to `T`.

### GetValueOrDefault&lt;T&gt;
Retrieves a value associated with the specified key and converts it to the requested type `T`.
*   **Parameters:**
    *   `key` (string): The configuration key to retrieve.
    *   `defaultValue` (T, optional): The value to return if the key is not found or conversion fails. Defaults to `default(T)`.
*   **Returns:** The converted value associated with the key, or `defaultValue` if the key does not exist or conversion fails.

### GetAllConfig
Retrieves all current configuration settings as a flat read-only dictionary.
*   **Returns:** An `IReadOnlyDictionary<string, string>` containing all configuration keys and their corresponding values.

## Usage

### Basic String Retrieval
```csharp
// Retrieve a required connection string
string connectionString = configuration.GetRequiredValue("ConnectionStrings:DefaultConnection");

// Retrieve an optional feature flag with a default
bool enableFeature = configuration.GetValueOrDefault("Features:NewUI", "false") == "true";
```

### Type-Safe Configuration Retrieval
```csharp
// Retrieve a typed integer setting
int timeout = configuration.GetValue<int>("Settings:TimeoutSeconds");

// Retrieve a typed setting with a fallback
int maxRetries = configuration.GetValueOrDefault("Settings:MaxRetries", 3);
```

## Notes

*   **Thread Safety:** These extension methods rely on the underlying `IConfiguration` implementation. In standard .NET implementations, `IConfiguration` is thread-safe for reading.
*   **Missing Keys:** Methods suffixed with `OrDefault` will gracefully handle missing configuration keys by returning the provided default value. Non-suffixed methods (`GetRequiredValue`, `GetValue<T>`) assume the key exists and will throw exceptions if the lookup fails, enforcing strict configuration requirements.
*   **Type Conversion:** `GetValue<T>` and `GetValueOrDefault<T>` utilize the underlying configuration binding mechanisms. If the configuration value cannot be successfully parsed or cast to type `T`, an exception will be thrown. Always ensure the configuration format aligns with the expected target type.
