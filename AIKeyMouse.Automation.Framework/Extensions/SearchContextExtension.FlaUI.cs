using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Infrastructure;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Tools;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static partial class SearchContextExtension
{
    // FlaUI - GetElement with ElementLocator
    public static AutomationElement? GetElement(this Window? window, ElementLocator locator)
    {
        return GetElement(window, locator, TimeSpan.FromSeconds(ConfiguredSettings.Instance.ShortTimeout));
    }

    public static AutomationElement? GetElement(this Window? window, ElementLocator locator, TimeSpan timeout)
    {
        if (window == null) throw new InvalidOperationException("Window is null.");

        var findAction = locator.ToFindFirstAction();
        var customMessage = $"Cannot find element by {locator.Kind} with value '{locator.Value}'.";

        var result = Retry.WhileNull(
            () => findAction(window),
            timeout,
            throwOnTimeout: false,
            ignoreException: true
        );

        if (!result.Success || result.Result == null)
        {
            throw new InvalidOperationException(customMessage);
        }

        return result.Result;
    }

    // FlaUI - GetElements with ElementLocator
    public static AutomationElement[] GetElements(this Window? window, ElementLocator locator)
    {
        if (window == null) return Array.Empty<AutomationElement>();
        var elements = locator.ToFindAllAction()(window);
        return elements ?? Array.Empty<AutomationElement>();
    }

    // Strongly-typed helper methods for FlaUI
    public static Button? GetButton(this Window? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsButton();
    }

    public static TextBox? GetTextBox(this Window? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsTextBox();
    }

    public static CheckBox? GetCheckBox(this Window? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsCheckBox();
    }

    public static ComboBox? GetComboBox(this Window? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsComboBox();
    }

    public static RadioButton? GetRadioButton(this Window? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsRadioButton();
    }

    public static Label? GetLabel(this Window? window, ElementLocator locator)
    {
        return window.GetElement(locator)?.AsLabel();
    }
}
