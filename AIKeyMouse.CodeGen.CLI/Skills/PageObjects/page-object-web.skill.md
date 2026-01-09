---
id: page-object-web
name: Web Page Object Generator
description: Generate Page Object Model classes for web automation using Selenium WebDriver
category: page-object
platform: web
llmParams:
  temperature: 0.3
  maxTokens: 2000
validation:
  requiredUsings:
    - OpenQA.Selenium
    - AIKeyMouse.Automation.Framework.Infrastructure
    - AIKeyMouse.Automation.Framework.DataObjects
    - AIKeyMouse.Automation.Framework.Extensions
  requiredBaseClass: PageBase
  validateSyntax: true
  autoFormat: true
output:
  defaultPath: Pages
  fileNamePattern: '{name}Page.cs'
  defaultNamespace: Pages
  codeExtraction:
    extractCodeBlocks: true
    language: csharp
    stripMarkdown: true
---

# System Prompt

You are an expert in creating Page Object Model (POM) classes for Selenium WebDriver automation. Generate clean, maintainable C# code following modern POM best practices with ElementLocator pattern and proper encapsulation.

# Prompt Template

Generate a Page Object class for a {{ platform }} page with the following details:

**Page Name:** {{ pageName }}
**Namespace:** {{ namespace | default: 'Pages' }}
{% if url -%}
**URL:** {{ url }}
{% endif -%}
{% if elements -%}
**Elements:**
{% for element in elements -%}
- {{ element.name }} ({{ element.type }}): {{ element.locator }}
{% endfor -%}
{% endif -%}
{% if actions -%}

**Actions:**
{% for action in actions -%}
- {{ action }}
{% endfor -%}
{% endif -%}

**Requirements:**
- Use ElementLocator pattern with private readonly fields for locators
- Use CSS selectors where applicable
- Include XML documentation
- Inherit from PageBase with DriverContext parameter
- Create Init() method for page-level initialization (container element, wait for page load)
- Use IWebElement properties with private set for containers
- Use expression-bodied properties for other elements (direct GetElement() calls)
- For elements inside the page container, use PageContainer.GetElement() instead of WebDriver.GetElement()
- Create action methods for user interactions
- Follow naming convention: {{ pageName }}Page

# Examples

## Example 1: Login Page with Container

### Input

```
Page Name: Login
Elements:
- Username textbox: #username
- Password textbox: #password
- Submit button: button[type='submit']
```

### Output

```csharp
using OpenQA.Selenium;
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Extensions;

namespace Pages;

/// <summary>
/// Page Object for Login page
/// </summary>
public class LoginPage : PageBase
{
    // Element Locators
    private readonly ElementLocator _pageContainerLocator = new(Locator.CssSelector, ".login-container");
    private readonly ElementLocator _usernameInputLocator = new(Locator.CssSelector, "#username");
    private readonly ElementLocator _passwordInputLocator = new(Locator.CssSelector, "#password");
    private readonly ElementLocator _submitButtonLocator = new(Locator.CssSelector, "button[type='submit']");

    // Properties
    public IWebElement PageContainer { get; private set; }
    public IWebElement UsernameInput => PageContainer.GetElement(_usernameInputLocator);
    public IWebElement PasswordInput => PageContainer.GetElement(_passwordInputLocator);
    public IWebElement SubmitButton => PageContainer.GetElement(_submitButtonLocator);

    public LoginPage(DriverContext driverContext) : base(driverContext)
    {
    }

    /// <summary>
    /// Initialize page and wait for container to load
    /// </summary>
    public void Init()
    {
        PageContainer ??= WebDriver.GetElement(_pageContainerLocator);
    }

    /// <summary>
    /// Enter username
    /// </summary>
    public LoginPage EnterUsername(string username)
    {
        UsernameInput.Clear();
        UsernameInput.SendKeys(username);
        return this;
    }

    /// <summary>
    /// Enter password
    /// </summary>
    public LoginPage EnterPassword(string password)
    {
        PasswordInput.Clear();
        PasswordInput.SendKeys(password);
        return this;
    }

    /// <summary>
    /// Click submit button
    /// </summary>
    public void Submit()
    {
        SubmitButton.Click();
    }
}
```

### Explanation

Modern Page Object using ElementLocator pattern. The Init() method initializes the page container and waits for the page to load, which should be called once when navigating to the page. Individual elements use expression-bodied properties that call GetElement() directly each time they're accessed. This ensures fresh element references and avoids stale element exceptions. Elements are searched within the PageContainer scope using PageContainer.GetElement() to avoid finding elements outside the page context. GetElement() includes WebDriverWait and handles stale element exceptions automatically.
