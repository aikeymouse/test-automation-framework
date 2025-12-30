using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Infrastructure;
using Windows.Win32.UI.Accessibility;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static partial class LocatorExtension
{
    // UI Automation - Convert ElementLocator to find first element action
    public static Func<UIAutomationWindow, UIAutomationElement?> ToFindFirstAction(this ElementLocator locator)
    {
        return locator.Kind switch
        {
            Locator.Id => window => window.FindFirstByAutomationId(locator.Value),
            Locator.Name => window => window.FindFirstByName(locator.Value),
            Locator.ClassName => window => window.FindFirstByClassName(locator.Value),
            Locator.TagName => window => window.FindFirstByControlType(GetControlType(locator.Value)),
            _ => throw new ArgumentException($"Locator type {locator.Kind} is not supported for UI Automation")
        };
    }

    // UI Automation - Convert ElementLocator to find all elements action
    public static Func<UIAutomationWindow, UIAutomationElement[]> ToFindAllAction(this ElementLocator locator)
    {
        return locator.Kind switch
        {
            Locator.Id => window => window.FindAllByAutomationId(locator.Value),
            Locator.Name => window => window.FindAllByName(locator.Value),
            Locator.ClassName => window => window.FindAllByClassName(locator.Value),
            Locator.TagName => window => window.FindAllByControlType(GetControlType(locator.Value)),
            _ => throw new ArgumentException($"Locator type {locator.Kind} is not supported for UI Automation")
        };
    }

    // Helper method to convert string to ControlType ID
    private static int GetControlType(string controlTypeName)
    {
        return controlTypeName.ToLower() switch
        {
            "button" => UIA_CONTROLTYPE_ID.UIA_ButtonControlTypeId,
            "text" or "textbox" or "edit" => UIA_CONTROLTYPE_ID.UIA_EditControlTypeId,
            "checkbox" => UIA_CONTROLTYPE_ID.UIA_CheckBoxControlTypeId,
            "radiobutton" => UIA_CONTROLTYPE_ID.UIA_RadioButtonControlTypeId,
            "combobox" => UIA_CONTROLTYPE_ID.UIA_ComboBoxControlTypeId,
            "listitem" => UIA_CONTROLTYPE_ID.UIA_ListItemControlTypeId,
            "list" => UIA_CONTROLTYPE_ID.UIA_ListControlTypeId,
            "menu" => UIA_CONTROLTYPE_ID.UIA_MenuControlTypeId,
            "menuitem" => UIA_CONTROLTYPE_ID.UIA_MenuItemControlTypeId,
            "tab" => UIA_CONTROLTYPE_ID.UIA_TabControlTypeId,
            "tabitem" => UIA_CONTROLTYPE_ID.UIA_TabItemControlTypeId,
            "table" => UIA_CONTROLTYPE_ID.UIA_TableControlTypeId,
            "tree" => UIA_CONTROLTYPE_ID.UIA_TreeControlTypeId,
            "treeitem" => UIA_CONTROLTYPE_ID.UIA_TreeItemControlTypeId,
            "window" => UIA_CONTROLTYPE_ID.UIA_WindowControlTypeId,
            "pane" => UIA_CONTROLTYPE_ID.UIA_PaneControlTypeId,
            "label" => UIA_CONTROLTYPE_ID.UIA_TextControlTypeId,
            _ => UIA_CONTROLTYPE_ID.UIA_CustomControlTypeId
        };
    }
}
