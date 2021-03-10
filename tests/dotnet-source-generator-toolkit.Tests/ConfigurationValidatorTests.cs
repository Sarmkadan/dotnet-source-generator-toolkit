#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Configuration;
using FluentAssertions;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ConfigurationValidator class.
/// </summary>
public sealed class ConfigurationValidatorTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidatorTests"/> class.
    /// </summary>
    private readonly ConfigurationValidator _validator = new();

    /// <summary>
    /// Verifies that validating null options returns an invalid result with an error.
    /// </summary>
    [Fact]
    public void Validate_WithNullOptions_ReturnsInvalidResultWithError()
    {
        // Arrange & Act
        var result = _validator.Validate(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("cannot be null", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifies that validating valid options returns a valid result with no errors.
    /// </summary>
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

    /// <summary>
    /// Verifies that validating options with a timeout below the minimum adds a timeout error.
    /// </summary>
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

    /// <summary>
    /// Verifies that getting the default options returns options with expected values.
    /// </summary>
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

    /// <summary>
    /// Verifies that a mocked validator configured to return a failure verifies the call and returns the configured result.
    /// </summary>
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
