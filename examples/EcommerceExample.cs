// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;

namespace DotNetSourceGeneratorToolkit.Examples;

/// E-commerce domain example with repositories, mappers, validators, and serializers
public class EcommerceExample
{
    [Repository]
    [Mapper]
    [Validator]
    [Serializer(Formats = new[] { "Json", "Xml" })]
    public class Product
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

    [Repository]
    [Mapper]
    [Validator]
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Processing, Shipped, Delivered
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = [];
    }

    [Mapper]
    [Validator]
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    [Mapper]
    public class ProductDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }

    [Mapper]
    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<OrderItemDto> Items { get; set; } = [];
    }

    [Mapper]
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    // Usage example
    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderMapper _orderMapper;
        private readonly IProductMapper _productMapper;
        private readonly IOrderValidator _orderValidator;

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

        public async Task<OrderDto?> GetOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return null;

            var validationResult = await _orderValidator.ValidateAsync(order);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Invalid order data");

            return _orderMapper.MapToDto(order);
        }

        public async Task<List<OrderDto>> GetCustomerOrdersAsync(int customerId)
        {
            var orders = await _orderRepository.WhereAsync(o => o.CustomerId == customerId);
            var dtos = _orderMapper.MapCollectionToDto(orders);
            return dtos.ToList();
        }

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
                if (product == null || product.StockQuantity < item.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
            }

            var createdOrder = await _orderRepository.CreateAsync(order);
            return _orderMapper.MapToDto(createdOrder);
        }

        public async Task<string> ExportOrderAsJsonAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new ArgumentException("Order not found");

            // Use generated JSON serializer
            var serializer = new ProductJsonSerializer();
            return await serializer.SerializeToJsonAsync(order as dynamic);
        }
    }
}
