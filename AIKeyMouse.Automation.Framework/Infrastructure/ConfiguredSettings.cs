using System.Configuration;
using AIKeyMouse.Automation.Framework.DataObjects;

namespace AIKeyMouse.Automation.Framework.Infrastructure;

public sealed class ConfiguredSettings
{
    private static readonly Lazy<ConfiguredSettings> lazy =
        new(() => new ConfiguredSettings());
    public static ConfiguredSettings Instance => lazy.Value;

    public bool OpenDevTools { get; set; } = false;
    public string WindowSize { get; set; } = "1920,1080";
    public bool IgnoreUnhandledPrompt { get; set; } = true;
    public bool DriverLog { get; set; } = false;
    public string TestResultsPath { get; set; } = "TestResults";
    public double ShortTimeout { get; set; } = 5.0;
    public double LongTimeout { get; set; } = 30.0;
    public string WebView2DebuggerAddress { get; set; } = "localhost:9222";
    // appsettings.json settings for mobile devices
    public TestDeviceSettings TestDevice { get; set; } = new();
    public AndroidSettings Android { get; set; } = new();
    public IOSSettings IOS { get; set; } = new();
    public BrowserStackSettings BrowserStack { get; set; } = new();
    public AppiumSettings Appium { get; set; } = new();

    public MobileDeviceType MobileDeviceType =>
        TestDevice.Type?.ToLower() switch
        {
            "androidbs" => MobileDeviceType.AndroidBS,
            "iosbs" => MobileDeviceType.iOSBS,
            _ => MobileDeviceType.None
        };

    public bool IsAndroid => MobileDeviceType == MobileDeviceType.AndroidBS;
    public bool IsIOS => MobileDeviceType == MobileDeviceType.iOSBS;


    private ConfiguredSettings()
    {
        OpenDevTools = GetConfigurationSetting("OpenDevTools").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        WindowSize = GetConfigurationSetting("WindowSize");
        IgnoreUnhandledPrompt = GetConfigurationSetting("IgnoreUnhandledPrompt").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        DriverLog = GetConfigurationSetting("DriverLog").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        TestResultsPath = GetConfigurationSetting("TestResultsPath");
        ShortTimeout = Convert.ToDouble(GetConfigurationSetting("ShortTimeout"));
        LongTimeout = Convert.ToDouble(GetConfigurationSetting("LongTimeout"));
        WebView2DebuggerAddress = GetConfigurationSetting("WebView2DebuggerAddress");

        TestDevice.Type = GetConfigurationSetting("TestDevice:Type");
        TestDevice.Orientation = GetConfigurationSetting("TestDevice:Orientation");

        Android.DeviceName = GetConfigurationSetting("Android:DeviceName");
        Android.OSVersion = GetConfigurationSetting("Android:OSVersion");
        Android.AutomationName = GetConfigurationSetting("Android:AutomationName");
        Android.PackagePath = GetConfigurationSetting("Android:PackagePath");
        Android.AppActivity = GetConfigurationSetting("Android:AppActivity");

        IOS.DeviceName = GetConfigurationSetting("IOS:DeviceName");
        IOS.OSVersion = GetConfigurationSetting("IOS:OSVersion");
        IOS.AutomationName = GetConfigurationSetting("IOS:AutomationName");
        IOS.PackagePath = GetConfigurationSetting("IOS:PackagePath");

        BrowserStack.User = GetConfigurationSetting("BrowserStack:User");
        BrowserStack.AccessKey = GetConfigurationSetting("BrowserStack:AccessKey");
        BrowserStack.DeviceLog = GetConfigurationSetting("BrowserStack:DeviceLog").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        BrowserStack.AppiumLog = GetConfigurationSetting("BrowserStack:AppiumLog").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        BrowserStack.NetworkLog = GetConfigurationSetting("BrowserStack:NetworkLog").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        BrowserStack.VisualLog = GetConfigurationSetting("BrowserStack:VisualLog").Equals("true", StringComparison.CurrentCultureIgnoreCase);
        BrowserStack.SessionNamePrefix = GetConfigurationSetting("BrowserStack:SessionNamePrefix");

        Appium.ServerUrl = GetConfigurationSetting("Appium:ServerUrl");
        Appium.AppWaitTimeout = Convert.ToDouble(GetConfigurationSetting("Appium:AppWaitTimeout"));
        Appium.ImplicitWaitTimeout = Convert.ToDouble(GetConfigurationSetting("Appium:ImplicitWaitTimeout"));
    }

    private static string GetConfigurationSetting(string key)
    {
        return ConfigurationManager.AppSettings[key] ?? string.Empty;
    }
}