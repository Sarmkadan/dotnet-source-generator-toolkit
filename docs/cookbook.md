# Generation Cookbook

Short, task-focused recipes. Each one is self-contained: copy it, swap the entity, run it.
New here? Start with [GETTING_STARTED.md](../GETTING_STARTED.md).

All service recipes assume:

```csharp
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using Microsoft.Extensions.Logging.Abstractions;

var product = new Entity { Name = "Product", Namespace = "Shop.Domain", TableName = "products" };
product.AddProperty(new EntityProperty { Name = "Id",   Type = "int",    IsPrimaryKey = true });
product.AddProperty(new EntityProperty { Name = "Name", Type = "string", IsRequired = true, MaxLength = 100 });
```

---

## Recipe: repository with CRUD

```csharp
var svc = new RepositoryGeneratorService(NullLogger<RepositoryGeneratorService>.Instance);
var result = await svc.GenerateRepositoryAsync(product);
```

Emits `IProductRepository` (GetById / GetAll / GetPaged / Exists / Create / Update / Delete /
Count) plus an in-memory `ProductRepository` implementation. The primary-key CLR type is taken
from the property flagged `IsPrimaryKey`.

## Recipe: entity-to-DTO mapper

```csharp
var svc = new MapperGeneratorService(NullLogger<MapperGeneratorService>.Instance);
var result = await svc.GenerateMapperAsync(product, product);
```

Emits a `ProductDto` plus a `ProductMapper` with `MapToDto`, `MapFromDto`, and a collection
overload. Property names are copied one-to-one.

## Recipe: FluentValidation validator

```csharp
var svc = new ValidatorGeneratorService(NullLogger<ValidatorGeneratorService>.Instance);
var result = await svc.GenerateValidatorAsync(product);
```

Emits a `ProductValidator : AbstractValidator<Product>` whose rules are derived from the
property metadata: `IsRequired` -> `NotEmpty`, `MaxLength`/`MinLength` -> length rules,
`RegexPattern` -> `Matches`, and `MinValue`/`MaxValue` -> range rules.

## Recipe: JSON / XML serializer

```csharp
var svc = new SerializerGeneratorService(NullLogger<SerializerGeneratorService>.Instance);
var json = await svc.GenerateSerializerAsync(product, SerializerFormat.Json);
var xml  = await svc.GenerateSerializerAsync(product, SerializerFormat.Xml);
```

`GenerateAllSerializersAsync` produces both formats for a list of entities in one call.

## Recipe: generate everything for a batch

```csharp
var entities = new List<Entity> { product /*, ... */ };
await new RepositoryGeneratorService(NullLogger<RepositoryGeneratorService>.Instance).GenerateAllRepositoriesAsync(entities);
await new MapperGeneratorService(NullLogger<MapperGeneratorService>.Instance).GenerateAllMappersAsync(entities);
await new ValidatorGeneratorService(NullLogger<ValidatorGeneratorService>.Instance).GenerateAllValidatorsAsync(entities);
await new SerializerGeneratorService(NullLogger<SerializerGeneratorService>.Instance).GenerateAllSerializersAsync(entities);
```

## Recipe: only regenerate what changed

`IncrementalGeneratorService` fingerprints source files and skips entities whose files are
unchanged since the last run:

```csharp
var ctx = await incremental.BuildContextAsync(projectInfo);
var changed = incremental.FilterChangedEntities(allEntities, ctx);
// ... generate for `changed` only ...
await incremental.CommitContextAsync(ctx);
```

---

## Recipe: auto ToString / Equals

`AutoImplementGenerator` is a real Roslyn `IIncrementalGenerator`. Mark a `partial` class with
`[GenerateToString]` and/or `[GenerateEquals]` (namespace
`DotNetSourceGeneratorToolkit.Generated`) and the members are emitted during compilation.

```csharp
using DotNetSourceGeneratorToolkit.Generated;

namespace Shop.Domain;

[GenerateToString]
[GenerateEquals]
public partial class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
```

`[GenerateToString]` expands to:

```csharp
public override string ToString() => $"Product {{ Id = {Id}, Name = {Name} }}";
```

`[GenerateEquals]` implements `IEquatable<Product>`, an `object` override, and a matching
`GetHashCode()` built from the public instance properties.

Only **public, non-static, readable, non-indexer** properties are included.

### Diagnostics

The generator reports precise compiler diagnostics for invalid inputs:

| ID       | Severity | Condition                                             |
|----------|----------|-------------------------------------------------------|
| SGTK001  | Error    | The annotated type is not declared `partial`.         |
| SGTK002  | Warning  | The type has no public instance properties (trivial). |
| SGTK003  | Error    | The type is `static`.                                 |

On an `SGTK001` or `SGTK003` error the generator skips emission for that type; `SGTK002` is a
warning and the (trivial) member is still generated.
