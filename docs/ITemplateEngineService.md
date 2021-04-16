# ITemplateEngineService

Defines a contract for rendering string-based templates with support for custom filters and file-based template sources. Implementations are expected to handle template parsing, variable substitution, and filter application in an asynchronous manner, with a synchronous validation step to verify template correctness before rendering.

## API

### TemplateEngineService

A concrete implementation of `ITemplateEngineService` that provides the actual template processing logic. This type is obtained through dependency injection or direct instantiation and exposes all members defined by the interface.

### public async Task\<string\> RenderAsync

Renders a template string by applying the provided model data and any registered filters.

**Parameters:**
- `string template` — The raw template string containing placeholders and filter expressions.
- `object model` — The data object whose properties are used to resolve placeholders in the template.

**Returns:** A `Task<string>` that resolves to the fully rendered output string.

**Exceptions:**
- `ArgumentNullException` — Thrown when `template` is null.
- `TemplateParseException` — Thrown when the template contains syntax errors that prevent parsing.
- `InvalidOperationException` — Thrown when a referenced filter has not been registered.

### public async Task\<string\> RenderFromFileAsync

Reads a template from a file path and renders it with the provided model data.

**Parameters:**
- `string filePath` — The absolute or relative path to the template file.
- `object model` — The data object used for placeholder resolution.

**Returns:** A `Task<string>` that resolves to the rendered output.

**Exceptions:**
- `ArgumentNullException` — Thrown when `filePath` is null.
- `FileNotFoundException` — Thrown when the specified file does not exist.
- `TemplateParseException` — Thrown when the file content cannot be parsed as a valid template.
- `InvalidOperationException` — Thrown when a referenced filter has not been registered.

### public bool ValidateTemplate

Synchronously checks whether a template string is syntactically valid and all referenced filters are registered.

**Parameters:**
- `string template` — The template string to validate.

**Returns:** `true` if the template is valid and can be rendered without parse errors; `false` otherwise.

**Exceptions:**
- `ArgumentNullException` — Thrown when `template` is null.

### public void RegisterFilter

Registers a custom filter function that can be invoked from within templates by name.

**Parameters:**
- `string filterName` — The unique name used to reference the filter in template expressions.
- `Func<object, object> filter` — The delegate that transforms the input value.

**Exceptions:**
- `ArgumentNullException` — Thrown when `filterName` or `filter` is null.
- `ArgumentException` — Thrown when `filterName` is empty or consists only of whitespace.
- `InvalidOperationException` — Thrown when a filter with the same name has already been registered.

## Usage

### Example 1: Inline Template Rendering with Custom Filter

```csharp
var engine = new TemplateEngineService();

// Register a filter that reverses a string
engine.RegisterFilter("reverse", value =>
{
    var str = value?.ToString() ?? string.Empty;
    var chars = str.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);
});

var template = "Hello, {{ Name | reverse }}!";
var model = new { Name = "World" };

if (engine.ValidateTemplate(template))
{
    string result = await engine.RenderAsync(template, model);
    Console.WriteLine(result); // Output: Hello, dlroW!
}
```

### Example 2: File-Based Rendering with Validation

```csharp
var engine = new TemplateEngineService();

engine.RegisterFilter("uppercase", value => value?.ToString()?.ToUpper() ?? string.Empty);

string templatePath = "./Templates/welcome.template";
// File content: "Welcome, {{ UserName | uppercase }}!"

var model = new { UserName = "Alice" };

string templateContent = File.ReadAllText(templatePath);

if (engine.ValidateTemplate(templateContent))
{
    string result = await engine.RenderFromFileAsync(templatePath, model);
    Console.WriteLine(result); // Output: Welcome, ALICE!
}
else
{
    Console.WriteLine("Template validation failed.");
}
```

## Notes

- `ValidateTemplate` performs only syntactic checks and filter existence verification; it does not validate the model's property availability. Rendering may still fail at runtime if the model lacks a property referenced in the template.
- `RegisterFilter` is not thread-safe by default. Concurrent registrations from multiple threads may result in race conditions. Callers should serialize filter registration during initialization.
- `RenderAsync` and `RenderFromFileAsync` are safe to call concurrently after all filters have been registered, provided the underlying implementation does not mutate shared state during rendering.
- When `RenderFromFileAsync` is called, the file is read at the time of invocation. External changes to the file between validation and rendering can lead to a `TemplateParseException` even if `ValidateTemplate` previously returned `true`.
- Filter functions receive the raw value from the model property and must handle null inputs gracefully to avoid runtime exceptions during rendering.
