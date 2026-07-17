# EcommerceExampleValidation

`EcommerceExampleValidation` is a static utility class that provides centralized validation logic for e-commerce domain entities. It exposes overloaded methods to validate objects of different e-commerce types, returning lists of validation error messages, checking validity as a boolean, or throwing an exception when validation fails.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(/* e-commerce entity type */ entity)
```

Validates the provided e-commerce entity and returns a read-only list of validation error messages. If the entity is fully valid, the returned list is empty. This overload is specialized per entity type and examines type-specific business rules.

**Parameters:**
- `entity` — The e-commerce entity instance to validate. The specific type varies by overload.

**Returns:**
- `IReadOnlyList<string>` — A list of error messages describing validation failures. An empty list indicates a valid entity.

**Throws:**
- Does not throw. All validation outcomes are communicated through the returned list.

---

### IsValid

```csharp
public static bool IsValid(/* e-commerce entity type */ entity)
```

Determines whether the provided e-commerce entity passes all validation rules. This is a convenience method that delegates to `Validate` and checks whether the resulting error list is empty.

**Parameters:**
- `entity` — The e-commerce entity instance to check. The specific type varies by overload.

**Returns:**
- `bool` — `true` if the entity is valid (no validation errors); `false` otherwise.

**Throws:**
- Does not throw. All outcomes are represented by the boolean return value.

---

### EnsureValid

```csharp
public static void EnsureValid(/* e-commerce entity type */ entity)
```

Asserts that the provided e-commerce entity is valid, throwing an exception if any validation errors are detected. This method is intended for scenarios where invalid state must halt processing immediately.

**Parameters:**
- `entity` — The e-commerce entity instance to validate. The specific type varies by overload.

**Returns:**
- `void`

**Throws:**
- Throws an exception (typically a domain-specific validation exception) when the entity fails validation. The exception message aggregates the validation error details.

## Usage

### Example 1: Validating an entity and collecting errors

```csharp
var order = new Order
{
    Id = Guid.NewGuid(),
    CustomerEmail = "invalid-email",
    Items = new List<OrderItem>()
};

IReadOnlyList<string> errors = EcommerceExampleValidation.Validate(order);

if (errors.Count > 0)
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
    // Output: Validation error: CustomerEmail is not a valid email address.
    // Output: Validation error: Order must contain at least one item.
}
```

### Example 2: Guarding a method with EnsureValid

```csharp
public void ProcessPayment(PaymentInfo payment)
{
    // Fail fast if payment info is invalid
    EcommerceExampleValidation.EnsureValid(payment);

    // Proceed with payment processing
    PaymentGateway.Charge(payment);
}

// Elsewhere in the code:
try
{
    var payment = new PaymentInfo { Amount = -50m, Currency = "ZZZ" };
    ProcessPayment(payment);
}
catch (ValidationException ex)
{
    // Log and handle the invalid payment attempt
    Logger.LogError(ex.Message);
}
```

## Notes

- **Overload resolution:** Each overload of `Validate`, `IsValid`, and `EnsureValid` is bound to a specific e-commerce entity type at compile time. Passing an unsupported type results in a compilation error.
- **Empty error lists:** An empty list from `Validate` or a `true` result from `IsValid` guarantees the entity satisfies all rules defined for its type. Callers should treat an empty list as authoritative confirmation of validity.
- **Exception aggregation:** `EnsureValid` throws a single exception whose message typically aggregates all validation failures, allowing callers to see every violation in one diagnostic payload.
- **Thread safety:** All methods are static and operate solely on the entity instance passed as an argument. They do not mutate shared state. Concurrent calls with different entity instances are safe. Concurrent calls with the same mutable entity instance are not safe if that instance is being modified by another thread during validation.
- **Immutability of returned lists:** The `IReadOnlyList<string>` returned by `Validate` is a snapshot of errors at the time of the call. Subsequent mutations to the entity do not retroactively alter the returned list.
