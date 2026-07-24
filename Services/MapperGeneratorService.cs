#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Generates mapper classes for transforming entities to/from DTOs
/// and between different entity types with property mapping logic.
/// </summary>
public sealed class MapperGeneratorService : GeneratorServiceBase, IMapperGeneratorService
{
    private readonly ILogger<MapperGeneratorService> _logger;

    /// <inheritdoc />
    protected override GeneratorType GeneratorKind => GeneratorType.Mapper;

    /// <summary>
    /// Initializes the mapper generator service.
    /// </summary>
    /// <param name="logger">The logger to use for generation diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public MapperGeneratorService(ILogger<MapperGeneratorService> logger)
        : base(logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<GenerationResult>> GenerateAllMappersAsync(List<Entity> entities)
    {
        if (entities is null || entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be null or empty");

        _logger.LogInformation("Generating mappers for {Count} entities", entities.Count);

        var results = new List<GenerationResult>();

        foreach (var entity in entities)
        {
            var dtoMapper = await GenerateMapperAsync(entity, entity);
            results.Add(dtoMapper);
        }

        var successCount = results.Count(r => r.Status == GenerationStatus.Completed);
        _logger.LogInformation("Generated {Success}/{Total} mappers", successCount, results.Count);

        return results;
    }

    public async Task<GenerationResult> GenerateMapperAsync(Entity sourceEntity, Entity targetEntity)
    {
        if (sourceEntity is null)
            throw new ArgumentNullException(nameof(sourceEntity));

        if (targetEntity is null)
            throw new ArgumentNullException(nameof(targetEntity));

        _logger.LogInformation("Generating mapper from {Source} to {Target}", sourceEntity.Name, targetEntity.Name);

        var result = new GenerationResult
        {
            EntityName = sourceEntity.Name,
            GeneratorType = GeneratorType.Mapper,
            Status = GenerationStatus.InProgress,
        };

        if (!ValidateOrReport(sourceEntity, result))
        {
            result.MarkAsCompleted(GenerationStatus.Failed, 0);
            return result;
        }

        try
        {
            var code = GenerateMapperCode(sourceEntity);
            result.GeneratedCode = code;
            result.OutputFilePath = BuildHintName(sourceEntity);
            result.MarkAsCompleted(GenerationStatus.Completed, 150);

            _logger.LogInformation("Mapper generated for: {EntityName}", sourceEntity.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mapper generation failed for: {EntityName}", sourceEntity.Name);
            ReportGenerationFailure(result, ex);
        }

        return await Task.FromResult(result);
    }

    private string GenerateMapperCode(Entity entity)
    {
        var dtoClassName = $"{entity.Name}Dto";
        var mapperClassName = $"{entity.Name}Mapper";

        var dtoProperties = GenerateDtoProperties(entity);
        var mapToDto = GenerateMapToDto(entity);
        var mapFromDto = GenerateMapFromDto(entity);

        var code = $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

#nullable enable

using System;
using System.Collections.Generic;

namespace {entity.Namespace}.Mappers
{{
    /// <summary>
    /// DTO class for {entity.Name} entity used in API responses.
    /// </summary>
    public sealed class {dtoClassName}
    {{
{dtoProperties}
    }}

    /// <summary>
    /// Mapper class for transforming {entity.Name} entities to/from DTOs.
    /// </summary>
    public sealed class {mapperClassName}
    {{
        /// <summary>Maps an entity to its DTO representation.</summary>
        public static {dtoClassName}? MapToDto({entity.Name}? entity)
        {{
            if (entity is null)
                return null;

{mapToDto}
            return dto;
        }}

        /// <summary>Maps a DTO back to entity.</summary>
        public static {entity.Name}? MapFromDto({dtoClassName}? dto)
        {{
            if (dto is null)
                return null;

{mapFromDto}
            return entity;
        }}

        /// <summary>Maps a collection of entities to DTOs.</summary>
        public static IEnumerable<{dtoClassName}> MapToDtos(IEnumerable<{entity.Name}>? entities)
        {{
            if (entities is null)
                return new List<{dtoClassName}>();

            var results = new List<{dtoClassName}>();
            foreach (var e in entities)
            {{
                var dto = MapToDto(e);
                if (dto is not null)
                    results.Add(dto);
            }}
            return results;
        }}
    }}
}}";

        return code;
    }

    private string GenerateDtoProperties(Entity entity)
    {
        var lines = new List<string>();

        foreach (var prop in entity.Properties)
        {
            var type = prop.GetClrTypeName();
            lines.Add($"        public {type} {prop.Name} {{ get; set; }}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private string GenerateMapToDto(Entity entity)
    {
        var lines = new List<string>();
        lines.Add($"            var dto = new {entity.Name}Dto();");
        lines.Add("");

        foreach (var prop in entity.Properties)
        {
            lines.Add($"            dto.{prop.Name} = entity.{prop.Name};");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private string GenerateMapFromDto(Entity entity)
    {
        var lines = new List<string>();
        lines.Add($"            var entity = new {entity.Name}();");
        lines.Add("");

        foreach (var prop in entity.Properties)
        {
            lines.Add($"            entity.{prop.Name} = dto.{prop.Name};");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
