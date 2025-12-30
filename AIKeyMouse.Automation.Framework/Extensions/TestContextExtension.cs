using AIKeyMouse.Automation.Framework.Infrastructure;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static class TestContextExtension
{
    public static void AddAttachements(this TestContext testContext, string[] filePaths)
    {
        foreach (var filePath in filePaths)
        {
            testContext.AddResultFile(filePath);
        }
    }
}