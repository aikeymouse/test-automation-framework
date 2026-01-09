# AIKeyMouse.CodeGen.CLI - Implementation Plan

## Executive Summary

Create a standalone .NET CLI tool that generates Page Objects and Step Definitions using free LLM providers (Groq + HuggingFace). The tool uses a customizable Skills system allowing users to define and share code generation patterns. No image/OCR analysis in initial version.

---

## 1. Project Overview

### 1.1 Goals
- Generate Page Object classes from feature files and HTML/XML sources
- Generate Step Definition classes from feature files and existing pages
- Support multiple platforms: Web (Selenium), Mobile (Appium), Desktop (FlaUI)
- Use free LLM providers with automatic failover
- Allow users to create and customize code generation skills
- Distribute as standalone executable (no .NET SDK required)

### 1.2 Non-Goals (Out of Scope for v1.0)
- ❌ Image/screenshot analysis and OCR
- ❌ VS Code extension (planned for later)
- ❌ Visual Studio extension
- ❌ Test recorder functionality
- ❌ Interactive UI element picker
- ❌ Real-time code suggestions

---

## 2. Technical Architecture

### 2.1 Project Structure

```
AIKeyMouse.CodeGen.CLI/
├── AIKeyMouse.CodeGen.CLI.csproj
├── Program.cs                          # Entry point, DI setup
├── GlobalUsings.cs                     # Global using directives
│
├── Commands/                           # CLI command handlers
│   ├── BaseCommand.cs                  # Common command functionality
│   ├── PageCommand.cs                  # Generate page objects
│   ├── StepsCommand.cs                 # Generate step definitions
│   ├── ChatCommand.cs                  # Interactive chat mode
│   ├── SkillCommand.cs                 # Skill management (list/create/validate)
│   └── ConfigCommand.cs                # Configuration management
│
├── Services/
│   ├── LLM/                           # LLM provider abstraction
│   │   ├── ILlmProvider.cs            # Provider interface
│   │   ├── LlmProviderFactory.cs      # Factory with failover logic
│   │   ├── GroqProvider.cs            # Groq API implementation
│   │   ├── HuggingFaceProvider.cs     # HuggingFace API implementation
│   │   ├── LlmRequest.cs              # Request model
│   │   └── LlmResponse.cs             # Response model
│   │
│   ├── CodeGeneration/                # Code generation services
│   │   ├── ICodeGenerator.cs          # Generator interface
│   │   ├── PageObjectGenerator.cs     # Page object generation
│   │   ├── StepDefinitionGenerator.cs # Step definition generation
│   │   ├── CodeFormatter.cs           # Roslyn-based formatting
│   │   └── CodeValidator.cs           # Syntax validation
│   │
│   ├── Parsers/                       # Input parsing services
│   │   ├── IParser.cs                 # Parser interface
│   │   ├── GherkinParser.cs           # Feature file parsing
│   │   ├── HtmlParser.cs              # HTML page source parsing
│   │   └── XmlParser.cs               # XML page source parsing
│   │
│   ├── Skills/                        # Skills system
│   │   ├── ISkillEngine.cs            # Skill engine interface
│   │   ├── SkillEngine.cs             # Skill execution engine
│   │   ├── SkillLoader.cs             # Load/validate skills
│   │   ├── PromptBuilder.cs           # Build LLM prompts from skills
│   │   └── SkillValidator.cs          # Validate skill definitions
│   │
│   └── Infrastructure/                # Cross-cutting services
│       ├── FileService.cs             # File I/O operations
│       ├── ConfigurationService.cs    # Config management
│       ├── LoggingService.cs          # Logging (Serilog)
│       └── RetryService.cs            # Retry logic (Polly)
│
├── Models/
│   ├── Configuration/                 # Configuration models
│   │   ├── CliConfig.cs               # Main CLI config
│   │   ├── LlmProviderConfig.cs       # LLM provider settings
│   │   └── ProjectConfig.cs           # Project-specific config
│   │
│   ├── Skills/                        # Skill definition models
│   │   ├── Skill.cs                   # Skill definition
│   │   ├── SkillParameter.cs          # Skill parameters
│   │   ├── SkillPrompt.cs             # Prompt configuration
│   │   ├── SkillPostProcessing.cs     # Post-processing rules
│   │   └── SkillOutput.cs             # Output configuration
│   │
│   ├── Generation/                    # Generation models
│   │   ├── GenerationRequest.cs       # Generation request
│   │   ├── GenerationResult.cs        # Generation result
│   │   ├── PageObjectModel.cs         # Page object metadata
│   │   └── StepDefinitionModel.cs     # Step definition metadata
│   │
│   └── Parsing/                       # Parsing models
│       ├── FeatureFile.cs             # Parsed feature file
│       ├── Scenario.cs                # Gherkin scenario
│       ├── Step.cs                    # Gherkin step
│       ├── Element.cs                 # UI element
│       └── PageStructure.cs           # Parsed page structure
│
├── Skills/                            # Built-in skill definitions
│   ├── PageObjects/
│   │   ├── page-object-web.skill.json
│   │   ├── page-object-mobile.skill.json
│   │   └── page-object-desktop.skill.json
│   ├── StepDefinitions/
│   │   ├── step-definition-web.skill.json
│   │   ├── step-definition-mobile.skill.json
│   │   └── step-definition-desktop.skill.json
│   ├── Shared/
│   │   ├── naming-conventions.skill.json
│   │   └── locator-strategy.skill.json
│   └── README.md                      # Skills documentation
│
├── Templates/                         # Liquid templates (optional)
│   ├── PageObjectTemplate.cs.liquid
│   └── StepDefinitionTemplate.cs.liquid
│
└── Config/
    ├── appsettings.json               # Default settings
    └── appsettings.Development.json   # Dev settings
```

### 2.2 Technology Stack

**Core Framework:**
- .NET 8.0 (for AOT compilation support)
- C# 12 (primary constructors, collection expressions)

**Key NuGet Packages:**
- `System.CommandLine` 2.0.0-beta4.24324.3 - CLI framework
- `Microsoft.Extensions.Http` 9.0.0 - HTTP client
- `Polly` 8.5.0 - Resilience & retry
- `Gherkin` 30.0.2 - Feature file parsing
- `HtmlAgilityPack` 1.11.71 - HTML parsing
- `Microsoft.CodeAnalysis.CSharp` 4.12.0 - Code analysis & formatting
- `Fluid.Core` 2.12.0 - Template engine
- `Serilog` 4.2.0 - Logging
- `YamlDotNet` 16.2.1 - YAML support (optional for skills)

**Distribution:**
- Native AOT (Ahead-of-Time compilation) for standalone executable
- Single-file publish for easy distribution
- Platform-specific builds (Windows x64, macOS ARM64, Linux x64)

---

## 3. Skills System Design

### 3.1 Skills Concept

Skills are reusable, composable code generation templates that define:
- **What to generate** (page objects, steps, etc.)
- **How to generate** (prompts, examples, rules)
- **Where to place** (output paths, naming)
- **How to validate** (required usings, syntax checks)

### 3.2 Skill Composition & Reusability

Skills support inheritance and composition:

```json
{
  "skillId": "page-object-web-custom",
  "name": "Custom Web Page Object",
  "extends": "page-object-web",  // Inherits from base skill
  "imports": [
    "naming-conventions",          // Import shared naming rules
    "locator-strategy"             // Import locator preferences
  ],
  "overrides": {
    "llmConfig.temperature": 0.3,  // Override specific values
    "namingConventions.className": "{{pageName}}"  // Custom naming
  }
}
```

**Skill Types:**
1. **Base Skills** - Complete, standalone definitions (e.g., `page-object-web`)
2. **Partial Skills** - Reusable fragments (e.g., `naming-conventions`)
3. **Override Skills** - Extend base skills with customizations

### 3.3 Skill Definition Schema

```json
{
  "$schema": "https://aikeymouse.com/schemas/skill-v1.json",
  
  // Identity
  "skillId": "page-object-web",
  "name": "Web Page Object Generator",
  "version": "1.0.0",
  "description": "Generates Selenium-based page objects",
  "author": "AIKeyMouse",
  "category": "page-object",
  "subcategory": "web",
  
  // Inheritance & Composition
  "extends": "base-skill-id",       // Optional: inherit from another skill
  "imports": [                       // Optional: import reusable fragments
    "naming-conventions",
    "locator-strategy"
  ],
  
  // Dependencies
  "targetFramework": "net8.0",
  "dependencies": [
    "AIKeyMouse.Automation.Framework >= 0.0.1",
    "OpenQA.Selenium >= 4.39.0"
  ],
  
  // Input Parameters
  "parameters": [
    {
      "name": "pageName",
      "type": "string",
      "required": true,
      "description": "Page name without suffix",
      "validation": "^[A-Z][a-zA-Z0-9]*$",
      "default": null
    },
    {
      "name": "elements",
      "type": "array",
      "required": true,
      "description": "UI elements with locators",
      "itemSchema": {
        "name": "string",
        "locatorType": "CssSelector|Id|Name|XPath|ClassName",
        "locatorValue": "string",
        "description": "string?"
      }
    }
  ],
  
  // LLM Configuration
  "llmConfig": {
    "provider": "auto",               // auto, groq, huggingface
    "model": null,                    // null = provider default
    "systemPrompt": "You are an expert C# test automation engineer...",
    "userPromptTemplate": "Generate a C# Page Object class:\n\nClass: {{pageName}}Page\nNamespace: {{namespace}}\n...",
    "temperature": 0.5,
    "maxTokens": 2500,
    "stopSequences": []
  },
  
  // Post-Processing
  "postProcessing": {
    "formatCode": true,
    "validateSyntax": true,
    "organizeUsings": true,
    "requiredUsings": [
      "AIKeyMouse.Automation.Framework.Infrastructure",
      "OpenQA.Selenium"
    ],
    "optionalUsings": [
      "OpenQA.Selenium.Support.UI"
    ]
  },
  
  // Output Configuration
  "output": {
    "fileNameTemplate": "{{pageName}}Page.cs",
    "defaultDirectory": "Pages",
    "useSubfolders": true,
    "subfolderTemplate": "{{category}}",
    "backupExisting": true,
    "overwritePrompt": true
  },
  
  // Naming Conventions
  "namingConventions": {
    "className": "{{pageName}}Page",
    "elementFieldPrefix": "_",
    "propertyPrefix": "",
    "methodPrefix": ""
  },
  
  // Examples for Few-Shot Learning
  "examples": [
    {
      "description": "Simple login page",
      "input": {
        "pageName": "Login",
        "elements": [
          {"name": "Username", "locatorType": "CssSelector", "locatorValue": "#username"}
        ]
      },
      "expectedOutput": "// C# code example..."
    }
  ],
  
  // Metadata
  "metadata": {
    "created": "2026-01-08",
    "lastModified": "2026-01-08",
    "tags": ["selenium", "web", "page-object"],
    "complexity": "intermediate"
  }
}
```

### 3.4 Built-in Skills

**Tier 1: Core Skills** (Must implement)
- `page-object-web.skill.json` - Selenium web page objects
- `step-definition-web.skill.json` - Reqnroll step definitions for web
- `naming-conventions.skill.json` - Shared naming rules
- `locator-strategy.skill.json` - CSS-first locator preferences

**Tier 2: Extended Skills** (Future)
- `page-object-mobile.skill.json` - Appium mobile page objects
- `page-object-desktop.skill.json` - FlaUI desktop page objects
- `step-definition-mobile.skill.json` - Mobile step definitions
- `step-definition-desktop.skill.json` - Desktop step definitions

### 3.5 User Custom Skills

Users can create skills in:
1. **Project directory:** `./skills/my-custom.skill.json`
2. **User directory:** `~/.aikeymouse/skills/my-custom.skill.json`
3. **Specify via CLI:** `--skill-path /path/to/custom.skill.json`

**Skill discovery order:**
1. Explicit path from CLI (`--skill-path`)
2. Project skills directory (`./skills/`)
3. User skills directory (`~/.aikeymouse/skills/`)
4. Built-in skills (embedded in executable)

---

## 4. LLM Provider Integration

### 4.1 Provider Abstraction

```csharp
public interface ILlmProvider
{
    string Name { get; }
    int Priority { get; }
    bool IsAvailable { get; }
    
    Task<LlmResponse> GenerateAsync(
        LlmRequest request, 
        CancellationToken cancellationToken = default);
    
    Task<bool> ValidateConnectionAsync(
        CancellationToken cancellationToken = default);
    
    Task<ProviderStatus> GetStatusAsync(
        CancellationToken cancellationToken = default);
}
```

### 4.2 Groq Provider (Primary)

**API Details:**
- Endpoint: `https://api.groq.com/openai/v1/chat/completions`
- Models: `llama-3.1-70b-versatile`, `llama-3.1-8b-instant`
- Rate Limit: 14,400 requests/day (free tier)
- API Key: Environment variable `GROQ_API_KEY`

**Model Selection:**
- Default: `llama-3.1-70b-versatile` (best quality)
- Fallback: `llama-3.1-8b-instant` (faster, lower quality)

### 4.3 HuggingFace Provider (Backup)

**API Details:**
- Endpoint: `https://api-inference.huggingface.co/models/{model}`
- Models: `deepseek-ai/deepseek-coder-6.7b-instruct`, `bigcode/starcoder`
- Rate Limit: ~1000 requests/day per model (can rotate models)
- API Key: Environment variable `HUGGINGFACE_API_KEY`

**Model Rotation:**
- Primary: `deepseek-ai/deepseek-coder-6.7b-instruct`
- Secondary: `bigcode/starcoder`
- Tertiary: `codellama/CodeLlama-13b-Instruct-hf`

### 4.4 Failover Strategy

```
┌─────────────────────────────────────────────┐
│  1. Try Groq (llama-3.1-70b-versatile)     │
│     ↓ (if fails or rate limited)           │
│  2. Try Groq (llama-3.1-8b-instant)        │
│     ↓ (if fails)                           │
│  3. Try HuggingFace (deepseek-coder)       │
│     ↓ (if fails)                           │
│  4. Try HuggingFace (starcoder)            │
│     ↓ (if fails)                           │
│  5. Try HuggingFace (codellama)            │
│     ↓ (if fails)                           │
│  6. Return error with retry suggestions    │
└─────────────────────────────────────────────┘
```

**Retry Policy (Polly):**
- Transient errors: Retry 3 times with exponential backoff
- Rate limit (429): Wait and retry with backoff
- Server error (5xx): Retry 2 times
- Client error (4xx except 429): Fail immediately

---

## 5. Input Parsers

### 5.1 Gherkin Parser

**Purpose:** Extract scenarios, steps, and potential page objects from `.feature` files

**Input:**
```gherkin
Feature: User Login

Scenario: Successful login
  Given I am on the login page
  When I enter username "john@example.com"
  And I enter password "password123"
  And I click the login button
  Then I should see the dashboard
```

**Output:**
```csharp
// Identified elements:
- UsernameInput (input field)
- PasswordInput (input field)
- LoginButton (button)
- Dashboard (page/element)

// Suggested page: LoginPage
// Suggested step: LoginSteps
```

### 5.2 HTML Parser

**Purpose:** Extract interactive elements and suggest locators (prefer CSS)

**Input:**
```html
<form id="loginForm">
  <input id="username" name="username" type="text" />
  <input id="password" name="password" type="password" />
  <button type="submit" class="btn-primary">Login</button>
</form>
```

**Output:**
```csharp
// Suggested elements with CSS selectors:
- UsernameInput: CssSelector "#username"
- PasswordInput: CssSelector "#password"
- LoginButton: CssSelector "button[type='submit']"
  // Alternative: CssSelector ".btn-primary"
```

**Locator Priority:**
1. `id` attribute → `CssSelector "#id"`
2. `name` attribute → `CssSelector "[name='value']"`
3. `class` attribute → `CssSelector ".class"`
4. `type` + `class` → `CssSelector "input[type='text'].class"`
5. XPath as last resort

### 5.3 XML Parser

**Purpose:** Parse Android/iOS app page source

**Input (Android):**
```xml
<android.widget.EditText 
  resource-id="com.example:id/username" 
  text="" 
  class="android.widget.EditText" />
```

**Output:**
```csharp
// Suggested element:
- UsernameInput: Id "com.example:id/username"
  // Alternative: ClassName "android.widget.EditText"
```

---

## 6. Code Generation

### 6.1 Page Object Generation

**Input:**
- Skill: `page-object-web.skill.json`
- Page name: `Login`
- Elements: `[(Username, CssSelector, #username), (Password, CssSelector, #password), (LoginButton, CssSelector, button[type='submit'])]`

**Generated Output:**
```csharp
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Extensions;
using OpenQA.Selenium;

namespace MyProject.Automation.Pages;

/// <summary>
/// Page Object for Login page
/// </summary>
public partial class LoginPage(DriverContext driverContext) : PageBase(driverContext)
{
    private readonly ElementLocator _usernameInput = new(Locator.CssSelector, "#username");
    private readonly ElementLocator _passwordInput = new(Locator.CssSelector, "#password");
    private readonly ElementLocator _loginButton = new(Locator.CssSelector, "button[type='submit']");

    public IWebElement UsernameInput => WebDriver.GetElement(_usernameInput);
    public IWebElement PasswordInput => WebDriver.GetElement(_passwordInput);
    public IWebElement LoginButton => WebDriver.GetElement(_loginButton);

    /// <summary>
    /// Performs login with provided credentials
    /// </summary>
    public void Login(string username, string password)
    {
        UsernameInput.SendKeys(username);
        PasswordInput.SendKeys(password);
        LoginButton.Click();
    }
}
```

### 6.2 Step Definition Generation

**Input:**
- Skill: `step-definition-web.skill.json`
- Feature file: `Login.feature`
- Referenced pages: `LoginPage`

**Generated Output:**
```csharp
using AIKeyMouse.Automation.Framework.Infrastructure;
using MyProject.Automation.Pages;
using Reqnroll;

namespace MyProject.Automation.StepDefinitions;

[Binding]
public class LoginSteps(DriverContext driverContext) : StepsBase(driverContext)
{
    private readonly LoginPage _loginPage = new(driverContext);

    [Given(@"I am on the login page")]
    public void GivenIAmOnTheLoginPage()
    {
        Driver.WebDriver.Navigate().GoToUrl("https://example.com/login");
    }

    [When(@"I enter username ""(.*)""")]
    public void WhenIEnterUsername(string username)
    {
        _loginPage.UsernameInput.SendKeys(username);
    }

    [When(@"I enter password ""(.*)""")]
    public void WhenIEnterPassword(string password)
    {
        _loginPage.PasswordInput.SendKeys(password);
    }

    [When(@"I click the login button")]
    public void WhenIClickTheLoginButton()
    {
        _loginPage.LoginButton.Click();
    }
}
```

### 6.3 Code Formatting & Validation

**Formatting (Roslyn):**
- Use `Microsoft.CodeAnalysis.CSharp.Formatting.Formatter`
- Apply consistent indentation (4 spaces)
- Organize using statements alphabetically
- Add file-scoped namespaces (C# 10+)

**Validation:**
- Parse with `CSharpSyntaxTree.ParseText()`
- Check for compilation errors
- Verify required using statements
- Validate naming conventions
- Ensure base class inheritance

---

## 7. Configuration System

### 7.1 Configuration Sources (Priority Order)

1. **CLI Arguments** (highest)
   ```bash
   aikeymouse-codegen page --provider Groq --temperature 0.3
   ```

2. **Environment Variables**
   ```bash
   export GROQ_API_KEY="gsk_xxx"
   export HUGGINGFACE_API_KEY="hf_xxx"
   export AIKEYMOUSE_PREFERRED_PROVIDER="Groq"
   ```

3. **Project Config** (`./aikeymouse.config.json`)
   ```json
   {
     "projectName": "MyAutomationProject",
     "rootNamespace": "MyProject.Automation",
     "llm": {
       "preferredProvider": "Groq",
       "temperature": 0.5
     }
   }
   ```

4. **User Config** (`~/.aikeymouse/config.json`)
   ```json
   {
     "llm": {
       "groq": {
         "apiKey": "gsk_xxx"
       },
       "huggingface": {
         "apiKey": "hf_xxx"
       }
     }
   }
   ```

5. **Default Config** (`appsettings.json` - embedded)

### 7.2 Configuration Model

```csharp
public class CliConfig
{
    public string ProjectName { get; set; } = "AutomationTests";
    public string RootNamespace { get; set; } = "AutomationTests";
    public string TargetFramework { get; set; } = "net8.0";
    
    public ProjectStructure Structure { get; set; } = new();
    public LlmConfiguration Llm { get; set; } = new();
    public CodeGenerationConfig CodeGeneration { get; set; } = new();
    public SkillsConfiguration Skills { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
}

public class LlmConfiguration
{
    public string PreferredProvider { get; set; } = "auto";
    public double Temperature { get; set; } = 0.5;
    public int MaxTokens { get; set; } = 2500;
    public GroqConfig Groq { get; set; } = new();
    public HuggingFaceConfig HuggingFace { get; set; } = new();
}

public class ProjectStructure
{
    public string PagesDirectory { get; set; } = "Pages";
    public string StepsDirectory { get; set; } = "StepDefinitions";
    public string FeaturesDirectory { get; set; } = "Features";
    public string SkillsDirectory { get; set; } = "Skills";
}
```

---

## 8. CLI Commands

### 8.1 Command Structure

```bash
aikeymouse-codegen <command> [options]

Commands:
  page      Generate page object classes
  steps     Generate step definition classes
  chat      Interactive code generation
  skill     Manage skills (list/create/validate)
  config    Manage configuration (init/set/get)
  
Options:
  --version     Show version information
  --help        Show help and usage
  --verbose     Enable verbose logging
```

### 8.2 Page Command

```bash
aikeymouse-codegen page [options]

Options:
  --feature <path>        Feature file to analyze
  --html <path>           HTML page source file
  --xml <path>            XML page source file (mobile)
  --name <name>           Page class name (required if no --feature)
  --namespace <ns>        Target namespace (default: from config)
  --output <path>         Output directory (default: Pages/)
  --skill <skillId>       Skill to use (default: page-object-web)
  --skill-path <path>     Custom skill file path
  --provider <name>       LLM provider (groq|huggingface|auto)
  --temperature <num>     LLM temperature (0.0-1.0)
  --dry-run              Preview without saving
  --overwrite            Overwrite existing files
  --verbose              Verbose output
  
Examples:
  # From feature file
  aikeymouse-codegen page --feature Features/Login.feature
  
  # From HTML source with custom name
  aikeymouse-codegen page --html login.html --name LoginPage
  
  # With custom skill and provider
  aikeymouse-codegen page --feature Login.feature --skill my-custom --provider Groq
  
  # Dry run to preview
  aikeymouse-codegen page --feature Login.feature --dry-run
```

### 8.3 Steps Command

```bash
aikeymouse-codegen steps [options]

Options:
  --feature <path>        Feature file to analyze (required)
  --pages <names>         Comma-separated page class names
  --name <name>           Step class name (default: from feature)
  --namespace <ns>        Target namespace (default: from config)
  --output <path>         Output directory (default: StepDefinitions/)
  --skill <skillId>       Skill to use (default: step-definition-web)
  --skill-path <path>     Custom skill file path
  --provider <name>       LLM provider (groq|huggingface|auto)
  --dry-run              Preview without saving
  --overwrite            Overwrite existing files
  
Examples:
  # Generate from feature file
  aikeymouse-codegen steps --feature Features/Login.feature
  
  # With explicit page references
  aikeymouse-codegen steps --feature Login.feature --pages LoginPage,DashboardPage
  
  # Custom output location
  aikeymouse-codegen steps --feature Login.feature --output Tests/Steps
```

### 8.4 Chat Command

```bash
aikeymouse-codegen chat <prompt>

Options:
  --provider <name>       LLM provider (groq|huggingface|auto)
  --temperature <num>     LLM temperature
  --output <path>         Save generated code to file
  
Examples:
  # Interactive generation
  aikeymouse-codegen chat "Generate a page object for login with username, password, and submit button"
  
  # Save to file
  aikeymouse-codegen chat "Generate login steps" --output LoginSteps.cs
```

### 8.5 Skill Command

```bash
aikeymouse-codegen skill <subcommand> [options]

Subcommands:
  list              List available skills
  create            Create new skill from template
  validate          Validate skill definition
  show              Show skill details
  
Options:
  list:
    --category <cat>    Filter by category
    --user-only        Show only user skills
    
  create:
    --name <name>       Skill name (required)
    --category <cat>    Category (page-object|step-definition)
    --extends <id>      Base skill to extend
    --output <path>     Output file path
    
  validate:
    --file <path>       Skill file to validate (required)
    
  show:
    --id <skillId>      Skill ID (required)
    
Examples:
  # List all skills
  aikeymouse-codegen skill list
  
  # List page object skills only
  aikeymouse-codegen skill list --category page-object
  
  # Create new skill
  aikeymouse-codegen skill create --name "My Page Object" --category page-object --extends page-object-web
  
  # Validate custom skill
  aikeymouse-codegen skill validate --file ./skills/custom.skill.json
  
  # Show skill details
  aikeymouse-codegen skill show --id page-object-web
```

### 8.6 Config Command

```bash
aikeymouse-codegen config <subcommand> [options]

Subcommands:
  init              Initialize project configuration
  set               Set configuration value
  get               Get configuration value
  show              Show all configuration
  
Options:
  init:
    --force           Overwrite existing config
    
  set:
    --key <key>       Config key (e.g., llm.groq.apiKey)
    --value <value>   Config value
    --user           Save to user config (~/.aikeymouse/)
    --project        Save to project config (./aikeymouse.config.json)
    
  get:
    --key <key>       Config key
    
Examples:
  # Initialize project config
  aikeymouse-codegen config init
  
  # Set API key in user config
  aikeymouse-codegen config set --key llm.groq.apiKey --value "gsk_xxx" --user
  
  # Get current provider
  aikeymouse-codegen config get --key llm.preferredProvider
  
  # Show all configuration
  aikeymouse-codegen config show
```

---

## 9. Standalone Executable Distribution

### 9.1 Native AOT Compilation

**.csproj Configuration:**
```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <PublishSingleFile>true</PublishSingleFile>
  <PublishTrimmed>true</PublishTrimmed>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier> <!-- or osx-arm64, linux-x64 -->
  <InvariantGlobalization>false</InvariantGlobalization>
  <IlcOptimizationPreference>Speed</IlcOptimizationPreference>
</PropertyGroup>
```

### 9.2 Build Commands

```bash
# Windows x64
dotnet publish -c Release -r win-x64 -p:PublishAot=true

# macOS ARM64
dotnet publish -c Release -r osx-arm64 -p:PublishAot=true

# Linux x64
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
```

### 9.3 Distribution Artifacts

```
dist/
├── windows/
│   └── aikeymouse-codegen.exe      (standalone Windows executable)
├── macos/
│   └── aikeymouse-codegen          (standalone macOS executable)
└── linux/
    └── aikeymouse-codegen          (standalone Linux executable)
```

### 9.4 Installation

**Option 1: Manual**
```bash
# Download executable for your platform
# Windows
curl -L https://github.com/aikeymouse/test-automation-framework/releases/latest/download/aikeymouse-codegen-win-x64.exe -o aikeymouse-codegen.exe

# Add to PATH
# Windows: Add to System Environment Variables
# macOS/Linux: mv aikeymouse-codegen /usr/local/bin/
```

**Option 2: Install Script** (Future)
```bash
# Windows (PowerShell)
iwr https://aikeymouse.com/install.ps1 | iex

# macOS/Linux
curl -fsSL https://aikeymouse.com/install.sh | bash
```

---

## 10. Implementation Phases

### Phase 1: Foundation (Week 1)
**Goal:** Basic project structure and configuration

- [x] Create project structure
- [ ] Set up .csproj with all dependencies
- [ ] Implement configuration system (CliConfig, ConfigurationService)
- [ ] Set up logging (Serilog)
- [ ] Create base command infrastructure (BaseCommand)
- [ ] Implement FileService for I/O operations

**Deliverable:** Working CLI that can load config and parse arguments

---

### Phase 2: LLM Integration (Week 1-2)
**Goal:** Working LLM provider abstraction with failover

- [ ] Define ILlmProvider interface
- [ ] Implement GroqProvider
- [ ] Implement HuggingFaceProvider
- [ ] Create LlmProviderFactory with failover logic
- [ ] Add retry policies with Polly
- [ ] Write unit tests for provider failover

**Deliverable:** Can make LLM requests with automatic failover

---

### Phase 3: Skills System (Week 2)
**Goal:** Load, validate, and execute skills

- [ ] Define Skill models (Skill, SkillParameter, etc.)
- [ ] Implement SkillLoader (load from JSON)
- [ ] Implement SkillValidator (schema validation)
- [ ] Create PromptBuilder (Fluid templates)
- [ ] Implement skill inheritance and composition
- [ ] Create 2 built-in skills: page-object-web, step-definition-web

**Deliverable:** Can load skills and build prompts

---

### Phase 4: Parsers (Week 2-3)
**Goal:** Parse feature files and HTML/XML sources

- [ ] Implement GherkinParser (parse .feature files)
- [ ] Implement HtmlParser (extract elements from HTML)
- [ ] Implement XmlParser (parse mobile app page source)
- [ ] Create element extraction logic with CSS preference
- [ ] Map parsed data to generation models

**Deliverable:** Can extract elements from various sources

---

### Phase 5: Code Generation (Week 3-4)
**Goal:** Generate, format, and validate C# code

- [ ] Implement PageObjectGenerator
- [ ] Implement StepDefinitionGenerator
- [ ] Implement CodeFormatter (Roslyn)
- [ ] Implement CodeValidator (syntax checking)
- [ ] Add post-processing (using statements, formatting)
- [ ] Handle file creation and backup

**Deliverable:** Can generate valid C# code

---

### Phase 6: CLI Commands (Week 4)
**Goal:** Working CLI commands

- [ ] Implement PageCommand
- [ ] Implement StepsCommand
- [ ] Implement ChatCommand
- [ ] Implement SkillCommand (list, create, validate)
- [ ] Implement ConfigCommand (init, set, get)
- [ ] Add help text and examples

**Deliverable:** Full CLI functionality

---

### Phase 7: Native AOT & Distribution (Week 5)
**Goal:** Standalone executable builds

- [ ] Configure project for Native AOT
- [ ] Test AOT compilation on Windows/macOS/Linux
- [ ] Create build scripts for all platforms
- [ ] Add embedded resources (skills, config)
- [ ] Test standalone executables
- [ ] Create installation documentation

**Deliverable:** Standalone executables for Windows/macOS/Linux

---

### Phase 8: Testing & Documentation (Week 5-6)
**Goal:** Production-ready release

- [ ] Write unit tests (80%+ coverage)
- [ ] Write integration tests
- [ ] End-to-end testing with real projects
- [ ] Create user documentation
- [ ] Create skill authoring guide
- [ ] Record demo videos
- [ ] Prepare for v1.0 release

**Deliverable:** v1.0 release with documentation

---

## 11. Success Criteria

### 11.1 Functional Requirements
- ✅ Generate valid, compilable C# page objects from feature files
- ✅ Generate valid, compilable C# step definitions from feature files
- ✅ Support CSS selector preference over XPath
- ✅ Allow users to create and use custom skills
- ✅ Automatic failover between LLM providers
- ✅ Standalone executable (no .NET SDK required)

### 11.2 Quality Requirements
- ✅ 95%+ generated code compiles without errors
- ✅ 90%+ of common patterns supported
- ✅ Average generation time < 10 seconds
- ✅ Graceful error handling with helpful messages
- ✅ Comprehensive logging for debugging

### 11.3 User Experience
- ✅ Clear, intuitive CLI commands
- ✅ Helpful error messages with suggestions
- ✅ Progress indicators for long operations
- ✅ Dry-run mode for preview
- ✅ Well-documented with examples

---

## 12. Future Enhancements (Post-v1.0)

### 12.1 VS Code Extension
- Chat sidebar for interactive generation
- Context menus for feature files
- Code actions (light bulb) for undefined steps
- Real-time skill validation
- Visual skill editor

### 12.2 Advanced Features
- Batch generation (process multiple features)
- Test data generator from examples tables
- Refactoring suggestions for existing code
- Code smell detection
- Performance optimization hints

### 12.3 Additional Platforms
- Mobile platform support (Appium skills)
- Desktop platform support (FlaUI skills)
- API testing support (RestSharp skills)

### 12.4 Collaboration
- Skill marketplace/repository
- Team skill sharing
- Version control for skills
- Skill analytics and usage tracking

---

## 13. Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| LLM API rate limits | High | Multi-provider failover, user can add own keys |
| LLM generates invalid code | High | Syntax validation, post-processing, user review |
| Native AOT limitations | Medium | Test early, use compatible libraries |
| Skill complexity | Medium | Start simple, provide clear examples |
| User adoption | Medium | Great documentation, video tutorials |

---

## 14. Next Steps

1. **Review this plan** - Confirm approach and scope
2. **Create repository structure** - Set up folders and basic files
3. **Start Phase 1** - Implement foundation (config, logging, CLI framework)
4. **Weekly check-ins** - Review progress and adjust plan

---

## Appendix A: Sample Skill Files

See `Skills/` directory for:
- `page-object-web.skill.json` - Full example
- `step-definition-web.skill.json` - Full example
- `naming-conventions.skill.json` - Partial skill example
- `locator-strategy.skill.json` - Partial skill example

## Appendix B: Architecture Diagrams

See `docs/architecture/` for:
- Component interaction diagram
- LLM failover flow
- Skill loading sequence
- Code generation pipeline

## Appendix C: Future Improvements

### MCP-Compatible Skills Format

**Current State:** Skills use JSON format with Liquid templates for code generation.

**Industry Standard:** Anthropic's Model Context Protocol (MCP) defines a skill format that has become a de facto standard by end of 2025:

```
my-skill/
├── SKILL.md          # YAML frontmatter + Markdown instructions
├── scripts/          # Executable code (Python, Bash, etc.)
├── references/       # Reference materials (error codes, guides)
└── assets/           # Templates, documents
```

**Example SKILL.md:**
```markdown
---
name: production-incident-triage
description: Use this skill for P0/P1 alerts for initial diagnosis and communication.
---

# Incident Triage Procedure
## 1. Context Collection
First, collect metrics for the last 15 minutes.
Use the tool query_grafana with the main-cluster-v2 dashboard.

## 2. Severity Check
IF error_rate > 5% OR latency p99 > 2s:
- Declare an incident using create_jira_ticket
- Use template from assets/incident-template.md
```

**Recommendation:**
- **Keep current JSON format** for simple code generation use cases
- **Add MCP format support** when expanding to agent-like capabilities:
  - Multi-step test generation workflows
  - Test analysis and improvement suggestions
  - Automated refactoring
  - Integration with external tools (Jira, GitHub, CI/CD)

**Benefits of MCP Format:**
- Industry standard with community support
- Human-readable Markdown instructions
- Supports executable scripts and workflows
- Better for complex, multi-step agent tasks
- Easier for non-developers to contribute skills

**Implementation Plan:**
1. Create `SkillFormatAdapter` to support both formats
2. Add MCP skill loader alongside current JSON loader
3. Update documentation with MCP examples
4. Maintain backward compatibility with existing JSON skills

---

**Document Version:** 1.0  
**Created:** 2026-01-08  
**Last Updated:** 2026-01-08  
**Author:** AIKeyMouse Team

