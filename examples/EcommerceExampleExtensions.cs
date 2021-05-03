using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public static class EcommerceExampleExtensions
{
    /// <summary>
    /// Formats the order details into a human-readable string.
    /// </summary>
    /// <param name="order">The order to format.</param>
    /// <returns>A formatted string representing the order details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="order"/> is null.</exception>
    public static string FormatOrderDetails(this EcommerceExample order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return $"Order {order.Id} - {order.OrderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} - {order.TotalAmount.ToString("C", CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Retrieves the total quantity of all items in the order.
    /// </summary>
    /// <param name="order">The order to retrieve the total quantity from.</param>
    /// <returns>The total quantity of all items in the order.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="order"/> is null.</exception>
    public static int GetTotalQuantity(this EcommerceExample order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.Items.Sum(item => item.Quantity);
    }

    /// <summary>
    /// Determines whether the order is recent (i.e., placed within the last 30 days).
    /// </summary>
    /// <param name="order">The order to check.</param>
    /// <returns>True if the order is recent; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="order"/> is null.</exception>
    public static bool IsRecentOrder(this EcommerceExample order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.OrderDate >= DateTime.Now.AddDays(-30);
    }

    /// <summary>
    /// Retrieves the most expensive item in the order.
    /// </summary>
    /// <param name="order">The order to retrieve the most expensive item from.</param>
    /// <returns>The most expensive item in the order, or null if the order has no items.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="order"/> is null.</exception>
    public static OrderItem? GetMostExpensiveItem(this EcommerceExample order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.Items?.OrderByDescending(item => item.UnitPrice).FirstOrDefault();
    }
}
