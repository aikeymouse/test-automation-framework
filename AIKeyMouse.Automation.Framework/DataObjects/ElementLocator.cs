namespace AIKeyMouse.Automation.Framework.DataObjects;

public class ElementLocator(Locator kind, string value)
{
    public Locator Kind { get; set; } = kind;
    public string Value { get; set; } = value;
}