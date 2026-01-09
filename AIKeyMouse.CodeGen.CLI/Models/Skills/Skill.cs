namespace AIKeyMouse.CodeGen.CLI.Models.Skills;

/// <summary>
/// Represents a code generation skill
/// </summary>
public class Skill
{
    /// <summary>
    /// Unique identifier for the skill
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Display name of the skill
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Skill description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Skill category (page-object, step-definition, helper, etc.)
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Target platform (web, mobile, desktop, all)
    /// </summary>
    public string Platform { get; set; } = "all";

    /// <summary>
    /// Parent skill ID to inherit from
    /// </summary>
    public string? Extends { get; set; }

    /// <summary>
    /// Other skill IDs to import/compose
    /// </summary>
    public List<string>? Imports { get; set; }

    /// <summary>
    /// System prompt template
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// User prompt template (Liquid syntax)
    /// </summary>
    public required string PromptTemplate { get; set; }

    /// <summary>
    /// Example inputs/outputs for few-shot learning
    /// </summary>
    public List<SkillExample>? Examples { get; set; }

    /// <summary>
    /// Validation rules for generated code
    /// </summary>
    public SkillValidation? Validation { get; set; }

    /// <summary>
    /// Output configuration
    /// </summary>
    public SkillOutput? Output { get; set; }

    /// <summary>
    /// LLM generation parameters
    /// </summary>
    public SkillLlmParams? LlmParams { get; set; }

    /// <summary>
    /// Custom metadata
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Example for few-shot learning
/// </summary>
public class SkillExample
{
    public required string Input { get; set; }
    public required string Output { get; set; }
    public string? Explanation { get; set; }
}

/// <summary>
/// Validation rules
/// </summary>
public class SkillValidation
{
    /// <summary>
    /// Require specific using statements
    /// </summary>
    public List<string>? RequiredUsings { get; set; }

    /// <summary>
    /// Require specific base class
    /// </summary>
    public string? RequiredBaseClass { get; set; }

    /// <summary>
    /// Require specific interfaces
    /// </summary>
    public List<string>? RequiredInterfaces { get; set; }

    /// <summary>
    /// Syntax validation
    /// </summary>
    public bool ValidateSyntax { get; set; } = true;

    /// <summary>
    /// Code formatting
    /// </summary>
    public bool AutoFormat { get; set; } = true;
}

/// <summary>
/// Output configuration
/// </summary>
public class SkillOutput
{
    /// <summary>
    /// Default output directory
    /// </summary>
    public string? DefaultPath { get; set; }

    /// <summary>
    /// File naming pattern (e.g., "{name}Page.cs")
    /// </summary>
    public string? FileNamePattern { get; set; }

    /// <summary>
    /// Default namespace
    /// </summary>
    public string? DefaultNamespace { get; set; }

    /// <summary>
    /// Code extraction patterns
    /// </summary>
    public SkillCodeExtraction? CodeExtraction { get; set; }
}

/// <summary>
/// Code extraction configuration
/// </summary>
public class SkillCodeExtraction
{
    /// <summary>
    /// Extract code between markers (e.g., ```csharp and ```)
    /// </summary>
    public bool ExtractCodeBlocks { get; set; } = true;

    /// <summary>
    /// Code block language identifier
    /// </summary>
    public string Language { get; set; } = "csharp";

    /// <summary>
    /// Remove markdown formatting
    /// </summary>
    public bool StripMarkdown { get; set; } = true;
}

/// <summary>
/// LLM generation parameters
/// </summary>
public class SkillLlmParams
{
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public string? PreferredModel { get; set; }
}
