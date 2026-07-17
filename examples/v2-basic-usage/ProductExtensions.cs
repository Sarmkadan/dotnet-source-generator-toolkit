#nullable enable

using System.Globalization;

/// <summary>
/// Provides extension methods for the <see cref="Product"/> class.
/// </summary>
public static class ProductExtensions
{
    /// <summary>
    /// Checks if a product is available and has a price greater than zero.
    /// </summary>
    /// <param name="product">The product to check.</param>
    /// <returns>True if the product is available and has a price greater than zero; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="product"/> is null.</exception>
    public static bool IsValidProduct(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return product.IsAvailable && product.Price > 0;
    }

    /// <summary>
    /// Formats a product's details into a string.
    /// </summary>
    /// <param name="product">The product to format.</param>
    /// <returns>A string containing the product's details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="product"/> is null.</exception>
    public static string FormatProductDetails(this Product product)
    {
        ArgumentNullException.ThrowIfNull(product);
        return $"Id: {product.Id}, Name: {product.Name}, Price: {product.Price}, Created Date: {product.CreatedDate:yyyy-MM-dd}";
    }

    /// <summary>
    /// Checks if a product's price is within a specified range.
    /// </summary>
    /// <param name="product">The product to check.</param>
    /// <param name="minPrice">The minimum price.</param>
    /// <param name="maxPrice">The maximum price.</param>
    /// <returns>True if the product's price is within the specified range; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="product"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="minPrice"/> is greater than <paramref name="maxPrice"/>.</exception>
    public static bool IsPriceWithinRange(this Product product, decimal minPrice, decimal maxPrice)
    {
        ArgumentNullException.ThrowIfNull(product);
return (minPrice, maxPrice) switch
{
(var min, var max) when min > max => throw new ArgumentException("Minimum price cannot be greater than maximum price", nameof(minPrice)),
_ => product.Price >= minPrice && product.Price <= maxPrice
};
    }

    /// <summary>
    /// Gets a list of products that are available and have a price greater than zero.
    /// </summary>
    /// <param name="products">The list of products to filter.</param>
    /// <returns>A list of available products with a price greater than zero.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="products"/> is null.</exception>
    public static IEnumerable<Product> GetAvailableProducts(this IEnumerable<Product> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        return products.Where(p => p.IsAvailable && p.Price > 0);
    }
}
