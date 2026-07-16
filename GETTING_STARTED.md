# Getting Started

A five-minute path from install to your first generated file. For the full reference (every
option, CLI flag, and architecture note) see [README.md](README.md); for copy-paste snippets
grouped by task see the [Generation Cookbook](docs/cookbook.md).

## 1. Install

```bash
dotnet add package sarmkadan.dotnet-source-generator-toolkit
```

Requires the .NET 10 SDK.

## 2. Describe an entity

Everything the template-based generators need is an `Entity` with a few `EntityProperty`
definitions:

```csharp
using DotNetSourceGeneratorToolkit.Domain;

var product = new Entity { Name = "Product", Namespace = "Shop.Domain", TableName = "products" };
product.AddProperty(new EntityProperty { Name = "Id",   Type = "int",    IsPrimaryKey = true });
product.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = true, MaxLength = 100 });
```

## 3. Generate

```csharp
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging.Abstractions;

var repos = new RepositoryGeneratorService(NullLogger<RepositoryGeneratorService>.Instance);
var result = await repos.GenerateRepositoryAsync(product);

Console.WriteLine(result.GeneratedCode);   // the generated IProductRepository + ProductRepository
Console.WriteLine(result.OutputFilePath);  // Repositories/ProductRepository.cs
```

Every generator returns a `GenerationResult` with `Status`, `GeneratedCode`, `OutputFilePath`,
and any `Errors`. Check `result.Status == GenerationStatus.Completed` before writing files.

## 4. Wire it into DI (optional)

```csharp
services.AddSourceGeneratorToolkit(); // registers all generator services
```

## 5. Attribute-driven generation in your own compilation

Beyond the service API, the toolkit ships a real Roslyn incremental generator,
`AutoImplementGenerator`, that reacts to marker attributes in your source. Mark a `partial`
class and the `ToString()` / value-equality members are emitted at compile time - no runtime
call needed:

```csharp
using DotNetSourceGeneratorToolkit.Generated;

[GenerateToString]
[GenerateEquals]
public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
```

See the [cookbook](docs/cookbook.md#recipe-auto-tostring--equals) for what this expands to and
which diagnostics it reports for invalid inputs.

## Where to next

- [Generation Cookbook](docs/cookbook.md) - task-focused recipes.
- [README.md](README.md) - complete reference and architecture.
- [docs/faq.md](docs/faq.md) - common questions.
