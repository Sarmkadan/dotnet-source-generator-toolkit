// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Configuration;
using FluentAssertions;
using Moq;

namespace DotNetSourceGeneratorToolkit.Tests;

public class ConfigurationValidatorTests
{
    private readonly ConfigurationValidator _validator = new();

    [Fact]
    public void Validate_WithNullOptions_ReturnsInvalidResultWithError()
    {
        // Arrange & Act
        var result = _validator.Validate(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("cannot be null", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithValidOptions_ReturnsValidResultWithNoErrors()
    {
        // Arrange
        var options = new ToolkitOptions
        {
            CacheExpirationMinutes = 30,
            MaxDegreeOfParallelism = 2,
            OperationTimeoutSeconds = 60,
            CodeFormattingLineLength = 120,
            OutputDirectory = "./output",
        };

        // Act
        var result = _validator.Validate(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WhenTimeoutBelowMinimum_AddsTimeoutError()
    {
        // Arrange
        var options = _validator.GetDefaults();
        options.OperationTimeoutSeconds = 5;

        // Act
        var result = _validator.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("timeout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetDefaults_ReturnsOptionsWithExpectedValues()
    {
        // Act
        var defaults = _validator.GetDefaults();

        // Assert
        defaults.OutputDirectory.Should().Be("./Generated");
        defaults.EnableCaching.Should().BeTrue();
        defaults.CacheExpirationMinutes.Should().Be(60);
        defaults.GenerateInterfaces.Should().BeTrue();
    }

    [Fact]
    public void MockedValidator_WhenConfiguredToReturnFailure_VerifiesCallAndReturnsConfiguredResult()
    {
        // Arrange
        var mockValidator = new Mock<IConfigurationValidator>();
        var failedResult = new ValidationResult { IsValid = false };
        failedResult.AddError("Simulated validation failure");

        mockValidator
            .Setup(v => v.Validate(It.IsAny<ToolkitOptions>()))
            .Returns(failedResult);

        var options = new ToolkitOptions { OutputDirectory = string.Empty };

        // Act
        var result = mockValidator.Object.Validate(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e == "Simulated validation failure");
        mockValidator.Verify(v => v.Validate(options), Times.Once);
    }
}
