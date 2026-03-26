// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Constants;

/// <summary>
/// Constants used throughout the source generation toolkit.
/// </summary>
public static class GenerationConstants
{
    // Generator Type Names
    public const string REPOSITORY_GENERATOR = "Repository";
    public const string MAPPER_GENERATOR = "Mapper";
    public const string VALIDATOR_GENERATOR = "Validator";
    public const string SERIALIZER_GENERATOR = "Serializer";
    public const string DTO_GENERATOR = "DTO";
    public const string SERVICE_GENERATOR = "Service";

    // File Extensions
    public const string CSHARP_EXTENSION = ".cs";
    public const string JSON_EXTENSION = ".json";
    public const string XML_EXTENSION = ".xml";

    // Default Directories
    public const string DEFAULT_OUTPUT_DIR = "Generated";
    public const string DEFAULT_TEMPLATES_DIR = "Templates";
    public const string REPOSITORIES_DIR = "Repositories";
    public const string MAPPERS_DIR = "Mappers";
    public const string VALIDATORS_DIR = "Validators";
    public const string SERIALIZERS_DIR = "Serializers";
    public const string SERVICES_DIR = "Services";
    public const string DTOS_DIR = "DTOs";

    // Naming Conventions
    public const string INTERFACE_PREFIX = "I";
    public const string REPOSITORY_SUFFIX = "Repository";
    public const string MAPPER_SUFFIX = "Mapper";
    public const string VALIDATOR_SUFFIX = "Validator";
    public const string SERIALIZER_SUFFIX = "Serializer";
    public const string DTO_SUFFIX = "Dto";
    public const string SERVICE_SUFFIX = "Service";

    // Attribute Names
    public const string GENERATE_REPOSITORY_ATTR = "GenerateRepository";
    public const string GENERATE_MAPPER_ATTR = "GenerateMapper";
    public const string GENERATE_VALIDATOR_ATTR = "GenerateValidator";
    public const string GENERATE_SERIALIZER_ATTR = "GenerateSerializer";
    public const string TABLE_NAME_ATTR = "TableName";
    public const string COLUMN_NAME_ATTR = "ColumnName";

    // Code Generation Markers
    public const string GENERATED_FILE_MARKER = "// AUTO-GENERATED - DO NOT EDIT MANUALLY";
    public const string GENERATION_TIMESTAMP_FORMAT = "yyyy-MM-dd HH:mm:ss";

    // Validation Rules
    public const int MAX_ENTITY_NAME_LENGTH = 256;
    public const int MAX_PROPERTY_NAME_LENGTH = 256;
    public const int MIN_ENTITY_PROPERTIES = 1;
    public const int MAX_PROPERTIES_PER_ENTITY = 500;

    // Performance
    public const int DEFAULT_BATCH_SIZE = 10;
    public const int DEFAULT_TIMEOUT_SECONDS = 30;
    public const int MAX_RETRY_ATTEMPTS = 3;

    // Serialization Formats
    public const string JSON_FORMAT = "json";
    public const string XML_FORMAT = "xml";
    public const string BINARY_FORMAT = "binary";

    // Target Frameworks
    public const string NETCORE_6_0 = "net6.0";
    public const string NETCORE_7_0 = "net7.0";
    public const string NETCORE_8_0 = "net8.0";
    public const string NETCORE_9_0 = "net9.0";
    public const string NETCORE_10_0 = "net10.0";
}

/// <summary>
/// Error message templates and codes.
/// </summary>
public static class ErrorMessages
{
    public const string ENTITY_NOT_FOUND = "Entity '{0}' not found";
    public const string INVALID_ENTITY_NAME = "Entity name '{0}' is invalid";
    public const string DUPLICATE_PROPERTY = "Property '{0}' already exists on entity '{1}'";
    public const string GENERATION_FAILED = "Code generation failed for {0}: {1}";
    public const string FILE_NOT_FOUND = "File not found: {0}";
    public const string INVALID_CONFIGURATION = "Invalid configuration: {0}";
    public const string VALIDATION_FAILED = "Validation failed with {0} errors";
    public const string OPERATION_TIMEOUT = "Operation timed out after {0} seconds";
}

/// <summary>
/// Success message templates.
/// </summary>
public static class SuccessMessages
{
    public const string GENERATION_COMPLETED = "Successfully generated code for {0}";
    public const string ENTITY_ANALYZED = "Entity '{0}' analyzed with {1} properties";
    public const string PROJECT_ANALYZED = "Project analyzed with {0} entities";
    public const string FILE_WRITTEN = "Generated file written to: {0}";
}

/// <summary>
/// Log message templates.
/// </summary>
public static class LogMessages
{
    public const string STARTING_ANALYSIS = "Starting analysis of project: {0}";
    public const string STARTING_GENERATION = "Starting code generation for {0}";
    public const string PROCESSING_ENTITY = "Processing entity: {0}";
    public const string GENERATION_DURATION = "Generation completed in {0}ms";
    public const string TOTAL_GENERATED_CODE = "Total lines of code generated: {0}";
}
