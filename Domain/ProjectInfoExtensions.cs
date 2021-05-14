#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace DotNetSourceGeneratorToolkit.Domain;

/// <summary>
/// Provides useful extension methods for the <see cref="ProjectInfo"/> class
/// to enhance project analysis, statistics, and generation workflows.
/// </summary>
public static class ProjectInfoExtensions
{
    /// <summary>
    /// Gets the total number of properties across all entities in the project.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Total property count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static int TotalProperties(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.Entities.Sum(e => e.Properties.Count);
    }

    /// <summary>
    /// Gets the total number of successful generation results.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Count of successful generation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static int SuccessfulGenerations(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.GenerationResults.Count(r => r.IsSuccessful());
    }

    /// <summary>
    /// Gets the total number of failed generation results.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Count of failed generation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static int FailedGenerations(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.GenerationResults.Count(r => !r.IsSuccessful());
    }

    /// <summary>
    /// Gets the total number of code lines generated across all successful generation results.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Total code lines count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static int TotalCodeLinesGenerated(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.GenerationResults
            .Where(r => r.IsSuccessful())
            .Sum(r => r.CodeLineCount);
    }

    /// <summary>
    /// Gets the total generation time in milliseconds across all generation results.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Total generation time in milliseconds.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static long TotalGenerationTimeMs(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.GenerationResults.Sum(r => r.GenerationDurationMs);
    }

    /// <summary>
    /// Gets the success rate percentage of generation operations (0-100).
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Success rate percentage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static double GenerationSuccessRate(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        var total = projectInfo.GenerationResults.Count;
        if (total == 0)
            return 0;

        var successful = projectInfo.SuccessfulGenerations();
        return (double)successful / total * 100;
    }

    /// <summary>
    /// Gets a summary report of the project's generation statistics.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Formatted statistics report.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static string GetGenerationReport(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        var stats = projectInfo.GetStatistics();
        var sb = new StringBuilder();

        sb.AppendLine($"=== Generation Report for {projectInfo.ProjectName} ===");
        sb.AppendLine($"Project ID: {projectInfo.Id}");
        sb.AppendLine($"Target Framework: {projectInfo.TargetFramework}");
        sb.AppendLine($"Root Namespace: {projectInfo.RootNamespace}");
        sb.AppendLine();
        sb.AppendLine($"Entities: {stats.TotalEntities}");
        sb.AppendLine($"Properties: {projectInfo.TotalProperties()}");
        sb.AppendLine();
        sb.AppendLine($"Generation Results: {projectInfo.GenerationResults.Count}");
        sb.AppendLine($"  - Successful: {projectInfo.SuccessfulGenerations()}");
        sb.AppendLine($"  - Failed: {projectInfo.FailedGenerations()}");
        sb.AppendLine($"  - Success Rate: {projectInfo.GenerationSuccessRate():F2}%");
        sb.AppendLine($"  - Total Lines: {projectInfo.TotalCodeLinesGenerated()}");
        sb.AppendLine($"  - Total Time: {projectInfo.TotalGenerationTimeMs()}ms");
        sb.AppendLine();
        sb.AppendLine($"Templates: {projectInfo.Templates.Count}");
        sb.AppendLine($"Referenced Assemblies: {projectInfo.ReferencedAssemblies.Count}");
        sb.AppendLine();

        if (projectInfo.HasAnalysisErrors && projectInfo.AnalysisErrors.Any())
        {
            sb.AppendLine("Analysis Errors:");
            foreach (var error in projectInfo.AnalysisErrors)
            {
                sb.AppendLine($"  - {error}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the most recently analyzed entity by creation date.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>The most recent entity or null if no entities exist.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static Entity? GetMostRecentEntity(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.Entities
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets all entities that have navigation properties (relationships).
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Collection of entities with navigation properties.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static IEnumerable<Entity> GetEntitiesWithNavigationProperties(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.Entities.Where(e => e.GetNavigationProperties().Any());
    }

    /// <summary>
    /// Gets all entities that have primary key properties.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Collection of entities with primary keys.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static IEnumerable<Entity> GetEntitiesWithPrimaryKeys(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.Entities.Where(e => e.GetPrimaryKeyProperty() is not null);
    }

    /// <summary>
    /// Gets the total number of unique property types across all entities.
    /// </summary>
    /// <param name="projectInfo">The project information.</param>
    /// <returns>Count of unique property types.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectInfo"/> is null.</exception>
    public static int CountUniquePropertyTypes(this ProjectInfo projectInfo)
    {
        ArgumentNullException.ThrowIfNull(projectInfo);

        return projectInfo.Entities
            .SelectMany(e => e.Properties)
            .Select(p => p.Type)
            .Distinct()
            .Count();
    }
}