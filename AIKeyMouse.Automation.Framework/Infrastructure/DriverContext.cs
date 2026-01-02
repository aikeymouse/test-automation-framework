using AIKeyMouse.Automation.Framework.DataObjects;
using BrowserStack;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Service.Options;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public partial class DriverContext
{
    public AppiumDriver? _driver;
    public AppiumLocalService? _appiumLocalService;
    
    public AppiumDriver? Driver => _driver;

    private Local? _browserStackLocal;
    private string _browserStackLocalIdentifier = $"Reqnroll_{Guid.NewGuid().ToString("N")[..20]}"; 

    public void StartDriver()
    {
        string bsSessionName = ConfiguredSettings.Instance.BrowserStack.SessionNamePrefix;
        if (string.IsNullOrEmpty(ConfiguredSettings.Instance.Appium.ServerUrl))
        {
            _appiumLocalService = CreateAppiumService();
            _appiumLocalService.Start();
        }

        if ((ConfiguredSettings.Instance.Appium.ServerUrl?.Contains("browserstack", StringComparison.OrdinalIgnoreCase)) ?? false)
        {
            StartBrowserStackLocal();
        }

        switch (ConfiguredSettings.Instance.MobileDeviceType)
        {
            case MobileDeviceType.AndroidBS:
                _driver = new AndroidDriver(new Uri(ConfiguredSettings.Instance.Appium.ServerUrl ?? throw new InvalidOperationException("Appium ServerUrl is not configured")), AndroidBSOptions, TimeSpan.FromSeconds(180));
                ((IJavaScriptExecutor)_driver).ExecuteScript("browserstack_executor: {\"action\": \"setSessionName\", \"arguments\": {\"name\":\"" + bsSessionName + "\"}}");
                break;
            case MobileDeviceType.iOSBS:
                _driver = new IOSDriver(new Uri(ConfiguredSettings.Instance.Appium.ServerUrl ?? throw new InvalidOperationException("Appium ServerUrl is not configured")), IOSBSOptions, TimeSpan.FromSeconds(180));
                ((IJavaScriptExecutor)_driver).ExecuteScript("browserstack_executor: {\"action\": \"setSessionName\", \"arguments\": {\"name\":\"" + bsSessionName + "\"}}");
                break;
            default:
                throw new NotSupportedException("The specified mobile device type is not supported.");
        }

    }

    private static AppiumOptions AndroidOptions
    {
        get
        {
            AppiumOptions options = new()
            {
                DeviceName = ConfiguredSettings.Instance.Android.DeviceName,
                AutomationName = ConfiguredSettings.Instance.Android.AutomationName,
                App = ConfiguredSettings.Instance.Android.PackagePath,
                PlatformVersion = ConfiguredSettings.Instance.Android.OSVersion
            };

            options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, ConfiguredSettings.Instance.Android.PackagePath);
            options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, ConfiguredSettings.Instance.Android.AppActivity);
            options.AddAdditionalAppiumOption("appWaitDuration", ConfiguredSettings.Instance.Appium.AppWaitTimeout);
            options.AddAdditionalAppiumOption("orientation", ConfiguredSettings.Instance.TestDevice.Orientation);
            options.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AutoGrantPermissions, true);
            options.AddAdditionalAppiumOption("androidInstallTimeout", 60000);
            options.AddAdditionalAppiumOption("setWebContentsDebuggingEnabled", true);

            return options;
        }
    }

    private AppiumOptions AndroidBSOptions
    {
        get
        {
            AppiumOptions options = AndroidOptions;
            options.AddAdditionalAppiumOption("os_version", ConfiguredSettings.Instance.Android.OSVersion);
            options.AddAdditionalAppiumOption("browserstack.local", "true");
            options.AddAdditionalAppiumOption("browserstack.localIdentifier", _browserStackLocalIdentifier);
            options.AddAdditionalAppiumOption("browserstack.deviceLogs", ConfiguredSettings.Instance.BrowserStack.DeviceLog);
            options.AddAdditionalAppiumOption("browserstack.appiumLogs", ConfiguredSettings.Instance.BrowserStack.AppiumLog);
            options.AddAdditionalAppiumOption("browserstack.networkLogs", ConfiguredSettings.Instance.BrowserStack.NetworkLog);
            options.AddAdditionalAppiumOption("browserstack.debug", ConfiguredSettings.Instance.BrowserStack.VisualLog);
            options.AddAdditionalAppiumOption("interactiveDebugging", true);
            options.AddAdditionalAppiumOption("autoGrantPermissions", true);
            options.AddAdditionalAppiumOption("relaxedSecurityEnabled", true);
            options.AddAdditionalAppiumOption("browserstack.idleTimeout", 180);
            options.AddAdditionalAppiumOption("orientation", ConfiguredSettings.Instance.TestDevice.Orientation);

            return options;
        }
    }

    private static AppiumOptions IOSOptions
    {
        get
        {
            AppiumOptions options = new()
            {
                DeviceName = ConfiguredSettings.Instance.IOS.DeviceName,
                AutomationName = ConfiguredSettings.Instance.IOS.AutomationName,
                App = ConfiguredSettings.Instance.IOS.PackagePath,
                PlatformVersion = ConfiguredSettings.Instance.IOS.OSVersion
            };

            options.AddAdditionalAppiumOption("appWaitDuration", ConfiguredSettings.Instance.Appium.AppWaitTimeout);
            options.AddAdditionalAppiumOption("orientation", ConfiguredSettings.Instance.TestDevice.Orientation);
            options.AddAdditionalAppiumOption("fullContextList", false); // switch to webview context only when needed

            return options;
        }
    }

    private AppiumOptions IOSBSOptions
    {
        get
        {
            AppiumOptions options = IOSOptions;
            options.AddAdditionalAppiumOption("browserstack.local", true);
            options.AddAdditionalAppiumOption("browserstack.localIdentifier", _browserStackLocalIdentifier);
            options.AddAdditionalAppiumOption("browserstack.deviceLogs", ConfiguredSettings.Instance.BrowserStack.DeviceLog);
            options.AddAdditionalAppiumOption("browserstack.appiumLogs", ConfiguredSettings.Instance.BrowserStack.AppiumLog);
            options.AddAdditionalAppiumOption("browserstack.networkLogs", ConfiguredSettings.Instance.BrowserStack.NetworkLog);
            options.AddAdditionalAppiumOption("browserstack.debug", ConfiguredSettings.Instance.BrowserStack.VisualLog);
            options.AddAdditionalAppiumOption("browserstack.idleTimeout", 180);
            options.AddAdditionalAppiumOption("orientation", ConfiguredSettings.Instance.TestDevice.Orientation);

            return options;
        }
    }

    public static AppiumLocalService CreateAppiumService()
    {
        var serverOptions = new OptionCollector();
        var relaxedSecurityOption = new KeyValuePair<string, string>("--relaxed-security", string.Empty);
        var insercureADB = new KeyValuePair<string, string>("--allow-insecure=adb_shell", string.Empty);

        serverOptions.AddArguments(relaxedSecurityOption);
        serverOptions.AddArguments(insercureADB);
        
        return new AppiumServiceBuilder()
            .UsingAnyFreePort()
            .WithArguments(serverOptions)
            .Build();
    }

    private void StartBrowserStackLocal()
    {
        var bsOptions = new List<KeyValuePair<string, string>>
        {
            new("key", ConfiguredSettings.Instance.BrowserStack.AccessKey ?? string.Empty),
            new("forcelocal", "true"),
            new("localIdentifier", _browserStackLocalIdentifier),
            new("relaxedSecurityEnabled", "true"),
            new("browserstack.debug", "true"),
            new("debug", "true")
        };

        _browserStackLocal = new Local();
        _browserStackLocal.start(bsOptions);
    }

    public void SetNativeContext()
    {
        if (_driver == null)
        {
            throw new InvalidOperationException("Appium driver is not initialized.");
        }

        var context = _driver.Context;
        if (!context.Equals("NATIVE_APP", StringComparison.OrdinalIgnoreCase))
        {
            _driver.Context = "NATIVE_APP";
        }
    }

    public void SetWebViewContext(string contextName = "WEBVIEW")
    {
        if (_driver == null)
        {
            throw new InvalidOperationException("Appium driver is not initialized.");
        }

        var contexts = _driver.Contexts;
        foreach (var ctx in contexts)
        {
            if (ctx.Contains(contextName, StringComparison.CurrentCultureIgnoreCase))
            {
                _driver.Context = ctx;
                return;
            }
        }

        throw new InvalidOperationException("No WebView context found.");
    }

    public void Dispose()
    {
        // Dispose Appium driver
        if (_driver != null)
        {
            _driver.Quit();
            _driver.Dispose();
            _driver = null;
        }

        _appiumLocalService?.Dispose();
        _appiumLocalService = null;

        if (_browserStackLocal != null && _browserStackLocal.isRunning())
        {
            _browserStackLocal.stop();
            _browserStackLocal = null;
        }

        // Dispose Web driver (handled in DriverContextWeb.cs)
        
        // Dispose Windows application (handled in DriverContextWindows.cs)
        CloseWindowsApplication();
    }
}