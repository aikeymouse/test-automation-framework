---
id: step-definition-web
name: Web Step Definition Generator
description: Generate Reqnroll step definition classes for web automation scenarios using StepsBase pattern
category: step-definition
platform: web
llmParams:
  temperature: 0.3
  maxTokens: 4000
validation:
  requiredUsings:
    - Reqnroll
    - FluentAssertions
  validateSyntax: true
  autoFormat: true
output:
  defaultPath: StepDefinitions
  fileNamePattern: '{name}Steps.cs'
  defaultNamespace: StepDefinitions
  codeExtraction:
    extractCodeBlocks: true
    language: csharp
    stripMarkdown: true
---

# System Prompt

You are an expert in creating Reqnroll step definition classes following the StepsBase pattern. Generate clean, maintainable C# code that properly binds Gherkin steps to Page Object automation code with inline instantiation and fluent chaining.

# Prompt Template

Generate a step definition class for the following scenario:

**Feature:** {{ featureName }}
{% if scenario -%}
**Scenario:** {{ scenario }}
{% endif -%}
**Namespace:** {{ namespace }}
**Root Namespace:** {{ rootNamespace }}

**Gherkin Steps:**
```gherkin
{{ gherkinSteps }}
```

{% if pages -%}
**Available Page Objects:**
{% for page in pages -%}
- **{{ page.name }}Page**
{% if page.methodsFound -%}
  Available methods:
{% for method in page.methods -%}
  * {{ method.returnType }} {{ method.name }}({% for param in method.parameters -%}{{ param.type }} {{ param.name }}{% if not forloop.last %}, {% endif %}{% endfor -%})
{% endfor -%}
{% else -%}
  (Page file not found - infer method names from steps)
{% endif -%}
{% endfor -%}
{% endif -%}

{% if tableDataClasses and tableDataClasses.size > 0 -%}
**Table Data Classes Needed:**
{% for dataClass in tableDataClasses -%}
- {{ dataClass.className }}
{% endfor -%}
{% endif -%}

**Requirements:**

1. **Class Structure:**
   - Inherit from `StepsBase`
   - Constructor: `public {{ featureName }}Steps(FeatureContext featureContext, ScenarioContext scenarioContext) : base(featureContext, scenarioContext) {}`
   - Add `[Binding]` attribute to class
   - Namespace: `{{ namespace }}`

2. **Using Statements:**
   - `using Reqnroll;`
   - `using {{ rootNamespace }}.Infrastructure;`
   - `using {{ rootNamespace }}.Pages;`
   - `using FluentAssertions;`

3. **Step Attributes:**
   - Use specific attributes: `[Given(@"...")]`, `[When(@"...")]`, `[Then(@"...")]`
   - Use flexible regex patterns (no ^ or $ anchors)
   - Extract parameters from Gherkin: `{username}` → method parameter `string username`

4. **Page Object Instantiation:**
   - Create page objects inline in each step method
   - Pattern: `var pageName = new PageName(_driverContext).Init();`
   - Example: `var loginPage = new LoginPage(_driverContext).Init();`
   - For steps referencing multiple pages, create multiple instances

5. **Page Detection from Step Text:**
   - `"login"` → `LoginPage`
   - `"dashboard"` → `DashboardPage`
   - `"home"` → `HomePage`
   - `"profile"` → `ProfilePage`
   - `"settings"` → `SettingsPage`
   - General: `"{word}"` → `{PascalCase}Page`

6. **Method Calls:**
{% if pages and pages.size > 0 -%}
   - Use exact method signatures from available page methods above
{% else -%}
   - Infer method names from Gherkin steps:
{% endif -%}
   - `"I enter {param} into X"` → `EnterX(string param)`
   - `"I click X"` → `ClickX()`
   - `"I select {param} from X"` → `SelectX(string param)`
   - `"I should see X"` → `page.IsXDisplayed().Should().BeTrue()`
   - `"X should contain {value}"` → `page.GetX().Should().Contain(value)`
   - `"X should be {value}"` → `page.GetX().Should().Be(value)`

7. **Fluent Chaining:**
   - Use chaining when methods return page object instance:
     ```csharp
     loginPage.EnterUsername(username)
              .EnterPassword(password)
              .ClickLoginButton();
     ```
   - Break chain when:
     * Void method encountered
     * FluentAssertions used (must be separate statement)
     * Different page object needed

8. **Parameter Extraction:**
   - Gherkin: `"I enter {username} and {password}"` → Method: `EnterCredentials(string username, string password)`
   - Extract all `{param}` placeholders as method parameters

{% if tableDataClasses and tableDataClasses.size > 0 -%}
9. **Table Handling:**
   - Generate internal data classes BEFORE the step class
   - Convert column names to PascalCase properties
   - Infer property types:
     * `int` for numeric values (all digits)
     * `decimal` for decimal numbers (contains .)
     * `bool` for "true"/"false"/"yes"/"no"
     * `string` as default
   - Use pattern: `var data = table.CreateInstance<DataClassName>();`
   - Example:
     ```csharp
     internal class NetworkConditionsData
     {
         public int Latency { get; set; }
         public int DownloadThroughput { get; set; }
         public int UploadThroughput { get; set; }
     }
     ```
{% endif -%}

10. **Code Quality:**
    - Include XML documentation for each step method
    - Use meaningful variable names
    - Handle assertions in `[Then]` steps using FluentAssertions
    - Return void for steps with void methods or assertions

# Examples

## Example 1: Simple Login with Fluent Chaining

### Input

```gherkin
Feature: Login
Given I am on the login page
When I enter "testuser" and "password123"
Then I should see the dashboard
```

### Output

```csharp
using Reqnroll;
using MyProject.Infrastructure;
using MyProject.Pages;
using FluentAssertions;

namespace MyProject.StepDefinitions;

[Binding]
public class LoginSteps : StepsBase
{
    public LoginSteps(FeatureContext featureContext, ScenarioContext scenarioContext) 
        : base(featureContext, scenarioContext)
    {
    }

    /// <summary>
    /// Navigate to login page
    /// </summary>
    [Given(@"I am on the login page")]
    public void GivenIAmOnTheLoginPage()
    {
        var loginPage = new LoginPage(_driverContext).Init();
        loginPage.NavigateTo();
    }

    /// <summary>
    /// Enter credentials and login
    /// </summary>
    [When(@"I enter (.*) and (.*)")]
    public void WhenIEnterCredentials(string username, string password)
    {
        var loginPage = new LoginPage(_driverContext).Init();
        loginPage.EnterUsername(username)
                 .EnterPassword(password)
                 .ClickLoginButton();
    }

    /// <summary>
    /// Verify dashboard is displayed
    /// </summary>
    [Then(@"I should see the dashboard")]
    public void ThenIShouldSeeTheDashboard()
    {
        var dashboardPage = new DashboardPage(_driverContext).Init();
        dashboardPage.IsDisplayed().Should().BeTrue("Dashboard should be visible after login");
    }
}
```

## Example 2: With Table Data

### Input

```gherkin
Feature: Network Emulation
Given I enable network emulation with the following parameters:
  | Latency | DownloadThroughput | UploadThroughput |
  | 100     | 50000              | 25000            |
When I disable network emulation
```

### Output

```csharp
using Reqnroll;
using MyProject.Infrastructure;
using MyProject.Helpers;
using FluentAssertions;

namespace MyProject.StepDefinitions;

internal class NetworkConditionsData
{
    public int Latency { get; set; }
    public int DownloadThroughput { get; set; }
    public int UploadThroughput { get; set; }
}

[Binding]
public class NetworkEmulationSteps : StepsBase
{
    public NetworkEmulationSteps(FeatureContext featureContext, ScenarioContext scenarioContext) 
        : base(featureContext, scenarioContext)
    {
    }

    /// <summary>
    /// Enable network emulation with parameters
    /// </summary>
    [Given(@"I enable network emulation with the following parameters:")]
    public void GivenIEnableNetworkEmulation(Table table)
    {
        var networkConditions = table.CreateInstance<NetworkConditionsData>();
        var devToolsHelper = new DevToolsHelper(_driverContext);
        devToolsHelper.SetNetworkConditions(networkConditions);
    }

    /// <summary>
    /// Disable network emulation
    /// </summary>
    [When(@"I disable network emulation")]
    public void WhenIDisableNetworkEmulation()
    {
        var devToolsHelper = new DevToolsHelper(_driverContext);
        devToolsHelper.DisableNetworkEmulation();
    }
}
```

## Example 3: Multiple Pages in One Step

### Input

```gherkin
Feature: Complete User Flow
When I login and navigate to dashboard
```

### Output

```csharp
using Reqnroll;
using MyProject.Infrastructure;
using MyProject.Pages;
using FluentAssertions;

namespace MyProject.StepDefinitions;

[Binding]
public class CompleteUserFlowSteps : StepsBase
{
    public CompleteUserFlowSteps(FeatureContext featureContext, ScenarioContext scenarioContext) 
        : base(featureContext, scenarioContext)
    {
    }

    /// <summary>
    /// Login and navigate to dashboard
    /// </summary>
    [When(@"I login and navigate to dashboard")]
    public void WhenILoginAndNavigateToDashboard()
    {
        var loginPage = new LoginPage(_driverContext).Init();
        loginPage.EnterUsername("testuser")
                 .EnterPassword("password")
                 .ClickLoginButton();
        
        var dashboardPage = new DashboardPage(_driverContext).Init();
        dashboardPage.WaitUntilLoaded();
    }
}
```

### Explanation

- Step definitions inherit from StepsBase
- Constructor passes contexts to base
- Page objects created inline with `new PageName(_driverContext).Init()`
- Fluent chaining used when applicable
- Separate statements for void methods and assertions
- Table data converted to internal PascalCase data classes
