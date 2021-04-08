# ValidatorGeneratorService

`ValidatorGeneratorService` is a sealed class responsible for orchestrating the generation of validator source code from annotated types. It provides asynchronous methods to produce one or more `GenerationResult` instances, each representing the output of a single validator generation pass. The service also exposes a synchronous validation helper that can be used to check preconditions before generation.

## API

### `public sealed class ValidatorGeneratorService`

A sealed implementation of the validator generation pipeline. This class cannot be inherited, ensuring that the generation logic remains consistent and cannot be overridden externally.

### `public async Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync`

Generates validators for all eligible types discovered by the service. It processes each candidate type asynchronously and returns a collection of `GenerationResult` objects, one per generated validator.

- **Parameters:** None (relies on internal discovery mechanisms).
- **Returns:** `Task<IEnumerable<GenerationResult>>` — a task that, when awaited, yields a sequence of generation results. Each `GenerationResult` contains the generated source code and associated metadata.
- **Exceptions:** May throw if the underlying discovery or generation infrastructure encounters an unrecoverable error (e.g., invalid metadata, I/O failures during output writing).

### `public async Task<GenerationResult> GenerateValidatorAsync`

Generates a validator for a single specified type. This overload allows targeted generation when the caller already knows which type requires validation code.

- **Parameters:** The specific type to generate a validator for (inferred from the signature context).
- **Returns:** `Task<GenerationResult>` — a task that, when awaited, yields the generation result for the single validator.
- **Exceptions:** Throws if the provided type is not suitable for validator generation (e.g., lacks required annotations, is an open generic type, or is otherwise invalid). May also throw for infrastructure-level failures.

### `public bool Validate`

Performs a synchronous validation check to determine whether a given candidate is eligible for validator generation. This method does not produce any source code; it only returns a boolean indicating validity.

- **Parameters:** The candidate to validate (inferred from the signature context).
- **Returns:** `bool` — `true` if the candidate meets all preconditions for validator generation; otherwise `false`.
- **Exceptions:** Typically does not throw; invalid or malformed candidates simply result in a `false` return value.

## Usage

### Example 1: Generating validators for all discovered types

```csharp
var service = new ValidatorGeneratorService();

IEnumerable<GenerationResult> results = await service.GenerateAllValidatorsAsync();

foreach (var result in results)
{
    Console.WriteLine($"Generated validator: {result.FileName}");
    // Persist or compile result.SourceText as needed
}
```

### Example 2: Validating a candidate and generating a single validator

```csharp
var service = new ValidatorGeneratorService();
Type candidateType = typeof(MyAnnotatedClass);

if (service.Validate(candidateType))
{
    GenerationResult result = await service.GenerateValidatorAsync(candidateType);
    Console.WriteLine($"Validator generated: {result.FileName}");
}
else
{
    Console.WriteLine("Type is not eligible for validator generation.");
}
```

## Notes

- **Edge cases:** `Validate` returns `false` for types that lack the required attributes, are abstract, are open generic types, or contain unsupported member patterns. `GenerateValidatorAsync` will throw if called with such a type despite a prior `Validate` check, if the type’s metadata changes between the two calls.
- **Thread safety:** The service is designed to be used from a single synchronization context. Concurrent calls to `GenerateAllValidatorsAsync` or overlapping `GenerateValidatorAsync` invocations may lead to race conditions on internal state; external synchronization is required if shared across threads.
- **Sealed class:** The `sealed` modifier prevents derivation, guaranteeing that the generation pipeline cannot be altered by subclassing. All customization must occur through the types and attributes fed into the service, not through inheritance.
