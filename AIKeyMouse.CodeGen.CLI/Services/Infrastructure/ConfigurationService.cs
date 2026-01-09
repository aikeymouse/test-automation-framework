using AIKeyMouse.CodeGen.CLI.Models.Configuration;

namespace AIKeyMouse.CodeGen.CLI.Services.Infrastructure;

/// <summary>
/// Service for managing configuration from multiple sources
/// </summary>
public class ConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfigurationService> _logger;
    private CliConfig? _cliConfig;

    public ConfigurationService(IConfiguration configuration, ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get the complete CLI configuration
    /// </summary>
    public CliConfig GetConfiguration()
    {
        if (_cliConfig != null)
            return _cliConfig;

        _cliConfig = new CliConfig();
        _configuration.Bind(_cliConfig);

        // Resolve API keys from environment variables if not set
        ResolveApiKeys(_cliConfig);

        // Expand user directory paths
        ExpandUserPaths(_cliConfig);

        _logger.LogDebug("Configuration loaded successfully");
        return _cliConfig;
    }

    /// <summary>
    /// Get a specific configuration value
    /// </summary>
    public T? GetValue<T>(string key)
    {
        return _configuration.GetValue<T>(key);
    }

    /// <summary>
    /// Set a configuration value (runtime only, not persisted)
    /// </summary>
    public void SetValue(string key, string value)
    {
        _configuration[key] = value;
        _cliConfig = null; // Clear cache to force reload
    }

    /// <summary>
    /// Save configuration to file
    /// </summary>
    public async Task SaveProjectConfigAsync(CliConfig config, string projectPath)
    {
        var configPath = Path.Combine(projectPath, "aikeymouse.config.json");
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(config, options);
        await File.WriteAllTextAsync(configPath, json);
        
        _logger.LogInformation("Project configuration saved to {ConfigPath}", configPath);
    }

    /// <summary>
    /// Load project-specific configuration
    /// </summary>
    public async Task<CliConfig?> LoadProjectConfigAsync(string projectPath)
    {
        var configPath = Path.Combine(projectPath, "aikeymouse.config.json");
        
        if (!File.Exists(configPath))
        {
            _logger.LogDebug("Project configuration not found at {ConfigPath}", configPath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(configPath);
            var config = JsonSerializer.Deserialize<CliConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Project configuration loaded from {ConfigPath}", configPath);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load project configuration from {ConfigPath}", configPath);
            return null;
        }
    }

    /// <summary>
    /// Initialize a new project configuration file
    /// </summary>
    public async Task InitializeProjectConfigAsync(string projectPath, bool force = false)
    {
        var configPath = Path.Combine(projectPath, "aikeymouse.config.json");
        
        if (File.Exists(configPath) && !force)
        {
            throw new InvalidOperationException($"Configuration file already exists at {configPath}. Use --force to overwrite.");
        }

        var defaultConfig = GetConfiguration();
        await SaveProjectConfigAsync(defaultConfig, projectPath);
    }

    /// <summary>
    /// Resolve API keys from environment variables
    /// </summary>
    private void ResolveApiKeys(CliConfig config)
    {
        // Groq API Key
        if (string.IsNullOrEmpty(config.Llm.Groq.ApiKey))
        {
            var groqKey = Environment.GetEnvironmentVariable(config.Llm.Groq.ApiKeyEnvironmentVariable);
            if (!string.IsNullOrEmpty(groqKey))
            {
                config.Llm.Groq.ApiKey = groqKey;
                _logger.LogDebug("Groq API key loaded from environment variable");
            }
        }

        // HuggingFace API Key
        if (string.IsNullOrEmpty(config.Llm.HuggingFace.ApiKey))
        {
            var hfKey = Environment.GetEnvironmentVariable(config.Llm.HuggingFace.ApiKeyEnvironmentVariable);
            if (!string.IsNullOrEmpty(hfKey))
            {
                config.Llm.HuggingFace.ApiKey = hfKey;
                _logger.LogDebug("HuggingFace API key loaded from environment variable");
            }
        }
    }

    /// <summary>
    /// Expand ~ to user home directory in paths
    /// </summary>
    private void ExpandUserPaths(CliConfig config)
    {
        var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (config.Skills.UserSkillsDirectory.StartsWith("~/") || config.Skills.UserSkillsDirectory.StartsWith("~\\"))
        {
            config.Skills.UserSkillsDirectory = Path.Combine(userHome, config.Skills.UserSkillsDirectory[2..]);
        }
    }

    /// <summary>
    /// Validate configuration
    /// </summary>
    public (bool IsValid, List<string> Errors) ValidateConfiguration(CliConfig config)
    {
        var errors = new List<string>();

        // Check API keys
        if (string.IsNullOrEmpty(config.Llm.Groq.ApiKey) && 
            string.IsNullOrEmpty(config.Llm.HuggingFace.ApiKey))
        {
            errors.Add("No LLM API keys configured. Set GROQ_API_KEY or HUGGINGFACE_API_KEY environment variable.");
        }

        // Validate temperature
        if (config.Llm.Temperature < 0 || config.Llm.Temperature > 1)
        {
            errors.Add($"Invalid temperature value: {config.Llm.Temperature}. Must be between 0.0 and 1.0.");
        }

        // Validate max tokens
        if (config.Llm.MaxTokens <= 0)
        {
            errors.Add($"Invalid max tokens: {config.Llm.MaxTokens}. Must be greater than 0.");
        }

        return (errors.Count == 0, errors);
    }
}
