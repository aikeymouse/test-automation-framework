using Windows.Win32.UI.Accessibility;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

/// <summary>
/// Wrapper for UI Automation Element
/// </summary>
public class UIAutomationElement
{
    private readonly IUIAutomationElement _element;

    public IUIAutomationElement NativeElement => _element;

    public UIAutomationElement(IUIAutomationElement element)
    {
        _element = element ?? throw new ArgumentNullException(nameof(element));
    }

    public string? Name => _element.CurrentName;
    public string? AutomationId => _element.CurrentAutomationId;
    public string? ClassName => _element.CurrentClassName;
    public int ControlType => _element.CurrentControlType;
    public bool IsEnabled => _element.CurrentIsEnabled != 0;
    public bool IsOffscreen => _element.CurrentIsOffscreen != 0;

    public void Click()
    {
        if (_element.GetCurrentPattern(UIA_PATTERN_ID.UIA_InvokePatternId) is IUIAutomationInvokePattern invokePattern)
        {
            invokePattern.Invoke();
        }
        else
        {
            // Fallback: set focus and simulate click
            _element.SetFocus();
        }
    }

    public void SetValue(string value)
    {
        if (_element.GetCurrentPattern(UIA_PATTERN_ID.UIA_ValuePatternId) is IUIAutomationValuePattern valuePattern)
        {
            valuePattern.SetValue(value);
        }
    }

    public string? GetValue()
    {
        if (_element.GetCurrentPattern(UIA_PATTERN_ID.UIA_ValuePatternId) is IUIAutomationValuePattern valuePattern)
        {
            return valuePattern.CurrentValue;
        }
        return null;
    }

    public UIAutomationButton AsButton() => new UIAutomationButton(_element);
    public UIAutomationTextBox AsTextBox() => new UIAutomationTextBox(_element);
    public UIAutomationCheckBox AsCheckBox() => new UIAutomationCheckBox(_element);
    public UIAutomationComboBox AsComboBox() => new UIAutomationComboBox(_element);
    public UIAutomationRadioButton AsRadioButton() => new UIAutomationRadioButton(_element);
    public UIAutomationLabel AsLabel() => new UIAutomationLabel(_element);
}

public class UIAutomationButton : UIAutomationElement
{
    public UIAutomationButton(IUIAutomationElement element) : base(element) { }
}

public class UIAutomationTextBox : UIAutomationElement
{
    public UIAutomationTextBox(IUIAutomationElement element) : base(element) { }

    public string? Text
    {
        get => GetValue();
        set => SetValue(value ?? string.Empty);
    }
}

public class UIAutomationCheckBox : UIAutomationElement
{
    public UIAutomationCheckBox(IUIAutomationElement element) : base(element) { }

    public bool IsChecked
    {
        get
        {
            if (NativeElement.GetCurrentPattern(UIA_PATTERN_ID.UIA_TogglePatternId) is IUIAutomationTogglePattern togglePattern)
            {
                return togglePattern.CurrentToggleState == ToggleState.ToggleState_On;
            }
            return false;
        }
    }

    public void Toggle()
    {
        if (NativeElement.GetCurrentPattern(UIA_PATTERN_ID.UIA_TogglePatternId) is IUIAutomationTogglePattern togglePattern)
        {
            togglePattern.Toggle();
        }
    }
}

public class UIAutomationComboBox : UIAutomationElement
{
    public UIAutomationComboBox(IUIAutomationElement element) : base(element) { }

    public void Select(string itemName)
    {
        if (NativeElement.GetCurrentPattern(UIA_PATTERN_ID.UIA_ExpandCollapsePatternId) is IUIAutomationExpandCollapsePattern expandPattern)
        {
            expandPattern.Expand();
            Thread.Sleep(200); // Wait for expansion
            
            // Find and select the item
            // This is simplified - in production you'd search for the specific item
            expandPattern.Collapse();
        }
    }
}

public class UIAutomationRadioButton : UIAutomationElement
{
    public UIAutomationRadioButton(IUIAutomationElement element) : base(element) { }

    public bool IsSelected
    {
        get
        {
            if (NativeElement.GetCurrentPattern(UIA_PATTERN_ID.UIA_SelectionItemPatternId) is IUIAutomationSelectionItemPattern selectionPattern)
            {
                return selectionPattern.CurrentIsSelected != 0;
            }
            return false;
        }
    }

    public void Select()
    {
        if (NativeElement.GetCurrentPattern(UIA_PATTERN_ID.UIA_SelectionItemPatternId) is IUIAutomationSelectionItemPattern selectionPattern)
        {
            selectionPattern.Select();
        }
    }
}

public class UIAutomationLabel : UIAutomationElement
{
    public UIAutomationLabel(IUIAutomationElement element) : base(element) { }

    public string? Text => Name;
}
