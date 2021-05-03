#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace DotNetSourceGeneratorToolkit.Examples;

/// <summary>
/// Provides validation helpers for e-commerce domain types (Product, Order, OrderItem).
/// </summary>
public static class EcommerceExampleValidation
{
    /// <summary>
    /// Validates a Product instance and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The Product instance to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this Product? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Product Id must be a positive integer, but was {value.Id}.");
        }

        // Validate Sku
        if (string.IsNullOrWhiteSpace(value.Sku))
        {
            errors.Add("Product Sku cannot be null or whitespace.");
        }
        else if (value.Sku.Length > 50)
        {
            errors.Add("Product Sku cannot exceed 50 characters.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Product Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 200)
        {
            errors.Add("Product Name cannot exceed 200 characters.");
        }

        // Validate Description
        if (value.Description.Length > 2000)
        {
            errors.Add("Product Description cannot exceed 2000 characters.");
        }

        // Validate Price
        if (value.Price <= 0m)
        {
            errors.Add("Product Price must be a positive decimal value.");
        }
        else if (value.Price > 999999.99m)
        {
            errors.Add("Product Price cannot exceed 999,999.99.");
        }

        // Validate StockQuantity
        if (value.StockQuantity < 0)
        {
            errors.Add("Product StockQuantity cannot be negative.");
        }

        // Validate CategoryId
        if (value.CategoryId <= 0)
        {
            errors.Add($"Product CategoryId must be a positive integer, but was {value.CategoryId}.");
        }

        // Validate CreatedAt
        if (value.CreatedAt == default)
        {
            errors.Add("Product CreatedAt cannot be the default DateTime value.");
        }
        else if (value.CreatedAt > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Product CreatedAt cannot be in the future.");
        }

        // Validate ModifiedAt
        if (value.ModifiedAt.HasValue && value.ModifiedAt.Value == default)
        {
            errors.Add("Product ModifiedAt cannot be the default DateTime value when set.");
        }
        else if (value.ModifiedAt.HasValue && value.ModifiedAt.Value > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Product ModifiedAt cannot be in the future.");
        }
        else if (value.ModifiedAt.HasValue && value.ModifiedAt.Value < value.CreatedAt)
        {
            errors.Add("Product ModifiedAt cannot be earlier than CreatedAt.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates an Order instance and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The Order instance to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this Order? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"Order Id must be a positive integer, but was {value.Id}.");
        }

        // Validate CustomerId
        if (value.CustomerId <= 0)
        {
            errors.Add($"Order CustomerId must be a positive integer, but was {value.CustomerId}.");
        }

        // Validate OrderDate
        if (value.OrderDate == default)
        {
            errors.Add("Order OrderDate cannot be the default DateTime value.");
        }
        else if (value.OrderDate > DateTime.UtcNow.AddMinutes(5))
        {
            errors.Add("Order OrderDate cannot be in the future.");
        }

        // Validate TotalAmount
        if (value.TotalAmount <= 0m)
        {
            errors.Add("Order TotalAmount must be a positive decimal value.");
        }
        else if (value.TotalAmount > 9999999.99m)
        {
            errors.Add("Order TotalAmount cannot exceed 9,999,999.99.");
        }

        // Validate Status
        if (string.IsNullOrWhiteSpace(value.Status))
        {
            errors.Add("Order Status cannot be null or whitespace.");
        }
        else if (value.Status.Length > 50)
        {
            errors.Add("Order Status cannot exceed 50 characters.");
        }
        else if (!IsValidOrderStatus(value.Status))
        {
            errors.Add("Order Status must be one of: Pending, Processing, Shipped, Delivered.");
        }

        // Validate ShippingAddress
        if (string.IsNullOrWhiteSpace(value.ShippingAddress))
        {
            errors.Add("Order ShippingAddress cannot be null or whitespace.");
        }
        else if (value.ShippingAddress.Length > 500)
        {
            errors.Add("Order ShippingAddress cannot exceed 500 characters.");
        }

        // Validate Items collection
        if (value.Items is null)
        {
            errors.Add("Order Items collection cannot be null.");
        }
        else if (value.Items.Count == 0)
        {
            errors.Add("Order must contain at least one OrderItem.");
        }
        else
        {
            // Validate each OrderItem
            for (int i = 0; i < value.Items.Count; i++)
            {
                var itemErrors = Validate(value.Items[i]);
                if (itemErrors.Count > 0)
                {
                    errors.AddRange(itemErrors.Select(e => $"Order Item at index {i}: {e}"));
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates an OrderItem instance and returns a list of human-readable validation errors.
    /// </summary>
    /// <param name="value">The OrderItem instance to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static IReadOnlyList<string> Validate(this OrderItem? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (value.Id <= 0)
        {
            errors.Add($"OrderItem Id must be a positive integer, but was {value.Id}.");
        }

        // Validate OrderId
        if (value.OrderId <= 0)
        {
            errors.Add($"OrderItem OrderId must be a positive integer, but was {value.OrderId}.");
        }

        // Validate ProductId
        if (value.ProductId <= 0)
        {
            errors.Add($"OrderItem ProductId must be a positive integer, but was {value.ProductId}.");
        }

        // Validate Quantity
        if (value.Quantity <= 0)
        {
            errors.Add("OrderItem Quantity must be a positive integer.");
        }
        else if (value.Quantity > 9999)
        {
            errors.Add("OrderItem Quantity cannot exceed 9,999.");
        }

        // Validate UnitPrice
        if (value.UnitPrice <= 0m)
        {
            errors.Add("OrderItem UnitPrice must be a positive decimal value.");
        }
        else if (value.UnitPrice > 999999.99m)
        {
            errors.Add("OrderItem UnitPrice cannot exceed 999,999.99.");
        }

        // Validate LineTotal
        if (value.LineTotal <= 0m)
        {
            errors.Add("OrderItem LineTotal must be a positive decimal value.");
        }
        else if (value.LineTotal > 9999999.99m)
        {
            errors.Add("OrderItem LineTotal cannot exceed 9,999,999.99.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a Product instance is valid.
    /// </summary>
    /// <param name="value">The Product instance to check.</param>
    /// <returns>True if valid, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this Product? value) => Validate(value).Count == 0;

    /// <summary>
    /// Checks if an Order instance is valid.
    /// </summary>
    /// <param name="value">The Order instance to check.</param>
    /// <returns>True if valid, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this Order? value) => Validate(value).Count == 0;

    /// <summary>
    /// Checks if an OrderItem instance is valid.
    /// </summary>
    /// <param name="value">The OrderItem instance to check.</param>
    /// <returns>True if valid, otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    public static bool IsValid(this OrderItem? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that a Product instance is valid, throwing an ArgumentException if not.
    /// </summary>
    /// <param name="value">The Product instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValid(this Product? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException("Product validation failed: " + string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Ensures that an Order instance is valid, throwing an ArgumentException if not.
    /// </summary>
    /// <param name="value">The Order instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValid(this Order? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException("Order validation failed: " + string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Ensures that an OrderItem instance is valid, throwing an ArgumentException if not.
    /// </summary>
    /// <param name="value">The OrderItem instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing error messages.</exception>
    public static void EnsureValid(this OrderItem? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException("OrderItem validation failed: " + string.Join(" ", errors));
        }
    }

    /// <summary>
    /// Determines whether the specified status string is a valid order status.
    /// </summary>
    /// <param name="status">The status string to validate.</param>
    /// <returns>True if the status is valid; otherwise, false.</returns>
    private static bool IsValidOrderStatus(string status)
    {
        return status is "Pending" or "Processing" or "Shipped" or "Delivered";
    }
}