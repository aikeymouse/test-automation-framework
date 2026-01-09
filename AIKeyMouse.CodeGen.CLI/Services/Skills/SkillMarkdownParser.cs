using AIKeyMouse.CodeGen.CLI.Models.Skills;
using System.Text.Json;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AIKeyMouse.CodeGen.CLI.Services.Skills;

/// <summary>
/// Parses skill definitions from Markdown format
/// </summary>
public class SkillMarkdownParser
{
    private readonly IDeserializer _yamlDeserializer;

    public SkillMarkdownParser()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Parse a skill from Markdown content
    /// </summary>
    public Skill Parse(string markdown)
    {
        var sections = SplitIntoSections(markdown);
        
        // Parse YAML frontmatter
        var frontmatter = sections.ContainsKey("frontmatter") 
            ? sections["frontmatter"] 
            : throw new InvalidOperationException("Skill markdown must have YAML frontmatter");

        var skill = ParseFrontmatter(frontmatter);

        // Parse sections
        if (sections.ContainsKey("system prompt"))
        {
            skill.SystemPrompt = sections["system prompt"].Trim();
        }

        if (sections.ContainsKey("prompt template"))
        {
            skill.PromptTemplate = sections["prompt template"].Trim();
        }

        if (sections.ContainsKey("examples"))
        {
            skill.Examples = ParseExamples(sections["examples"]);
        }

        return skill;
    }

    /// <summary>
    /// Split markdown into sections
    /// </summary>
    private Dictionary<string, string> SplitIntoSections(string markdown)
    {
        var sections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        // Extract YAML frontmatter
        var frontmatterMatch = Regex.Match(markdown, @"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline);
        if (frontmatterMatch.Success)
        {
            sections["frontmatter"] = frontmatterMatch.Groups[1].Value;
            markdown = markdown.Substring(frontmatterMatch.Length);
        }

        // Split by H1 headers
        var headerPattern = @"^#\s+(.+)$";
        var lines = markdown.Split('\n');
        
        string? currentSection = null;
        var currentContent = new List<string>();

        foreach (var line in lines)
        {
            var match = Regex.Match(line, headerPattern, RegexOptions.Multiline);
            if (match.Success)
            {
                // Save previous section
                if (currentSection != null)
                {
                    sections[currentSection] = string.Join('\n', currentContent);
                }

                // Start new section
                currentSection = match.Groups[1].Value.Trim();
                currentContent.Clear();
            }
            else if (currentSection != null)
            {
                currentContent.Add(line);
            }
        }

        // Save last section
        if (currentSection != null)
        {
            sections[currentSection] = string.Join('\n', currentContent);
        }

        return sections;
    }

    /// <summary>
    /// Parse YAML frontmatter into Skill object
    /// </summary>
    private Skill ParseFrontmatter(string yaml)
    {
        try
        {
            // Deserialize to dictionary first to handle dynamic structure
            var data = _yamlDeserializer.Deserialize<Dictionary<string, object>>(yaml);

            var skill = new Skill
            {
                Id = GetString(data, "id") ?? throw new InvalidOperationException("Skill must have an 'id'"),
                Name = GetString(data, "name") ?? throw new InvalidOperationException("Skill must have a 'name'"),
                Category = GetString(data, "category") ?? throw new InvalidOperationException("Skill must have a 'category'"),
                PromptTemplate = "", // Will be set from section
                Description = GetString(data, "description"),
                Platform = GetString(data, "platform") ?? "all",
                Extends = GetString(data, "extends"),
                Imports = GetStringList(data, "imports")
            };

            // Parse nested objects
            if (data.ContainsKey("llmParams"))
            {
                skill.LlmParams = ParseLlmParams(data["llmParams"]);
            }

            if (data.ContainsKey("validation"))
            {
                skill.Validation = ParseValidation(data["validation"]);
            }

            if (data.ContainsKey("output"))
            {
                skill.Output = ParseOutput(data["output"]);
            }

            if (data.ContainsKey("metadata"))
            {
                skill.Metadata = data["metadata"] as Dictionary<string, object>;
            }

            return skill;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse YAML frontmatter: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parse examples section
    /// </summary>
    private List<SkillExample> ParseExamples(string examplesSection)
    {
        var examples = new List<SkillExample>();
        
        // Split by H2 headers (## Example N)
        var examplePattern = @"##\s+Example\s+\d+:?\s*(.+)$";
        var lines = examplesSection.Split('\n');
        
        string? currentExampleName = null;
        var currentSections = new Dictionary<string, List<string>>();
        string? currentSubsection = null;

        foreach (var line in lines)
        {
            var exampleMatch = Regex.Match(line, examplePattern, RegexOptions.IgnoreCase);
            if (exampleMatch.Success)
            {
                // Save previous example
                if (currentExampleName != null && currentSections.Count > 0)
                {
                    examples.Add(CreateExample(currentSections));
                }

                // Start new example
                currentExampleName = exampleMatch.Groups[1].Value.Trim();
                currentSections.Clear();
                currentSubsection = null;
                continue;
            }

            // Check for subsection headers (### Input, ### Output, ### Explanation)
            var subsectionMatch = Regex.Match(line, @"^###\s+(.+)$");
            if (subsectionMatch.Success)
            {
                currentSubsection = subsectionMatch.Groups[1].Value.Trim().ToLower();
                if (!currentSections.ContainsKey(currentSubsection))
                {
                    currentSections[currentSubsection] = new List<string>();
                }
                continue;
            }

            // Add content to current subsection
            if (currentSubsection != null)
            {
                currentSections[currentSubsection].Add(line);
            }
        }

        // Save last example
        if (currentExampleName != null && currentSections.Count > 0)
        {
            examples.Add(CreateExample(currentSections));
        }

        return examples;
    }

    /// <summary>
    /// Create SkillExample from parsed sections
    /// </summary>
    private SkillExample CreateExample(Dictionary<string, List<string>> sections)
    {
        var input = sections.ContainsKey("input") 
            ? ExtractCodeBlock(string.Join('\n', sections["input"])) 
            : "";
        
        var output = sections.ContainsKey("output") 
            ? ExtractCodeBlock(string.Join('\n', sections["output"])) 
            : "";
        
        var explanation = sections.ContainsKey("explanation") 
            ? string.Join('\n', sections["explanation"]).Trim() 
            : null;

        return new SkillExample
        {
            Input = input,
            Output = output,
            Explanation = explanation
        };
    }

    /// <summary>
    /// Extract code from markdown code block
    /// </summary>
    private string ExtractCodeBlock(string content)
    {
        // Match ```language...``` blocks
        var match = Regex.Match(content, @"```(?:\w+)?\s*\n(.*?)\n```", RegexOptions.Singleline);
        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        return content.Trim();
    }

    /// <summary>
    /// Parse LLM parameters
    /// </summary>
    private SkillLlmParams? ParseLlmParams(object obj)
    {
        var dict = obj as Dictionary<object, object>;
        if (dict == null) return null;

        return new SkillLlmParams
        {
            Temperature = GetFloat(dict, "temperature"),
            MaxTokens = GetInt(dict, "maxTokens"),
            PreferredModel = GetString(dict, "preferredModel")
        };
    }

    /// <summary>
    /// Parse validation configuration
    /// </summary>
    private SkillValidation? ParseValidation(object obj)
    {
        var dict = obj as Dictionary<object, object>;
        if (dict == null) return null;

        return new SkillValidation
        {
            RequiredUsings = GetStringList(dict, "requiredUsings"),
            RequiredBaseClass = GetString(dict, "requiredBaseClass"),
            RequiredInterfaces = GetStringList(dict, "requiredInterfaces"),
            ValidateSyntax = GetBool(dict, "validateSyntax") ?? true,
            AutoFormat = GetBool(dict, "autoFormat") ?? true
        };
    }

    /// <summary>
    /// Parse output configuration
    /// </summary>
    private SkillOutput? ParseOutput(object obj)
    {
        var dict = obj as Dictionary<object, object>;
        if (dict == null) return null;

        var output = new SkillOutput
        {
            DefaultPath = GetString(dict, "defaultPath"),
            FileNamePattern = GetString(dict, "fileNamePattern"),
            DefaultNamespace = GetString(dict, "defaultNamespace")
        };

        if (dict.ContainsKey("codeExtraction"))
        {
            output.CodeExtraction = ParseCodeExtraction(dict["codeExtraction"]);
        }

        return output;
    }

    /// <summary>
    /// Parse code extraction configuration
    /// </summary>
    private SkillCodeExtraction? ParseCodeExtraction(object obj)
    {
        var dict = obj as Dictionary<object, object>;
        if (dict == null) return null;

        return new SkillCodeExtraction
        {
            ExtractCodeBlocks = GetBool(dict, "extractCodeBlocks") ?? true,
            Language = GetString(dict, "language") ?? "csharp",
            StripMarkdown = GetBool(dict, "stripMarkdown") ?? true
        };
    }

    // Helper methods for type conversion
    private string? GetString(Dictionary<string, object> dict, string key) =>
        dict.TryGetValue(key, out var value) ? value?.ToString() : null;

    private string? GetString(Dictionary<object, object> dict, string key) =>
        dict.TryGetValue(key, out var value) ? value?.ToString() : null;

    private List<string>? GetStringList(Dictionary<string, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        if (value is List<object> list) return list.Select(x => x.ToString()!).ToList();
        return null;
    }

    private List<string>? GetStringList(Dictionary<object, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        if (value is List<object> list) return list.Select(x => x.ToString()!).ToList();
        return null;
    }

    private int? GetInt(Dictionary<object, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        if (value is int i) return i;
        if (int.TryParse(value?.ToString(), out var result)) return result;
        return null;
    }

    private float? GetFloat(Dictionary<object, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        if (value is float f) return f;
        if (value is double d) return (float)d;
        if (float.TryParse(value?.ToString(), out var result)) return result;
        return null;
    }

    private bool? GetBool(Dictionary<object, object> dict, string key)
    {
        if (!dict.TryGetValue(key, out var value)) return null;
        if (value is bool b) return b;
        if (bool.TryParse(value?.ToString(), out var result)) return result;
        return null;
    }
}
