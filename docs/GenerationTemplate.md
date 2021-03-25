# GenerationTemplate
The `GenerationTemplate` type represents a template for generating code in the dotnet-source-generator-toolkit project. It encapsulates metadata and configuration options for code generation, allowing for customization and flexibility in the generation process. This type is crucial for defining the structure and behavior of generated code, making it a fundamental component of the toolkit.

## API
The `GenerationTemplate` type exposes the following public members:
* `Id`: A unique identifier for the template.
* `Name`: The name of the template.
* `Description`: A brief description of the template.
* `GeneratorType`: The type of generator associated with the template.
* `TemplateContent`: The content of the template.
* `OutputFileNamePattern`: A pattern for generating output file names.
* `OutputDirectory`: The directory where generated files will be output.
* `SupportedLanguages`: A list of languages supported by the template.
* `ConfigurationOptions`: A dictionary of configuration options for the template.
* `IsActive`: A flag indicating whether the template is active.
* `IsCustom`: A flag indicating whether the template is custom.
* `Version`: The version of the template.
* `CreatedAt` and `UpdatedAt`: Timestamps for when the template was created and last updated, respectively.
* `Author`: The author of the template, if applicable.
* `GenerateOutputFileName`: A method for generating an output file name based on the template's configuration.
* `Validate`: A method for validating the template's configuration, returning a collection of validation errors or warnings.
* `SupportsGeneratorType`: A flag indicating whether the template supports a specific generator type.

## Usage
Here are two examples of using the `GenerationTemplate` type in C#:
```csharp
// Example 1: Creating a new GenerationTemplate instance
var template = new GenerationTemplate
{
    Id = "my-template",
    Name = "My Template",
    Description = "A custom template for generating code.",
    GeneratorType = GeneratorType.CSharp,
    TemplateContent = "using System;",
    OutputFileNamePattern = "{0}.cs",
    OutputDirectory = "GeneratedCode",
    SupportedLanguages = new List<string> { "C#" },
    ConfigurationOptions = new Dictionary<string, string> { { "Option1", "Value1" } },
    IsActive = true,
    IsCustom = true,
    Version = 1,
    CreatedAt = DateTime.Now,
    UpdatedAt = DateTime.Now,
    Author = "John Doe"
};

// Example 2: Validating a GenerationTemplate instance
var templateToValidate = new GenerationTemplate
{
    Id = "invalid-template",
    Name = "Invalid Template",
    Description = "A template with invalid configuration.",
    GeneratorType = GeneratorType.CSharp,
    TemplateContent = "invalid content",
    OutputFileNamePattern = "{0}.invalid",
    OutputDirectory = "InvalidDirectory",
    SupportedLanguages = new List<string> { "InvalidLanguage" },
    ConfigurationOptions = new Dictionary<string, string> { { "InvalidOption", "InvalidValue" } },
    IsActive = false,
    IsCustom = false,
    Version = 0,
    CreatedAt = DateTime.MinValue,
    UpdatedAt = DateTime.MinValue,
    Author = null
};

var validationErrors = templateToValidate.Validate();
foreach (var error in validationErrors)
{
    Console.WriteLine(error);
}
```

## Notes
When working with `GenerationTemplate` instances, consider the following edge cases and thread-safety remarks:
* The `Validate` method may throw exceptions if the template's configuration is severely invalid. Handle these exceptions accordingly to ensure robust error handling.
* The `GenerateOutputFileName` method relies on the template's configuration being valid. If the configuration is invalid, this method may produce unexpected results or throw exceptions.
* The `SupportsGeneratorType` flag is used to determine whether a template supports a specific generator type. This flag should be checked before attempting to use a template with a particular generator type.
* `GenerationTemplate` instances are not inherently thread-safe. If multiple threads access and modify a `GenerationTemplate` instance concurrently, synchronization mechanisms should be employed to prevent data corruption and ensure thread safety.
