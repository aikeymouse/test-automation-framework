namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    public string CurrentDirectory { get; init; }
    public TestContext TestContext { get; init; }
    public bool IsScenarioFailed { get; set; }
}