using Windows.Win32.UI.Accessibility;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

/// <summary>
/// Wrapper for UI Automation Window element
/// </summary>
public class UIAutomationWindow
{
    private readonly IUIAutomationElement _element;
    private readonly CUIAutomation8 _automation;

    public IUIAutomationElement Element => _element;
    public CUIAutomation8 Automation => _automation;

    public UIAutomationWindow(IUIAutomationElement element, CUIAutomation8 automation)
    {
        _element = element ?? throw new ArgumentNullException(nameof(element));
        _automation = automation ?? throw new ArgumentNullException(nameof(automation));
    }

    public string? Name
    {
        get => _element.CurrentName;
    }

    public string? AutomationId
    {
        get => _element.CurrentAutomationId;
    }

    public string? ClassName
    {
        get => _element.CurrentClassName;
    }

    public bool IsEnabled
    {
        get => _element.CurrentIsEnabled != 0;
    }

    // Find methods
    public UIAutomationElement? FindFirstByAutomationId(string automationId)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_AutomationIdPropertyId, automationId);
        var element = _element.FindFirst(TreeScope.TreeScope_Descendants, condition);
        return element != null ? new UIAutomationElement(element) : null;
    }

    public UIAutomationElement? FindFirstByName(string name)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_NamePropertyId, name);
        var element = _element.FindFirst(TreeScope.TreeScope_Descendants, condition);
        return element != null ? new UIAutomationElement(element) : null;
    }

    public UIAutomationElement? FindFirstByClassName(string className)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_ClassNamePropertyId, className);
        var element = _element.FindFirst(TreeScope.TreeScope_Descendants, condition);
        return element != null ? new UIAutomationElement(element) : null;
    }

    public UIAutomationElement? FindFirstByControlType(int controlTypeId)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_ControlTypePropertyId, controlTypeId);
        var element = _element.FindFirst(TreeScope.TreeScope_Descendants, condition);
        return element != null ? new UIAutomationElement(element) : null;
    }

    public UIAutomationElement[] FindAllByAutomationId(string automationId)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_AutomationIdPropertyId, automationId);
        var elements = _element.FindAll(TreeScope.TreeScope_Descendants, condition);
        return ConvertToArray(elements);
    }

    public UIAutomationElement[] FindAllByName(string name)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_NamePropertyId, name);
        var elements = _element.FindAll(TreeScope.TreeScope_Descendants, condition);
        return ConvertToArray(elements);
    }

    public UIAutomationElement[] FindAllByClassName(string className)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_ClassNamePropertyId, className);
        var elements = _element.FindAll(TreeScope.TreeScope_Descendants, condition);
        return ConvertToArray(elements);
    }

    public UIAutomationElement[] FindAllByControlType(int controlTypeId)
    {
        var condition = _automation.CreatePropertyCondition(UIA_PROPERTY_ID.UIA_ControlTypePropertyId, controlTypeId);
        var elements = _element.FindAll(TreeScope.TreeScope_Descendants, condition);
        return ConvertToArray(elements);
    }

    private UIAutomationElement[] ConvertToArray(IUIAutomationElementArray? elementArray)
    {
        if (elementArray == null)
            return Array.Empty<UIAutomationElement>();

        var count = elementArray.Length;
        var result = new UIAutomationElement[count];
        
        for (int i = 0; i < count; i++)
        {
            result[i] = new UIAutomationElement(elementArray.GetElement(i));
        }

        return result;
    }
}
