#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Configuration;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ConfigurationLoader .env-style configuration file parsing.
/// Tests comments, blank lines, and key=value pairs.
/// </summary>
public sealed class ConfigurationLoaderEnvTests
{
    private readonly Mock<ILogger<ConfigurationLoader>> _mockLogger;
    private readonly Mock<IConfigurationValidator> _mockValidator;
    private readonly ConfigurationLoader _loader;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationLoaderEnvTests"/> class.
    /// </summary>
    public ConfigurationLoaderEnvTests()
    {
        _mockLogger = new Mock<ILogger<ConfigurationLoader>>();
        _mockValidator = new Mock<IConfigurationValidator>();
        _mockValidator.Setup(v => v.GetDefaults()).Returns(new ToolkitOptions());
        _mockValidator.Setup(v => v.Validate(It.IsAny<ToolkitOptions>())).Returns(new ValidationResult { IsValid = true });

        _loader = new ConfigurationLoader(_mockLogger.Object, _mockValidator.Object);
    }

    /// <summary>
    /// Verifies that parsing an empty .env file returns null.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithEmptyContent_ReturnsNull()
    {
        // Arrange
        var emptyContent = string.Empty;

        // Act
        var result = ParseEnvFileInternal(emptyContent);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that parsing a .env file with only whitespace returns null.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithWhitespaceContent_ReturnsNull()
    {
        // Arrange
        var whitespaceContent = "   \n\n  \r\n  ";

        // Act
        var result = ParseEnvFileInternal(whitespaceContent);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that parsing a .env file with only comments and blank lines returns null.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithOnlyCommentsAndBlanks_ReturnsNull()
    {
        // Arrange
        var commentContent = "# This is a comment\n\n# Another comment\n   \n# Final comment";

        // Act
        var result = ParseEnvFileInternal(commentContent);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that parsing a .env file with comments and blank lines works correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithCommentsAndBlankLines_ParsesCorrectly()
    {
        // Arrange
        var content = "# Configuration file for toolkit\n\nENABLE_CACHING=true\n\n# Cache settings\nCACHE_EXPIRATION_MINUTES=120\n\n";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(120);
    }

    /// <summary>
    /// Verifies that parsing a .env file with blank lines is handled correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithBlankLines_ParsesCorrectly()
    {
        // Arrange
        var content = "ENABLE_CACHING=true\n\n\nCACHE_EXPIRATION_MINUTES=90";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(90);
    }

    /// <summary>
    /// Verifies that parsing a .env file with various comment styles works.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithVariousCommentStyles_ParsesCorrectly()
    {
        // Arrange
        var content = "# Full line comment\nENABLE_CACHING=true\n// Inline comment style\nCACHE_EXPIRATION_MINUTES=120\n  # Comment with leading spaces\nOUTPUT_DIRECTORY=\"./custom-output\"";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(120);
        result.OutputDirectory.Should().Be("./custom-output");
    }

    /// <summary>
    /// Verifies that parsing boolean values works correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithBooleanValues_ParsesCorrectly()
    {
        // Arrange
        var content = "ENABLE_CACHING=true\nENABLE_CODE_FORMATTING=false\nVERBOSE_LOGGING=true\nBACKUP_EXISTING_FILES=false\nGENERATE_DTOS=true\nGENERATE_INTERFACES=false\nGENERATE_XML_COMMENTS=true";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.EnableCodeFormatting.Should().BeFalse();
        result.VerboseLogging.Should().BeTrue();
        result.BackupExistingFiles.Should().BeFalse();
        result.GenerateDtos.Should().BeTrue();
        result.GenerateInterfaces.Should().BeFalse();
        result.GenerateXmlComments.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that parsing integer values works correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithIntegerValues_ParsesCorrectly()
    {
        // Arrange
        var content = "CACHE_EXPIRATION_MINUTES=45\nCODE_FORMATTING_LINE_LENGTH=120\nMAX_DEGREE_OF_PARALLELISM=8\nOPERATION_TIMEOUT_SECONDS=600";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.CacheExpirationMinutes.Should().Be(45);
        result.CodeFormattingLineLength.Should().Be(120);
        result.MaxDegreeOfParallelism.Should().Be(8);
        result.OperationTimeoutSeconds.Should().Be(600);
    }

    /// <summary>
    /// Verifies that parsing string values works correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithStringValues_ParsesCorrectly()
    {
        // Arrange
        var content = "DEFAULT_NAMESPACE=\"MyApp.Core\"\nOUTPUT_DIRECTORY=\"./generated\"";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.DefaultNamespace.Should().Be("MyApp.Core");
        result.OutputDirectory.Should().Be("./generated");
    }

    /// <summary>
    /// Verifies that parsing values with quotes works correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithQuotedValues_ParsesCorrectly()
    {
        // Arrange
        var content = "DEFAULT_NAMESPACE=\"MyApp.Core\"\nOUTPUT_DIRECTORY=\"  ./output  \"";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.DefaultNamespace.Should().Be("MyApp.Core");
        result.OutputDirectory.Should().Be("./output");
    }

    /// <summary>
    /// Verifies that parsing with case-insensitive keys works.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithCaseInsensitiveKeys_ParsesCorrectly()
    {
        // Arrange
        var content = "enablecaching=true\nCACHE_EXPIRATION_MINUTES=90\nCodeFormatting=false";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(90);
        result.EnableCodeFormatting.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that parsing with alternative key names works.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithAlternativeKeyNames_ParsesCorrectly()
    {
        // Arrange
        var content = "enable_caching=true\ncache_expiration=120\ncode_formatting=false\nline_length=80\nverbose=true\nparallelism=4\ntimeout=300\ndtos=true\nnamespace=\"Test.Namespace\"\noutputdir=\"./out\"\nbackup=false\ninterfaces=true\nxmlcomments=false";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(120);
        result.EnableCodeFormatting.Should().BeFalse();
        result.CodeFormattingLineLength.Should().Be(80);
        result.VerboseLogging.Should().BeTrue();
        result.MaxDegreeOfParallelism.Should().Be(4);
        result.OperationTimeoutSeconds.Should().Be(300);
        result.GenerateDtos.Should().BeTrue();
        result.DefaultNamespace.Should().Be("Test.Namespace");
        result.OutputDirectory.Should().Be("./out");
        result.BackupExistingFiles.Should().BeFalse();
        result.GenerateInterfaces.Should().BeTrue();
        result.GenerateXmlComments.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that invalid lines are skipped gracefully.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithInvalidLines_SkipsGracefully()
    {
        // Arrange
        var content = "ENABLE_CACHING=true\nINVALID_LINE\nANOTHER_INVALID\nCACHE_EXPIRATION_MINUTES=90";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(90);
    }

    /// <summary>
    /// Verifies that empty values are handled correctly.
    /// </summary>
    [Fact]
    public void ParseEnvFile_WithEmptyValues_HandlesCorrectly()
    {
        // Arrange
        var content = "ENABLE_CACHING=\nCACHE_EXPIRATION_MINUTES=\nOUTPUT_DIRECTORY=";

        // Act
        var result = ParseEnvFileInternal(content);

        // Assert - empty values are valid key=value pairs, but won't parse to valid values
        result.Should().NotBeNull();
        result!.EnableCaching.Should().BeTrue(); // Default value preserved (empty string doesn't parse as bool)
        result.CacheExpirationMinutes.Should().Be(60); // Default value preserved
        result.OutputDirectory.Should().Be("./Generated"); // Default value preserved
    }

    /// <summary>
    /// Helper method to parse env content using the private ParseEnvFile method via reflection.
    /// </summary>
    private ToolkitOptions? ParseEnvFileInternal(string content)
    {
        // Use reflection to call the private ParseEnvFile method
        var method = typeof(ConfigurationLoader).GetMethod(
            "ParseEnvFile",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("ParseEnvFile method not found");
        }

        return (ToolkitOptions?)method.Invoke(null, new object[] { content });
    }

    /// <summary>
    /// Helper method to merge env options using the private MergeEnvOptions method via reflection.
    /// </summary>
    private ToolkitOptions MergeEnvOptionsInternal(ToolkitOptions defaults, ToolkitOptions envOptions)
    {
        // Use reflection to call the private MergeEnvOptions method
        var method = typeof(ConfigurationLoader).GetMethod(
            "MergeEnvOptions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
        {
            throw new InvalidOperationException("MergeEnvOptions method not found");
        }

        return (ToolkitOptions)method.Invoke(null, new object[] { defaults, envOptions })!;
    }

    /// <summary>
    /// Verifies that LoadEnvAsync with null path returns defaults.
    /// </summary>
    [Fact]
    public async Task LoadEnvAsync_WithNullPath_ReturnsDefaults()
    {
        // Arrange
        // Mock validator already returns defaults

        // Act
        var result = await _loader.LoadEnvAsync(null);

        // Assert
        result.Should().NotBeNull();
        // Should get defaults from validator
        result.EnableCaching.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that LoadEnvAsync with non-existent file returns defaults.
    /// </summary>
    [Fact]
    public async Task LoadEnvAsync_WithNonExistentFile_ReturnsDefaults()
    {
        // Arrange
        var nonExistentPath = "/path/that/does/not/exist.env";

        // Act
        var result = await _loader.LoadEnvAsync(nonExistentPath);

        // Assert
        result.Should().NotBeNull();
        result.EnableCaching.Should().BeTrue(); // Default value
    }

    /// <summary>
    /// Verifies that MergeEnvOptions correctly merges env options with defaults.
    /// </summary>
    [Fact]
    public void MergeEnvOptions_MergesCorrectly()
    {
        // Arrange
        var defaults = new ToolkitOptions
        {
            EnableCaching = false,
            CacheExpirationMinutes = 30,
            EnableCodeFormatting = false,
            CodeFormattingLineLength = 80,
            VerboseLogging = false,
            MaxDegreeOfParallelism = 2,
            OperationTimeoutSeconds = 60,
            GenerateDtos = false,
            DefaultNamespace = "Default.Namespace",
            OutputDirectory = "./default-output",
            BackupExistingFiles = false,
            GenerateInterfaces = false,
            GenerateXmlComments = false,
        };

        var envOptions = new ToolkitOptions
        {
            EnableCaching = true,
            CacheExpirationMinutes = 120,
            EnableCodeFormatting = true,
            CodeFormattingLineLength = 120,
            VerboseLogging = true,
            MaxDegreeOfParallelism = 8,
            OperationTimeoutSeconds = 300,
            GenerateDtos = true,
            DefaultNamespace = "Env.Namespace",
            OutputDirectory = "./env-output",
            BackupExistingFiles = true,
            GenerateInterfaces = true,
            GenerateXmlComments = true,
        };

        // Act
        var result = MergeEnvOptionsInternal(defaults, envOptions);

        // Assert
        result.EnableCaching.Should().BeTrue();
        result.CacheExpirationMinutes.Should().Be(120);
        result.EnableCodeFormatting.Should().BeTrue();
        result.CodeFormattingLineLength.Should().Be(120);
        result.VerboseLogging.Should().BeTrue();
        result.MaxDegreeOfParallelism.Should().Be(8);
        result.OperationTimeoutSeconds.Should().Be(300);
        result.GenerateDtos.Should().BeTrue();
        result.DefaultNamespace.Should().Be("Env.Namespace");
        result.OutputDirectory.Should().Be("./env-output");
        result.BackupExistingFiles.Should().BeTrue();
        result.GenerateInterfaces.Should().BeTrue();
        result.GenerateXmlComments.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that MergeEnvOptions preserves defaults when env options are not set.
    /// </summary>
    [Fact]
    public void MergeEnvOptions_PreservesDefaultsWhenNotSet()
    {
        // Arrange
        var processorCount = Environment.ProcessorCount;
        var defaults = new ToolkitOptions
        {
            EnableCaching = true,
            CacheExpirationMinutes = 60,
            EnableCodeFormatting = true,
            CodeFormattingLineLength = 100,
            VerboseLogging = false,
            MaxDegreeOfParallelism = processorCount,
            OperationTimeoutSeconds = 300,
            GenerateDtos = false,
            DefaultNamespace = "Default.Namespace",
            OutputDirectory = "./Generated",
            BackupExistingFiles = true,
            GenerateInterfaces = true,
            GenerateXmlComments = true,
        };

        var envOptions = new ToolkitOptions(); // All defaults

        // Act
        var result = MergeEnvOptionsInternal(defaults, envOptions);

        // Assert - should preserve defaults
        result.Should().BeEquivalentTo(defaults);
    }
}
