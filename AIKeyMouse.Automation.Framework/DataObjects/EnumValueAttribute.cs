namespace AIKeyMouse.Automation.Framework.DataObjects;

[AttributeUsage(AttributeTargets.Field)]
public class EnumValueAttribute(params string[] enumValues) : Attribute
{
    public string[] EnumValues = enumValues;
}