#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Demonstrates v2.0 Basic Usage Example - Attribute-driven mapper with custom converters.
/// This example demonstrates the new v2.0 features:
/// - Attribute-driven mapper generation
/// - Custom value converters (IValueConverter<TSource, TDestination>)
/// - Automatic converter selection based on mapping context
/// </summary>
Console.WriteLine("v2.0 Basic Usage Example - Attribute-driven mapper with custom converters");
Console.WriteLine("This example demonstrates the new v2.0 features:");
Console.WriteLine("- Attribute-driven mapper generation");
Console.WriteLine("- Custom value converters (IValueConverter<TSource, TDestination>)");
Console.WriteLine("- Automatic converter selection based on mapping context");

// Example demonstrating v2.0 attribute-driven mapper generator with custom converters

/// <summary>
/// Represents a product entity with properties for identification, name, price, and creation date.
/// </summary>
[Mapper]
public sealed class Product
{
    /// <summary>
    /// Gets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets the date and time when the product was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets a value indicating whether the product is available.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}

// Define a DTO for API responses
[Mapper]
public sealed class ProductDto
{
    /// <summary>
    /// Gets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product in a custom formatted string.
    /// </summary>
    public string PriceDisplay { get; set; } = string.Empty; // Custom formatted price

    /// <summary>
    /// Gets or sets the creation date of the product in a custom formatted string.
    /// </summary>
    public string CreatedDateString { get; set; } = string.Empty; // Custom formatted date

    /// <summary>
    /// Gets or sets the availability status of the product in a custom formatted string.
    /// </summary>
    public string AvailabilityStatus { get; set; } = string.Empty; // Custom formatted boolean
}

// Custom converter for price formatting
public sealed class PriceConverter : IValueConverter<decimal, string>
{
    /// <summary>
    /// Converts a decimal value to a custom formatted string.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    /// <param name="context">The mapping context.</param>
    /// <returns>The custom formatted string representation of the decimal value.</returns>
    public string Convert(decimal value, object? context = null)
    {
        return $"${value:N2}";
    }
}

// Custom converter for date formatting
public sealed class DateConverter : IValueConverter<DateTime, string>
{
    /// <summary>
    /// Converts a DateTime value to a custom formatted string.
    /// </summary>
    /// <param name="value">The DateTime value to convert.</param>
    /// <param name="context">The mapping context.</param>
    /// <returns>The custom formatted string representation of the DateTime value.</returns>
    public string Convert(DateTime value, object? context = null)
    {
        return value.ToString("yyyy-MM-dd");
    }
}

// Custom converter for boolean status
public sealed class AvailabilityConverter : IValueConverter<bool, string>
{
    /// <summary>
    /// Converts a boolean value to a custom formatted string.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <param name="context">The mapping context.</param>
    /// <returns>The custom formatted string representation of the boolean value.</returns>
    public string Convert(bool value, object? context = null)
    {
        return value ? "In Stock" : "Out of Stock";
    }
}

// Usage example
public sealed class ProductService
{
    /// <summary>
    /// Maps a product to its DTO representation.
    /// </summary>
    /// <param name="product">The product to map.</param>
    /// <returns>The product DTO.</returns>
    public ProductDto MapProductToDto(Product product)
    {
        // The mapper generator will automatically use custom converters
        // based on the IValueConverter<TSource, TDestination> interface
        var mapper = new ProductMapper();
        return mapper.MapToDto(product);
    }
}
