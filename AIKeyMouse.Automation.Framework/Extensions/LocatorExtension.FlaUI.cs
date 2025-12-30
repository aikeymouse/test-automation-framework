using AIKeyMouse.Automation.Framework.DataObjects;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static partial class LocatorExtension
{
    // FlaUI - Convert ElementLocator to find first element action
    public static Func<Window, AutomationElement?> ToFindFirstAction(this ElementLocator locator)
    {
        return locator.Kind switch
        {
            Locator.Id => window => window.FindFirstDescendant(cf => cf.ByAutomationId(locator.Value)),
            Locator.Name => window => window.FindFirstDescendant(cf => cf.ByName(locator.Value)),
            Locator.ClassName => window => window.FindFirstDescendant(cf => cf.ByClassName(locator.Value)),
            Locator.XPath => window => window.FindFirstByXPath(locator.Value),
            Locator.TagName => window => window.FindFirstDescendant(cf => cf.ByControlType(GetControlType(locator.Value))),
            _ => throw new ArgumentException($"Locator type {locator.Kind} is not supported for FlaUI")
        };
    }

    // FlaUI - Convert ElementLocator to find all elements action
    public static Func<Window, AutomationElement[]> ToFindAllAction(this ElementLocator locator)
    {
        return locator.Kind switch
        {
            Locator.Id => window => window.FindAllDescendants(cf => cf.ByAutomationId(locator.Value)),
            Locator.Name => window => window.FindAllDescendants(cf => cf.ByName(locator.Value)),
            Locator.ClassName => window => window.FindAllDescendants(cf => cf.ByClassName(locator.Value)),
            Locator.XPath => window => window.FindAllByXPath(locator.Value),
            Locator.TagName => window => window.FindAllDescendants(cf => cf.ByControlType(GetControlType(locator.Value))),
            _ => throw new ArgumentException($"Locator type {locator.Kind} is not supported for FlaUI")
        };
    }

    // Helper method to convert string to ControlType
    private static ControlType GetControlType(string controlTypeName)
    {
        return controlTypeName.ToLower() switch
        {
            "button" => ControlType.Button,
            "text" or "textbox" => ControlType.Text,
            "edit" => ControlType.Edit,
            "checkbox" => ControlType.CheckBox,
            "radiobutton" => ControlType.RadioButton,
            "combobox" => ControlType.ComboBox,
            "listitem" => ControlType.ListItem,
            "list" => ControlType.List,
            "menu" => ControlType.Menu,
            "menuitem" => ControlType.MenuItem,
            "tab" => ControlType.Tab,
            "tabitem" => ControlType.TabItem,
            "table" => ControlType.Table,
            "tree" => ControlType.Tree,
            "treeitem" => ControlType.TreeItem,
            "window" => ControlType.Window,
            "pane" => ControlType.Pane,
            "label" => ControlType.Text,
            _ => ControlType.Custom
        };
    }
}
