using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Infrastructure;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static class WindowExtension
{
    // Keep these for backward compatibility with AutomationId strings
    public static UIAutomationButton? GetButton(this UIAutomationWindow? window, string automationId)
    {
        return window?.GetButton(new ElementLocator(Locator.Id, automationId));
    }

    public static UIAutomationTextBox? GetTextBox(this UIAutomationWindow? window, string automationId)
    {
        return window?.GetTextBox(new ElementLocator(Locator.Id, automationId));
    }

    public static UIAutomationElement? GetElement(this UIAutomationWindow? window, string automationId)
    {
        return window?.GetElement(new ElementLocator(Locator.Id, automationId));
    }

    public static UIAutomationElement? GetElementByName(this UIAutomationWindow? window, string name)
    {
        return window?.GetElement(new ElementLocator(Locator.Name, name));
    }

    public static UIAutomationElement? GetElementByClassName(this UIAutomationWindow? window, string className)
    {
        return window?.GetElement(new ElementLocator(Locator.ClassName, className));
    }

    public static void ClickButton(this UIAutomationWindow? window, string automationId)
    {
        window?.GetButton(automationId)?.Click();
    }

    public static void SetText(this UIAutomationWindow? window, string automationId, string text)
    {
        var textBox = window?.GetTextBox(automationId);
        if (textBox != null)
        {
            textBox.Text = text;
        }
    }

    public static string? GetText(this UIAutomationWindow? window, string automationId)
    {
        return window?.GetTextBox(automationId)?.Text;
    }
}
