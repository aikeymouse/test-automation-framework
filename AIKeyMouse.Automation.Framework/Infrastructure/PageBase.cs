using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class PageBase(DriverContext driverContext)
{
    // Mobile (Appium)
    protected AppiumDriver? Driver { get; set; } = driverContext._driver;
    
    // Web & WebView2 (Selenium)
    protected IWebDriver? WebDriver { get; set; } = driverContext.WebDriver;
    
    // Windows Desktop (UI Automation)
    protected UIAutomationApplication? Application { get; set; } = driverContext.Application;
    protected UIAutomationWindow? Window { get; set; } = driverContext.Window;
    
    protected DriverContext DriverContext { get; set; } = driverContext;
}