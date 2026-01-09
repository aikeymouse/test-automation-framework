using AIKeyMouse.CodeGen.CLI.Models.Skills;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.Skills;

/// <summary>
/// Validates skill definitions
/// </summary>
public class SkillValidator
{
    private readonly ILogger<SkillValidator> _logger;

    public SkillValidator(ILogger<SkillValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validate a skill definition
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate(Skill skill)
    {
        var errors = new List<string>();

        // Required fields
        if (string.IsNullOrWhiteSpace(skill.Id))
            errors.Add("Skill Id is required");

        if (string.IsNullOrWhiteSpace(skill.Name))
            errors.Add("Skill Name is required");

        if (string.IsNullOrWhiteSpace(skill.Category))
            errors.Add("Skill Category is required");

        if (string.IsNullOrWhiteSpace(skill.PromptTemplate))
            errors.Add("Skill PromptTemplate is required");

        // Validate category
        var validCategories = new[] { "page-object", "step-definition", "helper", "utility", "custom" };
        if (!string.IsNullOrWhiteSpace(skill.Category) && 
            !validCategories.Contains(skill.Category.ToLowerInvariant()))
        {
            errors.Add($"Invalid category '{skill.Category}'. Valid categories: {string.Join(", ", validCategories)}");
        }

        // Validate platform
        var validPlatforms = new[] { "web", "mobile", "desktop", "all" };
        if (!string.IsNullOrWhiteSpace(skill.Platform) && 
            !validPlatforms.Contains(skill.Platform.ToLowerInvariant()))
        {
            errors.Add($"Invalid platform '{skill.Platform}'. Valid platforms: {string.Join(", ", validPlatforms)}");
        }

        // Validate LLM parameters
        if (skill.LlmParams != null)
        {
            if (skill.LlmParams.Temperature.HasValue && 
                (skill.LlmParams.Temperature.Value < 0 || skill.LlmParams.Temperature.Value > 2))
            {
                errors.Add("Temperature must be between 0 and 2");
            }

            if (skill.LlmParams.MaxTokens.HasValue && skill.LlmParams.MaxTokens.Value < 1)
            {
                errors.Add("MaxTokens must be greater than 0");
            }
        }

        // Validate examples
        if (skill.Examples != null)
        {
            for (int i = 0; i < skill.Examples.Count; i++)
            {
                var example = skill.Examples[i];
                if (string.IsNullOrWhiteSpace(example.Input))
                    errors.Add($"Example {i + 1}: Input is required");
                if (string.IsNullOrWhiteSpace(example.Output))
                    errors.Add($"Example {i + 1}: Output is required");
            }
        }

        // Validate output configuration
        if (skill.Output?.FileNamePattern != null)
        {
            if (!skill.Output.FileNamePattern.Contains("{"))
            {
                errors.Add("FileNamePattern should contain placeholders like {name}");
            }
        }

        var isValid = errors.Count == 0;
        
        if (!isValid)
        {
            _logger.LogWarning("Skill validation failed for {SkillId}: {Errors}", 
                skill.Id ?? "unknown", string.Join("; ", errors));
        }

        return (isValid, errors);
    }

    /// <summary>
    /// Validate multiple skills
    /// </summary>
    public Dictionary<string, (bool IsValid, List<string> Errors)> ValidateMany(IEnumerable<Skill> skills)
    {
        var results = new Dictionary<string, (bool IsValid, List<string> Errors)>();
        
        foreach (var skill in skills)
        {
            var skillId = skill.Id ?? $"unnamed-{Guid.NewGuid()}";
            results[skillId] = Validate(skill);
        }

        return results;
    }

    /// <summary>
    /// Check for circular inheritance
    /// </summary>
    public bool HasCircularInheritance(Skill skill, Dictionary<string, Skill> allSkills, HashSet<string>? visited = null)
    {
        visited ??= new HashSet<string>();

        if (string.IsNullOrWhiteSpace(skill.Extends))
            return false;

        if (visited.Contains(skill.Id))
        {
            _logger.LogError("Circular inheritance detected in skill {SkillId}", skill.Id);
            return true;
        }

        visited.Add(skill.Id);

        if (!allSkills.TryGetValue(skill.Extends, out var parent))
        {
            _logger.LogWarning("Parent skill {ParentId} not found for {SkillId}", skill.Extends, skill.Id);
            return false;
        }

        return HasCircularInheritance(parent, allSkills, visited);
    }
}
