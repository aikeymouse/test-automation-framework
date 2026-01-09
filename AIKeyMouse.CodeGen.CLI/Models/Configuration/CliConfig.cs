namespace AIKeyMouse.CodeGen.CLI.Models.Configuration;

/// <summary>
/// Main configuration class for the CLI tool
/// </summary>
public class CliConfig
{
    /// <summary>
    /// Project name
    /// </summary>
    public string ProjectName { get; set; } = "AutomationTests";

    /// <summary>
    /// Root namespace for generated code
    /// </summary>
    public string RootNamespace { get; set; } = "AutomationTests";

    /// <summary>
    /// Target framework version
    /// </summary>
    public string TargetFramework { get; set; } = "net8.0";

    /// <summary>
    /// Project directory structure settings
    /// </summary>
    public ProjectStructure Structure { get; set; } = new();

    /// <summary>
    /// LLM provider configuration
    /// </summary>
    public LlmConfiguration Llm { get; set; } = new();

    /// <summary>
    /// Code generation settings
    /// </summary>
    public CodeGenerationConfig CodeGeneration { get; set; } = new();

    /// <summary>
    /// Skills configuration
    /// </summary>
    public SkillsConfiguration Skills { get; set; } = new();

    /// <summary>
    /// Logging configuration
    /// </summary>
    public LoggingConfiguration Logging { get; set; } = new();
}

/// <summary>
/// Project directory structure configuration
/// </summary>
public class ProjectStructure
{
    /// <summary>
    /// Directory for page object classes
    /// </summary>
    public string PagesDirectory { get; set; } = "Pages";

    /// <summary>
    /// Directory for step definition classes
    /// </summary>
    public string StepsDirectory { get; set; } = "StepDefinitions";

    /// <summary>
    /// Directory for feature files
    /// </summary>
    public string FeaturesDirectory { get; set; } = "Features";

    /// <summary>
    /// Directory for custom skills
    /// </summary>
    public string SkillsDirectory { get; set; } = "Skills";

    /// <summary>
    /// Generated code output directory
    /// </summary>
    public string OutputDirectory { get; set; } = "Generated";
}

/// <summary>
/// LLM provider configuration
/// </summary>
public class LlmConfiguration
{
    /// <summary>
    /// Preferred LLM provider (auto, groq, huggingface)
    /// </summary>
    public string PreferredProvider { get; set; } = "auto";

    /// <summary>
    /// Default temperature for generation (0.0-1.0)
    /// </summary>
    public double Temperature { get; set; } = 0.5;

    /// <summary>
    /// Maximum tokens for generation
    /// </summary>
    public int MaxTokens { get; set; } = 2500;

    /// <summary>
    /// HTTP client timeout in seconds (should be >= max provider timeout)
    /// </summary>
    public int HttpClientTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Request timeout in seconds (used by non-Ollama providers)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Ollama provider configuration
    /// </summary>
    public OllamaConfig Ollama { get; set; } = new();

    /// <summary>
    /// Groq provider configuration
    /// </summary>
    public GroqConfig Groq { get; set; } = new();

    /// <summary>
    /// HuggingFace provider configuration
    /// </summary>
    public HuggingFaceConfig HuggingFace { get; set; } = new();
}

/// <summary>
/// Ollama local LLM configuration
/// </summary>
public class OllamaConfig
{
    /// <summary>
    /// Ollama API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "llama3.1:8b";

    /// <summary>
    /// Fallback model if default fails
    /// </summary>
    public string FallbackModel { get; set; } = "mistral:7b";

    /// <summary>
    /// Request timeout in seconds for generation
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300; // 5 minutes

    /// <summary>
    /// Provider priority (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 0; // Highest priority - local is fastest and free
}

/// <summary>
/// Groq provider configuration
/// </summary>
public class GroqConfig
{
    /// <summary>
    /// Environment variable name for API key
    /// </summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "GROQ_API_KEY";

    /// <summary>
    /// API key (optional, prefer environment variable)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Groq API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1";

    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "llama-3.1-70b-versatile";

    /// <summary>
    /// Fallback model if default fails
    /// </summary>
    public string FallbackModel { get; set; } = "llama-3.1-8b-instant";

    /// <summary>
    /// Provider priority (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 1;
}

/// <summary>
/// HuggingFace provider configuration
/// </summary>
public class HuggingFaceConfig
{
    /// <summary>
    /// Environment variable name for API key
    /// </summary>
    public string ApiKeyEnvironmentVariable { get; set; } = "HUGGINGFACE_API_KEY";

    /// <summary>
    /// API key (optional, prefer environment variable)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// HuggingFace Inference API base URL
    /// </summary>
    public string BaseUrl { get; set; } = "https://api-inference.huggingface.co/models";

    /// <summary>
    /// Default model to use
    /// </summary>
    public string DefaultModel { get; set; } = "deepseek-ai/deepseek-coder-6.7b-instruct";

    /// <summary>
    /// Additional models for rotation
    /// </summary>
    public List<string> AdditionalModels { get; set; } = new()
    {
        "bigcode/starcoder",
        "codellama/CodeLlama-13b-Instruct-hf"
    };

    /// <summary>
    /// Provider priority (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 2;
}

/// <summary>
/// Code generation configuration
/// </summary>
public class CodeGenerationConfig
{
    /// <summary>
    /// Default namespace for generated code
    /// </summary>
    public string DefaultNamespace { get; set; } = "AutomationTests";

    /// <summary>
    /// Default output path for generated files
    /// </summary>
    public string OutputPath { get; set; } = "./Generated";

    /// <summary>
    /// Path to skills directory
    /// </summary>
    public string SkillsPath { get; set; } = "./Skills";

    /// <summary>
    /// Path to templates directory
    /// </summary>
    public string TemplatesPath { get; set; } = "./Templates";

    /// <summary>
    /// Automatically format generated code
    /// </summary>
    public bool AutoFormat { get; set; } = true;

    /// <summary>
    /// Validate syntax before saving
    /// </summary>
    public bool ValidateSyntax { get; set; } = true;

    /// <summary>
    /// Create backups of existing files
    /// </summary>
    public bool CreateBackups { get; set; } = true;

    /// <summary>
    /// Preferred locator strategy (CssSelector, Id, XPath, etc.)
    /// </summary>
    public string PreferredLocator { get; set; } = "CssSelector";

    /// <summary>
    /// Page class suffix
    /// </summary>
    public string PageClassSuffix { get; set; } = "Page";

    /// <summary>
    /// Step definition class suffix
    /// </summary>
    public string StepClassSuffix { get; set; } = "Steps";
}

/// <summary>
/// Skills system configuration
/// </summary>
public class SkillsConfiguration
{
    /// <summary>
    /// Default skills directory
    /// </summary>
    public string DefaultSkillsDirectory { get; set; } = "./Skills";

    /// <summary>
    /// User skills directory
    /// </summary>
    public string UserSkillsDirectory { get; set; } = "~/.aikeymouse/skills";

    /// <summary>
    /// Automatically reload skills when changed
    /// </summary>
    public bool AutoReload { get; set; } = true;

    /// <summary>
    /// Validate skill definitions on load
    /// </summary>
    public bool ValidateOnLoad { get; set; } = true;

    /// <summary>
    /// Cache loaded skills in memory
    /// </summary>
    public bool CacheSkills { get; set; } = true;
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Minimum log level (Verbose, Debug, Information, Warning, Error, Fatal)
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Enable file logging
    /// </summary>
    public bool EnableFileLogging { get; set; } = true;

    /// <summary>
    /// Log file path template
    /// </summary>
    public string LogFilePath { get; set; } = "logs/aikeymouse-codegen-.log";

    /// <summary>
    /// Log file rolling interval (Day, Hour, etc.)
    /// </summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>
    /// Enable console logging
    /// </summary>
    public bool EnableConsoleLogging { get; set; } = true;

    /// <summary>
    /// Retain log files for days
    /// </summary>
    public int RetainedFileCountLimit { get; set; } = 7;
}
