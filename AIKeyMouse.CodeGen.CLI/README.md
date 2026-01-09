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

### Project Configuration

Initialize a project configuration:

```bash
dotnet aikeymouse-codegen config init
```

This creates `aikeymouse.config.json` in your project directory.

## Usage

### Generate Page Objects

```bash
# From feature file
dotnet aikeymouse-codegen page --feature Features/Login.feature

# From HTML page source
dotnet aikeymouse-codegen page --html login.html --name LoginPage

# With custom output directory
dotnet aikeymouse-codegen page --feature Login.feature --output Pages/Authentication
```

### Generate Step Definitions

```bash
# From feature file
dotnet aikeymouse-codegen steps --feature Features/Login.feature

# With explicit page references
dotnet aikeymouse-codegen steps --feature Login.feature --pages LoginPage,DashboardPage
```

### Interactive Chat

```bash
dotnet aikeymouse-codegen chat "Generate a page object for login with username, password fields and submit button"
```

### Manage Skills

```bash
# List available skills
dotnet aikeymouse-codegen skill list

# Create custom skill
dotnet aikeymouse-codegen skill create --name "My Page Object" --category page-object

# Validate skill
dotnet aikeymouse-codegen skill validate --file ./skills/custom.skill.json
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

This is currently in **Phase 1** implementation:
- ✅ Project structure created
- ✅ Configuration system implemented
- ✅ Logging infrastructure set up
- ✅ Base command framework ready
- ✅ Cocona CLI framework integrated
- ✅ .NET tool packaging configured
- ⏳ LLM providers (Phase 2)
- ⏳ Skills system (Phase 3)
- ⏳ Code generation (Phase 5)

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
├── Commands/           # CLI command handlers
├── Services/           # Core services (LLM, CodeGen, Parsers, Skills)
├── Models/            # Data models
├── Skills/            # Built-in skill definitions
├── Templates/         # Code templates
└── Config/            # Configuration files
```

## Documentation

See [CODEGEN_CLI_PLAN.md](../CODEGEN_CLI_PLAN.md) for the complete implementation plan.

## License

MIT License - see LICENSE file for details.
