// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Represents metadata and structure information about a .NET project
/// that has been analyzed for source generation.
/// </summary>
public class ProjectInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string ProjectName { get; set; } = string.Empty;

    public string ProjectPath { get; set; } = string.Empty;

    public string TargetFramework { get; set; } = "net10.0";

    public List<Entity> Entities { get; } = [];

    public List<GenerationTemplate> Templates { get; } = [];

    public List<GenerationResult> GenerationResults { get; } = [];

    public Dictionary<string, string> ProjectProperties { get; } = [];

    public List<string> ReferencedAssemblies { get; } = [];

    public string RootNamespace { get; set; } = string.Empty;

    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    public bool HasAnalysisErrors { get; set; }

    public List<string> AnalysisErrors { get; } = [];

    /// <summary>
    /// Adds an entity to the project and validates it.
    /// </summary>
    public void AddEntity(Entity entity)
    {
        if (entity == null)
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
        if (template == null)
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
        if (result != null)
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
}

public class ProjectStatistics
{
    public int TotalEntities { get; set; }

    public int TotalProperties { get; set; }

    public int TotalGenerated { get; set; }

    public int TotalFailed { get; set; }

    public int TotalCodeLines { get; set; }

    public long TotalGenerationTime { get; set; }

    public double SuccessRate => TotalGenerated + TotalFailed > 0
        ? (double)TotalGenerated / (TotalGenerated + TotalFailed) * 100
        : 0;
}
