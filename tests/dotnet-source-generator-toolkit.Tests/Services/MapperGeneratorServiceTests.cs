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
    public class MapperGeneratorServiceTests
    {
        private readonly MapperGeneratorService _service;

        public MapperGeneratorServiceTests()
        {
            _service = new MapperGeneratorService(NullLogger<MapperGeneratorService>.Instance);
        }

        [Fact]
        public async Task GenerateMapperAsync_WithNullSourceEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            Entity? nullSource = null;
            var targetEntity = CreateTestEntity("User");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GenerateMapperAsync(nullSource!, targetEntity));
        }

        [Fact]
        public async Task GenerateMapperAsync_WithNullTargetEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            var sourceEntity = CreateTestEntity("User");
            Entity? nullTarget = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GenerateMapperAsync(sourceEntity, nullTarget!));
        }

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

        [Fact]
        public async Task GenerateAllMappersAsync_WithNullEntities_ShouldThrowArgumentException()
        {
            // Arrange
            List<Entity>? nullEntities = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateAllMappersAsync(nullEntities!));
        }

        [Fact]
        public async Task GenerateAllMappersAsync_WithEmptyEntities_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyEntities = new List<Entity>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateAllMappersAsync(emptyEntities));
        }

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
