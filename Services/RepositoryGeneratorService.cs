// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using DotNetSourceGeneratorToolkit.Exceptions;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Generates repository pattern implementations including interfaces and concrete classes
/// with full CRUD operations and query methods for entities.
/// </summary>
public class RepositoryGeneratorService : IRepositoryGeneratorService
{
    private readonly ILogger<RepositoryGeneratorService> _logger;

    public RepositoryGeneratorService(ILogger<RepositoryGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<GenerationResult> GenerateRepositoryAsync(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _logger.LogInformation("Generating repository for entity: {EntityName}", entity.Name);

        var result = new GenerationResult
        {
            EntityName = entity.Name,
            GeneratorType = GeneratorType.Repository,
            Status = GenerationStatus.InProgress,
        };

        try
        {
            var interfaceCode = GenerateRepositoryInterface(entity);
            var implementationCode = GenerateRepositoryImplementation(entity);

            result.GeneratedCode = interfaceCode + Environment.NewLine + Environment.NewLine + implementationCode;
            result.OutputFilePath = Path.Combine("Repositories", $"{entity.Name}Repository.cs");
            result.MarkAsCompleted(GenerationStatus.Completed, 200);

            _logger.LogInformation("Repository generated successfully for: {EntityName}", entity.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Repository generation failed for entity: {EntityName}", entity.Name);
            result.AddError(ex.Message);
            result.MarkAsCompleted(GenerationStatus.Failed, 0);
        }

        return await Task.FromResult(result);
    }

    public async Task<IEnumerable<GenerationResult>> GenerateAllRepositoriesAsync(List<Entity> entities)
    {
        if (entities == null || entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be null or empty");

        _logger.LogInformation("Generating repositories for {Count} entities", entities.Count);

        var tasks = entities.Select(GenerateRepositoryAsync);
        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.Status == GenerationStatus.Completed);
        _logger.LogInformation("Generated {Success}/{Total} repositories", successCount, results.Length);

        return results;
    }

    private string GenerateRepositoryInterface(Entity entity)
    {
        var pkProperty = entity.GetPrimaryKeyProperty();
        var pkType = pkProperty?.GetClrTypeName() ?? "object";

        var code = $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace {entity.Namespace}.Repositories
{{
    /// <summary>
    /// Repository interface for {entity.Name} entity providing data access operations.
    /// </summary>
    public interface I{entity.Name}Repository
    {{
        /// <summary>Gets an entity by its primary key.</summary>
        Task<{entity.Name}> GetByIdAsync({pkType} id);

        /// <summary>Gets all entities from storage.</summary>
        Task<IEnumerable<{entity.Name}>> GetAllAsync();

        /// <summary>Gets entities with pagination support.</summary>
        Task<IEnumerable<{entity.Name}>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>Checks if an entity exists by primary key.</summary>
        Task<bool> ExistsAsync({pkType} id);

        /// <summary>Creates a new entity in storage.</summary>
        Task<{entity.Name}> CreateAsync({entity.Name} entity);

        /// <summary>Updates an existing entity.</summary>
        Task<{entity.Name}> UpdateAsync({entity.Name} entity);

        /// <summary>Deletes an entity by primary key.</summary>
        Task<bool> DeleteAsync({pkType} id);

        /// <summary>Counts total entities in storage.</summary>
        Task<int> CountAsync();
    }}
}}";

        return code;
    }

    private string GenerateRepositoryImplementation(Entity entity)
    {
        var pkProperty = entity.GetPrimaryKeyProperty();
        var pkType = pkProperty?.GetClrTypeName() ?? "object";
        var tableName = entity.TableName ?? entity.Name.ToLower();

        var code = $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace {entity.Namespace}.Repositories
{{
    /// <summary>
    /// Repository implementation for {entity.Name} entity providing complete data access.
    /// </summary>
    public class {entity.Name}Repository : I{entity.Name}Repository
    {{
        private List<{entity.Name}> _data = new();

        public async Task<{entity.Name}> GetByIdAsync({pkType} id)
        {{
            var entity = _data.FirstOrDefault(e => e.Id.Equals(id));
            return await Task.FromResult(entity);
        }}

        public async Task<IEnumerable<{entity.Name}>> GetAllAsync()
        {{
            return await Task.FromResult(_data.AsEnumerable());
        }}

        public async Task<IEnumerable<{entity.Name}>> GetPagedAsync(int pageNumber, int pageSize)
        {{
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var skip = (pageNumber - 1) * pageSize;
            var paged = _data.Skip(skip).Take(pageSize);

            return await Task.FromResult(paged);
        }}

        public async Task<bool> ExistsAsync({pkType} id)
        {{
            var exists = _data.Any(e => e.Id.Equals(id));
            return await Task.FromResult(exists);
        }}

        public async Task<{entity.Name}> CreateAsync({entity.Name} entity)
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _data.Add(entity);
            return await Task.FromResult(entity);
        }}

        public async Task<{entity.Name}> UpdateAsync({entity.Name} entity)
        {{
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existing = _data.FirstOrDefault(e => e.Id.Equals(entity.Id));
            if (existing == null)
                throw new InvalidOperationException($""Entity with id {{entity.Id}} not found"");

            _data.Remove(existing);
            _data.Add(entity);

            return await Task.FromResult(entity);
        }}

        public async Task<bool> DeleteAsync({pkType} id)
        {{
            var entity = _data.FirstOrDefault(e => e.Id.Equals(id));
            if (entity == null)
                return await Task.FromResult(false);

            return _data.Remove(entity) ? await Task.FromResult(true) : await Task.FromResult(false);
        }}

        public async Task<int> CountAsync()
        {{
            return await Task.FromResult(_data.Count);
        }}
    }}
}}";

        return code;
    }
}
