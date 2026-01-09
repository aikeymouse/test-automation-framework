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
    - SeleniumExtras.PageObjects
    - AIKeyMouse.Automation.Framework.Infrastructure
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

You are an expert in creating Page Object Model (POM) classes for Selenium WebDriver automation. Generate clean, maintainable C# code following POM best practices with proper encapsulation and reusable methods.

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
- Use CSS selectors where applicable
- Include XML documentation
- Inherit from PageBase
- Use IWebElement properties with [FindsBy] attributes
- Create action methods for user interactions
- Follow naming convention: {{ pageName }}Page

# Examples

## Example 1

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
using SeleniumExtras.PageObjects;
using AIKeyMouse.Automation.Framework.Infrastructure;

namespace Pages;

/// <summary>
/// Page Object for Login page
/// </summary>
public class LoginPage : PageBase
{
    [FindsBy(How = How.CssSelector, Using = "#username")]
    private IWebElement UsernameInput { get; set; }

    [FindsBy(How = How.CssSelector, Using = "#password")]
    private IWebElement PasswordInput { get; set; }

    [FindsBy(How = How.CssSelector, Using = "button[type='submit']")]
    private IWebElement SubmitButton { get; set; }

    public LoginPage(IWebDriver driver) : base(driver)
    {
        PageFactory.InitElements(driver, this);
    }

    /// <summary>
    /// Enter username
    /// </summary>
    public LoginPage EnterUsername(string username)
    {
        UsernameInput.SendKeys(username);
        return this;
    }

    /// <summary>
    /// Enter password
    /// </summary>
    public LoginPage EnterPassword(string password)
    {
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

    /// <summary>
    /// Complete login action
    /// </summary>
    public void Login(string username, string password)
    {
        EnterUsername(username);
        EnterPassword(password);
        Submit();
    }
}
```

### Explanation

Page Object with fluent interface, proper encapsulation, and reusable methods
