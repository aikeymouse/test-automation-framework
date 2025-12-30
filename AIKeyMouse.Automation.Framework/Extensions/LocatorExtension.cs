using AIKeyMouse.Automation.Framework.DataObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static partial class LocatorExtension
{
    public static By ToBy(this ElementLocator locator)
    {
        return locator.Kind switch
        {
            Locator.Id => By.Id(locator.Value),
            Locator.AccessibilityId => MobileBy.AccessibilityId(locator.Value),
            Locator.Name => By.Name(locator.Value),
            Locator.ClassName => By.ClassName(locator.Value),
            Locator.CssSelector => By.CssSelector(locator.Value),
            Locator.XPath => By.XPath(locator.Value),
            Locator.TagName => By.TagName(locator.Value),
            Locator.LinkText => By.LinkText(locator.Value),
            Locator.PartialLinkText => By.PartialLinkText(locator.Value),
            _ => throw new ArgumentException($"Unsupported locator type: {locator.Kind}")
        };
    }
}