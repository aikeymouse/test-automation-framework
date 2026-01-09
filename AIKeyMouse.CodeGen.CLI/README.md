# AIKeyMouse Code Generator CLI

AI-powered code generation tool for test automation using the AIKeyMouse.Automation.Framework.

## Features

- Generate Page Object Model classes from feature files and page sources
- Generate Step Definition classes from Gherkin scenarios
- Support for Web (Selenium), Mobile (Appium), and Desktop (UI Automation) platforms
- Customizable Skills system for code generation patterns
- Multiple LLM providers with automatic failover (Ollama, Groq, HuggingFace)
- Local-first: Prefers Ollama for privacy and speed, falls back to cloud providers
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

### LLM Providers

The tool tries providers in this order:

**1. Ollama (recommended - local, private, unlimited)**
```bash
# Install Ollama from https://ollama.ai
# Pull a code model
ollama pull llama3.1:8b
# Or use codellama, mistral, etc.
ollama pull codellama:13b

# Start Ollama (usually runs automatically)
ollama serve
```

**2. Groq (cloud - 14,400 free requests/day)**
```bash
export GROQ_API_KEY="gsk_your_api_key_here"
```

**3. HuggingFace (cloud - backup provider)**
```bash
export HUGGINGFACE_API_KEY="hf_your_api_key_here"
```

The tool automatically detects which providers are available and uses them in priority order.

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
dotnet aikeymouse-codegen page --name Login --skill-path ./skills/custom-page.skill.md

# Specify a page container (component-based approach)
dotnet aikeymouse-codegen page -n Login -u https://example.com/login -c "#loginForm"
dotnet aikeymouse-codegen page -n Navigation -u https://example.com -c "nav.main-header"
dotnet aikeymouse-codegen page -n UserProfile -u https://example.com/profile -c ".profile-container"
```

#### Container Selector Feature

The `-c` (container) option allows you to generate multiple page objects from the same page by targeting specific components. This enables a component-based Page Object Model architecture.

**Use Cases:**
- **Forms:** `-c "#loginForm"` - Generate a page object for just the login form
- **Navigation:** `-c "nav.header"` - Generate a page object for the navigation component
- **Grids/Tables:** `-c "#userTable"` - Generate a page object for a data table
- **Sections:** `-c ".dashboard-widgets"` - Generate a page object for a specific section

**Smart Container Detection:**

When no container is specified, the tool automatically detects the main content area using:
1. Common content IDs: `#content`, `#main`
2. Common content classes: `.content`, `.main`
3. Semantic HTML: `<main>`, `div[role='main']`
4. Fallback: `<body>` (entire page)

**Example - Multi-Component Page:**

```bash
# Generate separate page objects for different components on the same page
dotnet aikeymouse-codegen page -n LoginForm -u https://example.com/login -c "#loginForm"
dotnet aikeymouse-codegen page -n Header -u https://example.com/login -c "nav.site-header"
dotnet aikeymouse-codegen page -n Footer -u https://example.com/login -c "footer.site-footer"
```

This approach creates smaller, focused page objects that:
- Are easier to maintain
- Have clear responsibilities
- Can be reused across multiple pages
- Reduce coupling between page components

**Benefits:**
- **Scoped Element Detection:** Only finds elements within the specified container
- **Reusable Components:** Same component (e.g., navigation) can be shared across pages
- **Better Organization:** Separate page objects for forms, headers, sidebars, etc.
- **Reduced Complexity:** Smaller, focused classes instead of monolithic page objects
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
dotnet aikeymouse-codegen steps --feature Login.feature --skill-path ./skills/custom-steps.skill.md
```

## Skills System

Skills define how the CLI generates code using AI in Markdown format (`.skill.md`) with YAML frontmatter for easy editing and maintenance.

### Built-in Skills

**Web Platform:**
- `page-object-web.skill.md` - Selenium Page Object with ElementLocator and page container pattern
- `step-definition-web.skill.md` - Reqnroll step definitions with FluentAssertions

### Creating Custom Skills

See **[Skills/SKILL_FORMAT.md](Skills/SKILL_FORMAT.md)** for complete documentation on:
- File format and structure
- YAML frontmatter configuration
- Liquid template syntax
- Examples and best practices
- Testing and troubleshooting

**Quick start:**
```bash
# Copy the template
cp Skills/template.skill.md Skills/YourCategory/my-skill.skill.md

# Edit the skill file (see SKILL_FORMAT.md for details)

# Test your skill
dotnet build
dotnet run -- page --name Test --skill-path Skills/YourCategory/my-skill.skill.md
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
- Web Page Object (Selenium with ElementLocator and page container pattern)
- Web Step Definitions (Reqnroll with FluentAssertions)
- **Markdown format** with YAML frontmatter for easy editing

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
- **LLM Providers:** Ollama (local), Groq API, HuggingFace Inference API
- **Provider Detection:** Fast availability check (<150ms) with 30-second caching
- **Logging:** Serilog with console/file sinks
- **Configuration:** Microsoft.Extensions.Configuration (multi-source)
- **Skills Format:** Markdown with YAML frontmatter (YamlDotNet)
- **Parsing:** Gherkin, HtmlAgilityPack
- **Code Generation:** Roslyn (Microsoft.CodeAnalysis.CSharp)
- **Templating:** Fluid.Core (Liquid templates)
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
│       ├── SkillMarkdownParser.cs
│       ├── SkillValidator.cs
│       └── PromptBuilder.cs
├── Models/               # Data models
│   ├── Configuration/    # Configuration DTOs
│   ├── LLM/             # LLM request/response
│   ├── Parsing/         # Parsed data structures
│   └── Skills/          # Skill definitions
├── Skills/              # Built-in skill files
│   ├── SKILL_FORMAT.md  # Complete skill format guide
│   ├── template.skill.md # Template for creating new skills
│   ├── PageObjects/
│   │   └── page-object-web.skill.md
│   └── StepDefinitions/
│       └── step-definition-web.skill.md
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

- [Configuration Guide](CONFIGURATION.md) - Complete reference for all `appsettings.json` settings
- [Skill Format Guide](Skills/SKILL_FORMAT.md) - How to create custom skills in Markdown format
- [Skill Template](Skills/template.skill.md) - Ready-to-use template for new skills
- [Implementation Plan](../CODEGEN_CLI_PLAN.md) - Complete implementation plan

## License

MIT License - see LICENSE file for details.
