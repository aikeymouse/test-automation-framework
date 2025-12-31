namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    public required string CurrentDirectory { get; init; }
    public required TestContext TestContext { get; init; }
    public bool IsScenarioFailed { get; set; }
}