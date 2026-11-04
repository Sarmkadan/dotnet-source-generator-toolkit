#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents metadata and structure information about a .NET project
/// that has been analyzed for source generation.
/// </summary>
public sealed class ProjectInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the project.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file system path to the project.
    /// </summary>
    public string ProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target framework of the project.
    /// </summary>
    public string TargetFramework { get; set; } = "net10.0";

    /// <summary>
    /// Gets the collection of entities defined in the project.
    /// </summary>
    public List<Entity> Entities { get; } = [];

    /// <summary>
    /// Gets the collection of generation templates applicable to the project.
    /// </summary>
    public List<GenerationTemplate> Templates { get; } = [];

    /// <summary>
    /// Gets the collection of generation results produced for the project.
    /// </summary>
    public List<GenerationResult> GenerationResults { get; } = [];

    /// <summary>
    /// Gets the dictionary of project properties.
    /// </summary>
    public Dictionary<string, string> ProjectProperties { get; } = [];

    /// <summary>
    /// Gets the list of referenced assembly names.
    /// </summary>
    public List<string> ReferencedAssemblies { get; } = [];

    /// <summary>
    /// Gets or sets the root namespace of the project.
    /// </summary>
    public string RootNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the project was analyzed.
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether the project analysis encountered errors.
    /// </summary>
    public bool HasAnalysisErrors { get; set; }

    /// <summary>
    /// Gets the list of analysis error messages.
    /// </summary>
    public List<string> AnalysisErrors { get; } = [];

    /// <summary>
    /// Adds an entity to the project and validates it.
    /// </summary>
    public void AddEntity(Entity entity)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var validationErrors = entity.Validate();
        if (validationErrors.Any())
            throw new InvalidOperationException($"Entity validation failed: {string.Join(", ", validationErrors)}");

        if (Entities.Any(e => e.Name == entity.Name))
            throw new InvalidOperationException($"Entity '{entity.Name}' already exists in the project.");

        Entities.Add(entity);
    }

    /// <summary>
    /// Adds a generation template to the project.
    /// </summary>
    public void AddTemplate(GenerationTemplate template)
    {
        if (template is null)
            throw new ArgumentNullException(nameof(template));

        var validationErrors = template.Validate();
        if (validationErrors.Any())
            throw new InvalidOperationException($"Template validation failed: {string.Join(", ", validationErrors)}");

        Templates.Add(template);
    }

    /// <summary>
    /// Records a generation result for tracking and auditing.
    /// </summary>
    public void RecordGenerationResult(GenerationResult result)
    {
        if (result is not null)
            GenerationResults.Add(result);
    }

    /// <summary>
    /// Gets templates applicable for a specific generator type.
    /// </summary>
    public IEnumerable<GenerationTemplate> GetTemplatesForType(GeneratorType type)
    {
        return Templates.Where(t => t.SupportsGeneratorType(type) && t.IsActive);
    }

    /// <summary>
    /// Gets statistics about generation results.
    /// </summary>
    public ProjectStatistics GetStatistics()
    {
        return new ProjectStatistics
        {
            TotalEntities = Entities.Count,
            TotalProperties = Entities.Sum(e => e.Properties.Count),
            TotalGenerated = GenerationResults.Count(r => r.IsSuccessful()),
            TotalFailed = GenerationResults.Count(r => !r.IsSuccessful()),
            TotalCodeLines = GenerationResults.Sum(r => r.CodeLineCount),
            TotalGenerationTime = GenerationResults.Sum(r => r.GenerationDurationMs),
        };
    }

    /// <summary>
    /// Validates the entire project structure.
    /// </summary>
    public IEnumerable<string> Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ProjectName))
            errors.Add("Project name is required.");

        if (string.IsNullOrWhiteSpace(ProjectPath))
            errors.Add("Project path is required.");

        if (Entities.Count == 0)
            errors.Add("Project must contain at least one entity.");

        foreach (var entity in Entities)
        {
            var entityErrors = entity.Validate();
            errors.AddRange(entityErrors);
        }

        return errors;
    }

    /// <summary>
    /// Returns a short "Name vVersion" representation of the project for diagnostics and logging.
    /// The version is taken from the "Version" project property, or "unknown" when it is not set.
    /// </summary>
    public override string ToString()
    {
        var version = ProjectProperties.GetValueOrDefault("Version", "unknown");
        return $"{ProjectName} v{version}";
    }
}

public sealed class ProjectStatistics
{
    /// <summary>
    /// Gets or sets the total number of entities in the project.
    /// </summary>
    public int TotalEntities { get; set; }

    /// <summary>
    /// Gets or sets the total number of properties across all entities.
    /// </summary>
    public int TotalProperties { get; set; }

    /// <summary>
    /// Gets or sets the total number of successful generation operations.
    /// </summary>
    public int TotalGenerated { get; set; }

    /// <summary>
    /// Gets or sets the total number of failed generation operations.
    /// </summary>
    public int TotalFailed { get; set; }

    /// <summary>
    /// Gets or sets the total number of code lines generated.
    /// </summary>
    public int TotalCodeLines { get; set; }

    /// <summary>
    /// Gets or sets the total generation time in milliseconds.
    /// </summary>
    public long TotalGenerationTime { get; set; }

    /// <summary>
    /// Gets the success rate percentage of generation operations.
    /// </summary>
    public double SuccessRate => TotalGenerated + TotalFailed > 0
        ? (double)TotalGenerated / (TotalGenerated + TotalFailed) * 100
        : 0;
}
