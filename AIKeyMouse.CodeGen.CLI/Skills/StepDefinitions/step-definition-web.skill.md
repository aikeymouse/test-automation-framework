---
id: step-definition-web
name: Web Step Definition Generator
description: Generate Reqnroll step definition classes for web automation scenarios
category: step-definition
platform: web
llmParams:
  temperature: 0.3
  maxTokens: 2500
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

You are an expert in creating Reqnroll/SpecFlow step definition classes. Generate clean, maintainable C# code that properly binds Gherkin steps to automation code using Page Objects.

# Prompt Template

Generate a step definition class for the following scenario:

**Feature:** {{ featureName }}
{% if scenario -%}
**Scenario:** {{ scenario }}
{% endif -%}
**Namespace:** {{ namespace | default: 'StepDefinitions' }}

**Gherkin Steps:**
{{ gherkinSteps }}

{% if pageObjects -%}
**Available Page Objects:**
{% for page in pageObjects -%}
- {{ page }}
{% endfor -%}
{% endif -%}

**Requirements:**
- Use [Given], [When], [Then], [And] attributes
- Include XML documentation for each step
- Use dependency injection for Page Objects
- Follow naming convention: {{ featureName }}Steps
- Use ScenarioContext for sharing data between steps
- Handle assertions properly in Then steps

# Examples

## Example 1

### Input

```
Feature: Login
Steps:
Given I am on the login page
When I enter username "testuser"
And I enter password "password123"
And I click the login button
Then I should see the dashboard
```

### Output

```csharp
using Reqnroll;
using FluentAssertions;
using Pages;

namespace StepDefinitions;

[Binding]
public class LoginSteps
{
    private readonly LoginPage _loginPage;
    private readonly DashboardPage _dashboardPage;
    private readonly ScenarioContext _scenarioContext;

    public LoginSteps(LoginPage loginPage, DashboardPage dashboardPage, ScenarioContext scenarioContext)
    {
        _loginPage = loginPage;
        _dashboardPage = dashboardPage;
        _scenarioContext = scenarioContext;
    }

    /// <summary>
    /// Navigate to login page
    /// </summary>
    [Given(@"I am on the login page")]
    public void GivenIAmOnTheLoginPage()
    {
        _loginPage.NavigateTo();
    }

    /// <summary>
    /// Enter username
    /// </summary>
    [When(@"I enter username ""(.*)""")]
    public void WhenIEnterUsername(string username)
    {
        _loginPage.EnterUsername(username);
    }

    /// <summary>
    /// Enter password
    /// </summary>
    [When(@"I enter password ""(.*)""")]
    public void WhenIEnterPassword(string password)
    {
        _loginPage.EnterPassword(password);
    }

    /// <summary>
    /// Click login button
    /// </summary>
    [When(@"I click the login button")]
    public void WhenIClickTheLoginButton()
    {
        _loginPage.Submit();
    }

    /// <summary>
    /// Verify dashboard is displayed
    /// </summary>
    [Then(@"I should see the dashboard")]
    public void ThenIShouldSeeTheDashboard()
    {
        _dashboardPage.IsDisplayed().Should().BeTrue("Dashboard should be visible after login");
    }
}
```

### Explanation

Step definitions with DI, proper attributes, and FluentAssertions
