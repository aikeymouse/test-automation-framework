using FlaUI.Core.AutomationElements;
using AIKeyMouse.Automation.Framework.DataObjects;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static class WindowExtension
{
    // Keep these for backward compatibility with AutomationId strings
    public static Button? GetButton(this Window? window, string automationId)
    {
        return window?.GetButton(new ElementLocator(Locator.Id, automationId));
    }

    public static TextBox? GetTextBox(this Window? window, string automationId)
    {
        return window?.GetTextBox(new ElementLocator(Locator.Id, automationId));
    }

    public static AutomationElement? GetElement(this Window? window, string automationId)
    {
        return window?.GetElement(new ElementLocator(Locator.Id, automationId));
    }

    public static AutomationElement? GetElementByName(this Window? window, string name)
    {
        return window?.GetElement(new ElementLocator(Locator.Name, name));
    }

    public static AutomationElement? GetElementByClassName(this Window? window, string className)
    {
        return window?.GetElement(new ElementLocator(Locator.ClassName, className));
    }

    public static void ClickButton(this Window? window, string automationId)
    {
        window?.GetButton(automationId)?.Click();
    }

    public static void SetText(this Window? window, string automationId, string text)
    {
        var textBox = window?.GetTextBox(automationId);
        if (textBox != null)
        {
            textBox.Text = text;
        }
    }

    public static string? GetText(this Window? window, string automationId)
    {
        return window?.GetTextBox(automationId)?.Text;
    }
}
