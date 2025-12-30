using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using FlaUI.Core.AutomationElements;
using FlaUI.Core;
using FlaUI.UIA3;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class PageBase(DriverContext driverContext)
{
    // Mobile (Appium)
    protected AppiumDriver? Driver { get; set; } = driverContext._driver;
    
    // Web & WebView2 (Selenium)
    protected IWebDriver? WebDriver { get; set; } = driverContext.WebDriver;
    
    // Windows Desktop (FlaUI)
    protected Application? Application { get; set; } = driverContext.Application;
    protected UIA3Automation? Automation { get; set; } = driverContext.Automation;
    protected Window? Window { get; set; } = driverContext.Window;
    
    protected DriverContext DriverContext { get; set; } = driverContext;
}