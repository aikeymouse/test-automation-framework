# AIKeyMouse Code Generator CLI

AI-powered code generation tool for test automation using the AIKeyMouse.Automation.Framework.

## Features

- Generate Page Object Model classes from feature files and page sources
- Generate Step Definition classes from Gherkin scenarios
- Support for Web (Selenium), Mobile (Appium), and Desktop (UI Automation) platforms
- Customizable Skills system for code generation patterns
- Multiple LLM providers with automatic failover (Groq, HuggingFace)
- Distributed as .NET tool - easy installation and updates

## Installation

### Install as .NET Tool

**Local installation (recommended for projects):**
```bash
# In your project directory
dotnet tool install --local AIKeyMouse.CodeGen.CLI

# Or restore from manifest
dotnet tool restore
```

**Global installation:**
```bash
dotnet tool install --global AIKeyMouse.CodeGen.CLI

# Update
dotnet tool update --global AIKeyMouse.CodeGen.CLI
```

### Requirements

- .NET 8.0 SDK or later

## Configuration

### API Keys

Set up your LLM provider API keys:

```bash
# Groq (recommended - 14,400 free requests/day)
export GROQ_API_KEY="gsk_your_api_key_here"

# HuggingFace (backup provider)
export HUGGINGFACE_API_KEY="hf_your_api_key_here"
```

### Project Configuration (Optional)

Create a project-specific configuration file `aikeymouse.config.json`:

```json
{
  "Llm": {
    "Temperature": 0.3,
    "MaxTokens": 2000
  },
  "CodeGeneration": {
    "DefaultNamespace": "MyProject.Automation",
    "DefaultPlatform": "web"
  }
}
```

## Usage

### Check Version

```bash
dotnet aikeymouse-codegen version
```

### Test LLM Integration

```bash
dotnet aikeymouse-codegen test-llm --prompt "Write a simple C# hello world method"
```

### Generate Page Objects

```bash
# Basic page object generation
dotnet aikeymouse-codegen page --name Login

# Parse HTML from URL
dotnet aikeymouse-codegen page --name Login --url https://example.com/login

# Parse HTML from file
dotnet aikeymouse-codegen page --name Login --html-file login.html

# With custom output directory and namespace
dotnet aikeymouse-codegen page --name Login --output Pages/Auth --namespace MyApp.Pages

# Specify platform (web/mobile/desktop)
dotnet aikeymouse-codegen page --name Login --platform mobile

# Use custom skill file
dotnet aikeymouse-codegen page --name Login --skill-path ./skills/custom-page.skill.json
```

### Generate Step Definitions

```bash
# Generate from feature file
dotnet aikeymouse-codegen steps --feature Features/Login.feature

# With custom output and namespace
dotnet aikeymouse-codegen steps --feature Login.feature --output Steps --namespace MyApp.Steps

# With explicit page objects
dotnet aikeymouse-codegen steps --feature Login.feature --pages LoginPage,DashboardPage

# Generate for specific scenario only
dotnet aikeymouse-codegen steps --feature Login.feature --scenario "Successful login"

# Use custom skill
dotnet aikeymouse-codegen steps --feature Login.feature --skill-path ./skills/custom-steps.skill.json
```

## IDE Integration

### Visual Studio - External Tools

**Tools → External Tools → Add**

- **Title:** Generate Page Object
- **Command:** `dotnet`
- **Arguments:** `aikeymouse-codegen page --feature "$(ItemPath)" --output Pages`
- **Initial directory:** `$(ProjectDir)`
- **Use Output window:** ✓

### VS Code - Tasks

Add to `.vscode/tasks.json`:
```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Generate Page Object",
      "type": "shell",
      "command": "dotnet",
      "args": ["aikeymouse-codegen", "page", "--feature", "${file}"],
      "problemMatcher": []
    }
  ]
}
```

## Development Status

### Completed Phases

- ✅ **Phase 1:** Foundation - CLI framework, configuration, logging, file I/O
- ✅ **Phase 2:** LLM Integration - Groq and HuggingFace providers with failover
- ✅ **Phase 3:** Skills System - JSON/YAML skills with inheritance and templates
- ✅ **Phase 4:** Parsers - Gherkin feature file and HTML page parsing
- ✅ **Phase 5:** Code Generation - Page object and step definition commands

### Current Capabilities

**Working Commands:**
- `version` - Display tool version
- `test-llm` - Test LLM provider connectivity
- `page` - Generate Page Object Model classes
- `steps` - Generate Reqnroll step definitions

**Built-in Skills:**
- Web Page Object (Selenium with PageFactory pattern)
- Web Step Definitions (Reqnroll with FluentAssertions)

**Parsers:**
- Gherkin feature files (scenarios, steps, tags)
- HTML pages (elements, forms, CSS selectors, XPath)

### Future Enhancements (Phase 6+)

- Interactive chat command
- Skill management commands (list, create, validate)
- Config management command
- Additional platform skills (mobile, desktop)
- Enhanced element detection and grouping

## Technology Stack

- **CLI Framework:** Cocona 2.2.0 (stable)
- **LLM Providers:** Groq API, HuggingFace Inference API
- **Logging:** Serilog with console/file sinks
- **Configuration:** Microsoft.Extensions.Configuration (multi-source)
- **Parsing:** Gherkin, HtmlAgilityPack
- **Code Generation:** Roslyn (Microsoft.CodeAnalysis.CSharp)
- **Templating:** Fluid.Core
- **Resilience:** Polly for HTTP retry/failover
- **Distribution:** .NET Tool (requires .NET 8+ SDK)

## Project Structure

```
AIKeyMouse.CodeGen.CLI/
├── Commands/              # CLI command handlers
│   ├── BaseCommand.cs     # Base class for all commands
│   ├── PageCommand.cs     # Page object generation
│   └── StepsCommand.cs    # Step definition generation
├── Services/
│   ├── CodeGeneration/    # Code generation and validation
│   │   └── CodeGenerator.cs
│   ├── Infrastructure/    # Core infrastructure
│   │   ├── ConfigurationService.cs
│   │   ├── FileService.cs
│   │   └── LoggingService.cs
│   ├── LLM/              # LLM provider integration
│   │   ├── ILlmProvider.cs
│   │   ├── GroqProvider.cs
│   │   ├── HuggingFaceProvider.cs
│   │   └── LlmProviderFactory.cs
│   ├── Parsers/          # Input parsers
│   │   ├── GherkinParser.cs
│   │   └── HtmlParser.cs
│   └── Skills/           # Skills system
│       ├── SkillLoader.cs
│       ├── SkillValidator.cs
│       └── PromptBuilder.cs
├── Models/               # Data models
│   ├── Configuration/    # Configuration DTOs
│   ├── LLM/             # LLM request/response
│   ├── Parsing/         # Parsed data structures
│   └── Skills/          # Skill definitions
├── Skills/              # Built-in skill files
│   ├── PageObjects/
│   │   └── page-object-web.skill.json
│   └── StepDefinitions/
│       └── step-definition-web.skill.json
└── Config/              # Configuration files
    └── appsettings.json
```

## How It Works

1. **Command Execution**: User runs a command (e.g., `page --name Login`)
2. **Skill Loading**: Loads appropriate skill (built-in or custom) with templates and examples
3. **Input Parsing**: Parses HTML or feature files to extract elements/steps
4. **Context Building**: Combines skill template with parsed data
5. **Prompt Generation**: Renders Liquid template to create LLM prompt
6. **LLM Call**: Sends request to LLM provider (with automatic failover)
7. **Code Extraction**: Extracts generated code from LLM response
8. **Validation**: Validates syntax using Roslyn compiler
9. **Formatting**: Formats code with Roslyn formatter
10. **Output**: Writes to file with backup of existing code

## Documentation

See [CODEGEN_CLI_PLAN.md](../CODEGEN_CLI_PLAN.md) for the complete implementation plan.

## License

MIT License - see LICENSE file for details.
