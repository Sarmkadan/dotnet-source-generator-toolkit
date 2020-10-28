// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotNetSourceGeneratorToolkit.Domain;
using Microsoft.Extensions.Logging;

namespace DotNetSourceGeneratorToolkit.Services;

/// <summary>
/// Generates FluentValidation validators for entities with comprehensive
/// validation rules based on entity property definitions.
/// </summary>
public class ValidatorGeneratorService : IValidatorGeneratorService
{
    private readonly ILogger<ValidatorGeneratorService> _logger;

    public ValidatorGeneratorService(ILogger<ValidatorGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<GenerationResult>> GenerateAllValidatorsAsync(List<Entity> entities)
    {
        if (entities == null || entities.Count == 0)
            throw new ArgumentException("Entities collection cannot be null or empty");

        _logger.LogInformation("Generating validators for {Count} entities", entities.Count);

        var tasks = entities.Select(GenerateValidatorAsync);
        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.Status == GenerationStatus.Completed);
        _logger.LogInformation("Generated {Success}/{Total} validators", successCount, results.Length);

        return results;
    }

    public async Task<GenerationResult> GenerateValidatorAsync(Entity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _logger.LogInformation("Generating validator for entity: {EntityName}", entity.Name);

        var result = new GenerationResult
        {
            EntityName = entity.Name,
            GeneratorType = GeneratorType.Validator,
            Status = GenerationStatus.InProgress,
        };

        try
        {
            var code = GenerateValidatorCode(entity);
            result.GeneratedCode = code;
            result.OutputFilePath = Path.Combine("Validators", $"{entity.Name}Validator.cs");
            result.MarkAsCompleted(GenerationStatus.Completed, 180);

            _logger.LogInformation("Validator generated for: {EntityName}", entity.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validator generation failed for: {EntityName}", entity.Name);
            result.AddError(ex.Message);
            result.MarkAsCompleted(GenerationStatus.Failed, 0);
        }

        return await Task.FromResult(result);
    }

    private string GenerateValidatorCode(Entity entity)
    {
        var validationRules = GenerateValidationRules(entity);
        var validatorClassName = $"{entity.Name}Validator";

        var code = $@"// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using FluentValidation;

namespace {entity.Namespace}.Validators
{{
    /// <summary>
    /// FluentValidation validator for {entity.Name} entity.
    /// Provides comprehensive validation rules for all entity properties.
    /// </summary>
    public class {validatorClassName} : AbstractValidator<{entity.Name}>
    {{
        public {validatorClassName}()
        {{
            ConfigureRules();
        }}

        private void ConfigureRules()
        {{
{validationRules}
        }}
    }}

    /// <summary>
    /// Manual validator providing custom validation logic.
    /// </summary>
    public class {validatorClassName}Manual
    {{
        public bool Validate({entity.Name} entity, out List<string> errors)
        {{
            errors = new List<string>();

            if (entity == null)
            {{
                errors.Add(""{entity.Name} entity cannot be null"");
                return false;
            }}

            // Perform custom validation logic here
{GenerateCustomValidationLogic(entity)}

            return errors.Count == 0;
        }}
    }}
}}";

        return code;
    }

    private string GenerateValidationRules(Entity entity)
    {
        var rules = new List<string>();

        foreach (var prop in entity.Properties)
        {
            rules.Add($"            RuleFor(x => x.{prop.Name})");

            if (prop.IsRequired)
                rules.Add("                .NotEmpty().WithMessage($\"{{nameof({entity.Name}.{prop.Name})}} is required.\")");

            if (prop.MaxLength.HasValue)
                rules.Add($"                .MaximumLength({prop.MaxLength}).WithMessage($\"{{nameof({{entity.Name}}.{{prop.Name}})}} must not exceed {{prop.MaxLength}} characters.\")");

            if (prop.MinLength.HasValue)
                rules.Add($"                .MinimumLength({prop.MinLength}).WithMessage($\"{{nameof({{entity.Name}}.{{prop.Name}})}} must be at least {{prop.MinLength}} characters.\")");

            if (!string.IsNullOrEmpty(prop.RegexPattern))
                rules.Add($"                .Matches(@\"{prop.RegexPattern}\").WithMessage($\"{{nameof({{entity.Name}}.{{prop.Name}})}} format is invalid.\")");

            rules.Add("                ;");
            rules.Add("");
        }

        return string.Join(Environment.NewLine, rules);
    }

    private string GenerateCustomValidationLogic(Entity entity)
    {
        var logic = new List<string>();

        var requiredProps = entity.Properties.Where(p => p.IsRequired).ToList();
        if (requiredProps.Count > 0)
        {
            logic.Add("");
            logic.Add($"            // Validate required properties");

            foreach (var prop in requiredProps)
            {
                if (prop.Type == "string" || prop.Type.Contains("String"))
                    logic.Add($"            if (string.IsNullOrWhiteSpace(entity.{prop.Name})) errors.Add(\"{prop.Name} is required.\");");
                else
                    logic.Add($"            if (entity.{prop.Name} == null) errors.Add(\"{prop.Name} is required.\");");
            }
        }

        var lengthProps = entity.Properties.Where(p => p.MaxLength.HasValue || p.MinLength.HasValue).ToList();
        if (lengthProps.Count > 0)
        {
            logic.Add("");
            logic.Add($"            // Validate length constraints");

            foreach (var prop in lengthProps)
            {
                if (prop.MaxLength.HasValue && prop.Type.Contains("string", StringComparison.OrdinalIgnoreCase))
                    logic.Add($"            if (entity.{prop.Name}?.Length > {prop.MaxLength}) errors.Add(\"{prop.Name} exceeds maximum length.\");");

                if (prop.MinLength.HasValue && prop.Type.Contains("string", StringComparison.OrdinalIgnoreCase))
                    logic.Add($"            if (entity.{prop.Name}?.Length < {prop.MinLength}) errors.Add(\"{prop.Name} is too short.\");");
            }
        }

        return logic.Count == 0 ? "" : string.Join(Environment.NewLine, logic);
    }
}
