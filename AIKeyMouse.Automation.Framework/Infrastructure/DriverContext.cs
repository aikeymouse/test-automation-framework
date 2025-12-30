using OpenQA.Selenium.Appium;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    public AppiumDriver? _driver;
    
    public AppiumDriver? Driver => _driver;

    public void Dispose()
    {
        // Dispose Appium driver
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
            _driver = null;
        }

        // Dispose Web driver (handled in DriverContextWeb.cs)
        
        // Dispose Windows application (handled in DriverContextWindows.cs)
        CloseWindowsApplication();
    }
}