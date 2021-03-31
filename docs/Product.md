# Product

The `Product` class represents a product entity within the `dotnet-source-generator-toolkit` project. It stores core product data such as identifier, name, price, creation date, and availability status, and exposes computed display strings and conversion methods for formatting and mapping to a data transfer object (`ProductDto`). This class is typically used as a source model for code generation tasks.

## API

### Properties

| Member | Type | Description |
|--------|------|-------------|
| `Id` | `int` | Gets or sets the unique identifier of the product. |
| `Name` | `string` | Gets or sets the product name. |
| `Price` | `decimal` | Gets or sets the product price. |
| `CreatedDate` | `DateTime` | Gets or sets the date and time when the product was created. |
| `IsAvailable` | `bool` | Gets or sets a value indicating whether the product is currently available. |
| `PriceDisplay` | `string` | Gets a formatted string representation of the price (e.g., currency symbol and two decimal places). |
| `CreatedDateString` | `string` | Gets a formatted string representation of the creation date (e.g., "yyyy-MM-dd HH:mm:ss"). |
| `AvailabilityStatus` | `string` | Gets a human-readable string indicating availability (e.g., "Available" or "Unavailable"). |

### Methods

#### `Convert`

There are three overloads of the `Convert` method. Each overload returns a `string` and provides a different conversion context for the product. The exact parameters and behavior depend on the specific overload; consult the source code for details.

- **Return type**: `string`  
- **Exceptions**: None documented.

#### `MapProductToDto`

Maps the current `Product` instance to a new `ProductDto` object.

- **Signature**: `public ProductDto MapProductToDto()`  
- **Return type**: `ProductDto`  
- **Exceptions**: None documented.

## Usage

### Example 1: Creating and displaying a product

```csharp
var product = new Product
{
    Id = 101,
    Name = "Wireless Mouse",
    Price = 29.99m,
    CreatedDate = new DateTime(2025, 3, 15, 10, 30, 0),
    IsAvailable = true
};

Console.WriteLine(product.PriceDisplay);       // "$29.99"
Console.WriteLine(product.CreatedDateString);  // "2025-03-15 10:30:00"
Console.WriteLine(product.AvailabilityStatus); // "Available"
```

### Example 2: Mapping a product to a DTO

```csharp
var product = new Product
{
    Id = 42,
    Name = "USB-C Hub",
    Price = 49.99m,
    CreatedDate = DateTime.UtcNow,
    IsAvailable = false
};

ProductDto dto = product.MapProductToDto();
// dto now contains the mapped data from product
```

## Notes

- **Edge cases**:  
  - If `Name` is `null`, the computed display properties (`PriceDisplay`, `CreatedDateString`, `AvailabilityStatus`) still function, but `Name` itself will be `null`.  
  - A negative `Price` is allowed; `PriceDisplay` will format it with the currency symbol (e.g., "-$5.00").  
  - `CreatedDate` set to `DateTime.MinValue` will produce a `CreatedDateString` of "0001-01-01 00:00:00".  
  - `IsAvailable` set to `false` yields `AvailabilityStatus` as "Unavailable".

- **Thread safety**:  
  The `Product` class is not thread-safe. Its properties are mutable, and concurrent reads and writes to the same instance can lead to inconsistent state. If used in a multi-threaded context, external synchronization (e.g., locking or immutable copies) is required.
