using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DotNetSourceGeneratorToolkit.Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="MapperGeneratorService"/> covering argument validation, the generated DTO and mapper shape,
    /// per-property mapping in both directions, null guards in the generated code, and batch generation through
    /// <c>GenerateAllMappersAsync</c>.
    /// </summary>
    public class MapperGeneratorServiceTests
    {
        /// <summary>The service under test, constructed with a null logger in the test constructor.</summary>
        private readonly MapperGeneratorService _service;

        /// <summary>
        /// Creates a fresh <see cref="MapperGeneratorService"/> backed by <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{T}"/>
        /// so every test runs against an isolated service instance.
        /// </summary>
        public MapperGeneratorServiceTests()
        {
            _service = new MapperGeneratorService(NullLogger<MapperGeneratorService>.Instance);
        }

        /// <summary>
        /// Verifies that <c>GenerateMapperAsync</c> throws <see cref="ArgumentNullException"/> when the source entity is null.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_WithNullSourceEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            Entity? nullSource = null;
            var targetEntity = CreateTestEntity("User");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GenerateMapperAsync(nullSource!, targetEntity));
        }

        /// <summary>
        /// Verifies that <c>GenerateMapperAsync</c> throws <see cref="ArgumentNullException"/> when the target entity is null.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_WithNullTargetEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            var sourceEntity = CreateTestEntity("User");
            Entity? nullTarget = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GenerateMapperAsync(sourceEntity, nullTarget!));
        }

        /// <summary>
        /// Verifies that a valid entity with four properties produces a <c>Completed</c> result with no errors or warnings,
        /// non-empty generated code, and the output path <c>Mappers/UserMapper.cs</c>.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_WithValidEntity_ShouldGenerateValidMapper()
        {
            // Arrange
            var entity = CreateTestEntity("User");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Email", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Age", Type = "int" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Completed);
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            result.OutputFilePath.Should().Be("Mappers/UserMapper.cs");
        }

        /// <summary>
        /// Verifies that the generated <c>ProductDto</c> is a public sealed class whose properties carry the entity's
        /// types (<c>Guid</c>, <c>string</c>, <c>decimal</c>, <c>bool</c>) with get/set accessors.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldGenerateDtoClassWithCorrectProperties()
        {
            // Arrange
            var entity = CreateTestEntity("Product");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "Guid", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Price", Type = "decimal" });
            entity.AddProperty(new EntityProperty { Name = "IsActive", Type = "bool" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public sealed class ProductDto");
            generatedCode.Should().Contain("public Guid Id { get; set; }");
            generatedCode.Should().Contain("public string Name { get; set; }");
            generatedCode.Should().Contain("public decimal Price { get; set; }");
            generatedCode.Should().Contain("public bool IsActive { get; set; }");
        }

        /// <summary>
        /// Verifies that the generated <c>CustomerMapper</c> exposes <c>MapToDto</c>, <c>MapFromDto</c>, and <c>MapToDtos</c>
        /// with the expected nullable parameter and return signatures.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldGenerateMapperClassWithCorrectMethods()
        {
            // Arrange
            var entity = CreateTestEntity("Customer");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "FullName", Type = "string" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public sealed class CustomerMapper");
            generatedCode.Should().Contain("public static CustomerDto? MapToDto(Customer? entity)");
            generatedCode.Should().Contain("public static Customer? MapFromDto(CustomerDto? dto)");
            generatedCode.Should().Contain("public static IEnumerable<CustomerDto> MapToDtos(IEnumerable<Customer>? entities)");
        }

        /// <summary>
        /// Verifies that the entity-to-DTO direction assigns every property, for example <c>dto.OrderId = entity.OrderId;</c>.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldMapAllPropertiesFromEntityToDto()
        {
            // Arrange
            var entity = CreateTestEntity("Order");
            entity.AddProperty(new EntityProperty { Name = "OrderId", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "OrderDate", Type = "DateTime" });
            entity.AddProperty(new EntityProperty { Name = "TotalAmount", Type = "decimal" });
            entity.AddProperty(new EntityProperty { Name = "Status", Type = "string" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("dto.OrderId = entity.OrderId;");
            generatedCode.Should().Contain("dto.OrderDate = entity.OrderDate;");
            generatedCode.Should().Contain("dto.TotalAmount = entity.TotalAmount;");
            generatedCode.Should().Contain("dto.Status = entity.Status;");
        }

        /// <summary>
        /// Verifies that the DTO-to-entity direction assigns every property, for example <c>entity.EmployeeId = dto.EmployeeId;</c>.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldMapAllPropertiesFromDtoToEntity()
        {
            // Arrange
            var entity = CreateTestEntity("Employee");
            entity.AddProperty(new EntityProperty { Name = "EmployeeId", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "FirstName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "LastName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Department", Type = "string" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("entity.EmployeeId = dto.EmployeeId;");
            generatedCode.Should().Contain("entity.FirstName = dto.FirstName;");
            generatedCode.Should().Contain("entity.LastName = dto.LastName;");
            generatedCode.Should().Contain("entity.Department = dto.Department;");
        }

        /// <summary>
        /// Verifies that <c>MapToDto</c> guards against a null entity with an <c>if (entity is null)</c> check that returns null.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldIncludeNullCheckInMapToDto()
        {
            // Arrange
            var entity = CreateTestEntity("Item");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("if (entity is null)");
            generatedCode.Should().Contain("return null;");
        }

        /// <summary>
        /// Verifies that <c>MapFromDto</c> guards against a null DTO with an <c>if (dto is null)</c> check that returns null.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldIncludeNullCheckInMapFromDto()
        {
            // Arrange
            var entity = CreateTestEntity("Address");
            entity.AddProperty(new EntityProperty { Name = "Street", Type = "string" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("if (dto is null)");
            generatedCode.Should().Contain("return null;");
        }

        /// <summary>
        /// Verifies that <c>MapToDtos</c> guards against a null collection by returning an empty <c>List</c> of the DTO type.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldIncludeNullCheckInMapToDtos()
        {
            // Arrange
            var entity = CreateTestEntity("Book");
            entity.AddProperty(new EntityProperty { Name = "Title", Type = "string" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("if (entities is null)");
            generatedCode.Should().Contain("return new List<BookDto>();");
        }

        /// <summary>
        /// Verifies that the generated <c>MapToDtos</c> body iterates the entities, maps each one through <c>MapToDto</c>,
        /// skips nulls, and adds the non-null DTOs to the results list.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldGenerateMapToDtosMethod()
        {
            // Arrange
            var entity = CreateTestEntity("Category");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public static IEnumerable<CategoryDto> MapToDtos(IEnumerable<Category>? entities)");
            generatedCode.Should().Contain("foreach (var e in entities)");
            generatedCode.Should().Contain("var dto = MapToDto(e);");
            generatedCode.Should().Contain("if (dto is not null)");
            generatedCode.Should().Contain("results.Add(dto);");
        }

        /// <summary>
        /// Verifies that an entity with no properties still yields a <c>Completed</c> result with no errors and declares both
        /// the DTO and mapper classes.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldHandleEntityWithNoProperties()
        {
            // Arrange
            var entity = CreateTestEntity("EmptyEntity");
            // No properties added

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Completed);
            result.Errors.Should().BeEmpty();
            result.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            // Should still generate valid code even with no properties
            result.GeneratedCode.Should().Contain("public sealed class EmptyEntityDto");
            result.GeneratedCode.Should().Contain("public sealed class EmptyEntityMapper");
        }

        /// <summary>
        /// Verifies that properties of common C# types (<c>string</c>, <c>int</c>, <c>bool</c>, <c>DateTime</c>, <c>Guid</c>, <c>decimal</c>)
        /// are emitted with the same type names in the generated DTO.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldHandleEntityWithDifferentTypes()
        {
            // Arrange
            var entity = CreateTestEntity("ComplexType");
            entity.AddProperty(new EntityProperty { Name = "StringProp", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "IntProp", Type = "int" });
            entity.AddProperty(new EntityProperty { Name = "BoolProp", Type = "bool" });
            entity.AddProperty(new EntityProperty { Name = "DateTimeProp", Type = "DateTime" });
            entity.AddProperty(new EntityProperty { Name = "GuidProp", Type = "Guid" });
            entity.AddProperty(new EntityProperty { Name = "DecimalProp", Type = "decimal" });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public string StringProp { get; set; }");
            generatedCode.Should().Contain("public int IntProp { get; set; }");
            generatedCode.Should().Contain("public bool BoolProp { get; set; }");
            generatedCode.Should().Contain("public DateTime DateTimeProp { get; set; }");
            generatedCode.Should().Contain("public Guid GuidProp { get; set; }");
            generatedCode.Should().Contain("public decimal DecimalProp { get; set; }");
        }

        /// <summary>
        /// Verifies that properties marked <c>IsNullable</c> are emitted with the <c>?</c> suffix on their value types.
        /// </summary>
        [Fact]
        public async Task GenerateMapperAsync_ShouldHandleNullableValueTypes()
        {
            // Arrange
            var entity = CreateTestEntity("NullableEntity");
            entity.AddProperty(new EntityProperty { Name = "NullableInt", Type = "int", IsNullable = true });
            entity.AddProperty(new EntityProperty { Name = "NullableDateTime", Type = "DateTime", IsNullable = true });
            entity.AddProperty(new EntityProperty { Name = "NullableDecimal", Type = "decimal", IsNullable = true });

            // Act
            var result = await _service.GenerateMapperAsync(entity, entity);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public int? NullableInt { get; set; }");
            generatedCode.Should().Contain("public DateTime? NullableDateTime { get; set; }");
            generatedCode.Should().Contain("public decimal? NullableDecimal { get; set; }");
        }

        /// <summary>
        /// Verifies that <c>GenerateAllMappersAsync</c> throws <see cref="ArgumentException"/> when the entity list is null.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_WithNullEntities_ShouldThrowArgumentException()
        {
            // Arrange
            List<Entity>? nullEntities = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateAllMappersAsync(nullEntities!));
        }

        /// <summary>
        /// Verifies that <c>GenerateAllMappersAsync</c> throws <see cref="ArgumentException"/> when the entity list is empty.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_WithEmptyEntities_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyEntities = new List<Entity>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateAllMappersAsync(emptyEntities));
        }

        /// <summary>
        /// Verifies that <c>GenerateAllMappersAsync</c> returns one result per entity, each with a <c>Completed</c> status and no errors.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_ShouldGenerateMappersForAllEntities()
        {
            // Arrange
            var entities = new List<Entity>
            {
                CreateTestEntity("User"),
                CreateTestEntity("Product"),
                CreateTestEntity("Order")
            };
            entities[0].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entities[1].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entities[2].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });

            // Act
            var results = await _service.GenerateAllMappersAsync(entities);

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(3);
            results.All(r => r.Status == GenerationStatus.Completed).Should().BeTrue();
            results.All(r => r.Errors.Count == 0).Should().BeTrue();
        }

        /// <summary>
        /// Verifies that each result returned by <c>GenerateAllMappersAsync</c> carries the <c>EntityName</c> of its source entity.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_ShouldReturnResultsWithCorrectEntityNames()
        {
            // Arrange
            var entities = new List<Entity>
            {
                CreateTestEntity("Customer"),
                CreateTestEntity("Vendor")
            };
            entities[0].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entities[1].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });

            // Act
            var results = await _service.GenerateAllMappersAsync(entities);

            // Assert
            var resultList = results.ToList();
            resultList.Select(r => r.EntityName).Should().BeEquivalentTo("Customer", "Vendor");
        }

        /// <summary>
        /// Verifies that every result returned by <c>GenerateAllMappersAsync</c> has <see cref="GeneratorType.Mapper"/> as its generator type.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_ShouldSetCorrectGeneratorType()
        {
            // Arrange
            var entities = new List<Entity> { CreateTestEntity("Entity") };
            entities[0].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });

            // Act
            var results = await _service.GenerateAllMappersAsync(entities);

            // Assert
            var resultList = results.ToList();
            resultList.Should().AllSatisfy(r =>
                r.GeneratorType.Should().Be(GeneratorType.Mapper)
            );
        }

        /// <summary>
        /// Verifies that the output file paths follow the <c>Mappers/{Name}Mapper.cs</c> pattern and keep the input order.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_ShouldSetCorrectOutputFilePaths()
        {
            // Arrange
            var entities = new List<Entity>
            {
                CreateTestEntity("User"),
                CreateTestEntity("Product")
            };
            entities[0].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entities[1].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });

            // Act
            var results = await _service.GenerateAllMappersAsync(entities);

            // Assert
            var resultList = results.ToList();
            resultList[0].OutputFilePath.Should().Be("Mappers/UserMapper.cs");
            resultList[1].OutputFilePath.Should().Be("Mappers/ProductMapper.cs");
        }

        /// <summary>
        /// Verifies that the reported <c>CodeLineCount</c> is greater than zero for a generated mapper.
        /// </summary>
        [Fact]
        public async Task GenerateAllMappersAsync_ShouldSetCorrectCodeLineCount()
        {
            // Arrange
            var entity = CreateTestEntity("Item");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Description", Type = "string" });

            var entities = new List<Entity> { entity };

            // Act
            var results = await _service.GenerateAllMappersAsync(entities);

            // Assert
            var resultList = results.ToList();
            resultList[0].CodeLineCount.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Creates a minimal <see cref="Entity"/> with the given name, the namespace <c>TestNamespace</c>,
        /// and a fixed description. The entity has no properties; tests add them as needed.
        /// </summary>
        /// <param name="name">The entity name, used for the class name and the generated DTO and mapper names.</param>
        /// <returns>A new <see cref="Entity"/> with no properties.</returns>
        private static Entity CreateTestEntity(string name)
        {
            return new Entity
            {
                Name = name,
                Namespace = "TestNamespace",
                Description = "Test entity for unit tests"
            };
        }
    }
}
