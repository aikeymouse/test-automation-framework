using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Infrastructure;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static partial class SearchContextExtension
{
    // UI Automation - GetElement with ElementLocator
    public static UIAutomationElement? GetElement(this UIAutomationWindow? window, ElementLocator locator)
    {
        return GetElement(window, locator, TimeSpan.FromSeconds(ConfiguredSettings.Instance.ShortTimeout));
    }

    public static UIAutomationElement? GetElement(this UIAutomationWindow? window, ElementLocator locator, TimeSpan timeout)
    {
        if (window == null) throw new InvalidOperationException("Window is null.");

        var findAction = locator.ToFindFirstAction();
        var customMessage = $"Cannot find element by {locator.Kind} with value '{locator.Value}'.";
        var endTime = DateTime.Now.Add(timeout);

        while (DateTime.Now < endTime)
        {
            try
            {
                var element = findAction(window);
                if (element != null)
                {
                    return element;
                }
            }
            catch
            {
                // Ignore exceptions during retry
            }

            Thread.Sleep(500); // Retry interval
        }

        throw new InvalidOperationException(customMessage);
    }

    // UI Automation - GetElements with ElementLocator
    public static UIAutomationElement[] GetElements(this UIAutomationWindow? window, ElementLocator locator)
    {
        if (window == null) return Array.Empty<UIAutomationElement>();
        var elements = locator.ToFindAllAction()(window);
        return elements ?? Array.Empty<UIAutomationElement>();
    }

    // Strongly-typed helper methods for UI Automation
    public static UIAutomationButton? GetButton(this UIAutomationWindow? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsButton();
    }

    public static UIAutomationTextBox? GetTextBox(this UIAutomationWindow? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsTextBox();
    }

    public static UIAutomationCheckBox? GetCheckBox(this UIAutomationWindow? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsCheckBox();
    }

    public static UIAutomationComboBox? GetComboBox(this UIAutomationWindow? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsComboBox();
    }

    public static UIAutomationRadioButton? GetRadioButton(this UIAutomationWindow? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsRadioButton();
    }

    public static UIAutomationLabel? GetLabel(this UIAutomationWindow? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsLabel();
    }
}
