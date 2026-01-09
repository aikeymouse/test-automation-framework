using System.Text.Json;
using AIKeyMouse.CodeGen.CLI.Models.Skills;
using AIKeyMouse.CodeGen.CLI.Services.Infrastructure;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AIKeyMouse.CodeGen.CLI.Services.Skills;

/// <summary>
/// Loads skills from JSON/YAML files with inheritance support
/// </summary>
public class SkillLoader
{
    private readonly FileService _fileService;
    private readonly ILogger<SkillLoader> _logger;
    private readonly Dictionary<string, Skill> _skillCache = new();
    private readonly IDeserializer _yamlDeserializer;

    public SkillLoader(FileService fileService, ILogger<SkillLoader> logger)
    {
        _fileService = fileService;
        _logger = logger;
        
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Load a skill from file (JSON or YAML)
    /// </summary>
    public async Task<Skill> LoadSkillAsync(string path, CancellationToken cancellationToken = default)
    {
        // Check cache first
        if (_skillCache.TryGetValue(path, out var cachedSkill))
        {
            _logger.LogDebug("Returning cached skill from {Path}", path);
            return cachedSkill;
        }

        _logger.LogInformation("Loading skill from {Path}", path);

        var content = await _fileService.ReadFileAsync(path);
        var extension = Path.GetExtension(path).ToLowerInvariant();

        Skill skill;
        
        if (extension == ".json")
        {
            skill = JsonSerializer.Deserialize<Skill>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            }) ?? throw new InvalidOperationException($"Failed to deserialize skill from {path}");
        }
        else if (extension == ".yaml" || extension == ".yml")
        {
            skill = _yamlDeserializer.Deserialize<Skill>(content)
                ?? throw new InvalidOperationException($"Failed to deserialize skill from {path}");
        }
        else
        {
            throw new ArgumentException($"Unsupported skill file extension: {extension}");
        }

        // Process inheritance
        if (!string.IsNullOrWhiteSpace(skill.Extends))
        {
            skill = await ApplyInheritanceAsync(skill, Path.GetDirectoryName(path)!, cancellationToken);
        }

        // Process imports
        if (skill.Imports != null && skill.Imports.Count > 0)
        {
            skill = await ApplyImportsAsync(skill, Path.GetDirectoryName(path)!, cancellationToken);
        }

        // Cache the resolved skill
        _skillCache[path] = skill;

        return skill;
    }

    /// <summary>
    /// Load all skills from a directory
    /// </summary>
    public async Task<List<Skill>> LoadSkillsFromDirectoryAsync(
        string directoryPath,
        bool recursive = true,
        CancellationToken cancellationToken = default)
    {
        var skills = new List<Skill>();
        
        var skillFiles = _fileService.GetFiles(directoryPath, "*.skill.json", recursive)
            .Concat(_fileService.GetFiles(directoryPath, "*.skill.yaml", recursive))
            .Concat(_fileService.GetFiles(directoryPath, "*.skill.yml", recursive));

        foreach (var file in skillFiles)
        {
            try
            {
                var skill = await LoadSkillAsync(file, cancellationToken);
                skills.Add(skill);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load skill from {Path}", file);
            }
        }

        _logger.LogInformation("Loaded {Count} skills from {Directory}", skills.Count, directoryPath);
        return skills;
    }

    /// <summary>
    /// Clear the skill cache
    /// </summary>
    public void ClearCache()
    {
        _skillCache.Clear();
        _logger.LogDebug("Skill cache cleared");
    }

    /// <summary>
    /// Apply inheritance from parent skill
    /// </summary>
    private async Task<Skill> ApplyInheritanceAsync(
        Skill skill,
        string baseDirectory,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Applying inheritance from {Parent} to {Skill}", skill.Extends, skill.Id);

        // Load parent skill
        var parentPath = ResolveSkillPath(skill.Extends!, baseDirectory);
        var parent = await LoadSkillAsync(parentPath, cancellationToken);

        // Merge properties (child overrides parent)
        return new Skill
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description ?? parent.Description,
            Category = skill.Category,
            Platform = skill.Platform != "all" ? skill.Platform : parent.Platform,
            Extends = null, // Don't carry forward the extends
            Imports = skill.Imports ?? parent.Imports,
            SystemPrompt = skill.SystemPrompt ?? parent.SystemPrompt,
            PromptTemplate = skill.PromptTemplate, // Always use child template
            Examples = MergeLists(parent.Examples, skill.Examples),
            Validation = skill.Validation ?? parent.Validation,
            Output = skill.Output ?? parent.Output,
            LlmParams = MergeLlmParams(parent.LlmParams, skill.LlmParams),
            Metadata = MergeDictionaries(parent.Metadata, skill.Metadata)
        };
    }

    /// <summary>
    /// Apply imports from other skills
    /// </summary>
    private async Task<Skill> ApplyImportsAsync(
        Skill skill,
        string baseDirectory,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Applying {Count} imports to {Skill}", skill.Imports!.Count, skill.Id);

        var examples = skill.Examples?.ToList() ?? new List<SkillExample>();
        var metadata = skill.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) 
            ?? new Dictionary<string, object>();

        foreach (var import in skill.Imports!)
        {
            var importPath = ResolveSkillPath(import, baseDirectory);
            var importedSkill = await LoadSkillAsync(importPath, cancellationToken);

            // Merge examples
            if (importedSkill.Examples != null)
            {
                examples.AddRange(importedSkill.Examples);
            }

            // Merge metadata
            if (importedSkill.Metadata != null)
            {
                foreach (var kvp in importedSkill.Metadata)
                {
                    metadata.TryAdd(kvp.Key, kvp.Value);
                }
            }
        }

        skill.Examples = examples;
        skill.Metadata = metadata;
        skill.Imports = null; // Don't carry forward imports

        return skill;
    }

    private string ResolveSkillPath(string skillRef, string baseDirectory)
    {
        // If it's already a full path, use it
        if (Path.IsPathRooted(skillRef))
        {
            return skillRef;
        }

        // Try as relative path first
        var relativePath = Path.Combine(baseDirectory, skillRef);
        
        // Add extension if not present
        if (!Path.HasExtension(relativePath))
        {
            if (File.Exists($"{relativePath}.skill.json"))
                return $"{relativePath}.skill.json";
            if (File.Exists($"{relativePath}.skill.yaml"))
                return $"{relativePath}.skill.yaml";
            if (File.Exists($"{relativePath}.skill.yml"))
                return $"{relativePath}.skill.yml";
        }

        return relativePath;
    }

    private List<T>? MergeLists<T>(List<T>? parent, List<T>? child)
    {
        if (parent == null && child == null) return null;
        if (parent == null) return child;
        if (child == null) return parent;
        
        var merged = new List<T>(parent);
        merged.AddRange(child);
        return merged;
    }

    private SkillLlmParams? MergeLlmParams(SkillLlmParams? parent, SkillLlmParams? child)
    {
        if (parent == null) return child;
        if (child == null) return parent;

        return new SkillLlmParams
        {
            Temperature = child.Temperature ?? parent.Temperature,
            MaxTokens = child.MaxTokens ?? parent.MaxTokens,
            PreferredModel = child.PreferredModel ?? parent.PreferredModel
        };
    }

    private Dictionary<string, object>? MergeDictionaries(
        Dictionary<string, object>? parent,
        Dictionary<string, object>? child)
    {
        if (parent == null && child == null) return null;
        if (parent == null) return child;
        if (child == null) return parent;

        var merged = new Dictionary<string, object>(parent);
        foreach (var kvp in child)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }
}
