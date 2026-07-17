# BasicExampleJsonExtensions

Provides static extension methods for serializing and deserializing `BasicExample.User` and `BasicExample.UserDto` objects to and from JSON strings. The class acts as a lightweight wrapper around a JSON serializer (e.g., `System.Text.Json` or `Newtonsoft.Json`) and exposes both nullable and try-pattern deserialization overloads for each type.

## API

### `ToJson` (User)

```csharp
public static string ToJson(this BasicExample.User user)
```

Serializes a `BasicExample.User` instance to its JSON representation.

- **Parameters**  
  `user` – The `User` object to serialize. Must not be `null`.

- **Returns**  
  A `string` containing the JSON representation of the provided `User`.

- **Throws**  
  `ArgumentNullException` if `user` is `null`.  
  May throw any exception thrown by the underlying JSON serializer (e.g., `JsonException` if the object contains circular references or unsupported types).

---

### `FromJson` (User)

```csharp
public static BasicExample.User? FromJson(this string json)
```

Deserializes a JSON string into a `BasicExample.User` instance, or returns `null` if deserialization fails.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null`.

- **Returns**  
  A `BasicExample.User` instance if deserialization succeeds; otherwise, `null`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  Does **not** throw on malformed JSON – returns `null` instead.

---

### `TryFromJson` (User)

```csharp
public static bool TryFromJson(this string json, out BasicExample.User? result)
```

Attempts to deserialize a JSON string into a `BasicExample.User` instance without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null`.  
  `result` – When this method returns, contains the deserialized `User` object if successful, or `null` if deserialization failed.

- **Returns**  
  `true` if the JSON was successfully deserialized; otherwise, `false`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.

---

### `ToJson` (UserDto)

```csharp
public static string ToJson(this BasicExample.UserDto userDto)
```

Serializes a `BasicExample.UserDto` instance to its JSON representation.

- **Parameters**  
  `userDto` – The `UserDto` object to serialize. Must not be `null`.

- **Returns**  
  A `string` containing the JSON representation of the provided `UserDto`.

- **Throws**  
  `ArgumentNullException` if `userDto` is `null`.  
  May throw any exception thrown by the underlying JSON serializer.

---

### `FromJson` (UserDto)

```csharp
public static BasicExample.UserDto? FromJson(this string json)
```

Deserializes a JSON string into a `BasicExample.UserDto` instance, or returns `null` if deserialization fails.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null`.

- **Returns**  
  A `BasicExample.UserDto` instance if deserialization succeeds; otherwise, `null`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.  
  Does **not** throw on malformed JSON – returns `null` instead.

---

### `TryFromJson` (UserDto)

```csharp
public static bool TryFromJson(this string json, out BasicExample.UserDto? result)
```

Attempts to deserialize a JSON string into a `BasicExample.UserDto` instance without throwing exceptions.

- **Parameters**  
  `json` – The JSON string to deserialize. Must not be `null`.  
  `result` – When this method returns, contains the deserialized `UserDto` object if successful, or `null` if deserialization failed.

- **Returns**  
  `true` if the JSON was successfully deserialized; otherwise, `false`.

- **Throws**  
  `ArgumentNullException` if `json` is `null`.

## Usage

### Example 1: Serialize and deserialize a `User`

```csharp
using BasicExample;

var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };

// Serialize to JSON
string json = user.ToJson();
Console.WriteLine(json); // {"Id":1,"Name":"Alice","Email":"alice@example.com"}

// Deserialize back (nullable variant)
User? restored = json.FromJson();
if (restored != null)
{
    Console.WriteLine(restored.Name); // Alice
}
```

### Example 2: Safe deserialization with `TryFromJson`

```csharp
using BasicExample;

string invalidJson = "{ \"Id\": \"not-a-number\" }";

// Attempt to deserialize a UserDto
if (invalidJson.TryFromJson(out UserDto? dto))
{
    Console.WriteLine($"Deserialized: {dto.Id}");
}
else
{
    Console.WriteLine("Failed to deserialize JSON.");
}
```

## Notes

- **Null handling**: All methods throw `ArgumentNullException` when a `null` string or object is passed. The `FromJson` and `TryFromJson` methods treat a `null` input as invalid and will not attempt deserialization.
- **Malformed JSON**: The `FromJson` overloads return `null` on malformed JSON; they do not throw. The `TryFromJson` overloads return `false` and set `result` to `null`.
- **Thread safety**: The `BasicExampleJsonExtensions` class is stateless and contains only static methods. All methods are thread-safe as long as the underlying JSON serializer implementation is also thread-safe (which is the case for standard serializers like `System.Text.Json` and `Newtonsoft.Json`).
- **Type fidelity**: The JSON serialization respects the public properties of `BasicExample.User` and `BasicExample.UserDto`. Any properties not present in the target type during deserialization are silently ignored.
