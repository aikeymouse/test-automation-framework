namespace AIKeyMouse.Automation.Framework.DataObjects;

public class TestDeviceSettings
{
    public string? Type { get; set; }
    public string Orientation { get; set; } = "Portrait";
}

public class AndroidSettings
{
    public string? DeviceName { get; set; }
    public string? OSVersion { get; set; }
    public string? AutomationName { get; set; }
    public string? PackagePath { get; set; }
    public string? AppActivity { get; set; }
}

public class IOSSettings
{
    public string? DeviceName { get; set; }
    public string? OSVersion { get; set; }
    public string? AutomationName { get; set; }
    public string? PackagePath { get; set; }
}

public class BrowserStackSettings
{
    public string? User { get; set; }
    public string? AccessKey { get; set; }
    public bool DeviceLog { get; set; } = false;
    public bool AppiumLog { get; set; } = false;
    public bool NetworkLog { get; set; } = false;
    public bool VisualLog { get; set; } = false;
    public string SessionNamePrefix { get; set; } = "Reqnroll_Automation";
}

public class AppiumSettings
{
    public string? ServerUrl { get; set; }
    public double AppWaitTimeout { get; set; } = 30000;
    public double ImplicitWaitTimeout { get; set; } = 5000;
}