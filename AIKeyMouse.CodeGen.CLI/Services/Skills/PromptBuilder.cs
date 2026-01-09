using AIKeyMouse.CodeGen.CLI.Models.Skills;
using Fluid;
using Microsoft.Extensions.Logging;

namespace AIKeyMouse.CodeGen.CLI.Services.Skills;

/// <summary>
/// Builds prompts from skill templates using Fluid templating engine
/// </summary>
public class PromptBuilder
{
    private readonly ILogger<PromptBuilder> _logger;
    private readonly FluidParser _parser;

    public PromptBuilder(ILogger<PromptBuilder> logger)
    {
        _logger = logger;
        _parser = new FluidParser();
    }

    /// <summary>
    /// Build a prompt from a skill template
    /// </summary>
    public async Task<string> BuildPromptAsync(Skill skill, Dictionary<string, object> context)
    {
        _logger.LogDebug("Building prompt for skill {SkillId}", skill.Id);

        try
        {
            // Parse the template
            if (!_parser.TryParse(skill.PromptTemplate, out var template, out var error))
            {
                throw new InvalidOperationException($"Failed to parse prompt template: {error}");
            }

            // Create template context
            var templateContext = new TemplateContext(context);
            
            // Add skill metadata to context
            templateContext.SetValue("skill", new
            {
                skill.Id,
                skill.Name,
                skill.Category,
                skill.Platform
            });

            // Render the template
            var prompt = await template.RenderAsync(templateContext);

            _logger.LogDebug("Built prompt ({Length} chars) for skill {SkillId}", 
                prompt.Length, skill.Id);

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build prompt for skill {SkillId}", skill.Id);
            throw;
        }
    }

    /// <summary>
    /// Build system message with examples
    /// </summary>
    public string BuildSystemMessage(Skill skill)
    {
        var parts = new List<string>();

        // Add base system prompt
        if (!string.IsNullOrWhiteSpace(skill.SystemPrompt))
        {
            parts.Add(skill.SystemPrompt);
        }
        else
        {
            // Default system prompt
            parts.Add($"You are an expert code generator specializing in {skill.Category} for {skill.Platform} automation.");
            parts.Add("Generate clean, idiomatic C# code following best practices.");
            parts.Add("Use clear naming conventions and include XML documentation comments.");
        }

        // Add examples for few-shot learning
        if (skill.Examples != null && skill.Examples.Count > 0)
        {
            parts.Add("\n## Examples:");
            
            for (int i = 0; i < skill.Examples.Count; i++)
            {
                var example = skill.Examples[i];
                parts.Add($"\n### Example {i + 1}:");
                parts.Add($"**Input:**\n{example.Input}");
                parts.Add($"\n**Output:**\n```csharp\n{example.Output}\n```");
                
                if (!string.IsNullOrWhiteSpace(example.Explanation))
                {
                    parts.Add($"\n**Explanation:** {example.Explanation}");
                }
            }
        }

        // Add validation requirements
        if (skill.Validation != null)
        {
            parts.Add("\n## Requirements:");
            
            if (skill.Validation.RequiredUsings != null && skill.Validation.RequiredUsings.Count > 0)
            {
                parts.Add($"- Include these using statements: {string.Join(", ", skill.Validation.RequiredUsings)}");
            }

            if (!string.IsNullOrWhiteSpace(skill.Validation.RequiredBaseClass))
            {
                parts.Add($"- Inherit from: {skill.Validation.RequiredBaseClass}");
            }

            if (skill.Validation.RequiredInterfaces != null && skill.Validation.RequiredInterfaces.Count > 0)
            {
                parts.Add($"- Implement: {string.Join(", ", skill.Validation.RequiredInterfaces)}");
            }
        }

        // Add output instructions
        parts.Add("\n## Output Format:");
        parts.Add("- Wrap code in ```csharp code blocks");
        parts.Add("- Include only the generated code, no explanations outside the code block");
        parts.Add("- Ensure code is syntactically correct and well-formatted");

        return string.Join("\n", parts);
    }

    /// <summary>
    /// Extract code from LLM response
    /// </summary>
    public string ExtractCode(string response, SkillCodeExtraction? extraction = null)
    {
        extraction ??= new SkillCodeExtraction();

        if (!extraction.ExtractCodeBlocks)
        {
            return response;
        }

        // Find code blocks with language identifier
        var codeBlockPattern = $"```{extraction.Language}";
        var startIndex = response.IndexOf(codeBlockPattern, StringComparison.OrdinalIgnoreCase);
        
        if (startIndex == -1)
        {
            // Try without language identifier
            startIndex = response.IndexOf("```");
            if (startIndex == -1)
            {
                _logger.LogWarning("No code block found in response, returning as-is");
                return response.Trim();
            }
        }

        // Find the end of the opening marker
        var codeStart = response.IndexOf('\n', startIndex) + 1;
        
        // Find the closing ```
        var codeEnd = response.IndexOf("```", codeStart);
        
        if (codeEnd == -1)
        {
            _logger.LogWarning("Code block not properly closed, taking rest of response");
            return response.Substring(codeStart).Trim();
        }

        var code = response.Substring(codeStart, codeEnd - codeStart).Trim();

        if (extraction.StripMarkdown)
        {
            // Remove any remaining markdown
            code = code.Replace("**", "").Replace("*", "");
        }

        return code;
    }
}
