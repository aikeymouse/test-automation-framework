# AIKeyMouse Automation Framework

A unified test automation framework supporting **Web**, **Mobile**, and **Windows Desktop** applications using .NET 8.0.

## ⚠️ Desktop Automation Branch

This is the **desktop-automation** branch featuring native Windows UI Automation using **Microsoft CsWin32** instead of FlaUI.

### What's Different in This Branch?

- ✅ **Native Windows UI Automation** - Direct Windows API access via CsWin32
- ✅ **No .NET Framework dependencies** - Pure .NET 8.0 implementation
- ✅ **No compatibility warnings** - Native support without NU1701 warnings
- ✅ **Same API surface** - Drop-in replacement for FlaUI with identical usage patterns
- ✅ **Microsoft-maintained** - Uses official Windows APIs

## Features

- 🌐 **Web Automation** - Selenium WebDriver with EdgeDriver support for WebView2
- 📱 **Mobile Automation** - Appium for iOS and Android testing
- 🖥️ **Windows Desktop Automation** - Native UI Automation via CsWin32
- 🔄 **Unified API** - Common `ElementLocator` pattern across all platforms
- 🧪 **BDD Support** - Reqnroll (SpecFlow successor) integration
- 🎯 **Page Object Model** - Built-in base classes for clean test architecture

## Project Structure

```
AIKeyMouse.Automation.Framework/
├── DataObjects/          # ElementLocator, Locator enum, and data models
├── Extensions/           # Extension methods for all platforms
│   ├── LocatorExtension.cs          # Selenium/Appium locator conversion
│   ├── LocatorExtension.Win.cs      # Windows UI Automation locator conversion
│   ├── SearchContextExtension.cs    # Web/Mobile element finding with waits
│   ├── SearchContextExtension.Win.cs # Windows element finding with waits
│   └── WindowExtension.cs           # Window helpers
├── Infrastructure/       # Core framework classes
│   ├── DriverContext.cs             # Base driver management
│   ├── DriverContextWeb.cs          # Web/WebView2 driver management
│   ├── DriverContextWindows.cs      # Windows app driver management
│   ├── UIAutomationApplication.cs   # Windows app lifecycle management
│   ├── UIAutomationWindow.cs        # Window element wrapper
│   ├── UIAutomationElement.cs       # Element wrapper with UI Automation patterns
│   ├── PageBase.cs                  # Base class for page objects
│   └── ConfiguredSettings.cs        # Framework configuration
├── Helpers/             # Helper utilities
└── StepDefinitions/     # Reqnroll step definitions
```

## Supported Technologies

### NuGet Packages
- **Reqnroll 3.3.0** - BDD framework
- **Selenium.WebDriver 4.39.0** - Web automation
- **Appium.WebDriver 8.0.1** - Mobile automation
- **Microsoft.Windows.CsWin32 0.3.106** - Windows API code generation
- **Selenium.WebDriver.MSEdgeDriver 141.0.3537.71** - EdgeDriver for WebView2
- **MSTest.TestFramework 4.0.2** - Testing framework
- **WebDriverManager 2.17.6** - Browser driver management

### Target Framework
- **.NET 8.0-windows10.0.22621.0** (Windows 11 SDK)


## Quick Start

### Web/Mobile Automation Example

```csharp
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Extensions;

public class LoginPage : PageBase
{
    private ElementLocator UsernameField => new(Locator.Id, "username");
    private ElementLocator PasswordField => new(Locator.Id, "password");
    private ElementLocator LoginButton => new(Locator.XPath, "//button[@type='submit']");

    public void Login(string username, string password)
    {
        WebDriver.GetElement(UsernameField).SendKeys(username);
        WebDriver.GetElement(PasswordField).SendKeys(password);
        WebDriver.GetElement(LoginButton).Click();
    }
}
```

### Windows Desktop Automation Example

```csharp
using AIKeyMouse.Automation.Framework.Infrastructure;
using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Extensions;

public class CalculatorPage : PageBase
{
    private ElementLocator Button7 => new(Locator.Id, "num7Button");
    private ElementLocator ButtonPlus => new(Locator.Id, "plusButton");
    
    public void Add7Plus5()
    {
        Window.GetButton(Button7)?.Click();
        Window.GetButton(ButtonPlus)?.Click();
        // ...
    }
}
```

## Key Concepts

### ElementLocator Pattern

Unified element locator across all platforms:

```csharp
// Works for Web, Mobile, and Windows
var element = new ElementLocator(Locator.Id, "myElement");
var element2 = new ElementLocator(Locator.XPath, "//div[@class='test']");
var element3 = new ElementLocator(Locator.Name, "Submit");
```

### Supported Locator Types

| Locator Type | Web/Mobile | Windows (UI Automation) |
|--------------|------------|---------------------|
| `Id` | ✅ | ✅ (AutomationId) |
| `Name` | ✅ | ✅ |
| `ClassName` | ✅ | ✅ |
| `XPath` | ✅ | ✅ |
| `TagName` | ✅ | ✅ (ControlType) |
| `CssSelector` | ✅ | ❌ |
| `LinkText` | ✅ | ❌ |
| `PartialLinkText` | ✅ | ❌ |
| `AccessibilityId` | ✅ (Mobile) | ❌ |

### Automatic Waits

All element finding operations include built-in wait logic:

```csharp
// Web/Mobile - uses WebDriverWait with configured timeout
var element = searchContext.GetElement(locator); // Default timeout
var element = searchContext.GetElement(locator, TimeSpan.FromSeconds(20)); // Custom timeout

// Windows - uses custom retry logic with configured timeout
var element = window.GetElement(locator); // Default timeout
var button = window.GetButton(locator); // Strongly-typed, with wait
```

## DriverContext

Multi-platform driver management:

```csharp
public class MyTests : StepsBase
{
    public void TestWebApp()
    {
        // Access WebDriver for web/mobile
        Driver.WebDriver.Navigate().GoToUrl("https://example.com");
    }

    public void TestWindowsApp()
    {
        // Access Windows Application
        Driver.StartWindowsApplication(@"C:\Path\To\App.exe");
        var window = Driver.Window;
        window.GetButton(new ElementLocator(Locator.Id, "btn1"))?.Click();
    }
}
```

## Configuration

Configure timeouts and settings in `ConfiguredSettings`:

```csharp
public class ConfiguredSettings
{
    public int ShortTimeout { get; set; } = 10; // seconds
    public int MediumTimeout { get; set; } = 30;
    public int LongTimeout { get; set; } = 60;
}
```

## Installation

1. Clone the repository
2. Open in Visual Studio or VS Code
3. Restore NuGet packages: `dotnet restore`
4. Build: `dotnet build`

## Requirements

- .NET 8.0 SDK or later
- Windows OS (for Windows desktop automation)
- WebDrivers (managed automatically by WebDriverManager)
- Appium Server (for mobile testing)

## Known Issues

None currently known for the desktop-automation branch. The native UI Automation implementation eliminates the NU1701 warnings present in the FlaUI-based main branch.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

AIKeyMouse Team
