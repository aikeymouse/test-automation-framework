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
    - OpenQA.Selenium.Support.UI
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

**CRITICAL RULES:**
1. ALL static ElementLocator fields MUST be declared at the top of the class as private readonly fields
2. The page container locator (_pageContainerLocator) MUST be declared at the top - NEVER create it inline in Init()
3. Dynamic locators (where selector value comes from method parameters) can be created inline in the method that uses them
4. If elements are provided in the input, generate ONLY those exact elements - no additions, no omissions, no modifications
5. Use element types to determine appropriate action methods (text inputs get Enter methods, checkboxes get Check/Uncheck, buttons/links get Click)
6. Never hallucinate elements that weren't provided in the input

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
- **CRITICAL: All static ElementLocator fields MUST be declared at the top of the class as private readonly fields**
- **Exception: Dynamic locators where selector values come from method parameters can be created inline in the method**
- Use ElementLocator pattern with private readonly fields for all static locators
- Use CSS selectors where applicable
- Include XML documentation
- Inherit from PageBase with DriverContext parameter
- Create Init() method for page-level initialization (container element, wait for page load)
- Use IWebElement properties with private set for containers
- Use expression-bodied properties for other elements (direct GetElement() calls)
- For elements inside the page container, use PageContainer.GetElement() instead of WebDriver.GetElement()
- **CRITICAL: If elements are provided, generate ONLY those exact elements - do not add, remove, or modify the list**
- Create type-specific action methods based on element type:
  * text-input, password-input: EnterX(string value) - Clear() then SendKeys()
  * checkbox: CheckX() and UncheckX() - check current state before clicking  
  * button, submit-button, link: ClickX() - Click()
  * select: SelectX(string value) - use SelectElement wrapper
  * For any other type: ClickX() - Click()
- All input/checkbox methods return `this` for fluent chaining; button/link methods return void
- Follow naming convention: {{ pageName }}Page

# Examples

## Example 1: Login Page with Multiple Element Types

### Input

```
Page Name: Login
Elements:
- Username (text-input): #username
- Password (password-input): #password
- RememberMe (checkbox): #rememberMe
- LoginButton (button): #loginButton
- ForgotPasswordLink (link): #forgotPasswordLink
```

### Output

```csharp
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
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
    private readonly ElementLocator _usernameLocator = new(Locator.CssSelector, "#username");
    private readonly ElementLocator _passwordLocator = new(Locator.CssSelector, "#password");
    private readonly ElementLocator _rememberMeLocator = new(Locator.CssSelector, "#rememberMe");
    private readonly ElementLocator _loginButtonLocator = new(Locator.CssSelector, "#loginButton");
    private readonly ElementLocator _forgotPasswordLinkLocator = new(Locator.CssSelector, "#forgotPasswordLink");

    // Properties
    public IWebElement PageContainer { get; private set; }
    public IWebElement Username => PageContainer.GetElement(_usernameLocator);
    public IWebElement Password => PageContainer.GetElement(_passwordLocator);
    public IWebElement RememberMe => PageContainer.GetElement(_rememberMeLocator);
    public IWebElement LoginButton => PageContainer.GetElement(_loginButtonLocator);
    public IWebElement ForgotPasswordLink => PageContainer.GetElement(_forgotPasswordLinkLocator);

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
        Username.Clear();
        Username.SendKeys(username);
        return this;
    }

    /// <summary>
    /// Enter password
    /// </summary>
    public LoginPage EnterPassword(string password)
    {
        Password.Clear();
        Password.SendKeys(password);
        return this;
    }

    /// <summary>
    /// Check remember me checkbox
    /// </summary>
    public LoginPage CheckRememberMe()
    {
        if (!RememberMe.Selected)
        {
            RememberMe.Click();
        }
        return this;
    }

    /// <summary>
    /// Uncheck remember me checkbox
    /// </summary>
    public LoginPage UncheckRememberMe()
    {
        if (RememberMe.Selected)
        {
            RememberMe.Click();
        }
        return this;
    }

    /// <summary>
    /// Click login button
    /// </summary>
    public void ClickLoginButton()
    {
        LoginButton.Click();
    }

    /// <summary>
    /// Click forgot password link
    /// </summary>
    public void ClickForgotPasswordLink()
    {
        ForgotPasswordLink.Click();
    }
}
```

### Explanation

Modern Page Object using ElementLocator pattern. ALL locators including _pageContainerLocator are declared at the top of the class as private readonly fields. The Init() method uses the pre-declared _pageContainerLocator field - it does NOT create a new ElementLocator inline. Individual elements use expression-bodied properties that call GetElement() directly each time they're accessed. This ensures fresh element references and avoids stale element exceptions. Elements are searched within the PageContainer scope using PageContainer.GetElement() to avoid finding elements outside the page context. GetElement() includes WebDriverWait and handles stale element exceptions automatically.

Action methods are type-specific:
- Text/password inputs: EnterX() methods that Clear() then SendKeys()
- Checkboxes: CheckX() and UncheckX() methods that check current state before clicking
- Buttons and links: ClickX() methods that simply Click()

All action methods for inputs and checkboxes return `this` for fluent chaining, while final actions (button/link clicks) return void.

IMPORTANT: Notice that _pageContainerLocator is declared at the top with all other locators, NOT created inline in the Init() method.

**WRONG - DO NOT DO THIS:**
```csharp
// ❌ WRONG - Creating locator inline in Init()
public void Init()
{
    PageContainer ??= WebDriver.GetElement(new ElementLocator(Locator.CssSelector, ".login-container"));
}
```

**CORRECT:**
```csharp
// ✅ CORRECT - Locator declared at top of class
private readonly ElementLocator _pageContainerLocator = new(Locator.CssSelector, ".login-container");

public void Init()
{
    PageContainer ??= WebDriver.GetElement(_pageContainerLocator);
}
```
