// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetSourceGeneratorToolkit.Examples;

/// Integration example showing how to use generated code in a real application
public class IntegrationExample
{
    [Repository]
    [Mapper]
    [Validator]
    public class Customer
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    }

    [Mapper]
    public class CustomerDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// Example of proper dependency injection setup
    public class DependencyInjectionSetup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // Register generated repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            // Register generated mappers
            services.AddScoped<ICustomerMapper, CustomerMapper>();

            // Register generated validators
            services.AddScoped<ICustomerValidator, CustomerValidator>();

            // Register application services that use generated code
            services.AddScoped<ICustomerApplicationService, CustomerApplicationService>();
        }
    }

    /// Example application service using generated code
    public interface ICustomerApplicationService
    {
        Task<CustomerDto?> GetCustomerAsync(int customerId);
        Task<List<CustomerDto>> GetAllCustomersAsync();
        Task<CustomerDto> CreateCustomerAsync(CustomerDto dto);
        Task<CustomerDto> UpdateCustomerAsync(int customerId, CustomerDto dto);
        Task DeleteCustomerAsync(int customerId);
    }

    public class CustomerApplicationService : ICustomerApplicationService
    {
        private readonly ICustomerRepository _repository;
        private readonly ICustomerMapper _mapper;
        private readonly ICustomerValidator _validator;

        // Generated dependencies injected via constructor
        public CustomerApplicationService(
            ICustomerRepository repository,
            ICustomerMapper mapper,
            ICustomerValidator validator)
        {
            _repository = repository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<CustomerDto?> GetCustomerAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId);
            if (customer == null)
                return null;

            // Validate before returning
            var validationResult = await _validator.ValidateAsync(customer);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Customer data is invalid");

            return _mapper.MapToDto(customer);
        }

        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repository.GetAllAsync();
            return customers
                .Select(_mapper.MapToDto)
                .ToList();
        }

        public async Task<CustomerDto> CreateCustomerAsync(CustomerDto dto)
        {
            // Map from DTO to domain model
            var customer = _mapper.MapFromDto(dto);

            // Validate before creating
            var validationResult = await _validator.ValidateAsync(customer);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Invalid customer data");

            // Save to repository
            var savedCustomer = await _repository.CreateAsync(customer);

            return _mapper.MapToDto(savedCustomer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int customerId, CustomerDto dto)
        {
            // Get existing customer
            var existingCustomer = await _repository.GetByIdAsync(customerId);
            if (existingCustomer == null)
                throw new ArgumentException("Customer not found");

            // Map DTO to entity
            var updated = _mapper.MapFromDto(dto);
            updated.Id = customerId;

            // Validate before updating
            var validationResult = await _validator.ValidateAsync(updated);
            if (!validationResult.IsValid)
                throw new InvalidOperationException("Invalid customer data");

            // Update in repository
            await _repository.UpdateAsync(updated);

            return _mapper.MapToDto(updated);
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            var customer = await _repository.GetByIdAsync(customerId);
            if (customer == null)
                throw new ArgumentException("Customer not found");

            await _repository.DeleteAsync(customerId);
        }
    }

    /// Example of using the application service in a REST API controller
    public class CustomerController
    {
        private readonly ICustomerApplicationService _service;

        public CustomerController(ICustomerApplicationService service)
        {
            _service = service;
        }

        // GET /api/customers/{id}
        public async Task<IResult> GetCustomer(int id)
        {
            try
            {
                var customer = await _service.GetCustomerAsync(id);
                if (customer == null)
                    return Results.NotFound();

                return Results.Ok(customer);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // GET /api/customers
        public async Task<IResult> GetAllCustomers()
        {
            try
            {
                var customers = await _service.GetAllCustomersAsync();
                return Results.Ok(customers);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // POST /api/customers
        public async Task<IResult> CreateCustomer(CustomerDto dto)
        {
            try
            {
                var created = await _service.CreateCustomerAsync(dto);
                return Results.Created($"/api/customers/{created.Id}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.StatusCode(500);
            }
        }

        // PUT /api/customers/{id}
        public async Task<IResult> UpdateCustomer(int id, CustomerDto dto)
        {
            try
            {
                var updated = await _service.UpdateCustomerAsync(id, dto);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }

        // DELETE /api/customers/{id}
        public async Task<IResult> DeleteCustomer(int id)
        {
            try
            {
                await _service.DeleteCustomerAsync(id);
                return Results.NoContent();
            }
            catch (ArgumentException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
        }
    }

    /// Example of wiring up ASP.NET Core Minimal APIs
    public class WebApiSetup
    {
        public static void MapCustomerEndpoints(WebApplication app)
        {
            var group = app.MapGroup("/api/customers")
                .WithName("Customers")
                .WithOpenApi();

            group.MapGet("/", GetAllCustomers)
                .WithName("GetAllCustomers")
                .WithSummary("Get all customers");

            group.MapGet("/{id}", GetCustomer)
                .WithName("GetCustomer")
                .WithSummary("Get customer by ID");

            group.MapPost("/", CreateCustomer)
                .WithName("CreateCustomer")
                .WithSummary("Create a new customer");

            group.MapPut("/{id}", UpdateCustomer)
                .WithName("UpdateCustomer")
                .WithSummary("Update an existing customer");

            group.MapDelete("/{id}", DeleteCustomer)
                .WithName("DeleteCustomer")
                .WithSummary("Delete a customer");
        }

        private static async Task<IResult> GetAllCustomers(
            ICustomerApplicationService service)
        {
            var customers = await service.GetAllCustomersAsync();
            return Results.Ok(customers);
        }

        private static async Task<IResult> GetCustomer(
            int id,
            ICustomerApplicationService service)
        {
            var customer = await service.GetCustomerAsync(id);
            return customer != null ? Results.Ok(customer) : Results.NotFound();
        }

        private static async Task<IResult> CreateCustomer(
            CustomerDto dto,
            ICustomerApplicationService service)
        {
            var created = await service.CreateCustomerAsync(dto);
            return Results.Created($"/api/customers/{created.Id}", created);
        }

        private static async Task<IResult> UpdateCustomer(
            int id,
            CustomerDto dto,
            ICustomerApplicationService service)
        {
            var updated = await service.UpdateCustomerAsync(id, dto);
            return Results.Ok(updated);
        }

        private static async Task<IResult> DeleteCustomer(
            int id,
            ICustomerApplicationService service)
        {
            await service.DeleteCustomerAsync(id);
            return Results.NoContent();
        }
    }
}
