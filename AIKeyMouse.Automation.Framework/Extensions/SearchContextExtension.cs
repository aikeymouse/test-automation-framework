using AIKeyMouse.Automation.Framework.DataObjects;
using AIKeyMouse.Automation.Framework.Extensions;
using AIKeyMouse.Automation.Framework.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AIKeyMouse.Automation.Framework.Extensions;

public static partial class SearchContextExtension
{
    public static IWebElement GetElement(this ISearchContext searchContext, ElementLocator locator)
    {
        return GetElement(searchContext, locator, TimeSpan.FromSeconds(ConfiguredSettings.Instance.ShortTimeout));
    }

    public static IWebElement GetElement(this ISearchContext searchContext, ElementLocator locator, TimeSpan timeout)
    {
        var driver = searchContext.ToDriver() ?? throw new InvalidOperationException("The search context does not have an associated WebDriver.");
        var by = locator.ToBy();
        var customMessage = GetCustomMessage(locator);

        var wait = new WebDriverWait(driver, timeout) { Message = customMessage };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        wait.Until(drv =>
        {
            try
            {
                return searchContext.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });

        throw new NoSuchElementException(customMessage);
    }

    public static IList<IWebElement> GetElements(this ISearchContext searchContext, ElementLocator locator)
    {
        var elements = searchContext.FindElements(locator.ToBy()).ToList();

        return elements;
    }

    public static void WaitUntil(this ISearchContext searchContext, Func<IWebDriver, bool> condition, TimeSpan timeout, string? message = null)
    {
        var driver = searchContext.ToDriver() ?? throw new InvalidOperationException("The search context does not have an associated WebDriver.");
        var wait = new WebDriverWait(driver, timeout);
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        wait.Until(condition);
    }

    public static IWebDriver? ToDriver(this ISearchContext searchContext)
    {
        if (searchContext is not IWrapsDriver wrappedElement)
        {
            return searchContext as IWebDriver;
        }
        return wrappedElement.WrappedDriver;
    }

    private static T? As<T>(this IWebElement element) where T : class, IWebElement
    {
        var constractor = typeof(T).GetConstructor([typeof(IWebElement)]);
        if (constractor != null)
        {
            return constractor.Invoke([element]) as T;
        }
        
        throw new ArgumentNullException($"Cannot convert IWebElement to {typeof(T).Name}.");
    }

    private static string GetCustomMessage(ElementLocator locator) => $"Cannot find element by {locator.Kind} with value '{locator.Value}'.";
}