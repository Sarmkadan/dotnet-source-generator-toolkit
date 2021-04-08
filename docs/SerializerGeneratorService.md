# SerializerGeneratorService

The `SerializerGeneratorService` is a utility service for generating serialization code and performing runtime serialization operations. It provides methods to generate serialization logic for types and to serialize objects to various formats including JSON and binary.

## API

### `SerializerGeneratorService`

The main service class that provides serialization generation and runtime serialization capabilities.

### `public async Task<IEnumerable<GenerationResult>> GenerateAllSerializersAsync(Type[] types)`

Generates serialization code for all specified types asynchronously.

- **Parameters**
  - `types`: An array of `Type` objects representing the types to generate serializers for.
- **Return value**
  - A task that represents the asynchronous operation. The task result contains an enumerable of `GenerationResult` objects, each representing the generation outcome for a type.
- **Exceptions**
  - Throws `ArgumentNullException` if `types` is `null`.
  - Throws `ArgumentException` if any type in `types` is `null`.

### `public async Task<GenerationResult> GenerateSerializerAsync(Type type)`

Generates serialization code for a single type asynchronously.

- **Parameters**
  - `type`: The `Type` to generate a serializer for.
- **Return value**
  - A task that represents the asynchronous operation. The task result contains a `GenerationResult` object representing the generation outcome.
- **Exceptions**
  - Throws `ArgumentNullException` if `type` is `null`.

### `public static string Serialize<T>(T value)`

Serializes the specified value to a JSON string.

- **Type parameters**
  - `T`: The type of the value to serialize.
- **Parameters**
  - `value`: The value to serialize.
- **Return value**
  - A JSON string representation of the value.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static JsonElement ToJsonElement<T>(T value)`

Converts the specified value to a `JsonElement`.

- **Type parameters**
  - `T`: The type of the value to convert.
- **Parameters**
  - `value`: The value to convert.
- **Return value**
  - A `JsonElement` representing the value.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static string Serialize<T>(T value, JsonSerializerOptions options)`

Serializes the specified value to a JSON string using the provided serializer options.

- **Type parameters**
  - `T`: The type of the value to serialize.
- **Parameters**
  - `value`: The value to serialize.
  - `options`: The `JsonSerializerOptions` to use during serialization.
- **Return value**
  - A JSON string representation of the value.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` or `options` is `null`.

### `public static byte[] Serialize<T>(T value)`

Serializes the specified value to a byte array.

- **Type parameters**
  - `T`: The type of the value to serialize.
- **Parameters**
  - `value`: The value to serialize.
- **Return value**
  - A byte array representing the serialized value.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static int GetSerializedSize<T>(T value)`

Calculates the size, in bytes, that the serialized representation of the specified value would occupy.

- **Type parameters**
  - `T`: The type of the value to measure.
- **Parameters**
  - `value`: The value to measure.
- **Return value**
  - The size, in bytes, of the serialized representation.
- **Exceptions**
  - Throws `ArgumentNullException` if `value` is `null`.

### `public static string ToCamelCase(string input)`

Converts the specified string to camelCase.

- **Parameters**
  - `input`: The string to convert.
- **Return value**
  - The camelCase representation of the input string.
- **Exceptions**
  - Throws `ArgumentNullException` if `input` is `null`.

## Usage

### Generating serializers for multiple types
