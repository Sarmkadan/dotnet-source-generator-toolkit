# BasicExample

The `BasicExample` class represents a simple user entity within the `dotnet-source-generator-toolkit` project. It exposes basic identity and contact properties, along with asynchronous methods to retrieve or persist a user record. This type is intended for demonstration and testing of source-generated code patterns.

## API

### Properties

| Name | Type | Description |
|------|------|-------------|
| `Id` | `int` | The unique identifier for the user. |
| `Email` | `string` | The email address of the user. |
| `FirstName` | `string` | The user's first name. |
| `LastName` | `string` | The user's last name. |
| `CreatedAt` | `DateTime` | The timestamp when the user record was created. |
| `IsActive` | `bool` | Indicates whether the user account is currently active. |

### Methods

#### `GetUserAsync`

```csharp
public async Task<UserDto?> GetUserAsync()
```

- **Purpose**: Retrieves a user DTO based on the current instance's `Id`. Returns `null` if no matching user is found.
- **Parameters**: None.
- **Returns**: A `Task<UserDto?>` that resolves to the user data transfer object, or `null` if the user does not exist.
- **Throws**: No documented exceptions; however, underlying data access may throw `InvalidOperationException` if the `Id` is invalid or the data source is unavailable.

#### `CreateUserAsync`

```csharp
public async Task<User> CreateUserAsync()
```

- **Purpose**: Creates a new user record using the current instance's property values (`Id`, `Email`, `FirstName`, `LastName`, `CreatedAt`, `IsActive`).
- **Parameters**: None.
- **Returns**: A `Task<User>` that resolves to the newly created `User` object.
- **Throws**: May throw `ArgumentException` if required properties (e.g., `Email`) are null or empty, or if the `Id` conflicts with an existing record.

## Usage

### Example 1: Creating and retrieving a user

```csharp
var example = new BasicExample
{
    Id = 42,
    Email = "alice@example.com",
    FirstName = "Alice",
    LastName = "Johnson",
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

User createdUser = await example.CreateUserAsync();
Console.WriteLine($"Created user: {createdUser.FirstName} {createdUser.LastName}");

UserDto? retrieved = await example.GetUserAsync();
if (retrieved != null)
{
    Console.WriteLine($"Retrieved user email: {retrieved.Email}");
}
```

### Example 2: Handling a missing user

```csharp
var example = new BasicExample
{
    Id = 999,
    Email = "missing@example.com",
    FirstName = "Ghost",
    LastName = "User",
    CreatedAt = DateTime.UtcNow,
    IsActive = false
};

UserDto? result = await example.GetUserAsync();
if (result == null)
{
    Console.WriteLine("User not found. Consider creating it first.");
}
else
{
    Console.WriteLine($"Found user: {result.FirstName}");
}
```

## Notes

- **Edge Cases**:  
  - Setting `Id` to a non-positive value may cause `GetUserAsync` or `CreateUserAsync` to throw or return unexpected results.  
  - `Email` should be a valid email format; otherwise `CreateUserAsync` may reject the operation.  
  - `CreatedAt` is typically set to `DateTime.UtcNow`; using `DateTimeKind.Local` may lead to timezone inconsistencies.  
  - `IsActive` has no direct validation but may affect business logic in downstream consumers.

- **Thread Safety**:  
  Instances of `BasicExample` are not thread-safe. Concurrent reads and writes to its properties or concurrent calls to `GetUserAsync` / `CreateUserAsync` on the same instance may produce race conditions. Synchronize access externally if used in a multi-threaded context.
