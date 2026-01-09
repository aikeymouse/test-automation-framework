# Configuration Guide

This document describes all configuration options available in `appsettings.json`.

## Table of Contents
- [Logging](#logging)
- [LLM Providers](#llm-providers)
- [Code Generation](#code-generation)
- [Skills](#skills)
- [Project Structure](#project-structure)
- [Project Settings](#project-settings)

---

## Logging

Controls application logging behavior.

```json
{
  "Logging": {
    "MinimumLevel": "Information",
    "EnableFileLogging": true,
    "LogFilePath": "logs/aikeymouse-codegen-.log",
    "RollingInterval": "Day",
    "EnableConsoleLogging": true,
    "RetainedFileCountLimit": 7
  }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `MinimumLevel` | string | `"Information"` | Minimum log level: `Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal` |
| `EnableFileLogging` | boolean | `true` | Enable logging to file |
| `LogFilePath` | string | `"logs/aikeymouse-codegen-.log"` | Log file path template (date suffix added automatically) |
| `RollingInterval` | string | `"Day"` | Log file rolling interval: `Infinite`, `Year`, `Month`, `Day`, `Hour`, `Minute` |
| `EnableConsoleLogging` | boolean | `true` | Enable logging to console |
| `RetainedFileCountLimit` | integer | `7` | Number of log files to retain |

---

## LLM Providers

Configuration for AI language model providers. The system tries providers in priority order (lower number = higher priority).

```json
{
  "Llm": {
    "PreferredProvider": "auto",
    "Temperature": 0.5,
    "MaxTokens": 2500,
    "TimeoutSeconds": 60,
    "RetryAttempts": 3,
    "Ollama": { ... },
    "Groq": { ... },
    "HuggingFace": { ... }
  }
}
```

### General LLM Settings

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PreferredProvider` | string | `"auto"` | Preferred provider: `auto`, `ollama`, `groq`, `huggingface` |
| `Temperature` | number | `0.5` | Default generation temperature (0.0-1.0). Higher = more creative |
| `MaxTokens` | integer | `2500` | Maximum tokens for generation |
| `TimeoutSeconds` | integer | `60` | Request timeout for Groq/HuggingFace providers (seconds) |
| `RetryAttempts` | integer | `3` | Number of retry attempts on failure |

### Ollama Configuration

Local LLM provider (requires Ollama installed locally).

```json
{
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "DefaultModel": "llama3.1:8b",
    "FallbackModel": "mistral:7b",
    "TimeoutSeconds": 300,
    "Priority": 0
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | string | `"http://localhost:11434"` | Ollama API endpoint |
| `DefaultModel` | string | `"llama3.1:8b"` | Primary model to use |
| `FallbackModel` | string | `"mistral:7b"` | Model to use if default fails |
| `TimeoutSeconds` | integer | `300` | Request timeout (5 minutes recommended for local generation) |
| `Priority` | integer | `0` | Provider priority (0 = highest) |

**Installation:**
```bash
# macOS/Linux
curl -fsSL https://ollama.com/install.sh | sh

# Pull models
ollama pull llama3.1:8b
ollama pull mistral:7b
```

### Groq Configuration

Cloud-based LLM provider (requires API key).

```json
{
  "Groq": {
    "ApiKeyEnvironmentVariable": "GROQ_API_KEY",
    "BaseUrl": "https://api.groq.com/openai/v1",
    "DefaultModel": "llama-3.1-70b-versatile",
    "FallbackModel": "llama-3.1-8b-instant",
    "Priority": 1
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApiKeyEnvironmentVariable` | string | `"GROQ_API_KEY"` | Environment variable containing API key |
| `BaseUrl` | string | `"https://api.groq.com/openai/v1"` | Groq API endpoint |
| `DefaultModel` | string | `"llama-3.1-70b-versatile"` | Primary model to use |
| `FallbackModel` | string | `"llama-3.1-8b-instant"` | Model to use if default fails |
| `Priority` | integer | `1` | Provider priority |

**Setup:**
```bash
export GROQ_API_KEY="your-api-key-here"
```

### HuggingFace Configuration

Cloud-based LLM provider (requires API key).

```json
{
  "HuggingFace": {
    "ApiKeyEnvironmentVariable": "HUGGINGFACE_API_KEY",
    "BaseUrl": "https://api-inference.huggingface.co/models",
    "DefaultModel": "deepseek-ai/deepseek-coder-6.7b-instruct",
    "AdditionalModels": [
      "bigcode/starcoder",
      "codellama/CodeLlama-13b-Instruct-hf"
    ],
    "Priority": 2
  }
}
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ApiKeyEnvironmentVariable` | string | `"HUGGINGFACE_API_KEY"` | Environment variable containing API key |
| `BaseUrl` | string | `"https://api-inference.huggingface.co/models"` | HuggingFace API endpoint |
| `DefaultModel` | string | `"deepseek-ai/deepseek-coder-6.7b-instruct"` | Primary model to use |
| `AdditionalModels` | array | `[...]` | Alternative models available |
| `Priority` | integer | `2` | Provider priority |

**Setup:**
```bash
export HUGGINGFACE_API_KEY="your-api-key-here"
```

---

## Code Generation

Settings for generated code behavior.

```json
{
  "CodeGeneration": {
    "DefaultNamespace": "AutomationTests",
    "OutputPath": "./Generated",
    "SkillsPath": "./Skills",
    "TemplatesPath": "./Templates",
    "AutoFormat": true,
    "ValidateSyntax": true,
    "CreateBackups": true,
    "PreferredLocator": "CssSelector",
    "PageClassSuffix": "Page",
    "StepClassSuffix": "Steps"
  }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultNamespace` | string | `"AutomationTests"` | Default namespace for generated code |
| `OutputPath` | string | `"./Generated"` | Default output directory for generated files |
| `SkillsPath` | string | `"./Skills"` | Directory containing skill definitions |
| `TemplatesPath` | string | `"./Templates"` | Directory containing code templates |
| `AutoFormat` | boolean | `true` | Automatically format generated code |
| `ValidateSyntax` | boolean | `true` | Validate syntax before saving |
| `CreateBackups` | boolean | `true` | Create backups of existing files before overwriting |
| `PreferredLocator` | string | `"CssSelector"` | Preferred locator strategy: `CssSelector`, `XPath`, `Id`, `Name` |
| `PageClassSuffix` | string | `"Page"` | Suffix for page object class names |
| `StepClassSuffix` | string | `"Steps"` | Suffix for step definition class names |

---

## Skills

Configuration for the skills system.

```json
{
  "Skills": {
    "DefaultSkillsDirectory": "./Skills",
    "UserSkillsDirectory": "~/.aikeymouse/skills",
    "AutoReload": true,
    "ValidateOnLoad": true,
    "CacheSkills": true
  }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DefaultSkillsDirectory` | string | `"./Skills"` | Built-in skills directory |
| `UserSkillsDirectory` | string | `"~/.aikeymouse/skills"` | User-defined skills directory |
| `AutoReload` | boolean | `true` | Automatically reload skills when changed |
| `ValidateOnLoad` | boolean | `true` | Validate skill definitions on load |
| `CacheSkills` | boolean | `true` | Cache loaded skills in memory |

---

## Project Structure

Directory structure for generated files.

```json
{
  "Structure": {
    "PagesDirectory": "Pages",
    "StepsDirectory": "StepDefinitions",
    "FeaturesDirectory": "Features",
    "SkillsDirectory": "Skills",
    "OutputDirectory": "Generated"
  }
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PagesDirectory` | string | `"Pages"` | Directory for page object classes |
| `StepsDirectory` | string | `"StepDefinitions"` | Directory for step definition classes |
| `FeaturesDirectory` | string | `"Features"` | Directory for feature files |
| `SkillsDirectory` | string | `"Skills"` | Directory for custom skills |
| `OutputDirectory` | string | `"Generated"` | Root directory for all generated code |

---

## Project Settings

General project configuration.

```json
{
  "ProjectName": "AutomationTests",
  "RootNamespace": "AutomationTests",
  "TargetFramework": "net8.0"
}
```

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProjectName` | string | `"AutomationTests"` | Project name |
| `RootNamespace` | string | `"AutomationTests"` | Root namespace for generated code |
| `TargetFramework` | string | `"net8.0"` | Target .NET framework version |

---

## Environment Variables

The following environment variables are used:

| Variable | Required | Used By | Description |
|----------|----------|---------|-------------|
| `GROQ_API_KEY` | Optional | Groq provider | Groq API authentication |
| `HUGGINGFACE_API_KEY` | Optional | HuggingFace provider | HuggingFace API authentication |

**Note:** At least one LLM provider must be configured. Ollama (local) does not require API keys.

---

## Example: Minimal Configuration

```json
{
  "Logging": {
    "MinimumLevel": "Information",
    "EnableFileLogging": true,
    "EnableConsoleLogging": true
  },
  "Llm": {
    "PreferredProvider": "auto",
    "Ollama": {
      "BaseUrl": "http://localhost:11434",
      "DefaultModel": "llama3.1:8b",
      "Priority": 0
    }
  },
  "CodeGeneration": {
    "DefaultNamespace": "MyProject.Tests"
  }
}
```

---

## Troubleshooting

### Ollama Connection Issues
- Ensure Ollama is running: `ollama serve`
- Check models are installed: `ollama list`
- Verify BaseUrl is correct (default: `http://localhost:11434`)

### API Key Issues
- Verify environment variables are set
- Check variable names match configuration
- Restart terminal/IDE after setting variables

### Timeout Issues
- Increase `TimeoutSeconds` for slow models
- Ollama: 300s recommended for complex generation
- Cloud providers: 60s usually sufficient

### Performance Tips
- Use Ollama for unlimited local generation
- Cache skills with `CacheSkills: true`
- Enable syntax validation to catch errors early
- Use appropriate temperature (0.3-0.5 for code generation)
