#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Examples;

/// Demonstrates e-commerce domain example with repositories, mappers, validators, and serializers.
public sealed class EcommerceExample
{
    /// <summary>
    /// Represents a product entity with properties for identification, SKU, name, description, price, stock quantity, category ID, creation date, modification date, and activity status.
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    [Serializer(Formats = new[] { "Json", "Xml" })]
    public sealed class Product
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Represents an order entity with properties for identification, customer ID, order date, total amount, status, shipping address, and items.
    /// </summary>
    [Repository]
    [Mapper]
    [Validator]
    public sealed class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = [];
    }

    /// <summary>
    /// Represents an order item entity with properties for identification, order ID, product ID, quantity, unit price, and line total.
    /// </summary>
    [Mapper]
    [Validator]
    public sealed class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// Represents a product data transfer object (DTO) with properties for identification, SKU, name, price, and stock quantity.
    /// </summary>
    [Mapper]
    public sealed class ProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    /// <summary>
    /// Represents an order data transfer object (DTO) with properties for identification, customer ID, order date, total amount, status, and items.
    /// </summary>
    [Mapper]
    public sealed class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = [];
    }

    /// <summary>
    /// Represents an order item data transfer object (DTO) with properties for identification, product ID, quantity, unit price, and line total.
    /// </summary>
    [Mapper]
    public sealed class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>
    /// Provides a service for managing orders.
    /// </summary>
    public sealed class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderMapper _orderMapper;
        private readonly IProductMapper _productMapper;
        private readonly IOrderValidator _orderValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderService"/> class.
        /// </summary>
        /// <param name="orderRepository">The order repository instance.</param>
        /// <param name="productRepository">The product repository instance.</param>
        /// <param name="orderMapper">The order mapper instance.</param>
        /// <param name="productMapper">The product mapper instance.</param>
        /// <param name="orderValidator">The order validator instance.</param>
        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IOrderMapper orderMapper,
            IProductMapper productMapper,
            IOrderValidator orderValidator)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _orderMapper = orderMapper;
            _productMapper = productMapper;
            _orderValidator = orderValidator;
        }

        /// <summary>
        /// Retrieves an order by its ID.
        /// </summary>
        /// <param name="orderId">The ID of the order to retrieve.</param>
        /// <returns>The order DTO if the order is found, otherwise null.</returns>
        public async Task<OrderDto?> GetOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null)
                return null;

            var validationResult = await _orderValidator.ValidateAsync(order);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Invalid order data");

            return _orderMapper.MapToDto(order);
        }

        /// <summary>
        /// Retrieves a list of orders for a customer.
        /// </summary>
        /// <param name="customerId">The ID of the customer.</param>
        /// <returns>A list of order DTOs.</returns>
        public async Task<List<OrderDto>> GetCustomerOrdersAsync(int customerId)
        {
            var orders = await _orderRepository.WhereAsync(o => o.CustomerId == customerId);
            var dtos = _orderMapper.MapCollectionToDto(orders);
            return dtos.ToList();
        }

        /// <summary>
        /// Creates a new order based on the provided order DTO.
        /// </summary>
        /// <param name="orderDto">The order DTO to create a new order from.</param>
        /// <returns>The created order DTO.</returns>
        public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto)
        {
            var order = _orderMapper.MapFromDto(orderDto);

            var validationResult = await _orderValidator.ValidateAsync(order);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Order validation failed");

            // Validate stock availability
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product is null || product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
            }

            var createdOrder = await _orderRepository.CreateAsync(order);
            return _orderMapper.MapToDto(createdOrder);
        }

        /// <summary>
        /// Exports an order as JSON.
        /// </summary>
        /// <param name="orderId">The ID of the order to export.</param>
        /// <returns>The JSON representation of the order.</returns>
        public async Task<string> ExportOrderAsJsonAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null)
                throw new ArgumentException("Order not found");

            // Use generated JSON serializer
            var serializer = new ProductJsonSerializer();
            return await serializer.SerializeToJsonAsync(order as dynamic);
        }
    }
}
