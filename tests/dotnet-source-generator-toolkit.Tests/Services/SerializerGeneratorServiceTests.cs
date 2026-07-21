#nullable enable

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
    public class SerializerGeneratorServiceTests
    {
        private readonly SerializerGeneratorService _service;

        public SerializerGeneratorServiceTests()
        {
            _service = new SerializerGeneratorService(NullLogger<SerializerGeneratorService>.Instance);
        }

        [Fact]
        public async Task GenerateSerializerAsync_WithNullEntity_ShouldThrowArgumentNullException()
        {
            // Arrange
            Entity? nullEntity = null;
            var format = SerializerFormat.Json;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.GenerateSerializerAsync(nullEntity!, format));
        }

        [Fact]
        public async Task GenerateSerializerAsync_WithValidEntityAndJsonFormat_ShouldGenerateValidJsonSerializer()
        {
            // Arrange
            var entity = CreateTestEntity("User");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Email", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Age", Type = "int" });

            var format = SerializerFormat.Json;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Completed);
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            result.OutputFilePath.Should().Be("Serializers/UserJsonSerializer.cs");
            result.EntityName.Should().Be("User");
            result.GeneratorType.Should().Be(GeneratorType.Serializer);
            result.Metadata["Format"].Should().Be("Json");
        }

        [Fact]
        public async Task GenerateSerializerAsync_WithValidEntityAndXmlFormat_ShouldGenerateValidXmlSerializer()
        {
            // Arrange
            var entity = CreateTestEntity("Product");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "Guid", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Price", Type = "decimal" });
            entity.AddProperty(new EntityProperty { Name = "IsActive", Type = "bool" });

            var format = SerializerFormat.Xml;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Completed);
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            result.OutputFilePath.Should().Be("Serializers/ProductXmlSerializer.cs");
            result.EntityName.Should().Be("Product");
            result.GeneratorType.Should().Be(GeneratorType.Serializer);
            result.Metadata["Format"].Should().Be("Xml");
        }

        [Fact]
        public async Task GenerateSerializerAsync_WithValidEntityAndBinaryFormat_ShouldGenerateValidBinarySerializer()
        {
            // Arrange
            var entity = CreateTestEntity("Document");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "Guid", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Title", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Content", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Size", Type = "long" });

            var format = SerializerFormat.Binary;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Completed);
            result.Errors.Should().BeEmpty();
            result.Warnings.Should().BeEmpty();
            result.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            result.OutputFilePath.Should().Be("Serializers/DocumentBinarySerializer.cs");
            result.EntityName.Should().Be("Document");
            result.GeneratorType.Should().Be(GeneratorType.Serializer);
            result.Metadata["Format"].Should().Be("Binary");
        }

        [Fact]
        public async Task GenerateSerializerAsync_WithInvalidFormat_ShouldThrowNotSupportedException()
        {
            // Arrange
            var entity = CreateTestEntity("TestEntity");
            var invalidFormat = (SerializerFormat)999; // Invalid enum value

            // Act
            var result = await _service.GenerateSerializerAsync(entity, invalidFormat);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Failed);
            result.Errors.Should().ContainSingle();
            result.Errors[0].Should().Contain("Serializer format not supported");
        }

        [Fact]
        public async Task GenerateSerializerAsync_ShouldGenerateJsonSerializerWithCorrectMethods()
        {
            // Arrange
            var entity = CreateTestEntity("Customer");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "FullName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Email", Type = "string" });

            var format = SerializerFormat.Json;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public sealed class CustomerJsonSerializer");
            generatedCode.Should().Contain("public static string Serialize(Customer entity)");
            generatedCode.Should().Contain("public static Customer Deserialize(string json)");
            generatedCode.Should().Contain("public static JsonElement ToJsonElement(Customer entity)");
            generatedCode.Should().Contain("System.Text.Json");
            generatedCode.Should().Contain("JsonSerializerOptions");
            generatedCode.Should().Contain("PropertyNameCaseInsensitive = true");
        }

        [Fact]
        public async Task GenerateSerializerAsync_ShouldGenerateXmlSerializerWithCorrectMethods()
        {
            // Arrange
            var entity = CreateTestEntity("Order");
            entity.AddProperty(new EntityProperty { Name = "OrderId", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "CustomerName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "TotalAmount", Type = "decimal" });

            var format = SerializerFormat.Xml;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public sealed class OrderXmlSerializer");
            generatedCode.Should().Contain("public static string Serialize(Order entity)");
            generatedCode.Should().Contain("public static Order Deserialize(string xml)");
            generatedCode.Should().Contain("System.Xml.Linq");
            generatedCode.Should().Contain("XDocument");
            generatedCode.Should().Contain("XElement");
        }

        [Fact]
        public async Task GenerateSerializerAsync_ShouldGenerateBinarySerializerWithCorrectMethods()
        {
            // Arrange
            var entity = CreateTestEntity("File");
            entity.AddProperty(new EntityProperty { Name = "FileId", Type = "Guid", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "FileName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "FileSize", Type = "long" });

            var format = SerializerFormat.Binary;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("public sealed class FileBinarySerializer");
            generatedCode.Should().Contain("public static byte[] Serialize(File entity)");
            generatedCode.Should().Contain("public static File Deserialize(byte[] data)");
            generatedCode.Should().Contain("public static int GetSerializedSize(File entity)");
            generatedCode.Should().Contain("BinaryFormatter");
            generatedCode.Should().Contain("MemoryStream");
        }

        [Fact]
        public async Task GenerateAllSerializersAsync_WithNullEntities_ShouldThrowArgumentException()
        {
            // Arrange
            List<Entity>? nullEntities = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateAllSerializersAsync(nullEntities!));
        }

        [Fact]
        public async Task GenerateAllSerializersAsync_WithEmptyEntities_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyEntities = new List<Entity>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GenerateAllSerializersAsync(emptyEntities));
        }

        [Fact]
        public async Task GenerateAllSerializersAsync_WithValidEntities_ShouldGenerateSerializersForAllFormats()
        {
            // Arrange
            var entities = new List<Entity>
            {
                CreateTestEntity("User"),
                CreateTestEntity("Product")
            };

            entities[0].AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entities[0].AddProperty(new EntityProperty { Name = "Name", Type = "string" });

            entities[1].AddProperty(new EntityProperty { Name = "Id", Type = "Guid", IsPrimaryKey = true });
            entities[1].AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entities[1].AddProperty(new EntityProperty { Name = "Price", Type = "decimal" });

            // Act
            var results = await _service.GenerateAllSerializersAsync(entities);

            // Assert
            results.Should().NotBeNull();
            results.Should().HaveCount(4); // 2 entities * 2 formats (Json, Xml)

            var jsonResults = results.Where(r => r.Metadata["Format"] == "Json").ToList();
            jsonResults.Should().HaveCount(2);
            jsonResults.Should().AllSatisfy(r =>
            {
                r.Status.Should().Be(GenerationStatus.Completed);
                r.Errors.Should().BeEmpty();
                r.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            });

            var xmlResults = results.Where(r => r.Metadata["Format"] == "Xml").ToList();
            xmlResults.Should().HaveCount(2);
            xmlResults.Should().AllSatisfy(r =>
            {
                r.Status.Should().Be(GenerationStatus.Completed);
                r.Errors.Should().BeEmpty();
                r.GeneratedCode.Should().NotBeNullOrWhiteSpace();
            });
        }

        [Fact]
        public async Task GenerateSerializerAsync_ShouldHandleEntityWithMultiplePropertyTypes()
        {
            // Arrange
            var entity = CreateTestEntity("ComplexEntity");
            entity.AddProperty(new EntityProperty { Name = "Id", Type = "int", IsPrimaryKey = true });
            entity.AddProperty(new EntityProperty { Name = "Name", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "IsActive", Type = "bool" });
            entity.AddProperty(new EntityProperty { Name = "Price", Type = "decimal" });
            entity.AddProperty(new EntityProperty { Name = "CreatedDate", Type = "DateTime" });
            entity.AddProperty(new EntityProperty { Name = "GuidValue", Type = "Guid" });

            var format = SerializerFormat.Json;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(GenerationStatus.Completed);
            result.GeneratedCode.Should().Contain("public static string Serialize(ComplexEntity entity)");
            result.GeneratedCode.Should().Contain("public static ComplexEntity Deserialize(string json)");
            result.GeneratedCode.Should().Contain("JsonSerializer.Deserialize<ComplexEntity>");
            result.GeneratedCode.Should().Contain("Serialize(ComplexEntity entity)");
        }

        [Fact]
        public async Task GenerateSerializerAsync_ShouldIncludeCamelCasePropertyNamesInJson()
        {
            // Arrange
            var entity = CreateTestEntity("UserProfile");
            entity.AddProperty(new EntityProperty { Name = "FirstName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "LastName", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "EMailAddress", Type = "string" });

            var format = SerializerFormat.Json;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);
            var generatedCode = result.GeneratedCode;

            // Assert - The serializer uses JsonSerializer.Serialize which will handle camelCase conversion
            generatedCode.Should().Contain("JsonSerializerOptions");
            generatedCode.Should().Contain("PropertyNameCaseInsensitive = true");
            generatedCode.Should().Contain("public static string Serialize(UserProfile entity)");
        }

        [Fact]
        public async Task GenerateSerializerAsync_ShouldIncludeAllPropertiesInXml()
        {
            // Arrange
            var entity = CreateTestEntity("Article");
            entity.AddProperty(new EntityProperty { Name = "Title", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Content", Type = "string" });
            entity.AddProperty(new EntityProperty { Name = "Views", Type = "int" });

            var format = SerializerFormat.Xml;

            // Act
            var result = await _service.GenerateSerializerAsync(entity, format);
            var generatedCode = result.GeneratedCode;

            // Assert
            generatedCode.Should().Contain("new XElement(\"Title\"");
            generatedCode.Should().Contain("new XElement(\"Content\"");
            generatedCode.Should().Contain("new XElement(\"Views\"");
            generatedCode.Should().Contain("root.Element(\"Title\")?.Value");
            generatedCode.Should().Contain("root.Element(\"Content\")?.Value");
            generatedCode.Should().Contain("root.Element(\"Views\")?.Value");
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
