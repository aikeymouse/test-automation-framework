using AIKeyMouse.Automation.Framework.Infrastructure;
using OpenQA.Selenium;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static class DriverContextExtension
{
    public static void SaveScreenshot(this DriverContext driverContext)
    {
        var title = driverContext.TestContext.TestName;
        var filePath = driverContext.GetResultsFilePath($"{title}_screenshot.png");

        try
        {
            driverContext.TakeScreenshot().SaveAsFile(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while saving the screenshot.", ex);
        }
    }

    public static string GetResultsFilePath(this DriverContext driverContext, string fileName)
    {
        var resultDirectory = driverContext.TestContext.TestRunDirectory;
        if (string.IsNullOrEmpty(resultDirectory))
        {
            throw new InvalidOperationException("Test run directory is not available.");
        }
        var filePath = Path.Combine(resultDirectory, fileName);
        return filePath;
    }

    public static Screenshot TakeScreenshot(this DriverContext driverContext)
    {
        Screenshot screenshot;
        ITakesScreenshot screenshotDriver;

        try
        {
            if (driverContext.WebDriver != null)
            {
                screenshotDriver = driverContext.WebDriver as ITakesScreenshot
                    ?? throw new InvalidOperationException("WebDriver does not support taking screenshots.");
            }
            else
            {
                screenshotDriver = driverContext.Driver as ITakesScreenshot
                    ?? throw new InvalidOperationException("Driver does not support taking screenshots.");
            }
            screenshot = screenshotDriver.GetScreenshot();
            return screenshot;
        } catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while taking a screenshot.", ex);
        }
    }
}
