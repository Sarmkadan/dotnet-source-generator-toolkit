#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Extension methods for <see cref="ValidatorGeneratorService"/> providing additional
/// functionality for batch validation operations, result processing, and entity validation.
/// </summary>
public static class ValidatorGeneratorServiceExtensions
{
    /// <summary>
    /// Generates validators for entities and returns only successful generation results.
    /// </summary>
    /// <param name="service">The validator generator service</param>
    /// <param name="entities">List of entities to generate validators for</param>
    /// <returns>Collection of successful generation results</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="entities"/> is null</exception>
    public static async Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync(
        this ValidatorGeneratorService service,
        List<Entity> entities,
        Func<Entity, bool>? filter = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(entities);

        var filteredEntities = filter is null
            ? entities
            : entities.Where(filter).ToList();

        var results = await service.GenerateAllValidatorsAsync(filteredEntities);

        return results.Where(r => r.Status == GenerationStatus.Completed).ToList();
    }

    /// <summary>
    /// Generates a validator for a single entity with custom entity filtering.
    /// </summary>
    /// <param name="service">The validator generator service</param>
    /// <param name="entity">The entity to generate validator for</param>
    /// <param name="predicate">Optional predicate to validate entity before generation</param>
    /// <returns>Generation result</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="entity"/> is null</exception>
    public static async Task<GenerationResult> GenerateValidatorAsync(
        this ValidatorGeneratorService service,
        Entity entity,
        Func<Entity, bool>? predicate = null)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(entity);

        if (predicate?.Invoke(entity) == false)
        {
            return new GenerationResult
            {
                EntityName = entity.Name,
                GeneratorType = GeneratorType.Validator,
                Status = GenerationStatus.Skipped,
                OutputFilePath = string.Empty,
                GeneratedCode = string.Empty
            };
        }

        return await service.GenerateValidatorAsync(entity);
    }

    /// <summary>
    /// Generates validators for entities in parallel and returns combined statistics.
    /// </summary>
    /// <param name="service">The validator generator service</param>
    /// <param name="entities">List of entities to generate validators for</param>
    /// <returns>Statistics about the generation process</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is null</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="entities"/> is null or empty</exception>
    public static async Task<ValidatorGenerationStats> GenerateValidatorsWithStatsAsync(
        this ValidatorGeneratorService service,
        List<Entity> entities)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(entities);
        if (entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be empty", nameof(entities));

        var results = await service.GenerateAllValidatorsAsync(entities);

        var completedResults = results.Where(r => r.Status == GenerationStatus.Completed).ToList();
        var stats = new ValidatorGenerationStats
        {
            TotalEntities = entities.Count,
            Successful = completedResults.Count,
            Failed = results.Count(r => r.Status == GenerationStatus.Failed),
            Skipped = results.Count(r => r.Status == GenerationStatus.Skipped),
            InProgress = results.Count(r => r.Status == GenerationStatus.InProgress),
            TotalLinesGenerated = results.Sum(r => r.CodeLineCount),
            AverageExecutionTimeMs = completedResults.Count > 0
                ? completedResults.Average(r => r.GenerationDurationMs)
                : 0
        };

        return stats;
    }

    /// <summary>
    /// Validates an entity using the manual validation method from generated validators.
    /// </summary>
    /// <param name="service">The validator generator service</param>
    /// <param name="entity">The entity to validate</param>
    /// <returns>True if validation passes, false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="entity"/> is null</exception>
    public static async Task<bool> ValidateEntityAsync(
        this ValidatorGeneratorService service,
        Entity entity)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(entity);

        var validatorTypeName = $"{entity.Name}ValidatorManual";
        var assemblyQualifiedName = $"{entity.Namespace}.Validators.{validatorTypeName}, dotnet-source-generator-toolkit";
        var validatorType = Type.GetType(assemblyQualifiedName);

        if (validatorType is null)
        {
            return false;
        }

        var validateMethod = validatorType.GetMethod("Validate");

        if (validateMethod is null)
        {
            return false;
        }

        try
        {
            var validatorInstance = Activator.CreateInstance(validatorType);
            var parameters = new object[] { entity, new object() };
            var result = (bool)validateMethod.Invoke(validatorInstance, parameters)!;

            return result;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates multiple entities and returns validation results.
    /// </summary>
    /// <param name="service">The validator generator service</param>
    /// <param name="entities">Entities to validate</param>
    /// <returns>Collection of validation results for each entity</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="entities"/> is null</exception>
    public static async Task<IEnumerable<EntityValidationResult>> ValidateEntitiesAsync(
        this ValidatorGeneratorService service,
        IEnumerable<Entity> entities)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(entities);

        var validationTasks = entities.Select(async entity =>
        {
            var isValid = await service.ValidateEntityAsync(entity);
            var errors = new List<string>();

            if (!isValid)
            {
                errors.Add($"Validation failed for entity: {entity.Name}");
            }

            return new EntityValidationResult
            {
                EntityName = entity.Name,
                IsValid = isValid,
                Errors = errors
            };
        }).ToList();

        return await Task.WhenAll(validationTasks);
    }
}

/// <summary>
/// Statistics about validator generation process.
/// </summary>
public sealed class ValidatorGenerationStats
{
    /// <summary>Total number of entities processed</summary>
    public int TotalEntities { get; set; }

    /// <summary>Number of successfully generated validators</summary>
    public int Successful { get; set; }

    /// <summary>Number of failed generation attempts</summary>
    public int Failed { get; set; }

    /// <summary>Number of skipped entities</summary>
    public int Skipped { get; set; }

    /// <summary>Number of validators still in progress</summary>
    public int InProgress { get; set; }

    /// <summary>Total lines of generated code</summary>
    public int TotalLinesGenerated { get; set; }

    /// <summary>Average execution time in milliseconds</summary>
    public double AverageExecutionTimeMs { get; set; }

    /// <summary>Gets the success rate as a percentage</summary>
    public double SuccessRate => TotalEntities > 0 ? (double)Successful / TotalEntities * 100 : 0;
}

/// <summary>
/// Result of validating a single entity.
/// </summary>
public sealed class EntityValidationResult
{
    /// <summary>Name of the validated entity</summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>Indicates whether validation passed</summary>
    public bool IsValid { get; set; }

    /// <summary>Collection of validation error messages</summary>
    public List<string> Errors { get; set; } = new();
}