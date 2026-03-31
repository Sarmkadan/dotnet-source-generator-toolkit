// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Getting Started with .NET Source Generator Toolkit

## 5-Minute Quick Start

### Step 1: Install .NET 10 SDK

```bash
# macOS (with Homebrew)
brew install dotnet

# Windows (with Chocolatey)
choco install dotnet

# Linux (Ubuntu/Debian)
sudo apt-get install dotnet-sdk-10.0

# Or download from: https://dotnet.microsoft.com/download
```

### Step 2: Clone the Repository

```bash
git clone https://github.com/Sarmkadan/dotnet-source-generator-toolkit.git
cd dotnet-source-generator-toolkit
dotnet build
```

### Step 3: Create Your First Entity

Create a file `MyEntity.cs`:

```csharp
using DotNetSourceGeneratorToolkit.Domain;

namespace MyApp.Domain
{
    [Repository]
    [Mapper]
    [Validator]
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
```

### Step 4: Run the Toolkit

```bash
dotnet run -- --path . --format Json --output ./Generated
```

### Step 5: Use Generated Code

```csharp
// Generated ProductRepository is now available
var repository = new ProductRepository(dbContext);
var products = await repository.GetAllAsync();

// Generated ProductMapper
var mapper = new ProductMapper();
var dto = mapper.MapToDto(products.First());

// Generated ProductValidator
var validator = new ProductValidator();
var isValid = await validator.ValidateAsync(product);
```

## Understanding Attributes

### `[Repository]`

Generates a complete CRUD repository implementation.

```csharp
[Repository]
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

// Generated methods:
// - GetAllAsync()
// - GetByIdAsync(int id)
// - FindByEmailAsync(string email)
// - CreateAsync(User user)
// - UpdateAsync(User user)
// - DeleteAsync(int id)
// - ExistsAsync(int id)
// - GetPagedAsync(int pageNumber, int pageSize)
```

### `[Mapper]`

Generates bidirectional mapping between entities.

```csharp
[Mapper(Profile = "ApiResponse")]
public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

// Generated methods:
// - MapFromDto(UserDto dto) : User
// - MapToDto(User entity) : UserDto
// - MapFromDtoAsync(UserDto dto) : Task<User>
// - MapToDtoAsync(User entity) : Task<UserDto>
// - MapCollectionAsync(IEnumerable<T>)
```

### `[Validator]`

Generates validation rules with customizable messages.

```csharp
[Validator(MessageLanguage = "en")]
public class Order
{
    public decimal Amount { get; set; }
    public DateTime OrderDate { get; set; }
}

// Generated methods:
// - ValidateAsync(Order order) : Task<ValidationResult>
// - Validate(Order order) : ValidationResult
// - ValidatePropertyAsync(string propertyName, object value)
```

### `[Serializer]`

Generates serialization for multiple formats.

```csharp
[Serializer(Formats = new[] { "Json", "Xml", "Csv" })]
public class Report
{
    public string Title { get; set; } = string.Empty;
    public List<DataPoint> Items { get; set; } = [];
}

// Generated methods:
// - SerializeToJsonAsync(Report report) : Task<string>
// - SerializeToXmlAsync(Report report) : Task<string>
// - SerializeToCsvAsync(Report report) : Task<string>
// - DeserializeFromJsonAsync(string json) : Task<Report>
// - DeserializeFromXmlAsync(string xml) : Task<Report>
```

## Configuration Guide

### Basic Configuration

Create `toolkit-config.json`:

```json
{
  "enableCaching": true,
  "cacheExpirationMinutes": 60,
  "outputDirectory": "./Generated",
  "defaultNamespace": "MyApp.Generated"
}
```

### Advanced Configuration

```json
{
  "generation": {
    "enableCaching": true,
    "cacheExpirationMinutes": 60,
    "enableCodeFormatting": true,
    "codeFormattingLineLength": 100,
    "maxDegreeOfParallelism": 4
  },
  
  "output": {
    "outputDirectory": "./Generated",
    "backupExistingFiles": true,
    "defaultNamespace": "MyApp.Generated"
  },
  
  "features": {
    "generateDtos": true,
    "generateInterfaces": true,
    "generateXmlComments": true,
    "verboseLogging": false
  },
  
  "performance": {
    "operationTimeoutSeconds": 300,
    "batchProcessingEnabled": true,
    "batchSize": 10
  },
  
  "integration": {
    "webhookEnabled": true,
    "webhookUrl": "https://api.example.com/generation-complete",
    "webhookRetries": 3,
    "webhookTimeoutSeconds": 30
  }
}
```

## Directory Structure

Organize your project like this:

```
MyProject/
├── src/
│   ├── Domain/              # Your domain entities
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   └── Order.cs
│   ├── Application/         # Application logic
│   └── Infrastructure/      # Data access
├── Generated/               # Generated code (by toolkit)
│   ├── Repositories/
│   ├── Mappers/
│   ├── Validators/
│   └── Serializers/
├── Tests/                   # Unit tests
├── toolkit-config.json      # Configuration
└── MyProject.csproj
```

## Common Workflows

### Workflow 1: Generating Repository and Mapper

```csharp
// Step 1: Define entity with attributes
[Repository]
[Mapper]
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Step 2: Define DTO
[Mapper]
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Step 3: Run toolkit
// dotnet run -- --path . --format Json

// Step 4: Use generated code
public class CustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;
    
    public async Task<CustomerDto?> GetCustomerAsync(int id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer != null ? _mapper.MapToDto(customer) : null;
    }
}
```

### Workflow 2: Batch Processing Multiple Entities

```bash
# Configure for batch processing
dotnet run -- \
  --path ./src \
  --format Json \
  --max-parallelism 4 \
  --batch-size 10 \
  --output ./Generated \
  --verbose
```

### Workflow 3: Continuous Integration

```bash
# In your CI/CD pipeline
dotnet build
dotnet run -- --path . --dry-run  # Validate without writing
dotnet run -- --path .             # Generate code
dotnet test                          # Run tests on generated code
```

## Troubleshooting Common Issues

### Issue: "The type 'X' does not exist in the current context"

**Cause**: Missing using statements in generated code.

**Solution**:
```json
{
  "defaultNamespace": "MyApp.Generated",
  "generateXmlComments": true
}
```

### Issue: "File is locked" error

**Cause**: Visual Studio or another process is holding the file.

**Solution**:
```bash
# Close the IDE or project
# Then run:
dotnet run -- --path . --clear-cache
```

### Issue: "Timeout during generation"

**Cause**: Large number of entities or slow disk I/O.

**Solution**:
```json
{
  "operationTimeoutSeconds": 600,
  "batchProcessingEnabled": true,
  "batchSize": 5,
  "maxDegreeOfParallelism": 2
}
```

### Issue: "Memory usage is high"

**Cause**: Caching many entities.

**Solution**:
```json
{
  "enableCaching": false
}
```

Or reduce cache expiration:
```json
{
  "cacheExpirationMinutes": 15
}
```

## Next Steps

- Read [Architecture Documentation](./architecture.md)
- Check [API Reference](./api-reference.md)
- Review [Deployment Guide](./deployment.md)
- Browse [FAQ](./faq.md)
- Explore `examples/` directory for complete samples
