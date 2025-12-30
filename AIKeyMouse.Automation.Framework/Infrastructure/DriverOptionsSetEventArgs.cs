using OpenQA.Selenium;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public class DriverOptionsSetEventArgs(DriverOptions driverOptions) : EventArgs
{
    public DriverOptions DriverOptions { get; } = driverOptions;
}