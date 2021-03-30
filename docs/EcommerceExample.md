# EcommerceExample

The `EcommerceExample` type is a container for three related model classes—`Product`, `Order`, and `OrderItem`—that represent the core entities of a simple e‑commerce domain. These classes are designed to be used with object‑relational mapping (ORM) tools or as data transfer objects. Each class exposes public auto‑implemented properties that map directly to database columns or API payloads.

## API

### `EcommerceExample.Product`

Represents a product in the catalog.

| Property | Type | Purpose |
|----------|------|---------|
| `Id` | `int` | Primary key. Uniquely identifies the product. |
| `Sku` | `string` | Stock‑keeping unit code. |
| `Name` | `string` | Display name of the product. |
| `Description` | `string` | Detailed description of the product. |
| `Price` | `decimal` | Current selling price. |
| `StockQuantity` | `int` | Number of units available in inventory. |
| `CategoryId` | `int` | Foreign key to the product’s category. |
| `CreatedAt` | `DateTime` | Timestamp when the product was first added. |
| `ModifiedAt` | `DateTime?` | Timestamp of the last modification, or `null` if never modified. |
| `IsActive` | `bool` | Indicates whether the product is currently available for sale. |

All properties are read/write. No exceptions are thrown by the property accessors themselves; validation (e.g., non‑negative price) is expected to be enforced by the calling code or by a separate validation layer.

### `EcommerceExample.Order`

Represents a customer order.

| Property | Type | Purpose |
|----------|------|---------|
| `Id` | `int` | Primary key. Uniquely identifies the order. |
| `CustomerId` | `int` | Foreign key to the customer who placed the order. |
| `OrderDate` | `DateTime` | Date and time when the order was created. |
| `TotalAmount` | `decimal` | Sum of all line‑item totals for this order. |
| `Status` | `string` | Current order status (e.g., "Pending", "Shipped", "Delivered"). |
| `ShippingAddress` | `string` | Full shipping address for the order. |
| `Items` | `List<OrderItem>` | Collection of line items belonging to this order. |

All properties are read/write. The `Items` property can be `null` if not initialized; no exceptions are thrown by the getter or setter.

### `EcommerceExample.OrderItem`

Represents a single line item within an order.

| Property | Type | Purpose |
|----------|------|---------|
| `Id` | `int` | Primary key. Uniquely identifies the line item. |
| `OrderId` | `int` | Foreign key to the parent order. |
| `ProductId` | `int` | Foreign key to the product being purchased. |

All properties are read/write. No exceptions are thrown by the property accessors.

## Usage

### Example 1: Creating and populating a `Product`

```csharp
var product = new EcommerceExample.Product
{
    Id = 1,
    Sku = "WIDGET-001",
    Name = "Premium Widget",
    Description = "A high‑quality widget for everyday use.",
    Price = 19.99m,
    StockQuantity = 150,
    CategoryId = 5,
    CreatedAt = DateTime.UtcNow,
    ModifiedAt = null,
    IsActive = true
};

Console.WriteLine($"Product {product.Name} (SKU: {product.Sku}) costs {product.Price:C}.");
```

### Example 2: Building an order with line items

```csharp
var order = new EcommerceExample.Order
{
    Id = 100,
    CustomerId = 42,
    OrderDate = DateTime.UtcNow,
    TotalAmount = 59.97m,
    Status = "Pending",
    ShippingAddress = "123 Main St, Springfield, IL 62701",
    Items = new List<EcommerceExample.OrderItem>
    {
        new() { Id = 1, OrderId = 100, ProductId = 1 },
        new() { Id = 2, OrderId = 100, ProductId = 3 }
    }
};

foreach (var item in order.Items)
{
    Console.WriteLine($"Order {order.Id} contains product {item.ProductId}.");
}
```

## Notes

- **Edge cases**  
  - `Price` and `TotalAmount` are `decimal` and can be negative if not validated externally.  
  - `StockQuantity` can be negative, which may indicate a data integrity issue.  
  - `OrderDate` defaults to `DateTime.MinValue` if not explicitly set; always assign a meaningful value.  
  - `Status` and `ShippingAddress` are `string` and can be `null` or empty; consider using a dedicated enum or validation attributes.  
  - `Items` on `Order` can be `null`; always initialize the list before adding elements.  
  - `ModifiedAt` is `DateTime?`; a `null` value means the product has never been updated.

- **Thread safety**  
  These classes are mutable reference types and are **not thread‑safe**. Concurrent reads and writes to the same instance from multiple threads can cause data corruption or unexpected behavior. If instances are shared across threads (e.g., in a cache or static collection), use synchronization mechanisms such as locks, `ConcurrentDictionary`, or immutable snapshots. For typical web application scenarios where each request creates its own instances, no additional synchronization is required.
